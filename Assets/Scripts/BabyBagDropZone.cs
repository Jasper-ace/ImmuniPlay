using UnityEngine;
using UnityEngine.EventSystems;

public class BabyBagDropZone : MonoBehaviour, IDropHandler
{
    [SerializeField] private ItemDescPanel itemDescPanel;
    [SerializeField] private WrongItemFeedback wrongFeedback;

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;

        DraggableItem item = eventData.pointerDrag.GetComponent<DraggableItem>();
        if (item == null) return;

        if (item.isCorrectItem)
        {
            Debug.Log("Correct item: " + item.itemName);
            if (itemDescPanel != null) itemDescPanel.Show(item);
            else Debug.LogError("BabyBagDropZone: Item Desc Panel is not assigned!");
        }
        else
        {
            Debug.Log("Wrong item: " + item.itemName);
            item.ReturnToStart();   // send the item back to where it was
            if (wrongFeedback != null) wrongFeedback.ShowX();   // show the X pop-up
            else Debug.LogError("BabyBagDropZone: Wrong Feedback is not assigned!");
        }
    }
}