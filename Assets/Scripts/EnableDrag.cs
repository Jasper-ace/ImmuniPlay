using UnityEngine;

public class EnableDrag : MonoBehaviour
{
    public GameObject couple;

    public void EnableCouple()
    {
        couple.GetComponent<DragLeftOnly>().enabled = true;
    }
}