using UnityEngine;

public class HeartSpawner : MonoBehaviour
{
    public GameObject heartPrefab;

    public Transform spawnPoint;

    public Transform heartsParent;

    void Start()
    {
        Instantiate(
            heartPrefab,
            spawnPoint.position,
            Quaternion.identity,
            heartsParent
        );
    }
}