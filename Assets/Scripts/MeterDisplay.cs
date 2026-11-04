using System.Collections;
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
    public float noSignalDistance = 1000f;
    public float weakSignalDistance = 600f;
    public float strongSignalDistance = 300f;
    public float heartFoundDistance = 100f;

    [Header("Text Display")]
    public Text meterText;

    // =========================================================
    // HEARTBEAT
    // =========================================================

    [Header("Heartbeat")]

    [Tooltip("The single heartbeat sound used for all signal levels.")]
    public AudioClip heartbeatSound;

    [Range(0f, 1f)]
    public float weakHeartbeatVolume = 0.30f;

    [Range(0f, 1f)]
    public float strongHeartbeatVolume = 0.60f;

    [Range(0f, 1f)]
    public float heartFoundHeartbeatVolume = 1.00f;

    // =========================================================
    // INTERNAL VARIABLES
    // =========================================================

    private float currentDistance;

    private string currentState = "No Signal";

    private Coroutine heartbeatCoroutine;

    private float currentHeartbeatVolume;

    // =========================================================
    // START
    // =========================================================

    void Start()
    {
        // -----------------------------------------------------
        // Set initial signal objects
        // -----------------------------------------------------

        if (noSignalObject != null)
            noSignalObject.SetActive(true);

        if (weakSignalObject != null)
            weakSignalObject.SetActive(false);

        if (strongSignalObject != null)
            strongSignalObject.SetActive(false);

        if (heartFoundObject != null)
            heartFoundObject.SetActive(false);

        // -----------------------------------------------------
        // Set initial text
        // -----------------------------------------------------

        if (meterText != null)
            meterText.text = currentState;

        // -----------------------------------------------------
        // Make sure heartbeat is not running at start
        // -----------------------------------------------------

        StopHeartbeat();
    }

    // =========================================================
    // UPDATE
    // =========================================================

    void Update()
    {
        if (probe == null || target == null)
            return;

        // -----------------------------------------------------
        // Calculate distance
        // -----------------------------------------------------

        currentDistance =
            Vector3.Distance(
                probe.position,
                target.position
            );

        // -----------------------------------------------------
        // Update signal display
        // -----------------------------------------------------

        UpdateMeterDisplay();
    }

    // =========================================================
    // SIGNAL DISPLAY
    // =========================================================

    void UpdateMeterDisplay()
    {
        string newState = "No Signal";
        GameObject activeObject = noSignalObject;

        // =====================================================
        // HEART FOUND
        // =====================================================

        if (currentDistance <= heartFoundDistance)
        {
            newState = "Heart Found";
            activeObject = heartFoundObject;
        }

        // =====================================================
        // STRONG SIGNAL
        // =====================================================

        else if (currentDistance <= strongSignalDistance)
        {
            newState = "Strong";
            activeObject = strongSignalObject;
        }

        // =====================================================
        // WEAK SIGNAL
        // =====================================================

        else if (currentDistance <= weakSignalDistance)
        {
            newState = "Weak";
            activeObject = weakSignalObject;
        }

        // =====================================================
        // NO SIGNAL
        // =====================================================

        else
        {
            newState = "No Signal";
            activeObject = noSignalObject;
        }

        // =====================================================
        // ONLY UPDATE WHEN STATE CHANGES
        // =====================================================

        if (newState != currentState)
        {
            currentState = newState;

            // -------------------------------------------------
            // Disable all signal objects
            // -------------------------------------------------

            if (noSignalObject != null)
                noSignalObject.SetActive(false);

            if (weakSignalObject != null)
                weakSignalObject.SetActive(false);

            if (strongSignalObject != null)
                strongSignalObject.SetActive(false);

            if (heartFoundObject != null)
                heartFoundObject.SetActive(false);

            // -------------------------------------------------
            // Enable current signal object
            // -------------------------------------------------

            if (activeObject != null)
                activeObject.SetActive(true);

            // -------------------------------------------------
            // Update text
            // -------------------------------------------------

            if (meterText != null)
                meterText.text = currentState;

            // -------------------------------------------------
            // Update heartbeat
            // -------------------------------------------------

            UpdateHeartbeat();

            // -------------------------------------------------
            // Debug
            // -------------------------------------------------

            Debug.Log(
                "Signal changed to: " +
                currentState +
                " | Distance: " +
                currentDistance
            );
        }
    }

    // =========================================================
    // UPDATE HEARTBEAT
    // =========================================================

    void UpdateHeartbeat()
    {
        // -----------------------------------------------------
        // NO SIGNAL
        // -----------------------------------------------------

        if (currentState == "No Signal")
        {
            StopHeartbeat();
            return;
        }

        // -----------------------------------------------------
        // Make sure heartbeat exists
        // -----------------------------------------------------

        if (heartbeatSound == null)
        {
            Debug.LogWarning(
                "MeterDisplay: Heartbeat Sound is not assigned."
            );

            return;
        }

        // -----------------------------------------------------
        // Determine heartbeat volume
        // -----------------------------------------------------

        switch (currentState)
        {
            case "Weak":
                currentHeartbeatVolume = weakHeartbeatVolume;
                break;

            case "Strong":
                currentHeartbeatVolume = strongHeartbeatVolume;
                break;

            case "Heart Found":
                currentHeartbeatVolume = heartFoundHeartbeatVolume;
                break;

            default:
                currentHeartbeatVolume = 0f;
                break;
        }

        // -----------------------------------------------------
        // Restart heartbeat loop with new volume
        // -----------------------------------------------------

        StopHeartbeat();

        heartbeatCoroutine =
            StartCoroutine(HeartbeatLoop());
    }

    // =========================================================
    // HEARTBEAT LOOP
    // =========================================================

    IEnumerator HeartbeatLoop()
    {
        while (true)
        {
            // -------------------------------------------------
            // Stop if we reach No Signal
            // -------------------------------------------------

            if (currentState == "No Signal")
                yield break;

            // -------------------------------------------------
            // Make sure AudioManager exists
            // -------------------------------------------------

            if (AudioManager.Instance == null)
            {
                Debug.LogWarning(
                    "MeterDisplay: AudioManager.Instance is null."
                );

                yield break;
            }

            // -------------------------------------------------
            // Make sure heartbeat exists
            // -------------------------------------------------

            if (heartbeatSound == null)
                yield break;

            // -------------------------------------------------
            // Play heartbeat through AudioManager
            // -------------------------------------------------

            AudioManager.Instance.PlaySFX(
                heartbeatSound,
                currentHeartbeatVolume
            );

            // -------------------------------------------------
            // Wait for the heartbeat clip to finish
            // -------------------------------------------------

            yield return new WaitForSeconds(
                heartbeatSound.length
            );
        }
    }

    // =========================================================
    // STOP HEARTBEAT
    // =========================================================

    void StopHeartbeat()
    {
        if (heartbeatCoroutine != null)
        {
            StopCoroutine(heartbeatCoroutine);
            heartbeatCoroutine = null;
        }
    }

    // =========================================================
    // DIFFICULTY
    // =========================================================

    public void SetDifficulty(float difficulty)
    {
        noSignalDistance *= difficulty;
        weakSignalDistance *= difficulty;
        strongSignalDistance *= difficulty;
        heartFoundDistance *= difficulty;
    }

    // =========================================================
    // PUBLIC STOP METHOD
    // =========================================================

    public void StopHeartbeatPublic()
    {
        StopHeartbeat();
    }
}