using UnityEngine;

public class HeartbeatMinigameCondition : MonoBehaviour
{
    [Header("Transition Settings")]
    [Tooltip("The score threshold for a 'Good' result (0-100).")]
    public float threshold = 70f;

    [Header("Outcome Nodes")]
    public GameObject goodNode;
    public GameObject badNode;

    /// <summary>
    /// Returns the boolean result for the transition.
    /// Requirements: Compare numeric values, 70% or higher is Good.
    /// </summary>
    /// <returns>True if final score is >= 70%.</returns>
    public bool GetConditionResult()
    {
        if (ScoreManager.Instance != null)
        {
            // Read from the ScoreManager which stores percentage
            float finalScore = ScoreManager.Instance.score;
            return finalScore >= threshold;
        }
        
        Debug.LogWarning("ScoreManager.Instance is null. Returning false.");
        return false;
    }

    /// <summary>
    /// Executes the transition immediately after the minigame ends.
    /// </summary>
    public void ExecuteTransition()
    {
        bool isGood = GetConditionResult();

        // Perform the transition by activating the correct node
        if (goodNode != null) goodNode.SetActive(isGood);
        if (badNode != null) badNode.SetActive(!isGood);

        // Hide the minigame container (Minigame_1) to complete the transition
        Transform minigameRoot = transform.parent; // GameManager is child of Minigame_1
        if (minigameRoot != null)
        {
            minigameRoot.gameObject.SetActive(false);
        }

        Debug.Log($"Heartbeat Minigame Ended. Final Score: {(ScoreManager.Instance != null ? ScoreManager.Instance.score : 0)}%. Result: {(isGood ? "Good" : "Bad")}");
    }
}