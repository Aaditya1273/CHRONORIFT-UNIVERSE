using UnityEngine;

/// <summary>
/// AI Game Director
/// Coordinates all AI systems for optimal player experience
/// Manages puzzle generation, difficulty adjustment, and level creation
/// </summary>
public class AIGameDirector : MonoBehaviour
{
    [Header("AI Systems")]
    public AIPuzzleGenerator puzzleGenerator;
    public DynamicDifficultyAI difficultyAI;
    public ProceduralLevelGenerator levelGenerator;

    [Header("Game State")]
    public bool isAIEnabled = true;
    public int currentWorldIndex = 0;
    public int currentLevelNumber = 1;

    [Header("Statistics")]
    public int totalLevelsGenerated = 0;
    public int totalPuzzlesGenerated = 0;
    public float totalPlayTime = 0f;

    // Singleton
    private static AIGameDirector _instance;
    public static AIGameDirector Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<AIGameDirector>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("AIGameDirector");
                    _instance = go.AddComponent<AIGameDirector>();
                }
            }
            return _instance;
        }
    }

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Find or create AI systems
        InitializeAISystems();
    }

    void Update()
    {
        if (isAIEnabled)
        {
            totalPlayTime += Time.deltaTime;
        }
    }

    /// <summary>
    /// Initialize all AI systems
    /// </summary>
    private void InitializeAISystems()
    {
        if (puzzleGenerator == null)
        {
            puzzleGenerator = FindObjectOfType<AIPuzzleGenerator>();
            if (puzzleGenerator == null)
            {
                puzzleGenerator = gameObject.AddComponent<AIPuzzleGenerator>();
            }
        }

        if (difficultyAI == null)
        {
            difficultyAI = FindObjectOfType<DynamicDifficultyAI>();
            if (difficultyAI == null)
            {
                difficultyAI = gameObject.AddComponent<DynamicDifficultyAI>();
            }
        }

        if (levelGenerator == null)
        {
            levelGenerator = FindObjectOfType<ProceduralLevelGenerator>();
            if (levelGenerator == null)
            {
                levelGenerator = gameObject.AddComponent<ProceduralLevelGenerator>();
            }
        }

        Debug.Log("🤖 AI Game Director Initialized");
    }

    /// <summary>
    /// Generate next level based on player performance
    /// </summary>
    public void GenerateNextLevel()
    {
        if (!isAIEnabled)
        {
            Debug.LogWarning("AI is disabled");
            return;
        }

        Debug.Log("🎮 AI Director: Generating Next Level...");

        // Get recommended difficulty
        int difficulty = difficultyAI.GetRecommendedDifficulty();
        
        // Select world type
        AIPuzzleGenerator.WorldType world = SelectWorldType();

        // Update puzzle generator settings
        puzzleGenerator.currentWorld = world;
        puzzleGenerator.playerSkillLevel = difficulty;

        // Generate puzzle
        var puzzle = puzzleGenerator.GeneratePuzzle();
        totalPuzzlesGenerated++;

        // Generate level
        var level = levelGenerator.GenerateLevel();
        totalLevelsGenerated++;

        currentLevelNumber++;

        Debug.Log("✅ Level " + currentLevelNumber + " Generated");
        Debug.Log("World: " + world + " | Difficulty: " + difficulty);
        Debug.Log("Narrative: " + puzzle.aiNarrative);
    }

    /// <summary>
    /// Select world type based on progression
    /// </summary>
    private AIPuzzleGenerator.WorldType SelectWorldType()
    {
        // Progress through worlds based on level number
        if (currentLevelNumber <= 4)
            return AIPuzzleGenerator.WorldType.Desert;
        else if (currentLevelNumber <= 8)
            return AIPuzzleGenerator.WorldType.Lava;
        else if (currentLevelNumber <= 12)
            return AIPuzzleGenerator.WorldType.Void;
        else
            return AIPuzzleGenerator.WorldType.Nebula;
    }

    /// <summary>
    /// Record level completion
    /// </summary>
    public void OnLevelCompleted(bool success, float completionTime, int attempts = 1)
    {
        if (!isAIEnabled) return;

        Debug.Log("📊 AI Director: Recording Level Completion");

        // Get current difficulty
        int currentDifficulty = puzzleGenerator.playerSkillLevel;

        // Record in difficulty AI
        difficultyAI.RecordPuzzleAttempt(success, completionTime, currentDifficulty, attempts);

        // Log performance summary
        Debug.Log(difficultyAI.GetPerformanceSummary());

        // Provide adaptive hint if struggling
        if (difficultyAI.consecutiveFailures >= 2)
        {
            string hint = difficultyAI.GetAdaptiveHint();
            ShowHint(hint);
        }
    }

    /// <summary>
    /// Get AI-generated hint
    /// </summary>
    public string GetCurrentHint()
    {
        return difficultyAI.GetAdaptiveHint();
    }

    /// <summary>
    /// Show hint to player
    /// </summary>
    private void ShowHint(string hint)
    {
        Debug.Log("💡 AI Hint: " + hint);
        // In production, show UI notification
    }

    /// <summary>
    /// Get comprehensive AI report
    /// </summary>
    public string GetAIReport()
    {
        string report = "=== AI Game Director Report ===\n";
        report += "AI Enabled: " + isAIEnabled + "\n";
        report += "Current Level: " + currentLevelNumber + "\n";
        report += "Total Levels Generated: " + totalLevelsGenerated + "\n";
        report += "Total Puzzles Generated: " + totalPuzzlesGenerated + "\n";
        report += "Total Play Time: " + (totalPlayTime / 60f).ToString("F1") + " minutes\n";
        report += "\n";
        report += difficultyAI.GetPerformanceSummary();
        report += "\n";
        report += "Next Recommended Difficulty: " + difficultyAI.GetRecommendedDifficulty() + "\n";
        report += "Success Probability: " + (difficultyAI.PredictSuccessProbability(difficultyAI.GetRecommendedDifficulty()) * 100f).ToString("F1") + "%\n";
        report += "===============================";

        return report;
    }

    /// <summary>
    /// Toggle AI systems
    /// </summary>
    public void SetAIEnabled(bool enabled)
    {
        isAIEnabled = enabled;
        Debug.Log("AI Systems " + (enabled ? "Enabled" : "Disabled"));
    }

    /// <summary>
    /// Reset all AI data
    /// </summary>
    public void ResetAI()
    {
        if (difficultyAI != null)
        {
            difficultyAI.ResetPerformanceData();
        }

        currentLevelNumber = 1;
        totalLevelsGenerated = 0;
        totalPuzzlesGenerated = 0;
        totalPlayTime = 0f;

        Debug.Log("AI Systems Reset");
    }

    /// <summary>
    /// Get current puzzle data
    /// </summary>
    public AIPuzzleGenerator.PuzzleData GetCurrentPuzzle()
    {
        return puzzleGenerator != null ? puzzleGenerator.GetCurrentPuzzle() : null;
    }

    /// <summary>
    /// Get current level data
    /// </summary>
    public ProceduralLevelGenerator.LevelData GetCurrentLevel()
    {
        return levelGenerator != null ? levelGenerator.GetCurrentLevel() : null;
    }
}
