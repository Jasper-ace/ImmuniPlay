using UnityEngine;
using System.Collections;

public class SpawnerHearts : MonoBehaviour
{
    [Header("Heart")]
    public GameObject heartPrefab;

    [Header("Spawn Timing")]
    public float minSpawnTime = 0.8f;
    public float maxSpawnTime = 2f;

    private RectTransform spawnArea;

    void Awake()
    {
        spawnArea = GetComponent<RectTransform>();
    }

    void Start()
    {
        StartCoroutine(SpawnHearts());
    }

    IEnumerator SpawnHearts()
    {
        while (true)
        {
            float waitTime =
                Random.Range(minSpawnTime, maxSpawnTime);

            yield return new WaitForSeconds(waitTime);

            SpawnHeart();
        }
    }

    void SpawnHeart()
    {
        GameObject newHeart =
            Instantiate(heartPrefab, transform);

        RectTransform heartRect =
            newHeart.GetComponent<RectTransform>();

        float randomX = Random.Range(
            -spawnArea.rect.width / 2f,
            spawnArea.rect.width / 2f
        );

        float spawnY =
            -spawnArea.rect.height / 2f;

        heartRect.anchoredPosition =
            new Vector2(randomX, spawnY);
    }
}