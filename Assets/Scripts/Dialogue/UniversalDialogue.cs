using System;

using System.Collections;

using UnityEngine;

using UnityEngine.SceneManagement;

using UnityEngine.UI;

using TMPro;

public class UniversalDialogue : MonoBehaviour

{

    public enum TransitionType

    {

        None,

        DisableEnableGameObjects,

        NextScene,

        ChapterQuiz

    }

    [Serializable]

    public class CharacterEntry

    {

        [Tooltip("Name shown in the Inspector dropdown and above this character's dialogue.")]

        public string characterName = "Character";

        [Header("Dialogue UI")]

        public GameObject dialogueBubble;

        public TextMeshProUGUI dialogueText;

        [Tooltip("Optional parent GameObject containing the main character image and its shadow. Assign this so both flip together.")]

        public GameObject characterRoot;

        public Image characterImage;

        [Tooltip("Optional shadow image. Keep it as a child of Character Root so it flips with the character.")]

        public Image characterShadow;

        [Header("Dialogue Animations")]

        public DialogueBoxAutoSize autoSize;

        public DialoguePopAnimation popAnimation;

        public DialogueTriangleAnimation triangleAnimation;

        [Header("Optional Animators")]

        [Tooltip("Animator for the main character image or character rig.")]

        public Animator animator;

        [Tooltip("Optional Animator for the shadow, if the shadow has its own animation.")]

        public Animator shadowAnimator;

    }

    [Serializable]

    public class DialogueEntry

    {

        [TextArea(2, 5)]

        public string text;

        [Tooltip("Choose the speaker in the custom Inspector dropdown.")]

        public int speakerIndex;

        [Tooltip("Flip this speaker's character image horizontally for this line.")]

        public bool reverseCharacterX;

        [Tooltip("If enabled, flip this speaker's character horizontally when the entire dialogue finishes.")]
        public bool reverseCharacterXOnDialogueFinish;

        [Header("Optional Character Animation")]

        [Tooltip("If enabled, plays the state below from the beginning.")]

        public bool playAnimatorState;

        public string animatorStateName;

        [Tooltip("If enabled, sets this Animator trigger instead of playing a state.")]

        public bool useAnimatorTrigger;

        public string animatorTriggerName;

        [Header("Optional Character Animation End")]
        [Tooltip("When this dialogue line is advanced past, play this state on the character and shadow Animators.")]
        public bool playEndAnimatorState;
        public string endAnimatorStateName;

        [Tooltip("When this dialogue line is advanced past, set this trigger instead of playing a state.")]
        public bool useEndAnimatorTrigger;
        public string endAnimatorTriggerName;

        [Header("Voice")]

        public AudioClip voiceClip;

    }

    [Serializable]

    public class GameObjectGroup

    {

        public GameObject[] gameObjects;

    }

    [Header("CHARACTERS")]

    [SerializeField] private CharacterEntry[] characters;

    [Header("DIALOGUES")]

    [SerializeField] private DialogueEntry[] dialogues;

    [Header("DIALOGUE SETTINGS")]

    [Min(0.001f)]

    [SerializeField] private float typingSpeed = 0.035f;

    [Range(0f, 1f)]

    [SerializeField] private float voiceVolume = 1f;

    [SerializeField] private bool startAutomatically = true;

    [SerializeField] private bool hideAllBubblesAtStart = true;

    [Header("GAME OBJECTS ON DIALOGUE START")]

    [SerializeField] private GameObject[] showOnStart;

    [SerializeField] private GameObject[] hideOnStart;

    [Header("AFTER DIALOGUE")]

    [SerializeField] private TransitionType transitionType = TransitionType.None;

    [SerializeField] private GameObject[] showAfterDialogue;

    [SerializeField] private GameObject[] hideAfterDialogue;

    [SerializeField] private string nextSceneName;

    [Header("QUIZ SETTINGS")]

    [SerializeField] private string quizChapterFileName;

    [SerializeField] private string quizSceneName;

    [SerializeField] private string nextChapterScene;

    private int currentDialogueIndex = -1;

    private Coroutine typingCoroutine;

    private bool isTyping;

    private bool dialogueFinished;

    // Keep a state-based Optional Character Animation active until the player advances.
    private bool holdDialogueAnimation;
    private int heldDialogueIndex = -1;
    private float heldAnimationStartTime;
    private Vector3[] originalCharacterScales;
    // Stores each character's current facing so it persists across dialogue lines.
    private bool[] characterFacingReversed;

    private void Awake()
    {
        if (characters == null) return;

        originalCharacterScales = new Vector3[characters.Length];
        characterFacingReversed = new bool[characters.Length];

        for (int i = 0; i < characters.Length; i++)
        {
            Transform target = GetCharacterFlipTarget(characters[i]);
            if (target != null)
            {
                originalCharacterScales[i] = target.localScale;
                characterFacingReversed[i] = target.localScale.x < 0f;
            }
        }
    }

