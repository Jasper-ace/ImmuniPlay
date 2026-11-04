using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterManager : MonoBehaviour
{
    [System.Serializable]
    public class CharacterData
    {
        [Header("Character Information")]
        public string characterName;
        public string role;

        [TextArea(3, 8)]
        public string description;

        [Header("Character Appearance")]
        public Sprite characterImage;
        public Color roleColor = Color.white;

        [Header("Character Animation")]
        public RuntimeAnimatorController animatorController;
    }

    [Header("Character Card")]
    public CharacterCard characterCardPrefab;

    [Header("Content Parent")]
    public Transform content;

    [Header("Characters")]
    public CharacterData[] characters;

    [Header("Character Description Panel")]
    public GameObject descriptionPanel;

    public Image descriptionImage;
    public Animator descriptionAnimator;

    public TMP_Text descriptionName;
    public TMP_Text descriptionText;

    private void Start()
    {
        DisplayCharacters();

        // Hide description panel when scene starts
        if (descriptionPanel != null)
        {
            descriptionPanel.SetActive(false);
        }
    }

    public void DisplayCharacters()
    {
        if (characterCardPrefab == null)
        {
            Debug.LogError("CharacterManager: Character Card Prefab is not assigned!");
            return;
        }

        if (content == null)
        {
            Debug.LogError("CharacterManager: Content is not assigned!");
            return;
        }

        // Remove existing cards
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        // Create character cards
        for (int i = 0; i < characters.Length; i++)
        {
            CharacterData character = characters[i];

            CharacterCard card = Instantiate(
                characterCardPrefab,
                content
            );

            card.Setup(
                character.characterImage,
                character.characterName,
                character.role,
                character.roleColor,
                character.animatorController,
                this,
                i
            );
        }
    }

    public void ShowCharacter(int index)
    {
        if (index < 0 || index >= characters.Length)
        {
            Debug.LogError("CharacterManager: Invalid character index!");
            return;
        }

        CharacterData character = characters[index];

        // Set character image
        if (descriptionImage != null)
        {
            descriptionImage.sprite = character.characterImage;
        }

        // Set character name
        if (descriptionName != null)
        {
            descriptionName.text = character.characterName;
        }

        // Set description
        if (descriptionText != null)
        {
            descriptionText.text = character.description;
        }

        // Set character animator
        if (descriptionAnimator != null)
        {
            if (character.animatorController != null)
            {
                descriptionAnimator.runtimeAnimatorController =
                    character.animatorController;

                // Restart the animation from the beginning
                descriptionAnimator.Rebind();
                descriptionAnimator.Update(0f);
            }
            else
            {
                Debug.LogWarning(
                    "No Animator Controller assigned for " +
                    character.characterName
                );

                descriptionAnimator.runtimeAnimatorController = null;
            }
        }

        // Show panel
        if (descriptionPanel != null)
        {
            descriptionPanel.SetActive(true);
        }

        Debug.Log(
            "Showing character: " +
            character.characterName +
            " | Animator: " +
            (
                character.animatorController != null
                    ? character.animatorController.name
                    : "NONE"
            )
        );
    }

    public void CloseDescription()
    {
        if (descriptionPanel != null)
        {
            descriptionPanel.SetActive(false);
        }
    }
}