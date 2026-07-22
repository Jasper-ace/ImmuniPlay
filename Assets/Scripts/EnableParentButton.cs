using UnityEngine;

public class EnableParentButton : MonoBehaviour
{
    [Header("Optional Manual Parent Assignment")]
    [Tooltip("Leave this empty if you want the button to automatically enable its immediate transform parent.")]
    public GameObject targetParent;

    /// <summary>
    /// Call this function from your Button's OnClick() event in the Inspector.
    /// </summary>
    public void EnableParent()
    {
        // 1. If a specific target parent is assigned, enable that
        if (targetParent != null)
        {
            targetParent.SetActive(true);
            return;
        }

        // 2. Otherwise, dynamically find the immediate transform parent
        if (transform.parent != null)
        {
            transform.parent.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("This GameObject does not have a parent to enable!", this);
        }
    }
}