using System;

[Serializable]
public class MythFactStatement
{
    public string statement;
    public string correctAnswer;
    public string explanation;
}

[Serializable]
public class MythFactData
{
    public string miniGameID;
    public MythFactStatement[] statements;
}