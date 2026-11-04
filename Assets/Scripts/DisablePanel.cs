using UnityEngine;

public class DisablePanel : MonoBehaviour
{
    public GameObject targetParent;

    public void DisableParent()
    {
        targetParent.SetActive(false);
    }
}