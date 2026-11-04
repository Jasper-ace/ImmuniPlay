using UnityEngine;

public class TeddyDrag : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public GameObject babyCrying;
    public GameObject babyHappy;
    public Transform babyDropArea;       // i-drag ang Baby
    public float snapDistance = 1.5f;    // gaano kalapit para ma-trigger
    public float grabRadius = 1f;        // gaano kalawak ang pwedeng pindutin sa teddy

    private Camera cam;
    private bool isDragging;
    private bool done;
    private Vector3 startPos;
    private Vector3 offset;

    void Start()
    {
        cam = Camera.main;
        startPos = transform.position;
    }

    void Update()
    {
        if (done) return;

        Vector3 mouseWorld = GetMouseWorldPos();

        // Simula ng drag
        if (Input.GetMouseButtonDown(0))
        {
            if (Vector2.Distance(mouseWorld, transform.position) <= grabRadius)
            {
                isDragging = true;
                offset = transform.position - mouseWorld;
            }
        }

        // Habang nagda-drag
        if (isDragging && Input.GetMouseButton(0))
        {
            transform.position = mouseWorld + offset;
        }

        // Bitaw
        if (isDragging && Input.GetMouseButtonUp(0))
        {
            isDragging = false;

            float dist = Vector2.Distance(transform.position, babyDropArea.position);
            if (dist <= snapDistance)
                MakeBabyHappy();
            else
                transform.position = startPos; // balik kung hindi tumama
        }
    }

    void MakeBabyHappy()
    {
        done = true;
        babyCrying.SetActive(false);
        babyHappy.SetActive(true);
        gameObject.SetActive(false); // kasama na ang teddy sa BabyHappy art
    }

    Vector3 GetMouseWorldPos()
    {
        Vector3 mouse = Input.mousePosition;
        mouse.z = Mathf.Abs(cam.transform.position.z - transform.position.z);
        return cam.ScreenToWorldPoint(mouse);
    }
}