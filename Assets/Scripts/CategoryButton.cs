using UnityEngine;

public class CategoryButton : MonoBehaviour
{
    public string categoryTitle;
    [TextArea(3, 5)]
    public string categoryDescription;

    public Transform canvas; // drag your Canvas here
    public string[] miniGameScenes;

    public void OpenPopup()
    {
        CategoryPopup popup = UIController.Instance.CreatePopup();
        popup.Init(canvas, categoryTitle, categoryDescription, miniGameScenes);
    }
}
