using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ButtonHandler : MonoBehaviour
{
    [Header("Correct Answers")]
    public string[] correctAnswers = {
        "Spoiled Burger",
        "Moldy Closet",
        "Dirty Shoes",
        "Dusty Keyboard"
    };

    [Header("References")]
    public RectTransform canvasRect;
    public GameObject winPanel;
    public TimerManager timerManager;

    [Header("Shake Settings")]
    public float shakeDuration = 0.3f;
    public float shakeMagnitude = 15f;

    private int correctCount = 0;

    public void OnClick(Button btn)
    {
        TMP_Text txt = btn.GetComponentInChildren<TMP_Text>();
        Image img = btn.GetComponent<Image>();

        if (txt == null || img == null)
        {
            Debug.LogError("Missing TMP_Text or Image!");
            return;
        }

        string buttonText = txt.text.Trim().ToLower();

        bool isCorrect = false;

        foreach (string answer in correctAnswers)
        {
            if (buttonText == answer.Trim().ToLower())
            {
                isCorrect = true;
                break;
            }
        }

        if (isCorrect)
        {
            img.color = Color.green;
            btn.interactable = false;
            correctCount++;

            CheckWin();
        }
        else
        {
            StartCoroutine(WrongEffect(img));
        }
    }

    void CheckWin()
    {
        if (correctCount >= correctAnswers.Length)
        {
            Debug.Log("🎉 YOU WIN!");

            if (winPanel != null)
                winPanel.SetActive(true);

            if (timerManager != null)
                timerManager.StopTimer();

            DisableAllButtons();
        }
    }

    IEnumerator WrongEffect(Image img)
    {
        img.color = Color.red;

        if (canvasRect != null)
            StartCoroutine(ShakeUI());

        yield return new WaitForSeconds(0.5f);

        img.color = Color.white;
    }

    IEnumerator ShakeUI()
    {
        Vector3 originalPos = canvasRect.localPosition;

        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;

            canvasRect.localPosition = originalPos + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        canvasRect.localPosition = originalPos;
    }

    void DisableAllButtons()
    {
        Button[] buttons = FindObjectsOfType<Button>();

        foreach (Button b in buttons)
        {
            b.interactable = false;
        }
    }
}