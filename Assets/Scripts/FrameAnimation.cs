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

    void Start()
    {
        // Flip image if checkbox is checked
        Vector3 scale = targetImage.rectTransform.localScale;

        scale.x = flipImage ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);

        targetImage.rectTransform.localScale = scale;

        StartCoroutine(PlayAnimation());
    }

    IEnumerator PlayAnimation()
    {
        while (true)
        {
            targetImage.sprite = frames[currentFrame];

            currentFrame++;

            if (currentFrame >= frames.Length)
            {
                currentFrame = 0;
            }

            yield return new WaitForSeconds(frameRate);
        }
    }
}