using System.Collections;
using UnityEngine;

public class FollowNurseSequence : MonoBehaviour
{
    [Header("Idle GameObjects")]
    public GameObject fatherIdle;
    public GameObject motherIdle;
    public GameObject nurseIdle;

    [Header("Walking GameObjects")]
    public GameObject fatherWalking;
    public GameObject motherWalking;
    public GameObject nurseWalking;

    [Header("Speech Bubbles to Hide")]
    public GameObject motherBubble;
    public GameObject nurseBubble;

    [Header("Button to Hide")]
    public GameObject followButton;

    [Header("Transition")]
    public SceneFade sceneFade;
    public string nextSceneName = "Scene11";

    [Header("Movement Settings")]
    public float walkSpeed = 250f;
    public float walkDuration = 2.5f;

    private RectTransform fatherWalkingRect;
    private RectTransform motherWalkingRect;
    private RectTransform nurseWalkingRect;

    private bool isWalking = false;

    private void Start()
    {
        // Cache the RectTransforms of the walking objects
        if (fatherWalking != null) fatherWalkingRect = fatherWalking.GetComponent<RectTransform>();
        if (motherWalking != null) motherWalkingRect = motherWalking.GetComponent<RectTransform>();
        if (nurseWalking != null) nurseWalkingRect = nurseWalking.GetComponent<RectTransform>();
    }

    public void StartSequence()
    {
        if (isWalking) return;

        isWalking = true;

        // Hide speech bubbles
        if (motherBubble != null) motherBubble.SetActive(false);
        if (nurseBubble != null) nurseBubble.SetActive(false);

        // Hide the click button
        if (followButton != null) followButton.SetActive(false);

        // Deactivate idle GameObjects
        if (fatherIdle != null) fatherIdle.SetActive(false);
        if (motherIdle != null) motherIdle.SetActive(false);
        if (nurseIdle != null) nurseIdle.SetActive(false);

        // Activate walking GameObjects
        if (fatherWalking != null) fatherWalking.SetActive(true);
        if (motherWalking != null) motherWalking.SetActive(true);
        if (nurseWalking != null) nurseWalking.SetActive(true);

        StartCoroutine(WalkAndFadeRoutine());
    }

    private void Update()
    {
        if (!isWalking) return;

        // Move all walking characters to the right
        float step = walkSpeed * Time.deltaTime;
        if (fatherWalkingRect != null) fatherWalkingRect.anchoredPosition += Vector2.right * step;
        if (motherWalkingRect != null) motherWalkingRect.anchoredPosition += Vector2.right * step;
        if (nurseWalkingRect != null) nurseWalkingRect.anchoredPosition += Vector2.right * step;
    }

    private IEnumerator WalkAndFadeRoutine()
    {
        // Wait for characters to complete their walk
        yield return new WaitForSeconds(walkDuration);

        // Start scene fade transition
        if (sceneFade != null)
        {
            sceneFade.FadeToScene(nextSceneName);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
        }
    }
}

