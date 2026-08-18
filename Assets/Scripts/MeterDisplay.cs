using UnityEngine;
using UnityEngine.UI;

public class MeterDisplay : MonoBehaviour
{
    [Header("References")]
    public RectTransform probe;
    public RectTransform target;
    
    [Header("Signal States (GameObjects)")]
    public GameObject noSignalObject;
    public GameObject weakSignalObject;
    public GameObject strongSignalObject;
    public GameObject heartFoundObject;
    
    [Header("Distance Thresholds")]
    public float noSignalDistance = 1000f;      // Beyond this = No Signal
    public float weakSignalDistance = 600f;     // Beyond this = Weak
    public float strongSignalDistance = 300f;   // Beyond this = Strong
    public float heartFoundDistance = 100f;     // Within this = Heart Found!
    
    [Header("Text Display")]
    public Text meterText;
    
    private float currentDistance;
    private string currentState = "No Signal";

    void Start()
    {
        // Set No Signal as active at start, disable others
        if (noSignalObject != null) noSignalObject.SetActive(true);
        if (weakSignalObject != null) weakSignalObject.SetActive(false);
        if (strongSignalObject != null) strongSignalObject.SetActive(false);
        if (heartFoundObject != null) heartFoundObject.SetActive(false);
    }

    void Update()
    {
        if (probe == null || target == null) return;

        // Calculate distance between probe and target
        currentDistance = Vector3.Distance(probe.position, target.position);

        // Update meter based on distance
        UpdateMeterDisplay();
    }

    void UpdateMeterDisplay()
    {
        string newState = "No Signal";
        GameObject activeObject = noSignalObject;

        if (currentDistance <= heartFoundDistance)
        {
            newState = "Heart Found";
            activeObject = heartFoundObject;
            Debug.Log("❤️ HEART FOUND! Distance: " + currentDistance);
        }
        else if (currentDistance <= strongSignalDistance)
        {
            newState = "Strong";
            activeObject = strongSignalObject;
            Debug.Log("📡 STRONG Signal - Distance: " + currentDistance);
        }
        else if (currentDistance <= weakSignalDistance)
        {
            newState = "Weak";
            activeObject = weakSignalObject;
            Debug.Log("📶 WEAK Signal - Distance: " + currentDistance);
        }
        else
        {
            newState = "No Signal";
            activeObject = noSignalObject;
            Debug.Log("❌ No Signal - Distance: " + currentDistance);
        }

        // Update display if state changed
        if (newState != currentState)
        {
            currentState = newState;
            
            // Disable all objects
            if (noSignalObject != null) noSignalObject.SetActive(false);
            if (weakSignalObject != null) weakSignalObject.SetActive(false);
            if (strongSignalObject != null) strongSignalObject.SetActive(false);
            if (heartFoundObject != null) heartFoundObject.SetActive(false);
            
            // Enable the active one
            if (activeObject != null)
            {
                activeObject.SetActive(true);
            }
            
            // Update text
            if (meterText != null)
            {
                meterText.text = currentState;
            }
        }
    }

    /// <summary>
    /// Adjust distance thresholds for easier/harder difficulty
    /// </summary>
    public void SetDifficulty(float difficulty)
    {
        // difficulty: 0.5 = easy (larger ranges), 1.5 = hard (smaller ranges)
        noSignalDistance *= difficulty;
        weakSignalDistance *= difficulty;
        strongSignalDistance *= difficulty;
        heartFoundDistance *= difficulty;
    }
}