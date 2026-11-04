using UnityEngine;

public class TogglePanel : MonoBehaviour
{
    public GameObject targetParent;

    public void EnableParent()
    {
        targetParent.SetActive(true);
    }
}