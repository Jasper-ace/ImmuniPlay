using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections;

public class Crib : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [Header("Crib Movement")]
    [SerializeField] private float maxMoveDistance = 120f;
    [SerializeField] private float moveSmoothness = 10f;

    [Header("Swipe Detection")]
    [SerializeField] private float swipeThreshold = 20f;
    [SerializeField] private float requiredSwipeDuration = 2.5f;

    [Header("Baby GameObjects")]
    [SerializeField] private GameObject beforeBaby;
    [SerializeField] private GameObject afterBaby;
    [SerializeField] private float transitionDuration = 0.4f;

    [Header("Quiz Scene")]
    [SerializeField] private string quizSceneName = "Chapter Quiz";
    [SerializeField] private string chapterFileName = "Chapter2";
    [SerializeField] private string nextChapterScene = "Scene22";

    private RectTransform rectTransform;
    private Vector2 startPosition;
    private Vector2 targetPosition;

    private float rockingTime = 0f;
    private bool isDragging = false;
    private bool completed = false;


    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        startPosition = rectTransform.anchoredPosition;
        targetPosition = startPosition;

        if (beforeBaby != null)
            beforeBaby.SetActive(true);

        if (afterBaby != null)
            afterBaby.SetActive(false);
    }


    private void Update()
    {
        // Smoothly move toward the target X position
        rectTransform.anchoredPosition = Vector2.Lerp(
            rectTransform.anchoredPosition,
            targetPosition,
            Time.deltaTime * moveSmoothness
        );


        // Count time while the player is dragging
        if (isDragging && !completed)
        {
            rockingTime += Time.deltaTime;

            if (rockingTime >= requiredSwipeDuration)
            {
                CompleteCrib();
            }
        }
    }


    public void OnBeginDrag(PointerEventData eventData)
    {
        if (completed)
            return;

        isDragging = true;
    }


    public void OnDrag(PointerEventData eventData)
    {
        if (completed)
            return;

        // Only use horizontal movement
        float mouseX = eventData.delta.x;

        if (Mathf.Abs(mouseX) < 0.01f)
            return;

        float currentOffset = targetPosition.x - startPosition.x;

        currentOffset += mouseX;

        // Keep crib within left/right limits
        currentOffset = Mathf.Clamp(
            currentOffset,
            -maxMoveDistance,
            maxMoveDistance
        );

        targetPosition = new Vector2(
            startPosition.x + currentOffset,
            startPosition.y
        );
    }


    public void OnEndDrag(PointerEventData eventData)
    {
        if (completed)
            return;

        isDragging = false;

        // Return to center when released
        targetPosition = startPosition;
    }


    private void CompleteCrib()
    {
        if (completed)
            return;

        completed = true;
        isDragging = false;

        StopAllCoroutines();

        StartCoroutine(BabyTransition());
    }


    private IEnumerator BabyTransition()
    {
        // Return crib to center
        targetPosition = startPosition;

        yield return new WaitForSeconds(0.2f);


        // Hide awake baby
        if (beforeBaby != null)
            beforeBaby.SetActive(false);


        // Show sleeping baby
        if (afterBaby != null)
            afterBaby.SetActive(true);


        // Give the player time to see the sleeping baby
        yield return new WaitForSeconds(transitionDuration);


        // Tell the reusable quiz scene which chapter this came from
        QuizSceneData.chapterFileName = chapterFileName;

        // Tell the quiz where the next chapter starts
        QuizSceneData.nextChapterScene = nextChapterScene;


        Debug.Log("Current Chapter: " + chapterFileName);
        Debug.Log("Next Chapter Scene: " + nextChapterScene);
        Debug.Log("Loading Quiz Scene: " + quizSceneName);


        // Load the reusable Chapter Quiz scene
        SceneManager.LoadScene(quizSceneName);
    }
}