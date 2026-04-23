using UnityEngine;
using System.Collections;

public class BabyHitEffect : MonoBehaviour
{
    public GameObject normalBaby;
    public GameObject redBaby;

    public float effectDuration = 1f;

    public void TriggerHit()
    {
        StartCoroutine(HitEffect());
    }

    IEnumerator HitEffect()
    {
        normalBaby.SetActive(false);
        redBaby.SetActive(true);

        yield return new WaitForSeconds(effectDuration);

        redBaby.SetActive(false);
        normalBaby.SetActive(true);
    }
}