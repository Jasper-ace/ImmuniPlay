using UnityEngine;
using UnityEngine.UI;

public class EyeButton : MonoBehaviour
{
    [Header("Conversation")]
    public int conversationID;

    [Header("Manager")]
    public ConversationManager manager;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();

        if (button != null)
            button.onClick.AddListener(OnEyeClicked);
    }

    public void OnEyeClicked()
    {
        if (manager == null)
        {
            Debug.LogError("ConversationManager is not assigned!");
            return;
        }

        manager.StartConversation(conversationID, gameObject);
    }
}