    public bool IsTyping => isTyping;

    public bool IsFinished => dialogueFinished;

    private void Update()
    {
        KeepCurrentDialogueAnimationPlaying();
    }

    private void Start()
    {

        if (hideAllBubblesAtStart)
            HideAllBubbles();

        if (startAutomatically)
            StartDialogue();

    }

    ///&#x20;\<summary>Can be called by a UI Button when startAutomatically is disabled.\</summary>

    public void StartDialogue()
    {

        if (dialogues == null || dialogues.Length == 0)
        {

            Debug.LogWarning($"{nameof(UniversalDialogue)} on {name} has no dialogue entries.", this);

            FinishDialogue();

            return;

        }

        dialogueFinished = false;

        currentDialogueIndex = -1;

        SetActiveForArray(showOnStart, true);

        SetActiveForArray(hideOnStart, false);

        HideAllBubbles();

        AdvanceDialogue();

    }

    ///&#x20;\<summary>Connect this to a dialogue Continue button or input handler.\</summary>

    public void AdvanceDialogue()
    {

        if (dialogueFinished)
            return;

        if (isTyping)
        {

            CompleteCurrentTypingImmediately();

            return;

        }

        // Stop the current voice only when the player advances past this dialogue line.
        // A tap that only completes the typewriter effect returns above, so the voice continues.
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopDialogue();
        }

        // Only stop holding the starting animation if this line has an end animation configured.
        // If no end state/trigger is configured, keep the starting animation active.
        if (dialogues != null && currentDialogueIndex >= 0 && currentDialogueIndex < dialogues.Length)
        {
            DialogueEntry previousEntry = dialogues[currentDialogueIndex];
            bool hasEndAnimation = previousEntry != null &&
                ((previousEntry.playEndAnimatorState && !string.IsNullOrWhiteSpace(previousEntry.endAnimatorStateName)) ||
                 (previousEntry.useEndAnimatorTrigger && !string.IsNullOrWhiteSpace(previousEntry.endAnimatorTriggerName)));

            if (previousEntry != null && previousEntry.reverseCharacterXOnDialogueFinish &&
                characters != null && characters.Length > 0)
            {
                int speakerIndex = Mathf.Clamp(previousEntry.speakerIndex, 0, characters.Length - 1);
                ToggleCharacterFacing(speakerIndex);
            }

            if (hasEndAnimation)
            {
                holdDialogueAnimation = false;
                PlayCharacterAnimationEnd(previousEntry);
            }
        }

        currentDialogueIndex++;

        if (dialogues == null || currentDialogueIndex >= dialogues.Length)
        {

            FinishDialogue();

            return;

        }

