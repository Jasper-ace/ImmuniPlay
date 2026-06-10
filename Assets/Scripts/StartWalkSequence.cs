using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class StartWalkSequence : MonoBehaviour
{
    [Header("Objects")]
    public GameObject standingCouple;
    public GameObject walkingCouple;
    public GameObject button;

    [Header("Endpoint")]
    public RectTransform endpoint;

    [Header("Fade")]
    public Image fadePanel;

    [Header("Movement")]
    public float moveSpeed = 1000f;

    [Header("Scene")]
    public string nextSceneName = "Scene3";

    private bool walking = false;
    private bool fading = false;

    private RectTransform walkingRect;

    void Start()
    {
        walkingRect = walkingCouple.GetComponent<RectTransform>();

        if (fadePanel != null)
        {
            Color c = fadePanel.color;
            c.a = 0;
            fadePanel.color = c;
        }
    }

    public void StartWalking()
    {
        if (button != null)
            button.SetActive(false);

        if (standingCouple != null)
            standingCouple.SetActive(false);

        if (walkingCouple != null)
            walkingCouple.SetActive(true);

        walking = true;
    }

    void Update()
    {
        if (!walking || fading)
            return;

        // Move left
        walkingRect.anchoredPosition +=
            Vector2.left * moveSpeed * Time.deltaTime;

        // Check distance to endpoint
        float distance = Vector3.Distance(
            walkingRect.position,
            endpoint.position);

        // Debug
        Debug.Log("Distance: " + distance);

        if (distance < 100f)
        {
            Debug.Log("REACHED ENDPOINT");

            walking = false;

            StartCoroutine(FadeAndLoad());
        }
    }

    IEnumerator FadeAndLoad()
    {
        fading = true;

        float alpha = 0f;

        while (alpha < 1f)
        {
            alpha += Time.deltaTime;

            Color c = fadePanel.color;
            c.a = alpha;
            fadePanel.color = c;

            yield return null;
        }

        SceneManager.LoadScene(nextSceneName);
    }
}