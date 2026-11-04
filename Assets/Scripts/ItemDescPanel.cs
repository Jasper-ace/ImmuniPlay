using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemDescPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Button takeButton;
    [SerializeField] private ItemChecklist checklist; // optional, para sa checklist sa gilid

    private DraggableItem currentItem;

    void Awake() => takeButton.onClick.AddListener(OnTakeClicked);

    public void Show(DraggableItem item)
    {
        currentItem = item;
        descriptionText.text = $"{item.itemName}: {item.description}";
        gameObject.SetActive(true);
    }

    private void OnTakeClicked()
    {
        currentItem.MarkAsPacked();
        if (checklist != null) checklist.Check(currentItem.itemName);
        currentItem = null;
        gameObject.SetActive(false);
    }
}