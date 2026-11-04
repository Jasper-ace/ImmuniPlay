using UnityEngine;

public class SequentialButtonUnlocker : MonoBehaviour
{
    [Tooltip("The button (or GameObject) to enable when this one is clicked/advanced past. Leave empty if this is the last button in the chain.")]
    public GameObject nextButton;

    /// <summary>
    /// Attach this to the button's OnClick(). Hides this button and reveals the next one.
    /// </summary>
    public void AdvanceToNext()
    {
        gameObject.SetActive(false);

        if (nextButton != null)
        {
            nextButton.SetActive(true);
        }
    }
}