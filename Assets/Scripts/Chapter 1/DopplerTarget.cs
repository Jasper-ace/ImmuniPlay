using UnityEngine;
using UnityEngine.UI;

public class DopplerTarget : MonoBehaviour
{
    [Header("UI")]
    public GameObject outline;
    public GameObject residue;

    private RectTransform outlineRect;

    private bool completed = false;

    private void Awake()
    {
        if (outline != null)
        {
            outlineRect = outline.GetComponent<RectTransform>();
        }
    }

    private void Start()
    {
        completed = false;

        if (outline != null)
            outline.SetActive(true);

        if (residue != null)
            residue.SetActive(false);
    }

    public bool TryApplyLubricant(RectTransform lubricant)
    {
        if (completed)
            return false;

        if (lubricant == null)
            return false;

        if (outlineRect == null)
        {
            Debug.LogWarning(
                gameObject.name +
                " does not have an Outline RectTransform."
            );

            return false;
        }

        // Check if the lubricant overlaps the outline.
        if (!IsOverlapping(outlineRect, lubricant))
            return false;

        ApplyResidue();

        return true;
    }

    private bool IsOverlapping(
        RectTransform target,
        RectTransform lubricant)
    {
        Vector3[] targetCorners = new Vector3[4];
        Vector3[] lubricantCorners = new Vector3[4];

        target.GetWorldCorners(targetCorners);
        lubricant.GetWorldCorners(lubricantCorners);

        float targetMinX = targetCorners[0].x;
        float targetMaxX = targetCorners[2].x;

        float targetMinY = targetCorners[0].y;
        float targetMaxY = targetCorners[2].y;

        float lubricantMinX = lubricantCorners[0].x;
        float lubricantMaxX = lubricantCorners[2].x;

        float lubricantMinY = lubricantCorners[0].y;
        float lubricantMaxY = lubricantCorners[2].y;

        bool overlapX =
            targetMinX < lubricantMaxX &&
            targetMaxX > lubricantMinX;

        bool overlapY =
            targetMinY < lubricantMaxY &&
            targetMaxY > lubricantMinY;

        return overlapX && overlapY;
    }

    private void ApplyResidue()
    {
        completed = true;

        if (outline != null)
            outline.SetActive(false);

        if (residue != null)
            residue.SetActive(true);

        Debug.Log(
            gameObject.name +
            " → RESIDUE"
        );
    }

    public bool IsCompleted()
    {
        return completed;
    }
}