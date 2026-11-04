using UnityEngine;

public class DialogueTriangleAnimation : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private AnimationClip animationClip;

    public void PlayAnimation()
    {
        if (animator == null || animationClip == null)
            return;

        animator.Play(animationClip.name, 0, 0f);
    }
}