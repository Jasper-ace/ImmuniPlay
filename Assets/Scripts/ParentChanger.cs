using UnityEngine;
using UnityEngine.UI;

public class ParentChanger : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private GameObject sceneToDisable;
    [SerializeField] private GameObject sceneToEnable;

    private void Start()
    {
        if (button != null)
        {
            button.onClick.AddListener(ChangeScene);
        }
        else
        {
            Debug.LogError("Button is not assigned in inspector!");
        }
    }

    public void ChangeScene()
    {
        Debug.Log("Button clicked!");
        
        if (sceneToDisable != null)
        {
            sceneToDisable.SetActive(false);
            Debug.Log($"{sceneToDisable.name} disabled!");
        }

        if (sceneToEnable != null)
        {
            sceneToEnable.SetActive(true);
            Debug.Log($"{sceneToEnable.name} enabled!");
        }
    }
}