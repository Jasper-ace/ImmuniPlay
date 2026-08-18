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

        public bool isMother;
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

    [Header("Scene Manager")]
    public GameObject sceneManager;

    [Header("Next Scene (Optional)")]
    public string nextSceneName = "";

    [Header("Next Parent (Fallback)")]
    public GameObject nextParent;

    private int currentIndex = 0;
    private bool isTyping = false;
    private bool isTransitioning = false;
    private Coroutine typingCoroutine;

    void Start()
    {
        if (motherBubble != null)
            motherBubble.SetActive(false);

        if (fatherBubble != null)
            fatherBubble.SetActive(false);

        if (dialogues != null && dialogues.Length > 0)
        {
            ShowDialogue();
        }
    }

    void Update()
    {
        if (isTransitioning)
            return;

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

        // All dialogues finished
        if (currentIndex >= dialogues.Length)
        {
            isTransitioning = true;

            // =========================
            // NEXT SCENE
            // =========================
            if (!string.IsNullOrEmpty(nextSceneName))
            {
                if (sceneManager != null)
                {
                    SceneFade fade = sceneManager.GetComponent<SceneFade>();
                    if (fade != null)
                    {
                        // Call FadeToScene - it handles everything
                        fade.FadeToScene(nextSceneName);
                    }
                    else
                    {
                        Debug.LogError("SceneFade component not found on sceneManager!");
                        SceneManager.LoadScene(nextSceneName);
                    }
                }
                else
                {
                    Debug.LogWarning("Scene Manager not assigned, loading scene directly");
                    SceneManager.LoadScene(nextSceneName);
                }

                return;
            }

            // =========================
            // NEXT PARENT
            // =========================
            if (nextParent != null)
            {
                gameObject.SetActive(false);
                nextParent.SetActive(true);
                return;
            }

            // No next parent or scene
            gameObject.SetActive(false);

            return;
        }

        ShowDialogue();
    }

    void ShowDialogue()
    {
        if (motherBubble != null)
            motherBubble.SetActive(false);

        if (fatherBubble != null)
            fatherBubble.SetActive(false);

        Dialogue dialogue = dialogues[currentIndex];

        if (dialogue.isMother)
        {
            if (motherBubble != null)
                motherBubble.SetActive(true);

            if (motherTMP != null)
                motherTMP.text = "";

            typingCoroutine = StartCoroutine(
                TypeText(motherTMP, dialogue.text)
            );
        }
        else
        {
            if (fatherBubble != null)
                fatherBubble.SetActive(true);

            if (fatherTMP != null)
                fatherTMP.text = "";

            typingCoroutine = StartCoroutine(
                TypeText(fatherTMP, dialogue.text)
            );
        }
    }

    IEnumerator TypeText(TextMeshProUGUI target, string text)
    {
        isTyping = true;

        if (target != null)
            target.text = "";

        foreach (char c in text)
        {
            if (target != null)
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

        if (currentIndex >= dialogues.Length)
            return;

        Dialogue dialogue = dialogues[currentIndex];

        if (dialogue.isMother)
        {
            if (motherTMP != null)
                motherTMP.text = dialogue.text;
        }
        else
        {
            if (fatherTMP != null)
                fatherTMP.text = dialogue.text;
        }

        isTyping = false;
    }
}