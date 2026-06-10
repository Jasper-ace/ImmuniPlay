using UnityEngine;

public class TouchToNext : MonoBehaviour
{
    public GameObject nextObject;

    void Update()
    {
        if (gameObject.activeSelf)
        {
            if (Input.GetMouseButtonDown(0))
            {
                gameObject.SetActive(false);

                nextObject.SetActive(true);
            }
        }
    }
}