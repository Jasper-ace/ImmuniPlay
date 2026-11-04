using System.Collections;
using UnityEngine;

public class EPIStampSequence : MonoBehaviour
{
    [System.Serializable]
    public class VaccineStep
    {
        public string vaccineName;

        [Tooltip("Checkmark object. Its position is also where the stamp lands.")]
        public RectTransform stampTarget;
    }

    [Header("Stamp")]
    public RectTransform stamp;

    [Tooltip("Empty RectTransform positioned off-screen at the bottom-right.")]
    public RectTransform stampStartPosition;

    [Header("Vaccines")]
    public VaccineStep[] vaccineSteps;

    [Header("Continue Button")]
    public GameObject continueButton;

    [Header("Movement")]
    public float delayBeforeStamping = 0.5f;
    public float stampMoveDuration = 0.6f;
    public float delayBetweenVaccines = 0.3f;

    [Header("Stamp Size")]
    public float normalScale = 0.8f;
    public float impactScale = 1.15f;
    public float scaleDuration = 0.15f;

    [Header("Fade Out")]
    public float fadeOutDuration = 0.8f;

    private bool sequenceStarted = false;
    private CanvasGroup stampCanvasGroup;

    private void Awake()
    {
        // Get or create CanvasGroup for fading
        if (stamp != null)
        {
            stampCanvasGroup = stamp.GetComponent<CanvasGroup>();

            if (stampCanvasGroup == null)
            {
                stampCanvasGroup =
                    stamp.gameObject.AddComponent<CanvasGroup>();
            }
        }

        ResetVisuals();
    }

    private void OnEnable()
    {
        // Automatically start when the EPI popup appears
        StartSequence();
    }

    public void StartSequence()
    {
        if (sequenceStarted)
            return;

        sequenceStarted = true;

        StopAllCoroutines();
        StartCoroutine(PlaySequence());
    }

    private IEnumerator PlaySequence()
    {
        ResetVisuals();

        yield return new WaitForSeconds(delayBeforeStamping);

        // ==========================================
        // MOVE STAMP TO OFF-SCREEN START POSITION
        // ==========================================

        if (stampStartPosition != null)
        {
            stamp.position = stampStartPosition.position;
        }

        stamp.gameObject.SetActive(true);
        stampCanvasGroup.alpha = 1f;
        stamp.localScale = Vector3.one * normalScale;

        // ==========================================
        // STAMP EACH VACCINE
        // ==========================================

        for (int i = 0; i < vaccineSteps.Length; i++)
        {
            yield return StartCoroutine(
                StampVaccine(vaccineSteps[i])
            );

            // Small pause before next vaccine
            if (i < vaccineSteps.Length - 1)
            {
                yield return new WaitForSeconds(
                    delayBetweenVaccines
                );
            }
        }

        // ==========================================
        // FADE OUT
        // ==========================================

        yield return StartCoroutine(FadeOutStamp());

        // ==========================================
        // SHOW CONTINUE
        // ==========================================

        if (continueButton != null)
        {
            continueButton.SetActive(true);
        }

        Debug.Log("EPI vaccination card sequence completed.");
    }

    private IEnumerator StampVaccine(VaccineStep step)
    {
        if (stamp == null || step.stampTarget == null)
            yield break;

        Debug.Log("Stamping: " + step.vaccineName);

        // ==========================================
        // MOVE TO VACCINE
        // ==========================================

        Vector3 startPosition = stamp.position;
        Vector3 targetPosition = step.stampTarget.position;

        float elapsed = 0f;

        while (elapsed < stampMoveDuration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / stampMoveDuration;

            // Smooth movement
            t = Mathf.SmoothStep(0f, 1f, t);

            stamp.position = Vector3.Lerp(
                startPosition,
                targetPosition,
                t
            );

            yield return null;
        }

        stamp.position = targetPosition;

        // ==========================================
        // GET BIGGER
        // ==========================================

        yield return StartCoroutine(
            ChangeStampScale(
                normalScale,
                impactScale
            )
        );

        // ==========================================
        // RETURN TO NORMAL SIZE
        // ==========================================

        yield return StartCoroutine(
            ChangeStampScale(
                impactScale,
                normalScale
            )
        );

        // ==========================================
        // SHOW CHECKMARK
        // ==========================================

        step.stampTarget.gameObject.SetActive(true);

        yield return StartCoroutine(
            AnimateCheckmark(step.stampTarget)
        );
    }

    private IEnumerator ChangeStampScale(
        float fromScale,
        float toScale
    )
    {
        float elapsed = 0f;

        while (elapsed < scaleDuration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / scaleDuration;

            t = Mathf.SmoothStep(0f, 1f, t);

            stamp.localScale = Vector3.Lerp(
                Vector3.one * fromScale,
                Vector3.one * toScale,
                t
            );

            yield return null;
        }

        stamp.localScale = Vector3.one * toScale;
    }

    private IEnumerator AnimateCheckmark(
        RectTransform checkmark
    )
    {
        checkmark.localScale = Vector3.zero;

        float duration = 0.25f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / duration;

            t = Mathf.SmoothStep(0f, 1f, t);

            checkmark.localScale = Vector3.Lerp(
                Vector3.zero,
                Vector3.one,
                t
            );

            yield return null;
        }

        checkmark.localScale = Vector3.one;
    }

    private IEnumerator FadeOutStamp()
    {
        float startAlpha = stampCanvasGroup.alpha;

        float elapsed = 0f;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / fadeOutDuration;

            t = Mathf.SmoothStep(0f, 1f, t);

            stampCanvasGroup.alpha = Mathf.Lerp(
                startAlpha,
                0f,
                t
            );

            yield return null;
        }

        stampCanvasGroup.alpha = 0f;

        stamp.gameObject.SetActive(false);
    }

    private void ResetVisuals()
    {
        // Reset stamp
        if (stamp != null)
        {
            stamp.gameObject.SetActive(false);

            stamp.localScale = Vector3.one * normalScale;

            if (stampCanvasGroup != null)
            {
                stampCanvasGroup.alpha = 1f;
            }
        }

        // Hide checkmarks
        foreach (VaccineStep step in vaccineSteps)
        {
            if (step.stampTarget != null)
            {
                step.stampTarget.gameObject.SetActive(false);
            }
        }

        // Hide continue button
        if (continueButton != null)
        {
            continueButton.SetActive(false);
        }

        sequenceStarted = false;
    }

    public void ResetSequence()
    {
        StopAllCoroutines();

        sequenceStarted = false;

        ResetVisuals();

        StartSequence();
    }
}