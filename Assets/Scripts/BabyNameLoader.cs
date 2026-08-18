using UnityEngine;
using TMPro;
using System.IO;

public class BabyNameLoader : MonoBehaviour
{
    public TMP_Text babyNameText;

    [System.Serializable]
    public class SaveData
    {
        public string babyName;
        public string lastSceneChapter5;
    }

    void Start()
    {
        string path = Path.Combine(Application.persistentDataPath, "save.json");

        if (!File.Exists(path))
        {
            Debug.LogError("save.json not found!");
            return;
        }

        string json = File.ReadAllText(path);

        SaveData data = JsonUtility.FromJson<SaveData>(json);

        babyNameText.text = data.babyName + "'S SHIELD METER";

        Debug.Log("Baby Name: " + data.babyName);
    }
}