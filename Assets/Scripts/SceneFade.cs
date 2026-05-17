using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneFade : MonoBehaviour
{
    public Image fadeImage;
    public float fadeSpeed = 2f;

    public void FadeToScene(string sceneName)
    {
        StartCoroutine(FadeOut(sceneName));
    }

    IEnumerator FadeOut(string sceneName)
    {
        Color color = fadeImage.color;

        while (color.a < 1)
        {
            color.a += Time.deltaTime * fadeSpeed;
            fadeImage.color = color;
            yield return null;
        }

        SceneManager.LoadScene(sceneName);
    }
}