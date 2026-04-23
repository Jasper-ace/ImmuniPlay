using UnityEngine;

public class GermMove : MonoBehaviour
{
    public float speed = 100f;

    private RectTransform rectTransform;
    private RectTransform target;

    private BabyHitEffect babyEffect;

    public void SetTarget(RectTransform baby)
    {
        target = baby;

        // find the effect script
        babyEffect = FindObjectOfType<BabyHitEffect>();
    }

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (target == null) return;

        Vector2 direction = (target.anchoredPosition - rectTransform.anchoredPosition).normalized;
        rectTransform.anchoredPosition += direction * speed * Time.deltaTime;

        // 💥 HIT DETECTION
        if (Vector2.Distance(rectTransform.anchoredPosition, target.anchoredPosition) < 50f)
        {
            if (babyEffect != null)
            {
                babyEffect.TriggerHit(); // 🔥 trigger red effect
            }
                FindObjectOfType<GameManager>().GermMissed(); // 👈 add this
            Destroy(gameObject); // remove germ
        }
    }
}