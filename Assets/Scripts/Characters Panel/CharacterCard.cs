using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterCard : MonoBehaviour
{
    [Header("UI References")]
    public Image characterImage;
    public TMP_Text characterName;
    public TMP_Text roleText;
    public Image roleBackground;

    [Header("Character Animation")]
    public Animator characterAnimator;

    private CharacterManager characterManager;
    private int characterIndex;

    public void Setup(
        Sprite image,
        string name,
        string role,
        Color roleColor,
        RuntimeAnimatorController animatorController,
        CharacterManager manager,
        int index
    )
    {
        characterManager = manager;
        characterIndex = index;

        if (characterImage != null)
        {
            characterImage.sprite = image;
        }

        if (characterName != null)
        {
            characterName.text = name;
        }

        if (roleText != null)
        {
            roleText.text = role;
        }

        if (roleBackground != null)
        {
            roleBackground.color = roleColor;
        }

        if (characterAnimator != null && animatorController != null)
        {
            characterAnimator.runtimeAnimatorController = animatorController;
        }
    }

    public void OnCardClicked()
    {
        if (characterManager != null)
        {
            characterManager.ShowCharacter(characterIndex);
        }
    }
}