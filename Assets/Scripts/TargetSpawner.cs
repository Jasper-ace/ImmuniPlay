using UnityEngine;
using UnityEngine.UI;

public class TargetSpawner : MonoBehaviour
{
    [Header("Spawn Area")]
    public RectTransform spawnArea;           // The area where target can spawn
    public Vector2 minPosition = new Vector2(-200, -200);
    public Vector2 maxPosition = new Vector2(200, 200);

    [Header("Visibility")]
    public bool hideTarget = true;            // Make target invisible during game

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Image targetImage;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        targetImage = GetComponent<Image>();
        canvasGroup = GetComponent<CanvasGroup>();
        
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // Spawn single target at start
        SpawnRandomPosition();
    }

    /// <summary>
    /// Spawn target at a random position (INVISIBLE)
    /// </summary>
    public void SpawnRandomPosition()
    {
        // Random position within min/max bounds
        float randomX = Random.Range(minPosition.x, maxPosition.x);
        float randomY = Random.Range(minPosition.y, maxPosition.y);

        rectTransform.anchoredPosition = new Vector2(randomX, randomY);
        
        // Make target invisible
        if (hideTarget)
        {
            canvasGroup.alpha = 0f;  // Invisible but still detectable
        }

        Debug.Log("🎯 Target spawned INVISIBLY at: " + rectTransform.anchoredPosition);
        Debug.Log("📍 Find it with the probe!");
    }

    /// <summary>
    /// Make target VISIBLE when found (reveal)
    /// </summary>
    public void RevealTarget()
    {
        canvasGroup.alpha = 1f;
        Debug.Log("✅ Target FOUND and revealed!");
    }
}