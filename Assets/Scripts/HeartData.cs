using UnityEngine;
using UnityEngine.EventSystems;

public class HeartData : MonoBehaviour, IPointerClickHandler
{
    public bool isFastHeart = false;

    private RectTransform greenLine;

    void Start()
    {
        greenLine = GameObject.Find("GreenLine").GetComponent<RectTransform>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        RectTransform heartRect = GetComponent<RectTransform>();

        float distance =
            Mathf.Abs(
                heartRect.position.x -
                greenLine.position.x
            );

        Debug.Log("Distance = " + distance);

        // PERFECT
        if (distance <= 50f)
        {
            if (isFastHeart)
            {
                ScoreManager.Instance.AddScore(2f);
                Debug.Log("FAST PERFECT +2");
            }
            else
            {
                ScoreManager.Instance.AddScore(1f);
                Debug.Log("NORMAL PERFECT +1");
            }
        }
        // GOOD
        else if (distance <= 150f)
        {
            if (isFastHeart)
            {
                ScoreManager.Instance.AddScore(1f);
                Debug.Log("FAST GOOD +1");
            }
            else
            {
                ScoreManager.Instance.AddScore(0.5f);
                Debug.Log("NORMAL GOOD +0.5");
            }
        }
        else
        {
            Debug.Log("MISS CLICK +0");
        }

        Destroy(gameObject);
    }
}