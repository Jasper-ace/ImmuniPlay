using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class DragToTarget : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    public enum TransitionType
    {
        DisableAndEnable,
        NextScene
    }

    [Header("Target")]
    [SerializeField] private RectTransform targetArea;

    [Header("Draggable")]
    [SerializeField] private GameObject draggableObject;

    [Header("Snap Sound")]
    [Tooltip("Sound played when the draggable object snaps to the target.")]
    [SerializeField] private AudioClip snapSound;

    [Tooltip("Volume of the snap sound.")]
    [Range(0f, 1f)]
    [SerializeField] private float snapSoundVolume = 1f;

    [Header("After Target Reached")]
    [SerializeField] private TransitionType transitionType =
        TransitionType.DisableAndEnable;

    [Header("GameObjects To Show")]
    [SerializeField] private GameObject[] gameObjectsToShow;

    [Header("GameObjects To Hide")]
    [SerializeField] private GameObject[] gameObjectsToHide;

    [Header("Next Scene")]
    [SerializeField] private string nextSceneName;

    [Header("Settings")]
    [SerializeField] private float targetProximity = 80f;
    [SerializeField] private bool snapBackOnMiss = true;

    private RectTransform draggableRect;
    private Vector2 originalPosition;

    private bool isDragging;
    private bool completed;

    private void Awake()
    {
        if (draggableObject == null)
            draggableObject = gameObject;

        draggableRect = draggableObject.GetComponent<RectTransform>();

        if (draggableRect != null)
            originalPosition = draggableRect.anchoredPosition;
    }

    private void Start()
    {
        completed = false;
        isDragging = false;
    }

    //====================================================
    // BEGIN DRAG
    //====================================================

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (completed || draggableRect == null)
            return;

        isDragging = true;
    }

    //====================================================
    // DRAG
    //====================================================

    public void OnDrag(PointerEventData eventData)
    {
        if (completed || !isDragging || draggableRect == null)
            return;

        // Move draggable object
        draggableRect.anchoredPosition += eventData.delta;

        // Check target distance
        if (targetArea != null)
        {
            float distance = Vector3.Distance(
                draggableRect.position,
                targetArea.position
            );

            if (distance <= targetProximity)
            {
                // Snap the draggable to the target first.
                draggableRect.position = targetArea.position;

                // Play the snap sound through AudioManager SFX.
                if (snapSound != null && AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX(snapSound, snapSoundVolume);
                }

                CompleteDrag();
            }
        }
    }

    //====================================================
    // END DRAG
    //====================================================

    public void OnEndDrag(PointerEventData eventData)
    {
        if (completed)
            return;

        isDragging = false;

        // If target was missed, return to original position
        if (snapBackOnMiss && draggableRect != null)
        {
            draggableRect.anchoredPosition = originalPosition;
        }
    }

    //====================================================
    // COMPLETE
    //====================================================

    private void CompleteDrag()
    {
        if (completed)
            return;

        completed = true;
        isDragging = false;

        Debug.Log("Drag target reached!");

        if (transitionType == TransitionType.DisableAndEnable)
        {
            DisableAndEnableObjects();
        }
        else if (transitionType == TransitionType.NextScene)
        {
            LoadNextScene();
        }
    }

    //====================================================
    // DISABLE AND ENABLE
    //====================================================

    private void DisableAndEnableObjects()
    {
        // Hide objects
        if (gameObjectsToHide != null)
        {
            foreach (GameObject obj in gameObjectsToHide)
            {
                if (obj != null)
                    obj.SetActive(false);
            }
        }

        // Show objects
        if (gameObjectsToShow != null)
        {
            foreach (GameObject obj in gameObjectsToShow)
            {
                if (obj != null)
                    obj.SetActive(true);
            }
        }
    }

    //====================================================
    // NEXT SCENE
    //====================================================

    private void LoadNextScene()
    {
        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogWarning(
                "[DragToTarget] Next Scene Name is empty!"
            );

            return;
        }

        SceneManager.LoadScene(nextSceneName);
    }

    //====================================================
    // RESET
    //====================================================

    public void ResetDrag()
    {
        completed = false;
        isDragging = false;

        if (draggableRect != null)
            draggableRect.anchoredPosition = originalPosition;
    }
}