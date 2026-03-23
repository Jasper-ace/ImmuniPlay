using UnityEngine;

public class UIController : MonoBehaviour
{
    public static UIController Instance;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public CategoryPopup CreatePopup()
    {
        GameObject popupGo = Instantiate(Resources.Load<GameObject>("UI/CategoryPopup"));
        return popupGo.GetComponent<CategoryPopup>();
    }
}