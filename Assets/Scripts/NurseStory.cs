using UnityEngine;
using TMPro;
using System.Collections;

public class NurseStory : MonoBehaviour
{
    [Header("UI")]
    public GameObject nurseBubble;
    public GameObject bottomDialogue;

    public TextMeshProUGUI nurseText;
    public TextMeshProUGUI bottomText;

    [Header("Dialogue")]
    [TextArea(2, 5)]
    public string[] dialogues;

    public bool[] isNurseTalking;

    [Header("Typewriter")]
    public float typingSpeed = 0.03f;

    [Header("Scene Transition")]
    public SceneFade fadeManager;
    public string nextScene = "Scene5";

    private int step = -1;
    private bool isTyping = false;
    private string currentDialogue;

    void Start()
    {
        nurseBubble.SetActive(false);
        bottomDialogue.SetActive(false);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // If currently typing, finish instantly
            if (isTyping)
            {
                StopAllCoroutines();

                if (isNurseTalking[step])
                    nurseText.text = currentDialogue;
                else
                    bottomText.text = currentDialogue;

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
        step++;

        if (step >= dialogues.Length)
        {
            if (fadeManager != null)
            {
                fadeManager.FadeToScene(nextScene);
            }
            return;
        }

        currentDialogue = dialogues[step];

        if (isNurseTalking[step])
        {
            nurseBubble.SetActive(true);
            bottomDialogue.SetActive(false);

            StartCoroutine(TypeText(nurseText, currentDialogue));
        }
        else
        {
            nurseBubble.SetActive(false);
            bottomDialogue.SetActive(true);

            StartCoroutine(TypeText(bottomText, currentDialogue));
        }
    }

    IEnumerator TypeText(TextMeshProUGUI textUI, string message)
    {
        isTyping = true;
        textUI.text = "";

        foreach (char letter in message)
        {
            textUI.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }
}