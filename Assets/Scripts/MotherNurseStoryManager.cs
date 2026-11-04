using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MotherNurseStoryManager : MonoBehaviour
{
    [Header("Mother")]
    public GameObject motherBubble;
    public TMP_Text motherDialogueText;

    public DialogueBoxAutoSize motherAutoSize;
    public DialoguePopAnimation motherPopAnimation;
    public DialogueTriangleAnimation motherTriangleAnimation;


    [TextArea(2, 5)]
    public string[] motherDialogues;

    [Header("Nurse")]
    public GameObject nurseBubble;
    public TMP_Text nurseDialogueText;

    public DialogueBoxAutoSize nurseAutoSize;
    public DialoguePopAnimation nursePopAnimation;
    public DialogueTriangleAnimation nurseTriangleAnimation;

    [TextArea(2, 5)]
    public string[] nurseDialogues;

    public enum Speaker
    {
        Mother,
        Nurse
    }

    [Header("Dialogue Order")]
    public Speaker[] dialogueOrder;

    [Header("Typewriter")]
    public float typingSpeed = 0.03f;

    [Header("Scene Transition")]
    [Tooltip("Drag your SceneFade GameObject here.")]
    public GameObject fadeManager;

    [Tooltip("Type the next scene name exactly as it appears in Build Settings.")]
public string nextSceneName;

    private int currentDialogue = 0;
    private int motherIndex = 0;
    private int nurseIndex = 0;

    private Speaker currentSpeaker;

    private bool isTyping = false;
    private Coroutine typingCoroutine;

    private TMP_Text currentText;
    private string currentSentence;

    void Start()
    {
        motherBubble.SetActive(false);
        nurseBubble.SetActive(false);

        currentDialogue = 0;
        motherIndex = 0;
        nurseIndex = 0;

        ShowDialogue();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (isTyping)
            {
                FinishTyping();
            }
            else
            {
                NextDialogue();
            }
        }
    }

    void ShowDialogue()
    {
        if (currentDialogue >= dialogueOrder.Length)
        {
            EndDialogue();
            return;
        }

        motherBubble.SetActive(false);
        nurseBubble.SetActive(false);

        currentSpeaker = dialogueOrder[currentDialogue];

        if (currentSpeaker == Speaker.Mother)
        {
            if (motherIndex >= motherDialogues.Length)
            {
                Debug.LogError("Mother dialogue index out of range.");
                EndDialogue();
                return;
            }

            motherBubble.SetActive(true);

            currentText = motherDialogueText;
            currentSentence = motherDialogues[motherIndex];
            currentSentence = ProcessDialogue(currentSentence);

            // Handle Dialogue Animations

            if (motherAutoSize != null)
                motherAutoSize.ResizeForText(currentSentence);

            if (motherPopAnimation != null)
                motherPopAnimation.PlayPop();

            if (motherTriangleAnimation != null)
                motherTriangleAnimation.PlayAnimation();
        }
        else
        {
            if (nurseIndex >= nurseDialogues.Length)
            {
                Debug.LogError("Nurse dialogue index out of range.");
                EndDialogue();
                return;
            }

            nurseBubble.SetActive(true);

            currentText = nurseDialogueText;
            currentSentence = nurseDialogues[nurseIndex];
            currentSentence = ProcessDialogue(currentSentence);

            // Handle Dialogue Animations

            if (nurseAutoSize != null)
                nurseAutoSize.ResizeForText(currentSentence);

            if (nursePopAnimation != null)
                nursePopAnimation.PlayPop();

            if (nurseTriangleAnimation != null)
                nurseTriangleAnimation.PlayAnimation();
        }

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeSentence());
    }

   string ProcessDialogue(string text)
{
    Debug.Log("===== BABY NAME CHECK =====");

    if (SaveManager.Instance == null)
    {
        Debug.LogError("SaveManager.Instance is NULL!");
        return text;
    }

    string babyName = SaveManager.Instance.GetBabyName();

    Debug.Log("Baby Name from SaveManager: [" + babyName + "]");
    Debug.Log("Original dialogue: [" + text + "]");

    if (string.IsNullOrEmpty(babyName))
    {
        Debug.LogError("BABY NAME IS EMPTY!");
        return text;
    }

    string processedText = text.Replace("{babyName}", babyName);

    Debug.Log("Processed dialogue: [" + processedText + "]");

    return processedText;
}

    IEnumerator TypeSentence()
    {
        isTyping = true;

        currentText.text = "";

        foreach (char letter in currentSentence)
        {
            currentText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    void FinishTyping()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        currentText.text = currentSentence;
        isTyping = false;
    }

    void NextDialogue()
    {
        Debug.Log($"NextDialogue called | CurrentSpeaker={currentSpeaker} | MotherIndex={motherIndex} | NurseIndex={nurseIndex}");

        if (currentSpeaker == Speaker.Mother)
            motherIndex++;
        else
            nurseIndex++;

        currentDialogue++;

        ShowDialogue();
    }

    void EndDialogue()
    {
        motherBubble.SetActive(false);
        nurseBubble.SetActive(false);

        Debug.Log("Dialogue Finished.");

        if (!string.IsNullOrWhiteSpace(nextSceneName))
        {
            if (fadeManager != null)
            {
                // Calls FadeToScene(string) on any script attached to the GameObject
                fadeManager.SendMessage(
                    "FadeToScene",
                    nextSceneName,
                    SendMessageOptions.DontRequireReceiver
                );
            }
            else
            {
                Debug.LogWarning("FadeManager GameObject not assigned. Loading scene directly.");
                SceneManager.LoadScene(nextSceneName);
            }
        }
    }
}