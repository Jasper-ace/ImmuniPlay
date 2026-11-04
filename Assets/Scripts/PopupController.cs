using UnityEngine;
using System.Collections.Generic;

public class PopupController : MonoBehaviour
{
    [System.Serializable]
    public class PopupData
    {
        [Tooltip("The popup GameObject.")]
        public GameObject popup;

        [Tooltip("If enabled, this popup cannot be closed until the dialogue is finished.")]
        public bool requiresDialogue = false;
    }

    [Header("Popups")]
    public List<PopupData> popups = new List<PopupData>();

    private int currentPopupIndex = -1;
    private bool dialogueFinished = false;


    // ==========================================
    // OPEN POPUP
    // ==========================================

    public void OpenPopup(int index)
    {
        // Check index
        if (index < 0 || index >= popups.Count)
        {
            Debug.LogWarning("Invalid popup index: " + index);
            return;
        }

        // Close any currently open popup
        CloseCurrentPopupInstantly();

        PopupData popupData = popups[index];

        if (popupData.popup == null)
        {
            Debug.LogWarning("Popup " + index + " has no GameObject assigned.");
            return;
        }

        // Remember which popup is currently open
        currentPopupIndex = index;

        // Reset dialogue state
        dialogueFinished = !popupData.requiresDialogue;

        // Show popup
        popupData.popup.SetActive(true);

        Debug.Log(
            "Opened Popup " + index +
            " | Requires Dialogue: " +
            popupData.requiresDialogue
        );
    }


    // ==========================================
    // CLOSE CURRENT POPUP
    // ==========================================

    public void CloseCurrentPopup()
    {
        // No popup open
        if (currentPopupIndex < 0)
            return;

        PopupData popupData = popups[currentPopupIndex];

        // Check if dialogue is required
        if (popupData.requiresDialogue && !dialogueFinished)
        {
            Debug.Log(
                "Cannot close popup yet. Dialogue is still active."
            );

            return;
        }

        // Close popup
        if (popupData.popup != null)
        {
            popupData.popup.SetActive(false);
        }

        Debug.Log("Closed Popup " + currentPopupIndex);

        currentPopupIndex = -1;
        dialogueFinished = false;
    }


    // ==========================================
    // CLOSE CURRENT POPUP WITHOUT CHECKING
    // ==========================================

    private void CloseCurrentPopupInstantly()
    {
        if (currentPopupIndex < 0)
            return;

        PopupData popupData = popups[currentPopupIndex];

        if (popupData.popup != null)
        {
            popupData.popup.SetActive(false);
        }

        currentPopupIndex = -1;
        dialogueFinished = false;
    }


    // ==========================================
    // DIALOGUE FINISHED
    // ==========================================

    public void SetDialogueFinished()
    {
        dialogueFinished = true;

        Debug.Log("Dialogue finished. Popup can now close.");
    }


    // ==========================================
    // OPTIONAL: CLOSE SPECIFIC POPUP
    // ==========================================

    public void ClosePopup(int index)
    {
        if (index < 0 || index >= popups.Count)
        {
            Debug.LogWarning("Invalid popup index: " + index);
            return;
        }

        // If this isn't the currently active popup,
        // simply close it.
        if (index != currentPopupIndex)
        {
            if (popups[index].popup != null)
            {
                popups[index].popup.SetActive(false);
            }

            return;
        }

        // If it is the current popup, use the normal
        // dialogue protection.
        CloseCurrentPopup();
    }
}