        ShowDialogue(dialogues[currentDialogueIndex]);

    }

    private void ShowDialogue(DialogueEntry entry)
    {

        if (entry == null)
        {

            AdvanceDialogue();

            return;

        }

        if (characters == null || characters.Length == 0)
        {

            Debug.LogWarning($"{nameof(UniversalDialogue)} has dialogue entries but no characters configured.", this);

            FinishDialogue();

            return;

        }

        int speakerIndex = Mathf.Clamp(entry.speakerIndex, 0, characters.Length - 1);

        CharacterEntry speaker = characters[speakerIndex];

        HideAllBubbles();

        if (speaker.dialogueBubble != null)
            speaker.dialogueBubble.SetActive(true);

        if (speaker.dialogueText != null)
            speaker.dialogueText.text = string.Empty;

        int currentSpeakerIndex = Mathf.Clamp(entry.speakerIndex, 0, characters.Length - 1);

        // A line's Reverse Character X checkbox flips the character when this line appears.
        // Otherwise, keep the character's current facing from earlier lines.
        if (entry.reverseCharacterX)
            ToggleCharacterFacing(currentSpeakerIndex);
        else
            ApplyCharacterFacing(speaker, GetCharacterFacing(currentSpeakerIndex));

        PlayCharacterAnimation(speaker, entry);

        // A configured state animation becomes the held animation for this dialogue.
        // A trigger animation relinquishes state holding. If neither is configured,
        // preserve the previous held animation instead of letting it fall back to idle.
        bool hasStartState = entry.playAnimatorState &&
            !entry.useAnimatorTrigger &&
            !string.IsNullOrWhiteSpace(entry.animatorStateName);
        bool hasStartTrigger = entry.useAnimatorTrigger &&
            !string.IsNullOrWhiteSpace(entry.animatorTriggerName);

        if (hasStartState)
        {
            holdDialogueAnimation = true;
            heldDialogueIndex = currentDialogueIndex;
            heldAnimationStartTime = Time.time;
        }
        else if (hasStartTrigger)
        {
            holdDialogueAnimation = false;
            heldDialogueIndex = -1;
        }

        if (speaker.autoSize != null)
            speaker.autoSize.ResizeForText(entry.text ?? string.Empty);

        if (speaker.popAnimation != null)
            speaker.popAnimation.PlayPop();

        if (speaker.triangleAnimation != null)
            speaker.triangleAnimation.PlayAnimation();

        if (entry.voiceClip != null && AudioManager.Instance != null)
            AudioManager.Instance.PlayDialogue(entry.voiceClip, voiceVolume);

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText(speaker.dialogueText, entry.text ?? string.Empty));

    }

    private IEnumerator TypeText(TextMeshProUGUI target, string text)
    {

        isTyping = true;

        if (target == null)
        {

            isTyping = false;

            yield break;

        }

        target.text = string.Empty;

        foreach (char character in text)
        {

            target.text += character;

            yield return new WaitForSecondsRealtime(typingSpeed);

        }

        isTyping = false;

        typingCoroutine = null;

    }

    private void CompleteCurrentTypingImmediately()
    {

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        if (dialogues == null || currentDialogueIndex < 0 || currentDialogueIndex >= dialogues.Length)
        {

            isTyping = false;

            typingCoroutine = null;

            return;

        }

        DialogueEntry entry = dialogues[currentDialogueIndex];

        int speakerIndex = Mathf.Clamp(entry.speakerIndex, 0, Mathf.Max(0, characters.Length - 1));

        if (characters.Length > 0 && characters[speakerIndex].dialogueText != null)
            characters[speakerIndex].dialogueText.text = entry.text ?? string.Empty;

        isTyping = false;

        typingCoroutine = null;

    }

    private bool GetCharacterFacing(int characterIndex)
    {
        if (characterFacingReversed == null ||
            characterIndex < 0 || characterIndex >= characterFacingReversed.Length)
            return false;

        return characterFacingReversed[characterIndex];
    }

    private void ToggleCharacterFacing(int characterIndex)
    {
        if (characters == null || characterIndex < 0 || characterIndex >= characters.Length)
            return;

        bool newFacing = !GetCharacterFacing(characterIndex);
        characterFacingReversed[characterIndex] = newFacing;
        ApplyCharacterFacing(characters[characterIndex], newFacing);
    }

    private void ApplyCharacterFacing(CharacterEntry character, bool reverse)
    {
        if (character == null) return;

        Transform flipTarget = GetCharacterFlipTarget(character);
        if (flipTarget == null) return;

        int characterIndex = Array.IndexOf(characters, character);
        Vector3 originalScale = flipTarget.localScale;
        if (originalCharacterScales != null && characterIndex >= 0 && characterIndex < originalCharacterScales.Length)
            originalScale = originalCharacterScales[characterIndex];

        if (reverse)
            originalScale.x = -Mathf.Abs(originalScale.x);
        else
            originalScale.x = Mathf.Abs(originalScale.x);

        if (Mathf.Abs(originalScale.x) < 0.0001f)
            originalScale.x = reverse ? -1f : 1f;

        flipTarget.localScale = originalScale;

        if (characterFacingReversed != null && characterIndex >= 0 &&
            characterIndex < characterFacingReversed.Length)
        {
            characterFacingReversed[characterIndex] = reverse;
        }
    }

    private Transform GetCharacterFlipTarget(CharacterEntry character)
    {
        if (character == null) return null;
        if (character.characterRoot != null) return character.characterRoot.transform;
        if (character.characterImage != null) return character.characterImage.transform;
        return null;
    }

    private void KeepCurrentDialogueAnimationPlaying()
    {
        if (!holdDialogueAnimation || dialogues == null ||
            heldDialogueIndex < 0 || heldDialogueIndex >= dialogues.Length)
            return;

        DialogueEntry entry = dialogues[heldDialogueIndex];
        if (entry == null || !entry.playAnimatorState || entry.useAnimatorTrigger ||
            string.IsNullOrWhiteSpace(entry.animatorStateName) || characters == null || characters.Length == 0)
            return;

        int speakerIndex = Mathf.Clamp(entry.speakerIndex, 0, characters.Length - 1);
        CharacterEntry speaker = characters[speakerIndex];
        if (speaker == null)
            return;

        HoldAnimatorState(speaker.animator, entry.animatorStateName);
        HoldAnimatorState(speaker.shadowAnimator, entry.animatorStateName);
    }

    private void HoldAnimatorState(Animator targetAnimator, string stateName)
    {
        if (targetAnimator == null || !targetAnimator.isActiveAndEnabled ||
            string.IsNullOrWhiteSpace(stateName))
            return;

        int stateHash = Animator.StringToHash(stateName);
        AnimatorStateInfo currentState = targetAnimator.GetCurrentAnimatorStateInfo(0);

        bool currentIsTarget =
            currentState.shortNameHash == stateHash ||
            currentState.fullPathHash == stateHash;

        if (targetAnimator.IsInTransition(0))
        {
            AnimatorStateInfo nextState = targetAnimator.GetNextAnimatorStateInfo(0);
            bool nextIsTarget =
                nextState.shortNameHash == stateHash ||
                nextState.fullPathHash == stateHash;

            // If the Animator is already transitioning into the requested state, leave it alone.
            if (nextIsTarget)
                return;
        }
        else if (currentIsTarget)
        {
            // Let the clip advance naturally. Do not call Animator.Play every frame.
            return;
        }

        // If an Animator transition moved away from the held state, restore it at the
        // current normalized position so the clip does not restart or speed up.
        float normalizedTime = currentIsTarget ? currentState.normalizedTime : 0f;
        targetAnimator.Play(stateHash, 0, normalizedTime);
    }

    private void PlayCharacterAnimation(CharacterEntry character, DialogueEntry entry)
    {

        if (character == null || entry == null)
            return;

        PlayAnimator(character.animator, entry);

        PlayAnimator(character.shadowAnimator, entry);

    }

    private void PlayCharacterAnimationEnd(DialogueEntry entry)
    {
        if (entry == null || characters == null || characters.Length == 0)
            return;

        int speakerIndex = Mathf.Clamp(entry.speakerIndex, 0, characters.Length - 1);
        CharacterEntry speaker = characters[speakerIndex];
        if (speaker == null)
            return;

        PlayAnimatorEnd(speaker.animator, entry);
        PlayAnimatorEnd(speaker.shadowAnimator, entry);
    }

    private void PlayAnimatorEnd(Animator targetAnimator, DialogueEntry entry)
    {
        if (targetAnimator == null || entry == null)
            return;

        if (entry.useEndAnimatorTrigger && !string.IsNullOrWhiteSpace(entry.endAnimatorTriggerName))
        {
            targetAnimator.ResetTrigger(entry.endAnimatorTriggerName);
            targetAnimator.SetTrigger(entry.endAnimatorTriggerName);
        }
        else if (entry.playEndAnimatorState && !string.IsNullOrWhiteSpace(entry.endAnimatorStateName))
        {
            targetAnimator.Play(entry.endAnimatorStateName, 0, 0f);
        }
    }

    private void PlayAnimator(Animator targetAnimator, DialogueEntry entry)
    {

        if (targetAnimator == null)
            return;

        if (entry.useAnimatorTrigger && !string.IsNullOrWhiteSpace(entry.animatorTriggerName))
        {

            targetAnimator.ResetTrigger(entry.animatorTriggerName);

            targetAnimator.SetTrigger(entry.animatorTriggerName);

        }

        else if (entry.playAnimatorState && !string.IsNullOrWhiteSpace(entry.animatorStateName))
        {

            targetAnimator.Play(entry.animatorStateName, 0, 0f);

        }

    }

    private void HideAllBubbles()
    {

        if (characters == null)
            return;

        foreach (CharacterEntry character in characters)
        {

            if (character != null && character.dialogueBubble != null)
                character.dialogueBubble.SetActive(false);

        }

    }

    private void FinishDialogue()
    {

        if (dialogueFinished)
            return;

        dialogueFinished = true;

        isTyping = false;

        if (typingCoroutine != null)
        {

            StopCoroutine(typingCoroutine);

            typingCoroutine = null;

        }

        HideAllBubbles();

        SetActiveForArray(showAfterDialogue, true);

        SetActiveForArray(hideAfterDialogue, false);

        switch (transitionType)
        {

            case TransitionType.None:

                break;

            case TransitionType.DisableEnableGameObjects:

                // The arrays above are the requested objects to show/hide.

                break;

            case TransitionType.NextScene:

                if (string.IsNullOrWhiteSpace(nextSceneName))
                    Debug.LogWarning($"{nameof(UniversalDialogue)}: Next Scene Name is empty.", this);

                else

                    SceneManager.LoadScene(nextSceneName);

                break;

            case TransitionType.ChapterQuiz:

                if (string.IsNullOrWhiteSpace(quizSceneName))
                {

                    Debug.LogWarning($"{nameof(UniversalDialogue)}: Quiz Scene Name is empty.", this);

                    break;

                }

                // Uses the project's existing QuizSceneData class.

                QuizSceneData.chapterFileName = quizChapterFileName;

                QuizSceneData.nextChapterScene = nextChapterScene;

                SceneManager.LoadScene(quizSceneName);

                break;

        }

    }

    private void SetActiveForArray(GameObject[] objects, bool active)
    {

        if (objects == null)
            return;

        foreach (GameObject obj in objects)
        {

            if (obj != null)
                obj.SetActive(active);

        }

    }

}
