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

    // Last scene reached when replaying each completed chapter
    public string lastSceneReplayChapter1 = "";
    public string lastSceneReplayChapter2 = "";
    public string lastSceneReplayChapter3 = "";
    public string lastSceneReplayChapter4 = "";
    public string lastSceneReplayChapter5 = "";

    // Per-chapter quiz scores
    public int quizScoreChapter1 = 0;
    public int quizScoreChapter2 = 0;
    public int quizScoreChapter3 = 0;
    public int quizScoreChapter4 = 0;
    public int quizScoreChapter5 = 0;

    // Whether the player has attempted each chapter's quiz
    public bool quizAttemptedChapter1 = false;
    public bool quizAttemptedChapter2 = false;
    public bool quizAttemptedChapter3 = false;
    public bool quizAttemptedChapter4 = false;
    public bool quizAttemptedChapter5 = false;

    // Baby name
    public string babyName = "";

    // Vaccine choice
    public string IsGiveVaccine = ""; // "Give Vaccines" or "Delay Vaccines"
}


