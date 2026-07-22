using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DialogueTyper : MonoBehaviour
{
    [Header("Auto Start")]
    public bool autoStartDialogue = false;

    [Header("Dialogue Text")]
    public TMP_Text dialogueText;

    [TextArea(2, 5)]
    public string[] dialogues;

    [Header("Optional Dialogue Bubble (Leave Empty if Not Needed)")]
    public GameObject dialogueBubble;

    [Header("Objects To Show After Dialogue")]
    public GameObject[] objectsToShow;

    [Header("Optional Object Fade")]
    [Tooltip("Assign a black UI Image. Leave empty if you don't want object transition.")]
    public Image objectFadeImage;

    public float objectFadeDuration = 1f;

    [Header("Hide Main Parent After Dialogue")]
    public bool hideMainParent = false;
    public GameObject mainParent;

    [Header("Typing")]
    public float typingSpeed = 0.03f;

    [Header("Optional Scene Fade")]
    [Tooltip("Assign a black UI Image. Leave empty if you don't want scene transition.")]
    public Image fadeImage;

    [Tooltip("Leave empty if you don't want to load another scene.")]
    public string nextSceneName = "";

    public float fadeDuration = 1f;

    [Header("Events")]
    public UnityEvent onFinish;
    private bool canClick = true;
    private int currentIndex = 0;
    private bool isTyping = false;
    private bool ignoreNextClick = false;
    private bool isTransitioning = false;
    private Coroutine typingCoroutine;

   void Start()
{
    currentIndex = 0;

    if (fadeImage != null)
    {
        Color c = fadeImage.color;
        c.a = 0f;
        fadeImage.color = c;
        fadeImage.gameObject.SetActive(true);
    }

    if (objectFadeImage != null)
    {
        Color c = objectFadeImage.color;
        c.a = 0f;
        objectFadeImage.color = c;
        objectFadeImage.gameObject.SetActive(true);
    }

    if (autoStartDialogue)
    {
        RestartDialogue();
    }
}
   void Update()
{
    if (isTransitioning)
        return;

    if (!canClick)
        return;

    // Ignore the mouse click that opened the panel
    if (ignoreNextClick)
    {
        if (Input.GetMouseButtonUp(0))
            ignoreNextClick = false;

        return;
    }

    if (Input.GetMouseButtonDown(0))
    {
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
    Debug.Log(System.Environment.StackTrace);
    Debug.Log("NextDialogue BEFORE: " + currentIndex);

    currentIndex++;

    Debug.Log("NextDialogue AFTER: " + currentIndex);

    if (currentIndex >= dialogues.Length)
    {
        FinishDialogue();
        return;
    }

    typingCoroutine = StartCoroutine(TypeText());
}

    void FinishDialogue()
    {
        isTransitioning = true;

        if (dialogueBubble != null)
            dialogueBubble.SetActive(false);

        // Scene Transition
        if (fadeImage != null && !string.IsNullOrEmpty(nextSceneName))
        {
            StartCoroutine(FadeAndLoadScene());
            return;
        }

        // Object Transition
        if (objectFadeImage != null)
        {
            StartCoroutine(FadeAndShowObjects());
            return;
        }

        // No Transition
        ShowObjects();

        if (onFinish != null)
            onFinish.Invoke();

        currentIndex = 0;
        isTransitioning = false;
    }
public void RestartDialogue()
{
    Debug.Log("RestartDialogue called");
    if (typingCoroutine != null)
    {
    StopCoroutine(typingCoroutine);
    typingCoroutine = null;
    }

    currentIndex = 0;
    isTyping = false;
    isTransitioning = false;

    ignoreNextClick = true;

    dialogueText.text = "";

    if (dialogueBubble != null)
        dialogueBubble.SetActive(true);

    typingCoroutine = StartCoroutine(TypeText());

    StartCoroutine(EnableClickAfterDelay());
}  void ShowObjects()
    {
        foreach (GameObject obj in objectsToShow)
        {
            if (obj != null)
                obj.SetActive(true);
        }

        if (hideMainParent && mainParent != null)
            mainParent.SetActive(false);
    }
IEnumerator TypeText()
{
    Debug.Log("TypeText START - currentIndex = " + currentIndex);

    isTyping = true;
    dialogueText.text = "";

    foreach (char letter in dialogues[currentIndex])
    {
        dialogueText.text += letter;
        yield return new WaitForSeconds(typingSpeed);
    }

    isTyping = false;
    typingCoroutine = null;

    Debug.Log("TypeText END - currentIndex = " + currentIndex);
}
IEnumerator EnableClickAfterDelay()
{
    canClick = false;

    yield return new WaitForSeconds(0.2f);

    canClick = true;
}
IEnumerator FadeAndShowObjects()
{
    // Fade to Black
    yield return StartCoroutine(FadeOut(objectFadeImage, objectFadeDuration));

    // Show objects
    ShowObjects();

    enabled = false;
}

IEnumerator FadeAndLoadScene()
{
    // Fade Out
    yield return StartCoroutine(FadeOut(fadeImage, fadeDuration));

    // Load Scene
    SceneManager.LoadScene(nextSceneName);
}

IEnumerator FadeOut(Image image, float duration)
{
    float t = 0f;
    Color c = image.color;

    while (t < duration)
    {
        t += Time.deltaTime;
        c.a = Mathf.Lerp(0f, 1f, t / duration);
        image.color = c;
        yield return null;
    }

    c.a = 1f;
    image.color = c;
}

IEnumerator FadeIn(Image image, float duration)
{
    float t = 0f;
    Color c = image.color;

    while (t < duration)
    {
        t += Time.deltaTime;
        c.a = Mathf.Lerp(1f, 0f, t / duration);
        image.color = c;
        yield return null;
    }

    c.a = 0f;
    image.color = c;
}
}