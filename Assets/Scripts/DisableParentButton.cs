using UnityEngine;

public class DisableParentButton : MonoBehaviour
{
    [Header("Optional Target Parent")]
    [Tooltip("Drag a specific parent here. If left empty, it will automatically target the button's immediate parent.")]
    [SerializeField] private GameObject customParent;

    /// <summary>
    /// DISABLES the parent object. Attach to OnClick().
    /// </summary>
    public void DisableParent()
    {
        if (customParent != null)
        {
            customParent.SetActive(false);
            return;
        }

        if (transform.parent != null)
        {
            transform.parent.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("No parent found to disable on " + gameObject.name, this);
        }
    }
}