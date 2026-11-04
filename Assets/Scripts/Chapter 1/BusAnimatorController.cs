using System.Collections;
using UnityEngine;

public class BusAnimatorController : MonoBehaviour
{
    [Header("Bus")]
    public Animator animator;

    [Header("UI")]
    public GameObject couple;
    public GameObject walkButton;

    [Header("Timing")]
    public float walkButtonDelay = 1f;

    private void Start()
    {
        couple.SetActive(false);
        walkButton.SetActive(false);
    }

    // Animation Event calls this when the bus stops
    public void ShowCouple()
    {
        couple.SetActive(true);

        StartCoroutine(ShowWalkButtonAfterDelay());
    }

    private IEnumerator ShowWalkButtonAfterDelay()
    {
        yield return new WaitForSeconds(walkButtonDelay);

        walkButton.SetActive(true);
    }

    public void WalkButtonPressed()
    {
        walkButton.SetActive(false);

        animator.SetTrigger("ExitBus");
    }

    // Animation Event calls this at the END of the ExitBus animation
    public void DestroyBus()
    {
        Destroy(gameObject);
    }
}