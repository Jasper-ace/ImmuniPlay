using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// Attach to: MiniGame2 (the root object).
/// Requires DropperUtil.cs (PointerInput + DropperUtil) in the project.
/// No colliders needed.
///
/// Flow:
///   1. Drag + swipe Alcohol Wipe on the Target -> residue appears and fades away
///   2. Drag Vaccine to the Target -> syringe + hold button appear
///   3. Tap and HOLD the button -> plunger pushes, liquid drains
///   4. Done: baby happy -> crying, red dot appears, syringe disappears
///   5. Drag BandAid to the Target -> band aid appears on baby
///   6. After a delay: enable MiniGame3, disable MiniGame2
///
/// Changes in this version:
///   - Tools auto-found under "Tools/..." if the Inspector field is empty (+ clear warnings)
///   - Plunger pushes LEFT by default (needle on the left)
///   - Start positions cached as anchored/local positions (safe for aspect/canvas changes)
///   - Radii scale with screen height (resolution independent)
///   - Inactive-step tools are dimmed; the current tool gently pulses
///   - Wipe needs real movement (jitter ignored) and the residue fades in as you wipe
///   - Hold progress slowly drains when released (set decay to 0 to disable)
///   - Target ring tinted per step
///   - Components cached instead of fetched every frame; input ignored when Done
/// </summary>
public class MiniGame2Controller : MonoBehaviour
{
    private enum Step { Wipe, ResidueFade, Vaccine, Administer, BandAid, Done }
    private enum NeedleSide { Left, Right, Top, Bottom }

    [Header("Baby")]
    [Tooltip("Baby happy object (disabled after the injection)")]
    [FormerlySerializedAs("happyBaby")] [SerializeField] private GameObject babyHappy;
    [Tooltip("Baby crying object (enabled after the injection)")]
    [FormerlySerializedAs("cryingBaby")] [SerializeField] private GameObject babyCrying;
    [Tooltip("Baby/Target - the injection spot on the thigh")]
    [SerializeField] private Transform target;

    [Header("Resolution")]
    [Tooltip("Screen height the pixel radii below were designed for")]
    [SerializeField] private float referenceHeight = 1080f;

    [Header("Step 1 - Alcohol Wipe")]
    [Tooltip("Tools/Alcohol Wipe (auto-found if left empty)")]
    [SerializeField] private Transform alcoholWipe;
    [Tooltip("Baby/AlcoholResidue")]
    [SerializeField] private GameObject alcoholResidue;
    [Tooltip("Max distance (pixels at reference height) from the Target where swiping counts")]
    [SerializeField] private float wipeRadius = 150f;
    [Tooltip("Total swipe distance (pixels at reference height) needed to clean. Lower = faster")]
    [SerializeField] private float swipeDistance = 2500f;
    [Tooltip("Pointer movement per frame below this (pixels at reference height) is ignored, so jitter doesn't count")]
    [SerializeField] private float minSwipeStep = 3f;
    [Tooltip("Residue fades IN while wiping so the player sees progress")]
    [SerializeField] private bool showResidueWhileWiping = true;
    [Tooltip("Seconds the residue stays fully visible after the swipe, before it fades")]
    [SerializeField] private float residueHoldTime = 0.3f;
    [Tooltip("Seconds the residue takes to fade away")]
    [SerializeField] private float residueFadeDuration = 1.5f;

    [Header("Step 2 - Vaccine")]
    [Tooltip("Tools/Vaccine - the draggable one in the tray (auto-found if left empty)")]
    [FormerlySerializedAs("vaccineTool")] [SerializeField] private Transform vaccine;
    [Tooltip("Baby/VaccineParts (Liquid, Plunger, Syringe)")]
    [SerializeField] private GameObject vaccineParts;
    [Tooltip("Max distance (pixels at reference height) from the Target when releasing a tool")]
    [SerializeField] private float snapRadius = 200f;

