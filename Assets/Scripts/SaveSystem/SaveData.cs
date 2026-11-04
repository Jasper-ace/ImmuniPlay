using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public bool termsAccepted = false;
    
    public bool chapter1Completed = false;
    public bool chapter2Completed = false;
    public bool chapter3Completed = false;
    public bool chapter4Completed = false;
    public bool chapter5Completed = false;

    public string currentScene = "";

    public string lastSceneChapter1 = "";
    public string lastSceneChapter2 = "";
    public string lastSceneChapter3 = "";
    public string lastSceneChapter4 = "";
    public string lastSceneChapter5 = "";

    public string lastSceneReplayChapter1 = "";
    public string lastSceneReplayChapter2 = "";
    public string lastSceneReplayChapter3 = "";
    public string lastSceneReplayChapter4 = "";
    public string lastSceneReplayChapter5 = "";

    public int quizScoreChapter1 = 0;
    public int quizScoreChapter2 = 0;
    public int quizScoreChapter3 = 0;
    public int quizScoreChapter4 = 0;
    public int quizScoreChapter5 = 0;

    public bool quizAttemptedChapter1 = false;
    public bool quizAttemptedChapter2 = false;
    public bool quizAttemptedChapter3 = false;
    public bool quizAttemptedChapter4 = false;
    public bool quizAttemptedChapter5 = false;

    public string babyName = "";


    public string IsGiveVaccine = ""; 

    // Gallery: IDs of every video the player has finished watching.
    public List<string> viewedVideos = new List<string>();
}