using System;

[Serializable]
public class SaveData
{
    // Chapter completion flags
    public bool chapter1Completed = false;
    public bool chapter2Completed = false;
    public bool chapter3Completed = false;
    public bool chapter4Completed = false;
    public bool chapter5Completed = false;

    // Last scene the player was on
    public string currentScene = "";

    // Per-chapter quiz scores
    public int quizScoreChapter1 = 0;
    public int quizScoreChapter2 = 0;
    public int quizScoreChapter3 = 0;
    public int quizScoreChapter4 = 0;
    public int quizScoreChapter5 = 0;

    // Baby name
    public string babyName = "";
}
