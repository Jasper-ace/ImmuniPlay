using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Attach to: MiniGame1/Baby/Dropper (the nested dropper, disabled at start).
/// When enabled it plays this sequence automatically:
///   wait 0.5s -> Droplet1 ON
///   wait 0.5s -> Droplet1 OFF, Droplet2 ON
///   wait 0.5s -> Droplet2 OFF, dropper EXIT animation (slide + fade + shrink)
///   then onCompleted fires and the dropper is disabled.
/// Works with UI Images or Sprites. No colliders.
/// </summary>
public class NestedDropper : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Drag Droplet1 then Droplet2 here, in order")]
    [SerializeField] private GameObject[] droplets;

    [Header("Timing (seconds)")]
    [Tooltip("Delay after this dropper is enabled, before Droplet1 shows")]
    [SerializeField] private float firstDelay = 0.5f;
    [Tooltip("Delay between each droplet, and after the last one before the exit")]
    [SerializeField] private float delayBetween = 0.5f;

    [Header("Exit Animation")]
    [Tooltip("How far the dropper moves while leaving (local units). UI: ~300. Sprites: ~3")]
    [SerializeField] private Vector2 exitOffset = new Vector2(300f, 300f);
    [SerializeField] private float exitDuration = 0.5f;
    [SerializeField] private bool fadeOut = true;
    [Tooltip("End scale of the dropper while leaving. 1 = no scaling")]
    [SerializeField] private float exitScale = 0.8f;

    [Header("Completion")]
    [SerializeField] private bool hideDropperOnComplete = true;
    public UnityEvent onCompleted;

    [Header("Next Mini Game")]
    [Tooltip("Drag MiniGame1 here. It is disabled after the dropper exits.")]
    [SerializeField] private GameObject miniGameToDisable;
    [Tooltip("Drag MiniGame2 here. It is enabled after the dropper exits.")]
    [SerializeField] private GameObject miniGameToEnable;

    private Vector3 startLocalPosition;
    private Vector3 startLocalScale;
    private Graphic[] graphics;
    private Color[] graphicColors;
    private SpriteRenderer[] sprites;
    private Color[] spriteColors;
    private bool initialized;

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (initialized) return;

        startLocalPosition = transform.localPosition;
        startLocalScale = transform.localScale;

        graphics = GetComponentsInChildren<Graphic>(true);
        graphicColors = new Color[graphics.Length];
        for (int i = 0; i < graphics.Length; i++)
            graphicColors[i] = graphics[i].color;

        sprites = GetComponentsInChildren<SpriteRenderer>(true);
        spriteColors = new Color[sprites.Length];
        for (int i = 0; i < sprites.Length; i++)
            spriteColors[i] = sprites[i].color;

        initialized = true;
    }

    private void OnEnable()
    {
        Initialize();
        ResetVisuals();

        // All droplets hidden at start
        foreach (GameObject d in droplets)
            if (d != null) d.SetActive(false);

        StartCoroutine(Sequence());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private void ResetVisuals()
    {
        transform.localPosition = startLocalPosition;
        transform.localScale = startLocalScale;
        SetAlpha(1f);
    }

    private void SetAlpha(float a)
    {
        for (int i = 0; i < graphics.Length; i++)
        {
            Color c = graphicColors[i];
            c.a = graphicColors[i].a * a;
            graphics[i].color = c;
        }

        for (int i = 0; i < sprites.Length; i++)
        {
            Color c = spriteColors[i];
            c.a = spriteColors[i].a * a;
            sprites[i].color = c;
        }
    }

    private IEnumerator Sequence()
    {
        yield return new WaitForSeconds(firstDelay);

        for (int i = 0; i < droplets.Length; i++)
        {
            // Hide the previous droplet
            if (i > 0 && droplets[i - 1] != null)
                droplets[i - 1].SetActive(false);

            // Show the current droplet
            if (droplets[i] != null)
                droplets[i].SetActive(true);

            yield return new WaitForSeconds(delayBetween);
        }

        // Hide the last droplet (Droplet2)
        if (droplets.Length > 0 && droplets[droplets.Length - 1] != null)
            droplets[droplets.Length - 1].SetActive(false);

        // Dropper leaves with animation
        yield return StartCoroutine(ExitAnimation());

        onCompleted?.Invoke();

        if (hideDropperOnComplete)
            gameObject.SetActive(false);

        // Enable the next mini game FIRST, then disable the current one.
        // (Disabling MiniGame1 also disables this object, so it must be the last step.)
        if (miniGameToEnable != null)
            miniGameToEnable.SetActive(true);

        if (miniGameToDisable != null)
            miniGameToDisable.SetActive(false);
    }

    private IEnumerator ExitAnimation()
    {
        Vector3 fromPos = startLocalPosition;
        Vector3 toPos = startLocalPosition + (Vector3)exitOffset;
        Vector3 toScale = startLocalScale * exitScale;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, exitDuration);
            float eased = t * t; // ease-in: starts slow, speeds up

            transform.localPosition = Vector3.Lerp(fromPos, toPos, eased);
            transform.localScale = Vector3.Lerp(startLocalScale, toScale, eased);
            if (fadeOut) SetAlpha(1f - Mathf.Clamp01(t));

            yield return null;
        }

        transform.localPosition = toPos;
        transform.localScale = toScale;
        if (fadeOut) SetAlpha(0f);
    }
}
