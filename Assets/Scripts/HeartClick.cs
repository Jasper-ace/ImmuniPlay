using UnityEngine;
using UnityEngine.EventSystems;

public class HeartClick : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Heart Clicked!");

        Destroy(gameObject);
    }
}