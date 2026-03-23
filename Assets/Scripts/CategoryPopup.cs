using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class CategoryPopup : MonoBehaviour
{

    public TextMeshProUGUI title;
    public TextMeshProUGUI description;
    public Button button1;
    public Button button2;
    public Button button3;
    public Button button4;
    public Button button5;
    public Button cancel;


    public void Init(Transform canvas, string newTitle, string newDescription, string[] sceneNames)
    {
        title.text = newTitle;
        description.text = newDescription;

        transform.SetParent(canvas);
        transform.localScale = Vector3.one;
        GetComponent<RectTransform>().offsetMin = Vector2.zero;
        GetComponent<RectTransform>().offsetMax = Vector2.zero;

        // Assign buttons to load specific mini-games
        Button[] buttons = { button1, button2, button3, button4, button5 };

        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i; // local copy for closure
            if (i < sceneNames.Length && !string.IsNullOrEmpty(sceneNames[i]))
            {
                buttons[i].onClick.RemoveAllListeners();
                buttons[i].onClick.AddListener(() =>
                {
                    SceneLoader.Instance.LoadScene(sceneNames[index]);
                });
            }
            else
            {
                buttons[i].gameObject.SetActive(false); // hide unused buttons
            }
        }

        // removes the popup
        cancel.onClick.AddListener(() =>
        {
            GameObject.Destroy(this.gameObject);
        });
    }
}
