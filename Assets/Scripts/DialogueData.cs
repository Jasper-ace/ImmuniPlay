using UnityEngine;

public enum Speaker
{
    Mother,
    NPC1,
    NPC2,
    NPC3
}

[System.Serializable]
public class DialogueLine
{
    public Speaker speaker;
    [TextArea(3, 10)]
    public string dialogue;
}

[System.Serializable]
public class DialogueData
{
    public DialogueLine[] lines;
}
