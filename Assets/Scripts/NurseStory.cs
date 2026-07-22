using UnityEngine;
using TMPro;
using System.Collections;

public class NurseStory : MonoBehaviour
{
    [Header("UI")]
    public GameObject nurseBubble;
    public GameObject bottomDialogue;

    public TextMeshProUGUI nurseText;
    public TextMeshProUGUI bottomText;

    [Header("Dialogue")]
    [TextArea(2, 5)]
    public string[] dialogues;

    public bool[] isNurseTalking;

    [Header("Typewriter")]
    public float typingSpeed = 0.03f;

    [Header("Scene Transition")]
    public SceneFade fadeManager;
    public string nextScene = "Scene5";

    [Header("Objects To Hide After Dialogue")]
    public GameObject[] objectsToHide;

    private int step = -1;
    private bool isTyping = false;
    private string currentDialogue;

    void Start()
    {
        nurseBubble.SetActive(false);
        bottomDialogue.SetActive(false);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Ignore input if it's on a UI Button (like the Eye Button)
            if (UnityEngine.EventSystems.EventSystem.current != null)
            {
                var pointerData = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current)
                {
                    position = Input.mousePosition
                };
                var results = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
                UnityEngine.EventSystems.EventSystem.current.RaycastAll(pointerData, results);
                bool clickedButton = false;
                foreach (var result in results)
                {
                    if (result.gameObject.GetComponentInParent<UnityEngine.UI.Button>() != null)
                    {
                        clickedButton = true;
                        break;
                    }
                }
                if (clickedButton) return;
            }

            // Finish typing instantly
            if (isTyping)
            {
                StopAllCoroutines();

                if (isNurseTalking[step])
                    nurseText.text = currentDialogue;
                else
                    bottomText.text = currentDialogue;

                isTyping = false;
            }
            else
            {
                NextDialogue();
            }
        }
    }

    void NextDialogue()
    {
        step++;

        // End of dialogue
        if (step >= dialogues.Length)
        {
            // Hide chat bubbles
            nurseBubble.SetActive(false);
            bottomDialogue.SetActive(false);

            // Hide any additional objects
            foreach (GameObject obj in objectsToHide)
            {
                if (obj != null)
                    obj.SetActive(false);
            }

            // Change scene
            if (fadeManager != null)
            {
                fadeManager.FadeToScene(nextScene);
            }

            enabled = false;
            return;
        }

        currentDialogue = dialogues[step];

        if (isNurseTalking[step])
        {
            nurseBubble.SetActive(true);
            bottomDialogue.SetActive(false);

            StartCoroutine(TypeText(nurseText, currentDialogue));
        }
        else
        {
            nurseBubble.SetActive(false);
            bottomDialogue.SetActive(true);

            StartCoroutine(TypeText(bottomText, currentDialogue));
        }
    }

    IEnumerator TypeText(TextMeshProUGUI textUI, string message)
    {
        isTyping = true;
        textUI.text = "";

        foreach (char letter in message)
        {
            textUI.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }
}