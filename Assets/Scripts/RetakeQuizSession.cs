using System;
using UnityEngine;

// ---- These match your JSON exactly ----
[Serializable]
public class RetakeAnswer
{
    public string text;
    public bool isCorrect;
}

[Serializable]
public class RetakeQuestion
{
    public string question;
    public RetakeAnswer[] answers;
    public string explanation;
}

[Serializable]
public class RetakeChapterData
{
    public int chapterID;
    public string title;
    public string subtitle;
    public string introCharacterPrefab;
    public RetakeQuestion[] questions;
}

/// <summary>
/// Carries the chosen chapter between scenes and loads
/// Assets/Resources/Quizzes/Chapter{N}.json
/// </summary>
public static class RetakeQuizSession
{
    public static int SelectedChapter = 1;   // used if ChapterQuiz is played directly in the editor
    public static bool IsRetake = false;

    public static RetakeChapterData LoadChapter(int chapter)
    {
        string path = $"Quizzes/Chapter{chapter}";   // no ".json" for Resources.Load
        TextAsset file = Resources.Load<TextAsset>(path);

        if (file == null)
        {
            Debug.LogError($"[RetakeQuizSession] Missing file: Resources/{path}.json");
            return null;
        }

        RetakeChapterData data = JsonUtility.FromJson<RetakeChapterData>(file.text);

        if (data == null || data.questions == null || data.questions.Length == 0)
        {
            Debug.LogError($"[RetakeQuizSession] Chapter{chapter}.json has no questions or invalid format.");
            return null;
        }

        return data;
    }
}