    [Header("Step 3 - Administer (tap and hold)")]
    [Tooltip("Text Button (the HOLD TO ADMINISTER button). Needs an Image behind the text so it is visible")]
    [FormerlySerializedAs("holdButton")] [SerializeField] private GameObject textButton;
    [Tooltip("Seconds the button must be held to finish")]
    [SerializeField] private float administerDuration = 2f;
    [Tooltip("Progress lost per second when the button is released (0 = never drains)")]
    [SerializeField] private float holdDecayPerSecond = 0.35f;
    [Tooltip("Baby/VaccineParts/Plunger")]
    [SerializeField] private Transform plunger;
    [Tooltip("How far the plunger moves when fully pushed (local units). Needle is on the left, so push LEFT")]
    [SerializeField] private Vector2 plungerPushOffset = new Vector2(-60f, 0f);
    [Tooltip("Which end of the syringe the needle is on. The plunger moves TOWARD it, the liquid shrinks toward it. Use Bottom or Top for a vertical syringe")]
    [SerializeField] private NeedleSide needleSide = NeedleSide.Bottom;
    [Tooltip("ON: the plunger as placed in the scene is the fully pushed pose, so it starts pulled out and slides back to it. OFF: it starts where placed and moves by the offset")]
    [SerializeField] private bool plungerArtIsPushed = true;
    [Tooltip("Baby/VaccineParts/Liquid")]
    [SerializeField] private Transform liquid;
    [Tooltip("Liquid X scale when empty (used when the Liquid Image is not 'Filled')")]
    [SerializeField] private float emptyLiquidScale = 0f;

    [Header("Step 4 - Band Aid")]
    [Tooltip("Baby/RedDot")]
    [SerializeField] private GameObject redDot;
    [Tooltip("Tools/BandAid - the draggable one in the tray (auto-found if left empty)")]
    [FormerlySerializedAs("bandAidTool")] [SerializeField] private Transform toolsBandAid;
    [Tooltip("Baby/BandAid - the one placed on the baby")]
    [SerializeField] private GameObject babyBandAid;

    [Header("Tool Feedback")]
    [Tooltip("Alpha of tools that are not usable yet")]
    [SerializeField, Range(0f, 1f)] private float lockedToolAlpha = 0.4f;
    [Tooltip("Size pulse of the current tool (0 = no pulse)")]
    [SerializeField] private float pulseAmount = 0.06f;
    [SerializeField] private float pulseSpeed = 4f;
    [Tooltip("Tint of the Target ring per step (needs an Image or SpriteRenderer on Target)")]
    [SerializeField] private Color wipeTint = new Color(1f, 0.95f, 0.2f);
    [SerializeField] private Color vaccineTint = new Color(0.4f, 0.8f, 1f);
    [SerializeField] private Color bandAidTint = new Color(1f, 0.6f, 0.4f);

    [Header("Finish")]
    [SerializeField] private float completeDelay = 1.5f;
    [Tooltip("Drag MiniGame3 here")]
    [FormerlySerializedAs("miniGameToEnable")] [SerializeField] private GameObject miniGame3;
    [Tooltip("Drag MiniGame2 here (this object)")]
    [FormerlySerializedAs("miniGameToDisable")] [SerializeField] private GameObject miniGame2;
    public UnityEvent onCompleted;

    [Header("Other")]
    [SerializeField] private float returnDuration = 0.25f;

    private class ToolInfo
    {
        public Vector3 startPos;      // anchored (RectTransform) or local position
        public Vector3 baseScale;
        public CanvasGroup group;
    }

    private Step step;
    private Transform dragging;
    private Vector3 dragOffset;
    private Vector2 lastPointer;
    private bool isReturning;
    private bool holding;
    private float swipeProgress;
    private float administerProgress;

