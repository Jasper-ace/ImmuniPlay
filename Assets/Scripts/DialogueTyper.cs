using System.Collections;
using TMPro;
using UnityEngine;

public class DialogueTyper : MonoBehaviour
{
    public TMP_Text dialogueText;

    [TextArea(2, 5)]
    public string[] dialogues;

    public float typingSpeed = 0.03f;

    [Header("Objects To Show After Dialogue")]
    public GameObject[] objectsToShow;

    private int currentIndex = 0;

    private bool isTyping = false;

    private Coroutine typingCoroutine;

    void Start()
    {
        currentIndex = 0;

        if (dialogues.Length > 0)
        {
            typingCoroutine = StartCoroutine(TypeText());
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Finish current typing instantly
            if (isTyping)
            {
                StopCoroutine(typingCoroutine);

                dialogueText.text = dialogues[currentIndex];

                isTyping = false;
            }
            else
            {
                NextDialogue();
            }
        }
    }

    void NextDialogue()
    {
        currentIndex++;

        // Last dialogue reached
        if (currentIndex >= dialogues.Length)
        {
            gameObject.SetActive(false);

            foreach (GameObject obj in objectsToShow)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                }
            }

            return;
        }

        typingCoroutine = StartCoroutine(TypeText());
    }

    IEnumerator TypeText()
    {
        isTyping = true;

        dialogueText.text = "";

        foreach (char letter in dialogues[currentIndex].ToCharArray())
        {
            dialogueText.text += letter;

            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }
}