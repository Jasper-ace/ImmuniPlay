using System.Collections;
using UnityEngine;

public class MiniGameSwitcher : MonoBehaviour
{
    public GameObject miniGame1;
    public GameObject miniGame2;
    public float delayAfterWeight = 1.0f;   // pause so the player can read the weight

    void Start()
    {
        // miniGame1.SetActive(true);
        miniGame2.SetActive(false);
    }

    public void GoToMiniGame2()
    {
        StartCoroutine(Switch());
    }

    IEnumerator Switch()
    {
        yield return new WaitForSeconds(delayAfterWeight);
        miniGame1.SetActive(false);
        miniGame2.SetActive(true);
    }
}