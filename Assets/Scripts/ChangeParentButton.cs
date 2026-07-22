using UnityEngine;

public class ChangeParentButton : MonoBehaviour
{
    [Header("Parent to Disable")]
    public GameObject currentParent;

    [Header("Parent to Enable")]
    public GameObject targetParent;

    public void ChangeParent()
    {
        // Disable the current parent
        if (currentParent != null)
            currentParent.SetActive(false);

        // Enable the target parent
        if (targetParent != null)
            targetParent.SetActive(true);
    }
}