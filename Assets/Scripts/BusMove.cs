using UnityEngine;
using System.Collections;

public class BusMove : MonoBehaviour
{
    [Header("Bus")]
    public RectTransform stopPoint;
    public float speed = 500f;

    [Header("UI")]
    public GameObject couple;
    public GameObject walkButton;

    private RectTransform bus;

    void Start()
    {
        bus = GetComponent<RectTransform>();

        // Hide couple and button at start
        if (couple != null)
            couple.SetActive(false);

        if (walkButton != null)
            walkButton.SetActive(false);

        StartCoroutine(MoveBusSequence());
    }

    IEnumerator MoveBusSequence()
    {
        // Move bus to stop point
        while (Vector2.Distance(bus.anchoredPosition, stopPoint.anchoredPosition) > 5f)
        {
            bus.anchoredPosition = Vector2.MoveTowards(
                bus.anchoredPosition,
                stopPoint.anchoredPosition,
                speed * Time.deltaTime);

            yield return null;
        }

        // Stop for 1 second
        yield return new WaitForSeconds(1f);

        // Show couple and WALK button
        if (couple != null)
            couple.SetActive(true);

        if (walkButton != null)
            walkButton.SetActive(true);

        // Bus leaves to the left
        Vector2 exitPoint = new Vector2(-2000f, bus.anchoredPosition.y);

        while (Vector2.Distance(bus.anchoredPosition, exitPoint) > 5f)
        {
            bus.anchoredPosition = Vector2.MoveTowards(
                bus.anchoredPosition,
                exitPoint,
                speed * Time.deltaTime);

            yield return null;
        }

        // Hide bus after it leaves screen
        gameObject.SetActive(false);
    }
}