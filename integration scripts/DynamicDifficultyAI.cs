using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Dynamic Difficulty Adjustment AI
/// Analyzes player performance and adapts puzzle difficulty in real-time
/// Uses machine learning-inspired algorithms for personalized challenge
/// </summary>
public class DynamicDifficultyAI : MonoBehaviour
{
    [Header("Player Performance Tracking")]
    public int totalPuzzlesAttempted = 0;
    public int totalPuzzlesCompleted = 0;
    public float averageCompletionTime = 0f;
    public int consecutiveFailures = 0;
    public int consecutiveSuccesses = 0;

    [Header("AI Settings")]
    [Tooltip("How quickly AI adapts (0-1)")]
    [Range(0f, 1f)]
    public float adaptationRate = 0.3f;
    
    [Tooltip("Target success rate (0-1)")]
    [Range(0f, 1f)]
    public float targetSuccessRate = 0.7f;
    
    [Tooltip("Minimum difficulty")]
    public int minDifficulty = 10;
    
    [Tooltip("Maximum difficulty")]
    public int maxDifficulty = 100;

    [Header("Performance Metrics")]
    public float currentSkillRating = 50f;
    public float confidenceLevel = 0.5f;
    public DifficultyTrend trend = DifficultyTrend.Stable;

    public enum DifficultyTrend
    {
        Decreasing,
        Stable,
        Increasing
    }

    // Performance history
    private List<PerformanceData> performanceHistory = new List<PerformanceData>();
    private const int MAX_HISTORY_SIZE = 50;

    [System.Serializable]
    public class PerformanceData
    {
        public float timestamp;
        public bool success;
        public float completionTime;
        public int puzzleDifficulty;
        public int attemptsNeeded;
        public float skillRatingAtTime;
    }

    void Start()
    {
        // Load saved performance data if available
        LoadPerformanceData();
    }

    /// <summary>
    /// Record puzzle attempt result
    /// </summary>
    public void RecordPuzzleAttempt(bool success, float completionTime, int puzzleDifficulty, int attempts = 1)
    {
        totalPuzzlesAttempted++;
        
        if (success)
        {
            totalPuzzlesCompleted++;
            consecutiveSuccesses++;
            consecutiveFailures = 0;
            
            // Update average completion time
            averageCompletionTime = (averageCompletionTime * (totalPuzzlesCompleted - 1) + completionTime) / totalPuzzlesCompleted;
        }
        else
        {
            consecutiveFailures++;
            consecutiveSuccesses = 0;
        }

        // Store performance data
        PerformanceData data = new PerformanceData
        {
            timestamp = Time.time,
            success = success,
            completionTime = completionTime,
            puzzleDifficulty = puzzleDifficulty,
            attemptsNeeded = attempts,
            skillRatingAtTime = currentSkillRating
        };

        performanceHistory.Add(data);

        // Limit history size
        if (performanceHistory.Count > MAX_HISTORY_SIZE)
        {
            performanceHistory.RemoveAt(0);
        }

        // Update skill rating
        UpdateSkillRating(success, completionTime, puzzleDifficulty);

        // Analyze trend
        AnalyzeTrend();

        Debug.Log("📊 Performance Recorded: " + (success ? "✅ Success" : "❌ Failure"));
        Debug.Log("Skill Rating: " + currentSkillRating.ToString("F1") + " | Confidence: " + confidenceLevel.ToString("F2"));
    }

    /// <summary>
    /// Update player skill rating using ELO-like algorithm
    /// </summary>
    private void UpdateSkillRating(bool success, float completionTime, int puzzleDifficulty)
    {
        // Calculate expected success probability
        float expectedSuccess = CalculateExpectedSuccess(currentSkillRating, puzzleDifficulty);

        // Calculate actual result (1 for success, 0 for failure)
        float actualResult = success ? 1f : 0f;

        // Time bonus/penalty (faster = better)
        float timeModifier = 1f;
        if (success && averageCompletionTime > 0)
        {
            timeModifier = Mathf.Clamp(averageCompletionTime / completionTime, 0.5f, 1.5f);
        }

        // Update skill rating
        float K = 32f * adaptationRate * timeModifier; // K-factor
        float ratingChange = K * (actualResult - expectedSuccess);
        
        currentSkillRating += ratingChange;
        currentSkillRating = Mathf.Clamp(currentSkillRating, minDifficulty, maxDifficulty);

        // Update confidence level based on consistency
        UpdateConfidence();

        Debug.Log("Skill Rating Change: " + ratingChange.ToString("F2") + " (Expected: " + expectedSuccess.ToString("F2") + ")");
    }

    /// <summary>
    /// Calculate expected success probability
    /// </summary>
    private float CalculateExpectedSuccess(float skillRating, int puzzleDifficulty)
    {
        // Logistic function for probability
        float diff = skillRating - puzzleDifficulty;
        float probability = 1f / (1f + Mathf.Exp(-diff / 20f));
        
        return probability;
    }

    /// <summary>
    /// Update confidence level based on performance consistency
    /// </summary>
    private void UpdateConfidence()
    {
        if (performanceHistory.Count < 5)
        {
            confidenceLevel = 0.3f;
            return;
        }

        // Calculate variance in recent performance
        var recentPerformance = performanceHistory.TakeLast(10).ToList();
        float successRate = recentPerformance.Count(p => p.success) / (float)recentPerformance.Count;
        
        // Confidence increases when success rate is near target
        float deviation = Mathf.Abs(successRate - targetSuccessRate);
        confidenceLevel = 1f - (deviation * 2f);
        confidenceLevel = Mathf.Clamp01(confidenceLevel);
    }

