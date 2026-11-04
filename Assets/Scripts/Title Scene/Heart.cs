using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class Heart : MonoBehaviour, IPointerClickHandler
{
    // =========================================================
    // MOVEMENT
    // =========================================================

    [Header("Movement")]
    public float moveSpeed = 80f;
    public float driftAmount = 20f;


    // =========================================================
    // LIFETIME
    // =========================================================

    [Header("Lifetime")]
    public float lifetime = 4f;
    public float fadeInTime = 0.5f;
    public float fadeOutTime = 1.5f;


    // =========================================================
    // POP EFFECT
    // =========================================================

    [Header("Pop Effect")]
    public float popDuration = 0.15f;
    public float popScale = 1.5f;


    // =========================================================
    // POP SOUND
    // =========================================================

    [Header("Pop Sound")]

    [Tooltip("Sound played when the heart is tapped.")]
    public AudioClip popSound;

    [Tooltip("Volume of the pop sound.")]
    [Range(0f, 1f)]
    public float popVolume = 1f;


    // =========================================================
    // INTERNAL
    // =========================================================

    private RectTransform rectTransform;
    private Image heartImage;

    private float timer;
    private float startX;
    private float randomOffset;

    private bool isPopping = false;


    // =========================================================
    // AWAKE
    // =========================================================

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        heartImage = GetComponent<Image>();
    }


    // =========================================================
    // START
    // =========================================================

    void Start()
    {
        // Randomize each heart
        moveSpeed = Random.Range(50f, 100f);
        driftAmount = Random.Range(10f, 30f);
        lifetime = Random.Range(4f, 7f);

        // Random size
        float size = Random.Range(0.7f, 1.3f);
        rectTransform.localScale = Vector3.one * size;

        // Random rotation
        rectTransform.localRotation =
            Quaternion.Euler(
                0f,
                0f,
                Random.Range(-15f, 15f)
            );

        startX = rectTransform.anchoredPosition.x;

        randomOffset = Random.Range(0f, 100f);

        // Start invisible
        SetAlpha(0f);
    }


    // =========================================================
    // UPDATE
    // =========================================================

    void Update()
    {
        // Stop movement while popping
        if (isPopping)
            return;

        timer += Time.deltaTime;


        // -----------------------------------------------------
        // Move upward
        // -----------------------------------------------------

        rectTransform.anchoredPosition +=
            Vector2.up *
            moveSpeed *
            Time.deltaTime;


        // -----------------------------------------------------
        // Gentle left/right movement
        // -----------------------------------------------------

        float drift =
            Mathf.Sin(
                (timer + randomOffset) * 1.5f
            ) *
            driftAmount;

        Vector2 position =
            rectTransform.anchoredPosition;

        position.x =
            startX + drift;

        rectTransform.anchoredPosition =
            position;


        // -----------------------------------------------------
        // Fade in
        // -----------------------------------------------------

        if (timer < fadeInTime)
        {
            float alpha =
                timer / fadeInTime;

            SetAlpha(alpha);
        }


        // -----------------------------------------------------
        // Fade out
        // -----------------------------------------------------

        else if (timer > lifetime - fadeOutTime)
        {
            float alpha =
                Mathf.Clamp01(
                    (lifetime - timer) /
                    fadeOutTime
                );

            SetAlpha(alpha);
        }


        // -----------------------------------------------------
        // Destroy when lifetime ends
        // -----------------------------------------------------

        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }


    // =========================================================
    // TAP / CLICK
    // =========================================================

    public void OnPointerClick(
        PointerEventData eventData
    )
    {
        Pop();
    }


    // =========================================================
    // POP
    // =========================================================

    public void Pop()
    {
        if (isPopping)
            return;

        isPopping = true;


        // -----------------------------------------------------
        // PLAY POP SOUND
        // -----------------------------------------------------

        if (
            popSound != null &&
            AudioManager.Instance != null
        )
        {
            AudioManager.Instance.PlaySFX(
                popSound,
                popVolume
            );
        }


        // -----------------------------------------------------
        // START POP ANIMATION
        // -----------------------------------------------------

        StartCoroutine(
            PopAnimation()
        );
    }


    // =========================================================
    // POP ANIMATION
    // =========================================================

    IEnumerator PopAnimation()
    {
        Vector3 originalScale =
            rectTransform.localScale;

        float elapsed = 0f;


        while (elapsed < popDuration)
        {
            elapsed += Time.deltaTime;

            float progress =
                elapsed / popDuration;


            // -------------------------------------------------
            // Grow
            // -------------------------------------------------

            float scale =
                Mathf.Lerp(
                    1f,
                    popScale,
                    progress
                );

            rectTransform.localScale =
                originalScale * scale;


            // -------------------------------------------------
            // Fade out
            // -------------------------------------------------

            SetAlpha(
                1f - progress
            );

            yield return null;
        }


        // -----------------------------------------------------
        // Make sure it is fully invisible
        // -----------------------------------------------------

        SetAlpha(0f);


        // -----------------------------------------------------
        // Wait for pop sound before destroying
        // -----------------------------------------------------

        if (popSound != null)
        {
            yield return new WaitForSeconds(
                popSound.length
            );
        }


        // -----------------------------------------------------
        // Destroy heart
        // -----------------------------------------------------

        Destroy(gameObject);
    }


    // =========================================================
    // SET ALPHA
    // =========================================================

    void SetAlpha(float alpha)
    {
        if (heartImage != null)
        {
            Color color =
                heartImage.color;

            color.a = alpha;

            heartImage.color =
                color;
        }
    }
}