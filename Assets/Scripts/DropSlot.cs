using UnityEngine;
using UnityEngine.EventSystems;

public class DropSlot : MonoBehaviour, IDropHandler
{
    public bool isLeftSlot;

    public ShoeGameManager gameManager;

    public RectTransform snapPoint;


    // =========================================================
    // SOUND
    // =========================================================

    [Header("Sound")]

    public AudioClip correctPlacementSound;


    // =========================================================
    // DROP
    // =========================================================

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("Dropped on: " + gameObject.name);

        ShoeDrag shoe = eventData.pointerDrag.GetComponent<ShoeDrag>();

        Debug.Log("Shoe = " + shoe);

        if (shoe == null)
        {
            Debug.Log("shoe == null");
            return;
        }


        Debug.Log("Shoe isLeft = " + shoe.isLeft);
        Debug.Log("Slot isLeft = " + isLeftSlot);


        // =====================================================
        // WRONG SLOT
        // =====================================================

        if (shoe.isLeft != isLeftSlot)
        {
            Debug.Log("Wrong slot!");
            return;
        }


        // =====================================================
        // CORRECT SLOT
        // =====================================================

        shoe.isPlaced = true;

        Debug.Log("isPlaced = TRUE");


        RectTransform shoeRect =
            shoe.GetComponent<RectTransform>();

        shoeRect.position =
            snapPoint.position;


        // =====================================================
        // PLAY CORRECT PLACEMENT SOUND
        // =====================================================

        if (
            correctPlacementSound != null &&
            AudioManager.Instance != null
        )
        {
            AudioManager.Instance.PlaySFX(
                correctPlacementSound
            );
        }


        // =====================================================
        // DISABLE SHOE
        // =====================================================

        shoe.enabled = false;


        // =====================================================
        // TELL GAME MANAGER
        // =====================================================

        gameManager.ShoePlaced(isLeftSlot);
    }
}
