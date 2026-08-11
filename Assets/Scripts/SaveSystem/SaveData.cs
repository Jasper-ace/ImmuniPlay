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

    // Last scene the player was on (used for the CONTINUE button)
    public string currentScene = "";

    // Per-chapter last scene (used by chapter buttons to resume correctly)
    public string lastSceneChapter1 = "";
    public string lastSceneChapter2 = "";
    public string lastSceneChapter3 = "";
    public string lastSceneChapter4 = "";
    public string lastSceneChapter5 = "";

    // Per-chapter quiz scores
    public int quizScoreChapter1 = 0;
    public int quizScoreChapter2 = 0;
    public int quizScoreChapter3 = 0;
    public int quizScoreChapter4 = 0;
    public int quizScoreChapter5 = 0;

    // Baby name
    public string babyName = "";
}

