using UnityEngine;
using TMPro;

public class ChatManager : MonoBehaviour
{
    public GameObject chatBubble;
    public TMP_Text dialogueText;
    public TiltWeightGame tiltGame;
    

    [TextArea(2, 5)]
    public string[] dialogues;

    private int currentIndex = 0;

    void Start()
    {
        currentIndex = 0;
        ShowDialogue();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            NextDialogue();
        }
    }

    void NextDialogue()
    {
        currentIndex++;

        if (currentIndex >= dialogues.Length)
        {
            chatBubble.SetActive(false);
            tiltGame.canTilt = true;
            return;
        }

        ShowDialogue();
    }

    void ShowDialogue()
    {
        if (string.IsNullOrEmpty(dialogues[currentIndex]))
        {
            chatBubble.SetActive(false);
            tiltGame.canTilt = true;
        }
        else
        {
            chatBubble.SetActive(true);
            dialogueText.text = dialogues[currentIndex];
        }
    }
}