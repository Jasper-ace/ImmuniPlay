using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DialogueManager : MonoBehaviour
{
    public TextMeshProUGUI dialogueText;
    public Image dialogueImage;

    private string[] sentences = {
        "Hey there, welcome to ImmuniPlay! My name is Nurse Alex and I will be guiding and helping you understand the Expanded Program on Immunization (EPI)",
        "Our goal is to help you learn and understand the importance of immunization in our lives.",
        "And how vaccination protects our bodies from the world’s deadliest invisible enemies called germs that can make us very sick.",
        "This protection from getting sick is what we call immunization.",
        "Now let’s learn how vaccines protect us from getting sick and the different EPI vaccines!"
    };

    private int index = 0;

    void Start()
    {
        // Hide image at start
        if (dialogueImage != null)
            dialogueImage.gameObject.SetActive(false);

        // Show first sentence
        if (dialogueText != null)
            dialogueText.text = sentences[index];
        else
            Debug.LogError("Dialogue Text is NOT assigned!");
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            NextSentence();
        }
    }

    void NextSentence()
    {
        index++;

        if (index < sentences.Length)
        {
            dialogueText.text = sentences[index];

            // 👇 Show image only on 3rd sentence
            if (index == 2)
            {
                if (dialogueImage != null)
                    dialogueImage.gameObject.SetActive(true);
            }
            else
            {
                if (dialogueImage != null)
                    dialogueImage.gameObject.SetActive(false);
            }
        }
        else
        {
            // 👇 LAST CLICK → LOAD NEXT SCENE
            SceneManager.LoadScene("GameCategoryModal");
        }
    }
}