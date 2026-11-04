using System;
using System.Collections;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class BabyThermometerDrag : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("References")]
    [SerializeField] RectTransform target;                 // Baby/Target
    [SerializeField] GameObject placedThermometer;          // Baby/DigitalThermometer (hidden at start)
    [SerializeField] GameObject result;                     // Result (hidden at start)
    [SerializeField] TMP_Text tempText;                     // Result/TempText

    [Header("Completion GameObjects")]
    [Tooltip("These GameObjects will be hidden after the mini-game completes.")]
    [SerializeField] GameObject[] hideOnComplete;

    [Tooltip("These GameObjects will be shown after the mini-game completes.")]
    [SerializeField] GameObject[] showOnComplete;

    [Header("Settings")]
    [SerializeField] float snapRadius = 80f;                // how close to Target counts as a hit
    [SerializeField] bool snapToTarget = true;              // jump onto the target when dropped
    [SerializeField] float placedShowTime = 1.5f;            // how long the placed thermometer shows
    [SerializeField] float targetTemp = 35f;                 // celsius
    [SerializeField] float countDuration = 1.5f;             // how long the temp takes to count up
    [SerializeField] float returnSpeed = 2000f;              // speed of the return to start if missed

    public event Action<float> Completed;                    // passes the final temperature

    RectTransform _rect, _parent, _canvasRect;
    CanvasGroup _group;
    Vector2 _startPos, _grabOffset;
    bool _dragging, _busy, _done;

    void Awake()
    {
        _rect = transform as RectTransform;
        _parent = _rect.parent as RectTransform;
        Canvas canvas = GetComponentInParent<Canvas>();
        _canvasRect = canvas != null ? canvas.rootCanvas.transform as RectTransform : null;

        _group = GetComponent<CanvasGroup>();
        if (!_group) _group = gameObject.AddComponent<CanvasGroup>();

        _startPos = _rect.anchoredPosition;
        ResetGame();
    }

    void OnEnable()
    {
        if (_rect != null) ResetGame();
    }

    public void ResetGame()
    {
        StopAllCoroutines();
        _dragging = false;
        _busy = false;
        _done = false;

        _rect.anchoredPosition = _startPos;
        _group.alpha = 1f;
        _group.blocksRaycasts = true;

        if (placedThermometer) placedThermometer.SetActive(false);
        if (result) result.SetActive(false);
        if (tempText) tempText.text = FormatTemp(0f);
    }

    public void OnBeginDrag(PointerEventData e)
    {
        if (_done || _busy) return;

        _dragging = true;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _parent, e.position, e.pressEventCamera, out Vector2 p);
        _grabOffset = _rect.anchoredPosition - p;
    }

    public void OnDrag(PointerEventData e)
    {
        if (!_dragging) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _parent, e.position, e.pressEventCamera, out Vector2 p);
        _rect.anchoredPosition = p + _grabOffset;
    }

    public void OnEndDrag(PointerEventData e)
    {
        if (!_dragging) return;
        _dragging = false;

        if (IsOnTarget())
        {
            if (snapToTarget) SnapToTarget();
            StartCoroutine(PlayResult());
        }
        else
        {
            StartCoroutine(ReturnToStart());
        }
    }

    bool IsOnTarget()
    {
        if (target == null || _canvasRect == null) return false;

        // Compare in canvas space so it works with any parent / scale.
        Vector2 a = _canvasRect.InverseTransformPoint(_rect.position);
        Vector2 b = _canvasRect.InverseTransformPoint(target.position);
        return Vector2.Distance(a, b) <= snapRadius;
    }

    void SnapToTarget()
    {
        if (target != null) _rect.position = target.position;
    }

    IEnumerator ReturnToStart()
    {
        _busy = true;
        while (Vector2.Distance(_rect.anchoredPosition, _startPos) > 1f)
        {
            _rect.anchoredPosition = Vector2.MoveTowards(
                _rect.anchoredPosition, _startPos, returnSpeed * Time.deltaTime);
            yield return null;
        }

        _rect.anchoredPosition = _startPos;
        _busy = false;
    }

    IEnumerator PlayResult()
    {
        _busy = true;
        _done = true;

        // 1) Hide the dragged thermometer and show the one placed on the baby.
        _group.alpha = 0f;
        _group.blocksRaycasts = false;
        if (placedThermometer) placedThermometer.SetActive(true);

        yield return new WaitForSeconds(placedShowTime);

        // 2) Hide it again and show the result.
        if (placedThermometer) placedThermometer.SetActive(false);
        if (result) result.SetActive(true);

        // 3) Count the temperature up to the target temperature.
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, countDuration);
            if (tempText)
                tempText.text = FormatTemp(Mathf.Lerp(0f, targetTemp, Mathf.Clamp01(t)));
            yield return null;
        }

        if (tempText) tempText.text = FormatTemp(targetTemp);

        // Apply GameObject visibility changes after the mini-game finishes.
        SetGameObjectsActive(hideOnComplete, false);
        SetGameObjectsActive(showOnComplete, true);

        _busy = false;
        Completed?.Invoke(targetTemp);
    }

    void SetGameObjectsActive(GameObject[] objects, bool activeState)
    {
        if (objects == null) return;

        foreach (GameObject obj in objects)
        {
            if (obj != null)
                obj.SetActive(activeState);
        }
    }

    static string FormatTemp(float v)
    {
        return v.ToString("0.00", CultureInfo.InvariantCulture);
    }
}
