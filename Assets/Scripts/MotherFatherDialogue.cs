using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Plays a back-and-forth dialogue between Mother and Father using two separate chat bubbles.
/// Each line pops in with a spawn animation, types out with a typewriter effect,
/// then after the LAST line, automatically loads the next scene.
///
/// Setup:
/// - Create two chat bubble UI objects (MotherBubble, FatherBubble), each with its own TMP text child.
/// - Both bubbles should be ACTIVE in the scene by default (this script disables them at runtime in Awake).
/// - Attach this script to an ALWAYS-ACTIVE GameObject (NOT to either bubble itself),
///   e.g. an empty "DialogueManager" object. Attaching it to a bubble that starts inactive
///   will stop Awake/Start from ever running.
/// - Fill in the Dialogue Lines list in the Inspector: pick Speaker (Mother/Father) and Text per line.
/// - Set Next Scene Name to the scene to load after the last line.
/// </summary>
public class MotherFatherDialogue : MonoBehaviour
{
    public enum Speaker { Mother, Father }

    [System.Serializable]
    public class DialogueLine
    {
        public Speaker speaker;
        [TextArea(1, 3)] public string text;
    }

    [Header("Bubble References")]
    [SerializeField] private GameObject motherBubbleRoot;
    [SerializeField] private TextMeshProUGUI motherBubbleText;
    [SerializeField] private GameObject fatherBubbleRoot;
    [SerializeField] private TextMeshProUGUI fatherBubbleText;

    [Header("Dialogue Content")]
    [SerializeField] private List<DialogueLine> dialogueLines = new List<DialogueLine>();

    [Header("Timing")]
    [SerializeField] private float delayBeforeStart = 1.0f;
    [SerializeField] private float typewriterSpeed = 0.03f;
    [SerializeField] private float delayAfterLine = 1.0f;     // pause after a line finishes typing, before next
    [SerializeField] private float delayBeforeSceneChange = 1.0f;

    [Header("Spawn Animation")]
    [SerializeField] private float spawnDuration = 0.25f;
    [SerializeField] private AnimationCurve spawnCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private Vector3 spawnStartScale = Vector3.zero;
    [SerializeField] private Vector3 spawnEndScale = Vector3.one;

    [Header("Scene Transition")]
    [SerializeField] private string nextSceneName;

    private void Awake()
    {
        // Hide both bubbles at the start so nothing flashes before the sequence begins.
        if (motherBubbleRoot != null) motherBubbleRoot.SetActive(false);
        if (fatherBubbleRoot != null) fatherBubbleRoot.SetActive(false);
    }

    private void Start()
    {
        StartCoroutine(PlayDialogue());
    }

    private IEnumerator PlayDialogue()
    {
        yield return new WaitForSeconds(delayBeforeStart);

        for (int i = 0; i < dialogueLines.Count; i++)
        {
            DialogueLine line = dialogueLines[i];

            GameObject activeRoot = line.speaker == Speaker.Mother ? motherBubbleRoot : fatherBubbleRoot;
            GameObject otherRoot = line.speaker == Speaker.Mother ? fatherBubbleRoot : motherBubbleRoot;
            TextMeshProUGUI activeText = line.speaker == Speaker.Mother ? motherBubbleText : fatherBubbleText;

            // Hide the other person's bubble while this one is talking
            if (otherRoot != null) otherRoot.SetActive(false);

            // Show + spawn this bubble
            activeRoot.SetActive(true);
            activeRoot.transform.localScale = spawnStartScale;
            if (activeText != null) activeText.text = "";

            yield return StartCoroutine(SpawnAnimation(activeRoot));

            // Typewriter the line
            if (activeText != null)
                yield return StartCoroutine(TypewriterEffect(activeText, line.text));

            // Pause before moving to the next line
            yield return new WaitForSeconds(delayAfterLine);
        }

        // Dialogue finished — wait a beat, then change scene
        yield return new WaitForSeconds(delayBeforeSceneChange);

        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
    }

    private IEnumerator SpawnAnimation(GameObject root)
    {
        float elapsed = 0f;

        while (elapsed < spawnDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / spawnDuration);
            float curved = spawnCurve.Evaluate(t);

            root.transform.localScale = Vector3.LerpUnclamped(spawnStartScale, spawnEndScale, curved);
            yield return null;
        }

        root.transform.localScale = spawnEndScale;
    }

    private IEnumerator TypewriterEffect(TextMeshProUGUI textComponent, string fullText)
    {
        textComponent.text = "";

        foreach (char c in fullText)
        {
            textComponent.text += c;
            yield return new WaitForSeconds(typewriterSpeed);
        }
    }
}