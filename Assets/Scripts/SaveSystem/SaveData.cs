using System;

[Serializable]
public class SaveData
{
    // Scene
    public string currentScene;

    // Parent Navigator
    public int parentIndex;

    // Quiz
    public bool quizStarted;
    public bool quizCompleted;
    public int questionIndex;
    public int score;
}