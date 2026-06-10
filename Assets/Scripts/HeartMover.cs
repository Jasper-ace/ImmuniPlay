using UnityEngine;
using UnityEngine.EventSystems;

public class HeartMover : MonoBehaviour, IPointerClickHandler
{
    public float speed = 400f;

    void Update()
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Heart Clicked!");

        Destroy(gameObject);
    }
}