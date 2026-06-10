using UnityEngine;

public class ShowHiddenObject : MonoBehaviour
{
    public GameObject hiddenObject;

    public void ShowObject()
    {
        hiddenObject.SetActive(true);
    }
}