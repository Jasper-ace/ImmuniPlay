
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class HandDropZone : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Drop area. Defaults to this object's RectTransform.")]
    public RectTransform dropArea;

    [Tooltip("Red X indicator for incorrect items.")]
    public GameObject wrongItemIndicator;

    [Tooltip("Green arrow indicator.")]
    public GameObject arrow;

    [Header("Correct Drop GameObjects")]
    [Tooltip("GameObjects to hide when the correct item is dropped.")]
    public GameObject[] hideOnCorrect;

    [Tooltip("GameObjects to show when the correct item is dropped.")]
    public GameObject[] showOnCorrect;

    [Header("Settings")]
    public float wrongIndicatorSeconds = 1.2f;

    [Tooltip("Optional scene to load after a correct drop. Leave empty to stay in the current scene.")]
    public string nextSceneName;

    [Header("Events")]
    public UnityEvent onCorrectItem;
    public UnityEvent onWrongItem;

    private Coroutine wrongRoutine;
    private bool finished;

    private void Awake()
    {
        if (dropArea == null)
            dropArea = GetComponent<RectTransform>();

        if (wrongItemIndicator != null)
            wrongItemIndicator.SetActive(false);
    }

    public bool IsOverHand(RectTransform item, Camera cam)
    {
        if (dropArea == null || item == null)
            return false;

        Vector2 screenPoint =
            RectTransformUtility.WorldToScreenPoint(cam, item.position);

        return RectTransformUtility.RectangleContainsScreenPoint(
            dropArea, screenPoint, cam);
    }

    public void HandleDrop(ItemDraggable item)
    {
        if (finished || item == null)
            return;

        if (item.isVaccine)
            OnCorrect(item);
        else
            OnWrong(item);
    }

    private void OnCorrect(ItemDraggable item)
    {
        finished = true;

        item.locked = true;

        if (item.Rect != null && dropArea != null)
            item.Rect.position = dropArea.position;

        if (arrow != null)
            arrow.SetActive(false);

        if (wrongItemIndicator != null)
            wrongItemIndicator.SetActive(false);

        // Notify other systems before hiding the mini-game.
        onCorrectItem?.Invoke();

        // Hide the completed mini-game objects.
        SetGameObjectsActive(hideOnCorrect, false);

        // Show the next part of the current scene.
        SetGameObjectsActive(showOnCorrect, true);

        // Load another scene immediately if a scene name was provided.
        // No coroutine is used here, so disabling Hand is safe.
        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
    }

    private void SetGameObjectsActive(GameObject[] objects, bool activeState)
    {
        if (objects == null)
            return;

        foreach (GameObject obj in objects)
        {
            if (obj != null)
                obj.SetActive(activeState);
        }
    }

    private void OnWrong(ItemDraggable item)
    {
        item.ReturnToStart();

        onWrongItem?.Invoke();

        if (wrongItemIndicator == null)
            return;

        if (wrongRoutine != null)
            StopCoroutine(wrongRoutine);

        wrongRoutine = StartCoroutine(ShowWrongIndicator());
    }

    private IEnumerator ShowWrongIndicator()
    {
        wrongItemIndicator.SetActive(true);

        yield return new WaitForSeconds(wrongIndicatorSeconds);

        if (wrongItemIndicator != null)
            wrongItemIndicator.SetActive(false);

        wrongRoutine = null;
    }
}
