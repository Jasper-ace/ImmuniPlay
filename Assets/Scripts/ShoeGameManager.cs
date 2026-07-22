using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ShoeGameManager : MonoBehaviour
{
    [Header("Carpet Image")]
    public Image carpetImage;

    [Header("Carpet Sprites")]
    public Sprite carpetEmpty;
    public Sprite carpetLeft;
    public Sprite carpetRight;
    public Sprite carpetComplete;

    [Header("Parents")]
    public GameObject shoeMiniGameParent;
    public GameObject nextParent;

    [Header("Fade")]
    public Image fadeImage;
    public float fadeDuration = 0.5f;

    private bool leftPlaced = false;
    private bool rightPlaced = false;

    private void Start()
    {
        // Hide the next parent at the start
        if (nextParent != null)
            nextParent.SetActive(false);

        // Make sure the fade image starts transparent
        if (fadeImage != null)
        {
            Color color = fadeImage.color;
            color.a = 0f;
            fadeImage.color = color;
        }

        // Set the initial carpet
        if (carpetImage != null && carpetEmpty != null)
            carpetImage.sprite = carpetEmpty;
    }

    public void ShoePlaced(bool isLeft)
    {
        if (isLeft)
            leftPlaced = true;
        else
            rightPlaced = true;

        UpdateCarpet();

        if (leftPlaced && rightPlaced)
        {
            StartCoroutine(CompleteMiniGame());
        }
    }

    void UpdateCarpet()
    {
        if (leftPlaced && rightPlaced)
        {
            carpetImage.sprite = carpetComplete;
        }
        else if (leftPlaced)
        {
            carpetImage.sprite = carpetLeft;
        }
        else if (rightPlaced)
        {
            carpetImage.sprite = carpetRight;
        }
        else
        {
            carpetImage.sprite = carpetEmpty;
        }
    }

    IEnumerator CompleteMiniGame()
    {
        // Fade Out
        yield return StartCoroutine(FadeOut());

        // Switch Parents
        if (shoeMiniGameParent != null)
            shoeMiniGameParent.SetActive(false);

        if (nextParent != null)
            nextParent.SetActive(true);

        // Fade In
        yield return StartCoroutine(FadeIn());
    }

    IEnumerator FadeOut()
    {
        if (fadeImage == null)
            yield break;

        float timer = 0f;
        Color color = fadeImage.color;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            color.a = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        color.a = 1f;
        fadeImage.color = color;
    }

    IEnumerator FadeIn()
    {
        if (fadeImage == null)
            yield break;

        float timer = 0f;
        Color color = fadeImage.color;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            color.a = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        color.a = 0f;
        fadeImage.color = color;
    }
}