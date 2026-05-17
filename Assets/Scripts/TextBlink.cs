using System.Collections;
using UnityEngine;
using TMPro;

public class TextBlink : MonoBehaviour
{
    public TextMeshProUGUI textUI;
    public float fadeSpeed = 2f;

    void Start()
    {
        if (textUI == null)
        {
            textUI = GetComponent<TextMeshProUGUI>();
        }

        StartCoroutine(FadeBlink());
    }

    IEnumerator FadeBlink()
    {
        while (true)
        {
            // Fade Out
            while (textUI.alpha > 0)
            {
                textUI.alpha -= Time.deltaTime * fadeSpeed;
                yield return null;
            }

            // Fade In
            while (textUI.alpha < 1)
            {
                textUI.alpha += Time.deltaTime * fadeSpeed;
                yield return null;
            }
        }
    }
}