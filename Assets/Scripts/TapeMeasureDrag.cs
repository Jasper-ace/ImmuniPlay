using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TapeMeasureDrag : MonoBehaviour,
    IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("References")]
    [SerializeField] RectTransform start;         // Indicator/Start
    [SerializeField] RectTransform end;           // Indicator/End
    [SerializeField] RectTransform tapeMeasure;   // has RectMask2D
    [SerializeField] RectTransform tapeImage;     // child of TapeMeasure
    [SerializeField] RectTransform tapeShadow;    // child of TapeMeasure (optional)
    [SerializeField] TMP_Text measureTxt;         // MeasureTxt

    [Header("Draggable Object")]
    [Tooltip("The object that follows the pointer. Leave empty to drag Start itself.")]
    [SerializeField] RectTransform dragHandle;
    [SerializeField] bool snapBackIfReleasedEarly = true;

    [Header("Mini Game Switch")]
    [SerializeField] GameObject currentMiniGame;  // MiniGame1 (this game)
    [SerializeField] GameObject nextMiniGame;     // MiniGame2
    [SerializeField] float switchDelay = 1.5f;    // seconds to show the result first

    [Header("Tape Size / Position")]
    [SerializeField] float tapeHeight = 60f;
    [SerializeField] float leftPadding = 40f;
    [SerializeField] float rightPadding = 0f;
    [SerializeField] float yOffset = 0f;
    [SerializeField] Vector2 shadowOffset = new Vector2(0f, -6f);

    [Header("Settings")]
    [SerializeField] float maxHeightCm = 53f;
    [SerializeField] float completeThreshold = 0.97f;
    [SerializeField] bool resetIfReleasedEarly = true;

    public event Action<float> Completed;

    RectTransform _space;
    Vector2 _a, _b;
    float _fullWidth;
    bool _dragging, _done;

    void Awake()
    {
        if (!Validate()) { enabled = false; return; }

        if (!dragHandle) dragHandle = start;

        _space = start.parent as RectTransform;
        _a = start.anchoredPosition;
        _b = end.anchoredPosition;

        SetupTape();
        ResetTape();
    }

    void OnEnable()
    {
        // runs every time this mini game is shown
        if (_space != null) ResetGame();
    }

    public void ResetGame()
    {
        StopAllCoroutines();
        _done = false;
        _dragging = false;
        ResetTape();
    }

    bool Validate()
    {
        bool ok = true;
        if (!start)       { Debug.LogError("TapeMeasureDrag: Start is not assigned", this); ok = false; }
        if (!end)         { Debug.LogError("TapeMeasureDrag: End is not assigned", this); ok = false; }
        if (!tapeMeasure) { Debug.LogError("TapeMeasureDrag: Tape Measure is not assigned", this); ok = false; }
        if (!tapeImage)   { Debug.LogError("TapeMeasureDrag: Tape Image is not assigned", this); ok = false; }
        if (!measureTxt)  { Debug.LogError("TapeMeasureDrag: Measure Txt is not assigned", this); ok = false; }
        return ok;
    }

    void SetupTape()
    {
        var parent = tapeMeasure.parent as RectTransform;

        tapeMeasure.localScale = Vector3.one;
        tapeImage.localScale = Vector3.one;
        tapeMeasure.localRotation = Quaternion.identity;
        tapeImage.localRotation = Quaternion.identity;
        if (tapeShadow)
        {
            tapeShadow.localScale = Vector3.one;
            tapeShadow.localRotation = Quaternion.identity;
        }

        foreach (var g in tapeMeasure.GetComponentsInChildren<Graphic>(true))
            g.raycastTarget = false;

        Vector3 s = parent.InverseTransformPoint(start.position);
        Vector3 e = parent.InverseTransformPoint(end.position);
        _fullWidth = (e.x - s.x) + leftPadding + rightPadding;

        tapeMeasure.anchorMin = tapeMeasure.anchorMax = new Vector2(0.5f, 0.5f);
        tapeMeasure.pivot = new Vector2(0f, 0.5f);
        tapeMeasure.position = start.position;
        tapeMeasure.anchoredPosition += new Vector2(-leftPadding, yOffset);
        tapeMeasure.sizeDelta = new Vector2(0f, tapeHeight + Mathf.Abs(shadowOffset.y) + 20f);

        FixChild(tapeImage, tapeHeight, Vector2.zero);
        if (tapeShadow) FixChild(tapeShadow, tapeHeight, shadowOffset);
    }

    void FixChild(RectTransform rt, float height, Vector2 pos)
    {
        rt.anchorMin = rt.anchorMax = new Vector2(0f, 0.5f);
        rt.pivot = new Vector2(0f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(_fullWidth, height);
    }

    public void OnPointerDown(PointerEventData e)
    {
        if (_done) return;
        _dragging = true;
        tapeMeasure.gameObject.SetActive(true);
        UpdateProgress(e);
    }

    public void OnDrag(PointerEventData e)
    {
        if (!_dragging || _done) return;
        UpdateProgress(e);
    }

    public void OnPointerUp(PointerEventData e)
    {
        if (!_dragging) return;
        _dragging = false;

        if (!_done)
        {
            if (resetIfReleasedEarly) ResetTape();
            else if (snapBackIfReleasedEarly) MoveHandle(0f);
        }
    }

    void UpdateProgress(PointerEventData e)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _space, e.position, e.pressEventCamera, out Vector2 p);

        Vector2 ab = _b - _a;
        float t = Mathf.Clamp01(Vector2.Dot(p - _a, ab) / ab.sqrMagnitude);
        Apply(t);

        if (t >= completeThreshold)
        {
            Apply(1f);
            _done = true;
            _dragging = false;
            Completed?.Invoke(maxHeightCm);
            StartCoroutine(SwitchToNextMiniGame());
        }
    }

    IEnumerator SwitchToNextMiniGame()
    {
        yield return new WaitForSeconds(switchDelay);

        // enable the next one first, then disable this one
        // (disabling this one stops this coroutine, so it must be last)
        if (nextMiniGame) nextMiniGame.SetActive(true);
        if (currentMiniGame) currentMiniGame.SetActive(false);
    }

    void Apply(float t)
    {
        tapeMeasure.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Horizontal, t * _fullWidth);

        MoveHandle(t);

        float cm = t * maxHeightCm;
        measureTxt.text = $"Height: {Mathf.RoundToInt(cm)} cm";
    }

    void MoveHandle(float t)
    {
        if (!dragHandle) return;
        Vector2 pos = Vector2.Lerp(_a, _b, t);
        dragHandle.position = _space.TransformPoint(pos);
    }

    void ResetTape()
    {
        Apply(0f);
        tapeMeasure.gameObject.SetActive(false);
    }
}