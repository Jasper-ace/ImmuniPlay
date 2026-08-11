using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class ParentDialogueManager : MonoBehaviour
{
    [System.Serializable]
    public class Dialogue
    {
        [TextArea(2, 5)]
        public string text;

        public bool isMother; // true = Mother, false = Father
    }

    [Header("Dialogue")]
    public Dialogue[] dialogues;

    [Header("Mother")]
    public GameObject motherBubble;
    public TextMeshProUGUI motherTMP;

    [Header("Father")]
    public GameObject fatherBubble;
    public TextMeshProUGUI fatherTMP;

    [Header("Typewriter")]
    public float typingSpeed = 0.03f;

    [Header("Next Scene (Optional)")]
    public string nextSceneName = ""; // Leave empty to use nextParent instead
    
    [Header("Next Parent (Fallback)")]
    public GameObject nextParent;

    private int currentIndex = 0;
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    void Start()
    {
        motherBubble.SetActive(false);
        fatherBubble.SetActive(false);

        if (dialogues.Length > 0)
            ShowDialogue();
    }

    void Update()
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        if (isTyping)
        {
            FinishTyping();
        }
        else
        {
            NextDialogue();
        }
    }

    void NextDialogue()
    {
        currentIndex++;

        if (currentIndex >= dialogues.Length)
        {
            // If a scene name is provided, load it
            if (!string.IsNullOrEmpty(nextSceneName))
            {
                SceneManager.LoadScene(nextSceneName);
            }
            // Otherwise, activate the next parent GameObject if assigned
            else if (nextParent != null)
            {
                nextParent.SetActive(true);
            }

            gameObject.SetActive(false);
            return;
        }

        ShowDialogue();
    }

    void ShowDialogue()
    {
        motherBubble.SetActive(false);
        fatherBubble.SetActive(false);

        Dialogue dialogue = dialogues[currentIndex];

        if (dialogue.isMother)
        {
            motherBubble.SetActive(true);
            motherTMP.text = "";

            typingCoroutine = StartCoroutine(TypeText(motherTMP, dialogue.text));
        }
        else
        {
            fatherBubble.SetActive(true);
            fatherTMP.text = "";

            typingCoroutine = StartCoroutine(TypeText(fatherTMP, dialogue.text));
        }
    }

    IEnumerator TypeText(TextMeshProUGUI target, string text)
    {
        isTyping = true;

        target.text = "";

        foreach (char c in text)
        {
            target.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        typingCoroutine = null;
    }

    void FinishTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        Dialogue dialogue = dialogues[currentIndex];

        if (dialogue.isMother)
        {
            motherTMP.text = dialogue.text;
        }
        else
        {
            fatherTMP.text = dialogue.text;
        }

        isTyping = false;
    }
}