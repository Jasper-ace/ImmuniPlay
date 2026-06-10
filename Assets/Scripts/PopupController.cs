using UnityEngine;

public class PopupController : MonoBehaviour
{
    public GameObject infoPopup1;
    public GameObject infoPopup2;
    public GameObject infoPopup3;

    public void OpenPopup1()
    {
        infoPopup1.SetActive(true);
    }

    public void ClosePopup1()
    {
        infoPopup1.SetActive(false);
    }

    public void OpenPopup2()
    {
        infoPopup2.SetActive(true);
    }

    public void ClosePopup2()
    {
        infoPopup2.SetActive(false);
    }

    public void OpenPopup3()
    {
        infoPopup3.SetActive(true);
    }

    public void ClosePopup3()
    {
        infoPopup3.SetActive(false);
    }
}