    private CanvasGroup residueGroup;
    private RectTransform buttonRect;
    private Image liquidImage;
    private Image targetImage;
    private SpriteRenderer targetSprite;
    private Camera uiCamera;
    private Vector3 plungerStartLocal;
    private Vector3 liquidStartScale;
    private Vector3 liquidStartPos;
    private Vector2 pushOffset;
    private readonly Dictionary<Transform, ToolInfo> tools = new Dictionary<Transform, ToolInfo>();
    private bool initialized;

    private float ScreenScale => Mathf.Max(0.1f, Screen.height / Mathf.Max(1f, referenceHeight));

    // ---------------------------------------------------------------- setup

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (initialized) return;

        // Fall back to the scene hierarchy if the Inspector fields were left empty
        if (alcoholWipe == null) alcoholWipe = FindChildByPath("Tools/Alcohol Wipe", "alcoholWipe");
        if (vaccine == null) vaccine = FindChildByPath("Tools/Vaccine", "vaccine");
        if (toolsBandAid == null) toolsBandAid = FindChildByPath("Tools/BandAid", "toolsBandAid");

        if (target == null) Debug.LogError("[MiniGame2] 'Target' is not assigned.", this);

        CacheTool(alcoholWipe);
        CacheTool(vaccine);
        CacheTool(toolsBandAid);

        if (plunger != null) plungerStartLocal = plunger.localPosition;

        // The plunger always travels LEFT (toward the needle), whatever sign was typed in the Inspector
        // Travel distance = the length of the Inspector offset; direction comes from Needle Side
        pushOffset = NeedleDirection() * plungerPushOffset.magnitude;

        if (liquid != null)
        {
            liquidStartScale = liquid.localScale;
            liquidStartPos = liquid.localPosition;
            liquidImage = liquid.GetComponent<Image>();

            // The liquid shrinks toward the needle, so the needle-side edge stays fixed
            if (liquidImage != null)
            {
                liquidImage.type = Image.Type.Filled;
                bool vertical = needleSide == NeedleSide.Top || needleSide == NeedleSide.Bottom;
                liquidImage.fillMethod = vertical ? Image.FillMethod.Vertical : Image.FillMethod.Horizontal;

                switch (needleSide)
                {
                    case NeedleSide.Left: liquidImage.fillOrigin = (int)Image.OriginHorizontal.Left; break;
                    case NeedleSide.Right: liquidImage.fillOrigin = (int)Image.OriginHorizontal.Right; break;
                    case NeedleSide.Top: liquidImage.fillOrigin = (int)Image.OriginVertical.Top; break;
                    default: liquidImage.fillOrigin = (int)Image.OriginVertical.Bottom; break;
                }
            }
        }

        if (alcoholResidue != null)
        {
            residueGroup = alcoholResidue.GetComponent<CanvasGroup>();
            if (residueGroup == null) residueGroup = alcoholResidue.AddComponent<CanvasGroup>();
        }

        if (textButton != null) buttonRect = textButton.GetComponent<RectTransform>();

        if (target != null)
        {
            targetImage = target.GetComponent<Image>();
            targetSprite = target.GetComponent<SpriteRenderer>();
        }

        uiCamera = DropperUtil.GetCamera(transform);

