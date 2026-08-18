using UnityEngine;
using UnityEngine.UI;

public class ChangeButton : MonoBehaviour
{
    public Button button1;
    public GameObject button2;

    public void ChangeToButton2()
    {
        button1.gameObject.SetActive(false);
        button2.SetActive(true);
    }
}