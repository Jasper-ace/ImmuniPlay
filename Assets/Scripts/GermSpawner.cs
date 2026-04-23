using UnityEngine;

public class GermSpawner : MonoBehaviour
{
    public GameObject germPrefab;
    public RectTransform canvas;
    public RectTransform baby;

    public float spawnInterval = 2f;
    public int maxGerms = 10;   // 👈 LIMIT

    private int spawnedCount = 0;

    void Start()
    {
        InvokeRepeating(nameof(SpawnGerm), 1f, spawnInterval);
    }

    void SpawnGerm()
    {
        // ❌ Stop if reached max
        if (spawnedCount >= maxGerms)
        {
            CancelInvoke(nameof(SpawnGerm));
            return;
        }

        float x = Random.Range(-canvas.rect.width / 2, canvas.rect.width / 2);
        float y = Random.Range(-canvas.rect.height / 2, canvas.rect.height / 2);

        GameObject germ = Instantiate(germPrefab, canvas);
        RectTransform rt = germ.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(x, y);

        germ.GetComponent<GermMove>().SetTarget(baby);

        spawnedCount++; // ✅ count it
    }
}