        initialized = true;
    }

    private Transform FindChildByPath(string path, string fieldName)
    {
        Transform found = transform.Find(path);
        if (found == null)
        {
            // Tools may not be a child of this object - search the whole scene root as a last resort
            Transform root = transform.root;
            found = root.Find(path);
        }

        if (found == null)
            Debug.LogError("[MiniGame2] '" + fieldName + "' is not assigned and '" + path + "' was not found.", this);
        else
            Debug.LogWarning("[MiniGame2] '" + fieldName + "' was empty - auto-assigned '" + path + "'. Assign it in the Inspector.", this);

        return found;
    }

    private void CacheTool(Transform tool)
    {
        if (tool == null) return;

        ToolInfo info = new ToolInfo
        {
            startPos = GetPos(tool),
            baseScale = tool.localScale,
            group = tool.GetComponent<CanvasGroup>()
        };
        if (info.group == null) info.group = tool.gameObject.AddComponent<CanvasGroup>();

        tools[tool] = info;
    }

    // Anchored position for UI, local position otherwise: both survive aspect/canvas changes
    private static Vector3 GetPos(Transform t)
    {
        RectTransform rt = t as RectTransform;
        return rt != null ? (Vector3)rt.anchoredPosition3D : t.localPosition;
    }

    private static void SetPos(Transform t, Vector3 p)
    {
        RectTransform rt = t as RectTransform;
        if (rt != null) rt.anchoredPosition3D = p;
        else t.localPosition = p;
    }

    private static void Show(GameObject go, bool on)
    {
        if (go != null) go.SetActive(on);
    }

    private void OnEnable()
    {
        Initialize();
        ResetGame();
    }

    private void ResetGame()
    {
        StopAllCoroutines();

        step = Step.Wipe;
        dragging = null;
        isReturning = false;
        holding = false;
        swipeProgress = 0f;
        administerProgress = 0f;

        Show(babyHappy, true);
        Show(babyCrying, false);
        Show(alcoholResidue, false);
        Show(vaccineParts, false);
        Show(textButton, false);
        Show(redDot, false);
        Show(babyBandAid, false);
        if (target != null) target.gameObject.SetActive(true);

        foreach (KeyValuePair<Transform, ToolInfo> kv in tools)
        {
            kv.Key.gameObject.SetActive(true);
            SetPos(kv.Key, kv.Value.startPos);
            kv.Key.localScale = kv.Value.baseScale;
        }

        if (residueGroup != null) residueGroup.alpha = 1f;
        ApplyAdminister(0f);
        RefreshToolStates();
    }

    // --------------------------------------------------------------- update

    private void Update()
    {
        if (step == Step.Done) return;

        Vector2 screen = PointerInput.Position;

        if (!isReturning)
        {
            Transform tool = GetCurrentTool();
            if (tool != null)
                HandleToolDrag(tool, screen);
            else if (step == Step.Administer)
                HandleAdminister(screen);
        }

        AnimateCurrentTool();
        lastPointer = screen;
    }

    private Transform GetCurrentTool()
    {
        switch (step)
        {
            case Step.Wipe: return alcoholWipe;
            case Step.Vaccine: return vaccine;
            case Step.BandAid: return toolsBandAid;
            default: return null;
        }
    }

    // ------------------------------------------------------- tool feedback

    // Dim every tool that is not the current one, tint the Target ring for the step
    private void RefreshToolStates()
    {
        Transform current = GetCurrentTool();
        foreach (KeyValuePair<Transform, ToolInfo> kv in tools)
        {
            kv.Value.group.alpha = (kv.Key == current) ? 1f : lockedToolAlpha;
            kv.Key.localScale = kv.Value.baseScale;
        }

        Color tint = wipeTint;
        if (step == Step.Vaccine) tint = vaccineTint;
        else if (step == Step.BandAid) tint = bandAidTint;

        if (targetImage != null) targetImage.color = tint;
        if (targetSprite != null) targetSprite.color = tint;
    }

    // Gentle pulse on the current tool while it is idle in the tray
    private void AnimateCurrentTool()
    {
        if (pulseAmount <= 0f || dragging != null || isReturning) return;

        Transform current = GetCurrentTool();
        if (current == null || !current.gameObject.activeSelf) return;

        ToolInfo info;
        if (!tools.TryGetValue(current, out info)) return;

        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        current.localScale = info.baseScale * pulse;
    }

    // -------------------------------------------------------------- dragging

    private void HandleToolDrag(Transform tool, Vector2 screen)
    {
        if (dragging == null)
        {
            if (PointerInput.Down && DropperUtil.IsOver(tool, screen))
            {
                dragging = tool;
                dragOffset = tool.position - DropperUtil.ScreenToWorld(tool, screen);
                tool.localScale = tools[tool].baseScale; // stop pulsing while held
                lastPointer = screen;                    // no jump on the first wipe frame
            }
            return;
        }

        if (PointerInput.Held)
        {
            dragging.position = DropperUtil.ScreenToWorld(dragging, screen) + dragOffset;

            if (step == Step.Wipe)
                UpdateWipe(screen);
        }

        // The wipe may have finished and cleared 'dragging'
        if (dragging == null) return;

        if (PointerInput.Up || !PointerInput.Held)
        {
            Transform released = dragging;
            dragging = null;
            OnToolReleased(released);
        }
    }

    private bool IsNearTarget(Transform tool, float radiusAtReference)
    {
        Vector2 toolScreen = DropperUtil.WorldToScreen(tool, tool.position);
        Vector2 targetScreen = DropperUtil.WorldToScreen(tool, target.position);
        return Vector2.Distance(toolScreen, targetScreen) <= radiusAtReference * ScreenScale;
    }

    private void OnToolReleased(Transform tool)
    {
        if (target != null && IsNearTarget(tool, snapRadius))
        {
            if (step == Step.Vaccine) { PlaceVaccine(); return; }
            if (step == Step.BandAid) { PlaceBandAid(); return; }
        }

        StartCoroutine(ReturnToStart(tool));
    }

    private IEnumerator ReturnToStart(Transform tool)
    {
        isReturning = true;
        Vector3 from = GetPos(tool);
        Vector3 to = tools[tool].startPos;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, returnDuration);
            SetPos(tool, Vector3.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t)));
            yield return null;
        }

        SetPos(tool, to);
        isReturning = false;
    }

    // ---------------------------------------------------------- step 1: wipe

    private void UpdateWipe(Vector2 screen)
    {
        // The wipe only counts while it is on the Target
        if (target == null || !IsNearTarget(alcoholWipe, wipeRadius)) return;

        // Swiping = real pointer movement on the target (tiny jitter is ignored)
        float moved = (screen - lastPointer).magnitude;
        if (moved < minSwipeStep * ScreenScale) return;

        swipeProgress += moved / Mathf.Max(1f, swipeDistance * ScreenScale);

        // Visible progress: residue fades in as the player wipes
        if (showResidueWhileWiping && alcoholResidue != null)
        {
            if (!alcoholResidue.activeSelf) alcoholResidue.SetActive(true);
            if (residueGroup != null) residueGroup.alpha = Mathf.Clamp01(swipeProgress);
        }

        if (swipeProgress >= 1f)
            CompleteWipe();
    }

    private void CompleteWipe()
    {
        dragging = null;
        step = Step.ResidueFade;
        Show(alcoholWipe.gameObject, false);
        RefreshToolStates();
        StartCoroutine(ResidueRoutine());
    }

    // The swipe is done: residue is fully visible, stays a moment, then fades away
    private IEnumerator ResidueRoutine()
    {
        if (target != null) target.gameObject.SetActive(false);

        Show(alcoholResidue, true);
        if (residueGroup != null) residueGroup.alpha = 1f;

        yield return new WaitForSeconds(residueHoldTime);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, residueFadeDuration);
            if (residueGroup != null) residueGroup.alpha = 1f - Mathf.Clamp01(t);
            yield return null;
        }

        Show(alcoholResidue, false);
        if (target != null) target.gameObject.SetActive(true);

        step = Step.Vaccine;
        RefreshToolStates();
    }

    // ------------------------------------------------------- step 2: vaccine

    private void PlaceVaccine()
    {
        Show(vaccine.gameObject, false);
        Show(vaccineParts, true);
        Show(textButton, true);
        if (target != null) target.gameObject.SetActive(false);

        administerProgress = 0f;
        holding = false;
        ApplyAdminister(0f);

        step = Step.Administer;
        RefreshToolStates();
    }

    // ------------------------------------------------------ step 3: administer

    private void HandleAdminister(Vector2 screen)
    {
        if (textButton == null) return;

        if (PointerInput.Down && buttonRect != null &&
            RectTransformUtility.RectangleContainsScreenPoint(buttonRect, screen, uiCamera))
        {
            holding = true;
        }

        if (holding && !PointerInput.Held)
            holding = false;

        if (holding)
        {
            administerProgress = Mathf.Clamp01(administerProgress + Time.deltaTime / Mathf.Max(0.01f, administerDuration));
        }
        else if (holdDecayPerSecond > 0f && administerProgress > 0f)
        {
            administerProgress = Mathf.Max(0f, administerProgress - holdDecayPerSecond * Time.deltaTime);
        }
        else
        {
            return;
        }

        ApplyAdminister(administerProgress);

        if (administerProgress >= 1f)
            FinishAdminister();
    }

    private Vector2 NeedleDirection()
    {
        switch (needleSide)
        {
            case NeedleSide.Left: return Vector2.left;
            case NeedleSide.Right: return Vector2.right;
            case NeedleSide.Top: return Vector2.up;
            default: return Vector2.down;
        }
    }

    private void ApplyAdminister(float p)
    {
if (plunger != null)
{
    Vector3 startPosition = plungerStartLocal;

    plunger.localPosition = new Vector3(
        startPosition.x - Mathf.Abs(plungerPushOffset.x) * p,
        startPosition.y,
        startPosition.z
    );
}

        if (liquid == null) return;

        // Filled Image (needs a sprite): origin on the needle side, so that edge stays put
        if (liquidImage != null && liquidImage.sprite != null && liquidImage.type == Image.Type.Filled)
        {
            liquidImage.fillAmount = 1f - p;
            return;
        }

        // Fallback: scale along the syringe axis, keeping the needle-side edge fixed whatever the pivot is
        bool vertical = needleSide == NeedleSide.Top || needleSide == NeedleSide.Bottom;
        RectTransform rt = liquid as RectTransform;

        float startScale = vertical ? liquidStartScale.y : liquidStartScale.x;
        float scale = Mathf.Lerp(startScale, startScale * emptyLiquidScale, p);

        Vector3 newScale = liquidStartScale;
        if (vertical) newScale.y = scale; else newScale.x = scale;
        liquid.localScale = newScale;

        if (rt == null) return;

        float pivot = vertical ? rt.pivot.y : rt.pivot.x;
        float size = vertical ? rt.rect.height : rt.rect.width;
        float startPos = vertical ? liquidStartPos.y : liquidStartPos.x;

        // Fixed edge on the needle side
        bool needleAtLowEnd = needleSide == NeedleSide.Left || needleSide == NeedleSide.Bottom;
        float fixedEdge = needleAtLowEnd
            ? startPos - pivot * size * startScale
            : startPos + (1f - pivot) * size * startScale;

        float newPos = needleAtLowEnd
            ? fixedEdge + pivot * size * scale
            : fixedEdge - (1f - pivot) * size * scale;

        Vector3 pos = liquidStartPos;
        if (vertical) pos.y = newPos; else pos.x = newPos;
        liquid.localPosition = pos;
    }

    private void FinishAdminister()
    {
        holding = false;

        Show(textButton, false);
        Show(vaccineParts, false);
        Show(redDot, true);
        if (target != null) target.gameObject.SetActive(true);

        // Baby happy -> crying
        Show(babyHappy, false);
        Show(babyCrying, true);

        step = Step.BandAid;
        RefreshToolStates();
    }

    // ------------------------------------------------------- step 4: band aid

    private void PlaceBandAid()
    {
        Show(toolsBandAid.gameObject, false);
        Show(babyBandAid, true);
        Show(redDot, false);
        if (target != null) target.gameObject.SetActive(false);

        step = Step.Done; // Update() now ignores all input
        StartCoroutine(FinishRoutine());
    }

    private IEnumerator FinishRoutine()
    {
        yield return new WaitForSeconds(completeDelay);

        onCompleted?.Invoke();

        // Enable the next mini game FIRST, then disable this one (must be last step)
        Show(miniGame3, true);
        Show(miniGame2, false);
    }
}