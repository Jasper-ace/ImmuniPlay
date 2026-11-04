using UnityEngine;
using System.IO;

public class JsonFileButtonToggle : MonoBehaviour
{
    public GameObject targetButton;   // drag the Chapters button GameObject here
    public string jsonFileName = "save.json"; // the exact filename to check for

    private string filePath;

    void Awake()
    {
        filePath = Path.Combine(Application.persistentDataPath, jsonFileName);
    }

    void OnEnable()
    {
        UpdateButtonState();
    }

    public void UpdateButtonState()
    {
        bool jsonExists = File.Exists(filePath);
        targetButton.SetActive(jsonExists);
    }
}