using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StoryManager : MonoBehaviour
{
    public GameObject coupleObject;
    public Image storyImage;
    public Sprite[] storySprites;

    public TextMeshProUGUI dialogueText;
    public string[] dialogues;

    public RectTransform speechBubble;

    // true = boy
    // false = girl
    public bool[] isBoyTalking;

    public Vector2 leftPosition;
    public Vector2 rightPosition;

    int currentIndex = 0;

    void Start()
    {
        UpdateStory();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            currentIndex++;

            if (currentIndex < dialogues.Length)
            {
                UpdateStory();
            }
        }
    }

    void UpdateStory()
{
    storyImage.sprite = storySprites[currentIndex];

    // Hide bubble if no dialogue
    if (string.IsNullOrEmpty(dialogues[currentIndex]))
    {
        speechBubble.gameObject.SetActive(false);
    }
    else
    {
        speechBubble.gameObject.SetActive(true);
        dialogueText.text = dialogues[currentIndex];
    }

    // Boy speaking
    if (isBoyTalking[currentIndex])
    {
        speechBubble.anchoredPosition = rightPosition;

        // Flip bubble
        speechBubble.localScale = new Vector3(-1, 1, 1);

        // Keep text normal
        dialogueText.transform.localScale = new Vector3(-1, 1, 1);
    }
    // Girl speaking
    else
    {
        speechBubble.anchoredPosition = leftPosition;

        // Normal bubble
        speechBubble.localScale = new Vector3(1, 1, 1);

        // Normal text
        dialogueText.transform.localScale = new Vector3(1, 1, 1);
    }
    // Element 11 special interaction
if (currentIndex == 11)
{
    coupleObject.SetActive(true);
    storyImage.gameObject.SetActive(false);
}
else
{
    coupleObject.SetActive(false);
    storyImage.gameObject.SetActive(true);
}
}
}