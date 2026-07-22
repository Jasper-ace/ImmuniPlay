using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    private string savePath;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        savePath = Path.Combine(Application.persistentDataPath, "save.json");

        Debug.Log("Save Path:");
        Debug.Log(savePath);
    }

    // ------------------------------
    // TEMPORARY TEST
    // Remove this later
    // ------------------------------
    private void Start()
    {
        SaveCurrentProgress(
            5,
            true,
            false,
            2,
            2
        );

        LoadGame();
    }

    // ------------------------------
    // SAVE
    // ------------------------------
    public void SaveCurrentProgress(
        int parentIndex,
        bool quizStarted,
        bool quizCompleted,
        int questionIndex,
        int score)
    {
        Debug.Log("SaveCurrentProgress called");

        SaveData data = new SaveData();

        data.currentScene = SceneManager.GetActiveScene().name;
        data.parentIndex = parentIndex;
        data.quizStarted = quizStarted;
        data.quizCompleted = quizCompleted;
        data.questionIndex = questionIndex;
        data.score = score;

        string json = JsonUtility.ToJson(data, true);

        File.WriteAllText(savePath, json);

        Debug.Log("========== GAME SAVED ==========");
        Debug.Log(json);
    }

    // ------------------------------
    // LOAD
    // ------------------------------
    public SaveData LoadGame()
    {
        if (!File.Exists(savePath))
        {
            Debug.Log("No Save File Found.");
            return null;
        }

        string json = File.ReadAllText(savePath);

        Debug.Log("========== GAME LOADED ==========");
        Debug.Log(json);

        SaveData data = JsonUtility.FromJson<SaveData>(json);

        Debug.Log("Scene: " + data.currentScene);
        Debug.Log("Parent: " + data.parentIndex);
        Debug.Log("Question: " + data.questionIndex);
        Debug.Log("Score: " + data.score);

        return data;
    }

    // ------------------------------
    // DELETE SAVE
    // ------------------------------
    public void DeleteSave()
    {
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log("Save Deleted");
        }
    }
}