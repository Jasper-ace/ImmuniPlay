using UnityEngine;
using UnityEngine.EventSystems;

public class GermClick : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
{
    FindObjectOfType<GameManager>().AddScore();
    Destroy(gameObject);
}
}