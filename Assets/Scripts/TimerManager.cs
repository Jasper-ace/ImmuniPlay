using UnityEngine;
using TMPro;

public class TimerManager : MonoBehaviour
{
    public float timeLeft = 20f;
    public TMP_Text timerText;

    public GameObject timeUpPanel;

    private bool isRunning = true;

    void Update()
    {
        if (!isRunning) return;

        if (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            timerText.text = "Timer: " + Mathf.Ceil(timeLeft).ToString() + "s";
        }
        else
        {
            timeLeft = 0;
            timerText.text = "Time's Up!";
            isRunning = false;

            if (timeUpPanel != null)
                timeUpPanel.SetActive(true);

            DisableAllButtons();
        }
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    void DisableAllButtons()
    {
        UnityEngine.UI.Button[] buttons = FindObjectsOfType<UnityEngine.UI.Button>();

        foreach (var b in buttons)
        {
            b.interactable = false;
        }
    }
}