    /// <summary>
    /// Analyze performance trend
    /// </summary>
    private void AnalyzeTrend()
    {
        if (performanceHistory.Count < 10)
        {
            trend = DifficultyTrend.Stable;
            return;
        }

        // Compare recent skill ratings
        var recent = performanceHistory.TakeLast(10).Select(p => p.skillRatingAtTime).ToList();
        float recentAvg = recent.Average();
        
        var older = performanceHistory.Skip(performanceHistory.Count - 20).Take(10).Select(p => p.skillRatingAtTime).ToList();
        float olderAvg = older.Count > 0 ? older.Average() : recentAvg;

        float difference = recentAvg - olderAvg;

        if (difference > 5f)
            trend = DifficultyTrend.Increasing;
        else if (difference < -5f)
            trend = DifficultyTrend.Decreasing;
        else
            trend = DifficultyTrend.Stable;
    }

    /// <summary>
    /// Get recommended difficulty for next puzzle
    /// </summary>
    public int GetRecommendedDifficulty()
    {
        int baseDifficulty = Mathf.RoundToInt(currentSkillRating);

        // Adjust based on consecutive failures/successes
        if (consecutiveFailures >= 3)
        {
            // Make it easier
            baseDifficulty = Mathf.RoundToInt(baseDifficulty * 0.8f);
            Debug.Log("⬇️ Reducing difficulty due to consecutive failures");
        }
        else if (consecutiveSuccesses >= 5)
        {
            // Make it harder
            baseDifficulty = Mathf.RoundToInt(baseDifficulty * 1.2f);
            Debug.Log("⬆️ Increasing difficulty due to consecutive successes");
        }

        // Adjust based on confidence
        if (confidenceLevel < 0.4f)
        {
            // Lower difficulty when uncertain
            baseDifficulty = Mathf.RoundToInt(baseDifficulty * 0.9f);
        }

        // Add slight randomness for variety
        int randomVariation = Random.Range(-5, 5);
        baseDifficulty += randomVariation;

        return Mathf.Clamp(baseDifficulty, minDifficulty, maxDifficulty);
    }

    /// <summary>
    /// Get player performance summary
    /// </summary>
    public string GetPerformanceSummary()
    {
        float successRate = totalPuzzlesAttempted > 0 ? 
            (totalPuzzlesCompleted / (float)totalPuzzlesAttempted) * 100f : 0f;

        string summary = "=== Player Performance Analysis ===\n";
        summary += "Skill Rating: " + currentSkillRating.ToString("F1") + "\n";
        summary += "Confidence: " + (confidenceLevel * 100f).ToString("F0") + "%\n";
        summary += "Success Rate: " + successRate.ToString("F1") + "%\n";
        summary += "Puzzles Completed: " + totalPuzzlesCompleted + "/" + totalPuzzlesAttempted + "\n";
        summary += "Avg Completion Time: " + averageCompletionTime.ToString("F1") + "s\n";
        summary += "Trend: " + trend.ToString() + "\n";
        summary += "Consecutive Successes: " + consecutiveSuccesses + "\n";
        summary += "Consecutive Failures: " + consecutiveFailures + "\n";
        summary += "===================================";

        return summary;
    }

    /// <summary>
    /// Predict success probability for given difficulty
    /// </summary>
    public float PredictSuccessProbability(int puzzleDifficulty)
    {
        return CalculateExpectedSuccess(currentSkillRating, puzzleDifficulty);
    }

    /// <summary>
    /// Get adaptive hints based on performance
    /// </summary>
    public string GetAdaptiveHint()
    {
        if (consecutiveFailures >= 2)
        {
            return "💡 Hint: Try using the Tromoxite Warp to slow down time and plan your moves carefully.";
        }
        else if (consecutiveFailures >= 4)
        {
            return "💡 Hint: Look for alternative paths. Not all routes are obvious in the temporal rifts.";
        }
        else if (consecutiveSuccesses >= 5)
        {
            return "🔥 You're on fire! Ready for a greater challenge?";
        }
        else
        {
            return "Keep going! You're making progress through the ChronoRift.";
        }
    }

    /// <summary>
    /// Reset performance data
    /// </summary>
    public void ResetPerformanceData()
    {
        totalPuzzlesAttempted = 0;
        totalPuzzlesCompleted = 0;
        averageCompletionTime = 0f;
        consecutiveFailures = 0;
        consecutiveSuccesses = 0;
        currentSkillRating = 50f;
        confidenceLevel = 0.5f;
        performanceHistory.Clear();
        
        Debug.Log("Performance data reset");
    }

    /// <summary>
    /// Save performance data (placeholder)
    /// </summary>
    private void SavePerformanceData()
    {
        // In production, save to PlayerPrefs or blockchain
        PlayerPrefs.SetFloat("SkillRating", currentSkillRating);
        PlayerPrefs.SetInt("TotalAttempts", totalPuzzlesAttempted);
        PlayerPrefs.SetInt("TotalCompleted", totalPuzzlesCompleted);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Load performance data (placeholder)
    /// </summary>
    private void LoadPerformanceData()
    {
        if (PlayerPrefs.HasKey("SkillRating"))
        {
            currentSkillRating = PlayerPrefs.GetFloat("SkillRating");
            totalPuzzlesAttempted = PlayerPrefs.GetInt("TotalAttempts");
            totalPuzzlesCompleted = PlayerPrefs.GetInt("TotalCompleted");
            
            Debug.Log("Loaded performance data: Skill Rating = " + currentSkillRating);
        }
    }

    void OnApplicationQuit()
    {
        SavePerformanceData();
    }
}
