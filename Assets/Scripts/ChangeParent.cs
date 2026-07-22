using UnityEngine;

public class ChangeParent : MonoBehaviour
{
    [Header("Current Parent (Hide)")]
    public GameObject currentParent;

    [Header("Target Parent (Show)")]
    public GameObject targetParent;

    [Header("Optional: Object to Move")]
    public Transform objectToMove;

    public void SwitchParent()
    {
        // Hide the current parent
        if (currentParent != null)
        {
            currentParent.SetActive(false);
        }

        // Show the target parent
        if (targetParent != null)
        {
            targetParent.SetActive(true);
        }

        // Optional: Move the object to the new parent
        if (objectToMove != null && targetParent != null)
        {
            objectToMove.SetParent(targetParent.transform, false);
        }
    }
}