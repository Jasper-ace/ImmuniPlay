using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class ConversationManager : MonoBehaviour
{
    [Header("Bubbles")]
    public GameObject patientBubble;
    public TextMeshProUGUI patientText;

    public GameObject nurseBubble;
    public TextMeshProUGUI nurseText;

    [Header("Character")]
    public GameObject nurseCharacter; // 👈 Assign nurse image here

    [Header("Dialogue")]
    [TextArea(2,5)]
    public string[] dialogues;

    public bool[] isPatientSpeaking;

    [Header("Typing Settings")]
    public float typingSpeed = 0.03f;

    private int index = 0;
    private Coroutine typingCoroutine;
    private bool isTyping = false;

    void Start()
    {
        // Hide UI at start
        patientBubble.SetActive(false);
        nurseBubble.SetActive(false);

        // 👇 Hide nurse character initially
        if (nurseCharacter != null)
            nurseCharacter.SetActive(false);

        ShowDialogue();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // If typing → complete instantly
            if (isTyping)
            {
                StopCoroutine(typingCoroutine);
                CompleteText();
            }
            else
            {
                NextDialogue();
            }
        }
    }

    void NextDialogue()
    {
        index++;

        if (index < dialogues.Length)
        {
            ShowDialogue();
        }
        else
        {
            SceneManager.LoadScene("GameCategoryModal");
        }
    }

    void ShowDialogue()
    {
        // Hide both bubbles
        patientBubble.SetActive(false);
        nurseBubble.SetActive(false);

        // 👉 FIRST CLICK → Patient only
        if (index == 0)
        {
            patientBubble.SetActive(true);
            StartTyping(patientText, dialogues[index]);
            return;
        }

        // 👉 SECOND CLICK → Nurse appears + speaks
        if (index == 1)
        {
            if (nurseCharacter != null)
                nurseCharacter.SetActive(true);

            nurseBubble.SetActive(true);
            StartTyping(nurseText, dialogues[index]);
            return;
        }

        // 👉 AFTER → follow pattern
        if (isPatientSpeaking[index])
        {
            patientBubble.SetActive(true);
            StartTyping(patientText, dialogues[index]);
        }
        else
        {
            nurseBubble.SetActive(true);
            StartTyping(nurseText, dialogues[index]);
        }
    }

    void StartTyping(TextMeshProUGUI targetText, string sentence)
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText(targetText, sentence));
    }

    IEnumerator TypeText(TextMeshProUGUI targetText, string sentence)
    {
        isTyping = true;
        targetText.text = "";

        foreach (char letter in sentence)
        {
            targetText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    void CompleteText()
    {
        isTyping = false;

        if (index == 0)
        {
            patientText.text = dialogues[index];
        }
        else if (index == 1)
        {
            nurseText.text = dialogues[index];
        }
        else
        {
            if (isPatientSpeaking[index])
                patientText.text = dialogues[index];
            else
                nurseText.text = dialogues[index];
        }
    }
}