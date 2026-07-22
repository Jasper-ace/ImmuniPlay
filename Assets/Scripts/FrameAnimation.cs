using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FrameAnimation : MonoBehaviour
{
    public Image targetImage;
    public Sprite[] frames;
    public float frameRate = 0.1f;

    [Header("Options")]
    public bool flipImage = false;

    private int currentFrame = 0;
    private Coroutine animationCoroutine;

    void Awake()
    {
        // Flip only once
        Vector3 scale = targetImage.rectTransform.localScale;
        scale.x = flipImage ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
        targetImage.rectTransform.localScale = scale;
    }

    void OnEnable()
    {
        currentFrame = 0;

        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);

        animationCoroutine = StartCoroutine(PlayAnimation());
    }

    void OnDisable()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }
    }

    IEnumerator PlayAnimation()
    {
        while (true)
        {
            targetImage.sprite = frames[currentFrame];

            currentFrame = (currentFrame + 1) % frames.Length;

            yield return new WaitForSeconds(frameRate);
        }
    }
}