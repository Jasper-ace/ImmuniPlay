using UnityEngine;

public class NextButton : MonoBehaviour
{
    public ConversationManager manager;

    public void Next()
    {
        manager.NextDialogue();
    }
}