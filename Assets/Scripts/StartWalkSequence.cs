using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class StartWalkSequence : MonoBehaviour
{
    public enum MovementDirection
    {
        Left,
        Right
    }

    public enum TransitionType
    {
        UnityScene,
        HierarchyObject
    }

    [Header("Objects")]
    public GameObject standingCharacters;
    public GameObject walkingCharacters;
    public GameObject button;

    [Header("Movement")]
    public MovementDirection direction = MovementDirection.Left;
    public float moveSpeed = 1000f;

    [Header("Endpoint")]
    public RectTransform endpoint;

    [Header("Fade")]
    public Image fadePanel;

    [Tooltip("How long the fade to black takes, in seconds.")]
    public float fadeDuration = 1f;

    [Header("Transition")]
    public TransitionType transitionType = TransitionType.HierarchyObject;

    [Tooltip("GameObject to hide before showing the next hierarchy object.")]
    public GameObject currentObject;

    [Tooltip("Drag the next scene/content GameObject here.")]
    public GameObject nextObject;

    [Tooltip("Used only when Transition Type is Unity Scene.")]
    public string nextUnitySceneName = "Scene5";

    private bool walking = false;
    private bool fading = false;

    private RectTransform walkingRect;


    void Start()
    {
        // Get walking couple RectTransform
        if (walkingCharacters != null)
        {
            walkingRect = walkingCharacters.GetComponent<RectTransform>();
        }

        // Make sure fade starts invisible
        if (fadePanel != null)
        {
            Color color = fadePanel.color;
            color.a = 0f;
            fadePanel.color = color;
        }
    }


    // ==========================================
    // BUTTON
    // ==========================================

    public void StartWalking()
    {
        // Prevent sequence from starting multiple times
        if (walking || fading)
            return;

        // Hide button
        if (button != null)
        {
            button.SetActive(false);
        }

        // Hide standing couple
        if (standingCharacters != null)
        {
            standingCharacters.SetActive(false);
        }

        // Show walking couple
        if (walkingCharacters != null)
        {
            walkingCharacters.SetActive(true);
        }

        walking = true;

        Debug.Log("WALKING STARTED - Direction: " + direction);
    }


    // ==========================================
    // MOVEMENT
    // ==========================================

    void Update()
    {
        if (!walking || fading)
            return;

        if (walkingRect == null || endpoint == null)
            return;


        // Get world-space corners
        Vector3[] corners = new Vector3[4];
        walkingRect.GetWorldCorners(corners);

        /*
         * corners:
         *
         * 0 = bottom-left
         * 1 = top-left
         * 2 = top-right
         * 3 = bottom-right
         */


        // ==========================================
        // WALK LEFT
        // ==========================================

        if (direction == MovementDirection.Left)
        {
            float characterLeft = corners[0].x;
            float endpointX = endpoint.position.x;

            if (characterLeft <= endpointX)
            {
                ReachEndpoint();
                return;
            }

            walkingRect.anchoredPosition +=
                Vector2.left * moveSpeed * Time.deltaTime;
        }


        // ==========================================
        // WALK RIGHT
        // ==========================================

        else
        {
            float characterRight = corners[2].x;
            float endpointX = endpoint.position.x;

            if (characterRight >= endpointX)
            {
                ReachEndpoint();
                return;
            }

            walkingRect.anchoredPosition +=
                Vector2.right * moveSpeed * Time.deltaTime;
        }
    }


    // ==========================================
    // ENDPOINT REACHED
    // ==========================================

    void ReachEndpoint()
    {
        // Stop movement
        walking = false;

        // Stop walking animation
        StopWalkingAnimation();

        // Start fade
        StartCoroutine(FadeAndTransition());
    }


    // ==========================================
    // STOP WALKING ANIMATION
    // ==========================================

    void StopWalkingAnimation()
    {
        if (walkingCharacters == null)
            return;

        Animator[] animators =
            walkingCharacters.GetComponentsInChildren<Animator>();

        foreach (Animator animator in animators)
        {
            animator.speed = 0f;
        }

        Debug.Log("WALKING ANIMATION STOPPED");
    }


    // ==========================================
    // FADE
    // ==========================================

    IEnumerator FadeAndTransition()
    {
        fading = true;

        float elapsedTime = 0f;


        // ==========================================
        // NO FADE PANEL
        // ==========================================

        if (fadePanel == null)
        {
            Debug.LogWarning("Fade Panel is not assigned!");

            PerformTransition();

            yield break;
        }


        // ==========================================
        // INSTANT TRANSITION
        // ==========================================

        if (fadeDuration <= 0f)
        {
            Color instantColor = fadePanel.color;
            instantColor.a = 1f;
            fadePanel.color = instantColor;

            PerformTransition();

            yield break;
        }


        // ==========================================
        // FADE TO BLACK
        // ==========================================

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;

            float progress = elapsedTime / fadeDuration;

            progress = Mathf.Clamp01(progress);

            Color color = fadePanel.color;

            color.a = Mathf.Lerp(0f, 1f, progress);

            fadePanel.color = color;

            yield return null;
        }


        // Make sure it is completely black
        Color finalColor = fadePanel.color;
        finalColor.a = 1f;
        fadePanel.color = finalColor;


        Debug.Log("FADE COMPLETE");


        // Change scene/content
        PerformTransition();
    }


    // ==========================================
    // PERFORM TRANSITION
    // ==========================================

    void PerformTransition()
    {
        // ==========================================
        // LOAD ACTUAL UNITY SCENE
        // ==========================================

        if (transitionType == TransitionType.UnityScene)
        {
            if (string.IsNullOrEmpty(nextUnitySceneName))
            {
                Debug.LogError("Next Unity Scene Name is empty!");
                return;
            }

            Debug.Log("Loading Unity Scene: " + nextUnitySceneName);

            SceneManager.LoadScene(nextUnitySceneName);

            return;
        }


        // ==========================================
        // SWITCH HIERARCHY OBJECT
        // ==========================================

        if (transitionType == TransitionType.HierarchyObject)
        {
            if (currentObject != null)
            {
                currentObject.SetActive(false);

                Debug.Log(
                    "Hiding current object: " +
                    currentObject.name
                );
            }

            if (nextObject != null)
            {
                nextObject.SetActive(true);

                Debug.Log(
                    "Showing next object: " +
                    nextObject.name
                );
            }
            else
            {
                Debug.LogError(
                    "Next Object is not assigned!"
                );
            }
        }
    }
}