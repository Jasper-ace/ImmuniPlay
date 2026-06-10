using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject normalHeart;
    public GameObject fastHeart;

    public Transform spawnPoint;
    public Transform heartsParent;

    public int totalHearts = 10;

    private int heartsSpawned = 0;
    public float score = 0;
    private GameObject currentHeart;

    void Start()
    {
        StartCoroutine(SpawnHearts());
    }

    IEnumerator SpawnHearts()
    {
        while (heartsSpawned < totalHearts)
        {
            SpawnHeart();

            heartsSpawned++;

            float waitTime = Random.Range(0.5f, 2f);

            yield return new WaitForSeconds(waitTime);
        }
    }

    void SpawnHeart()
    {
        float chance = Random.value;

        GameObject heartToSpawn;

        if (chance <= 0.25f)
        {
            heartToSpawn = fastHeart;
        }
        else
        {
            heartToSpawn = normalHeart;
        }

        currentHeart = Instantiate(
    heartToSpawn,
    spawnPoint.position,
    Quaternion.identity,
    heartsParent
);
    }
}