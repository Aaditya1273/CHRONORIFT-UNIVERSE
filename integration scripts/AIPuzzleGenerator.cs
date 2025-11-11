using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// AI-Powered Puzzle Generator
/// Generates unique space-time puzzles based on player skill level
/// Uses procedural generation with AI-driven difficulty scaling
/// </summary>
public class AIPuzzleGenerator : MonoBehaviour
{
    [Header("Puzzle Settings")]
    [Tooltip("Current world (Desert, Lava, Void, Nebula)")]
    public WorldType currentWorld = WorldType.Desert;
    
    [Tooltip("Player skill level (0-100)")]
    [Range(0, 100)]
    public int playerSkillLevel = 50;

    [Header("AI Generation Parameters")]
    [Tooltip("Complexity multiplier based on skill")]
    public AnimationCurve complexityCurve;
    
    [Tooltip("Random seed for reproducibility")]
    public int randomSeed = 0;

    [Header("Puzzle Elements")]
    public List<PuzzleElement> availableElements;
    public List<Anomaly> availableAnomalies;

    // Puzzle generation state
    private System.Random rng;
    private PuzzleData currentPuzzle;

    public enum WorldType
    {
        Desert,      // Basic mechanics
        Lava,        // Moving platforms, lava hazards
        Void,        // Void regions, disappearing platforms
        Nebula       // Temporal loops, spatial rifts
    }

    [System.Serializable]
    public class PuzzleElement
    {
        public string name;
        public ElementType type;
        public int complexityScore;
        public Vector2 sizeRange;
    }

    public enum ElementType
    {
        Platform,
        MovingPlatform,
        BreakablePlatform,
        TeleportPad,
        GravityZone,
        TemporalRift,
        VoidRegion,
        Checkpoint
    }

    [System.Serializable]
    public class Anomaly
    {
        public string name;
        public AnomalyType type;
        public int difficultyLevel;
        public float effectStrength;
    }

    public enum AnomalyType
    {
        AntiGravity,
        TimeSlowdown,
        TimeSpeedup,
        ReverseControls,
        InvisiblePlatforms,
        MovingVoids,
        TemporalEcho,
        SpatialDistortion
    }

    public class PuzzleData
    {
        public string puzzleId;
        public WorldType world;
        public int difficulty;
        public List<PuzzleElement> elements;
        public List<Anomaly> activeAnomalies;
        public Vector3 startPosition;
        public Vector3 goalPosition;
        public int estimatedTime;
        public string aiNarrative;
    }

    void Start()
    {
        // Initialize RNG
        if (randomSeed == 0)
            randomSeed = System.DateTime.Now.Millisecond;
        
        rng = new System.Random(randomSeed);

        // Initialize complexity curve if not set
        if (complexityCurve == null || complexityCurve.length == 0)
        {
            complexityCurve = AnimationCurve.EaseInOut(0, 0.5f, 100, 2.5f);
        }

        // Generate initial puzzle
        GeneratePuzzle();
    }

    /// <summary>
    /// Generate a new AI-driven puzzle
    /// </summary>
    public PuzzleData GeneratePuzzle()
    {
        Debug.Log("🧩 Generating AI Puzzle...");
        Debug.Log("World: " + currentWorld + " | Skill Level: " + playerSkillLevel);

        currentPuzzle = new PuzzleData
        {
            puzzleId = System.Guid.NewGuid().ToString(),
            world = currentWorld,
            difficulty = CalculateDifficulty(),
            elements = new List<PuzzleElement>(),
            activeAnomalies = new List<Anomaly>(),
            startPosition = Vector3.zero,
            goalPosition = Vector3.zero
        };

        // Generate puzzle layout
        GeneratePuzzleLayout();

        // Add anomalies based on difficulty
        GenerateAnomalies();

        // Generate AI narrative
        GenerateNarrative();

        // Calculate estimated completion time
        currentPuzzle.estimatedTime = CalculateEstimatedTime();

        Debug.Log("✅ Puzzle Generated: " + currentPuzzle.puzzleId);
        Debug.Log("Difficulty: " + currentPuzzle.difficulty + " | Elements: " + currentPuzzle.elements.Count);
        Debug.Log("Anomalies: " + currentPuzzle.activeAnomalies.Count);

        return currentPuzzle;
    }

    /// <summary>
    /// Calculate puzzle difficulty based on player skill
    /// </summary>
    private int CalculateDifficulty()
    {
        // Use complexity curve to scale difficulty
        float complexity = complexityCurve.Evaluate(playerSkillLevel);
        
        // Add world-specific difficulty modifier
        float worldModifier = GetWorldDifficultyModifier();
        
        // Calculate final difficulty (1-100)
        int difficulty = Mathf.RoundToInt(playerSkillLevel * complexity * worldModifier);
        
        return Mathf.Clamp(difficulty, 1, 100);
    }

    private float GetWorldDifficultyModifier()
    {
        switch (currentWorld)
        {
            case WorldType.Desert: return 1.0f;
            case WorldType.Lava: return 1.3f;
            case WorldType.Void: return 1.6f;
            case WorldType.Nebula: return 2.0f;
            default: return 1.0f;
        }
    }

    /// <summary>
    /// Generate puzzle layout with platforms and obstacles
    /// </summary>
    private void GeneratePuzzleLayout()
    {
        int elementCount = Mathf.RoundToInt(5 + (currentPuzzle.difficulty / 10f));
        
        // Set start position
        currentPuzzle.startPosition = new Vector3(0, 0, 0);
        
        // Generate path to goal
        Vector3 currentPos = currentPuzzle.startPosition;
        
        for (int i = 0; i < elementCount; i++)
        {
            PuzzleElement element = SelectElement();
            
            if (element != null)
            {
                // Clone element with random position offset
                PuzzleElement newElement = new PuzzleElement
                {
                    name = element.name + "_" + i,
                    type = element.type,
                    complexityScore = element.complexityScore,
                    sizeRange = element.sizeRange
                };
                
                currentPuzzle.elements.Add(newElement);
                
                // Update position for next element
                currentPos += new Vector3(
                    rng.Next(-5, 5),
                    rng.Next(0, 3),
                    rng.Next(2, 6)
                );
            }
        }
        
        // Set goal position
        currentPuzzle.goalPosition = currentPos + new Vector3(0, 2, 5);
    }

    /// <summary>
    /// Select appropriate puzzle element based on difficulty
    /// </summary>
    private PuzzleElement SelectElement()
    {
        if (availableElements == null || availableElements.Count == 0)
        {
            // Create default elements if none exist
            InitializeDefaultElements();
        }

        // Filter elements by difficulty
        var suitableElements = availableElements.Where(e => 
            e.complexityScore <= currentPuzzle.difficulty + 10 &&
            e.complexityScore >= currentPuzzle.difficulty - 10
        ).ToList();

        if (suitableElements.Count == 0)
            suitableElements = availableElements;

        // Select random element
        int index = rng.Next(suitableElements.Count);
        return suitableElements[index];
    }

    /// <summary>
    /// Generate anomalies to increase challenge
    /// </summary>
    private void GenerateAnomalies()
    {
        // Number of anomalies based on difficulty
        int anomalyCount = currentPuzzle.difficulty / 25; // 0-4 anomalies
        
        if (availableAnomalies == null || availableAnomalies.Count == 0)
        {
            InitializeDefaultAnomalies();
        }

        for (int i = 0; i < anomalyCount; i++)
        {
            // Select anomaly appropriate for world and difficulty
            var suitableAnomalies = availableAnomalies.Where(a =>
                a.difficultyLevel <= currentPuzzle.difficulty &&
                IsAnomalySuitableForWorld(a.type)
            ).ToList();

            if (suitableAnomalies.Count > 0)
            {
                int index = rng.Next(suitableAnomalies.Count);
                Anomaly anomaly = suitableAnomalies[index];
                
                // Clone with randomized strength
                Anomaly newAnomaly = new Anomaly
                {
                    name = anomaly.name,
                    type = anomaly.type,
                    difficultyLevel = anomaly.difficultyLevel,
                    effectStrength = anomaly.effectStrength * (0.8f + (float)rng.NextDouble() * 0.4f)
                };
                
                currentPuzzle.activeAnomalies.Add(newAnomaly);
            }
        }
    }

    private bool IsAnomalySuitableForWorld(AnomalyType type)
    {
        switch (currentWorld)
        {
            case WorldType.Desert:
                return type == AnomalyType.AntiGravity || type == AnomalyType.TimeSlowdown;
            
            case WorldType.Lava:
                return type == AnomalyType.AntiGravity || type == AnomalyType.TimeSpeedup || 
                       type == AnomalyType.MovingVoids;
            
            case WorldType.Void:
                return type == AnomalyType.InvisiblePlatforms || type == AnomalyType.MovingVoids ||
                       type == AnomalyType.SpatialDistortion;
            
            case WorldType.Nebula:
                return true; // All anomalies available in Nebula
            
            default:
                return true;
        }
    }

    /// <summary>
    /// Generate AI narrative for the puzzle
    /// </summary>
    private void GenerateNarrative()
    {
        string[] narrativeTemplates = new string[]
        {
            "The temporal fabric fractures before you. Navigate the {0} to restore the timeline.",
            "Chrono-anomalies detected in sector {1}. Your Tromoxite Warp must pierce through {0}.",
            "Reality bends in this {0} region. Only the skilled can reclaim the Bristonite shards.",
            "The Shattered Epoch reveals its secrets through {0}. Will you unravel the mystery?",
            "Temporal storms rage ahead. Master the {0} to claim your destiny."
        };

        string worldDesc = GetWorldDescription();
        int templateIndex = rng.Next(narrativeTemplates.Length);
        
        currentPuzzle.aiNarrative = string.Format(
            narrativeTemplates[templateIndex],
            worldDesc,
            currentPuzzle.puzzleId.Substring(0, 8)
        );
    }

    private string GetWorldDescription()
    {
        switch (currentWorld)
        {
            case WorldType.Desert: return "desert drifts";
            case WorldType.Lava: return "lava labyrinths";
            case WorldType.Void: return "void vortices";
            case WorldType.Nebula: return "nebula knots";
            default: return "unknown realm";
        }
    }

    /// <summary>
    /// Calculate estimated completion time
    /// </summary>
    private int CalculateEstimatedTime()
    {
        // Base time + element complexity + anomaly penalties
        int baseTime = 30; // seconds
        int elementTime = currentPuzzle.elements.Count * 5;
        int anomalyTime = currentPuzzle.activeAnomalies.Count * 10;
        
        return baseTime + elementTime + anomalyTime;
    }

    /// <summary>
    /// Initialize default puzzle elements
    /// </summary>
    private void InitializeDefaultElements()
    {
        availableElements = new List<PuzzleElement>
        {
            new PuzzleElement { name = "Basic Platform", type = ElementType.Platform, complexityScore = 10, sizeRange = new Vector2(2, 4) },
            new PuzzleElement { name = "Moving Platform", type = ElementType.MovingPlatform, complexityScore = 30, sizeRange = new Vector2(2, 3) },
            new PuzzleElement { name = "Breakable Platform", type = ElementType.BreakablePlatform, complexityScore = 40, sizeRange = new Vector2(1.5f, 2.5f) },
            new PuzzleElement { name = "Teleport Pad", type = ElementType.TeleportPad, complexityScore = 50, sizeRange = new Vector2(1, 1) },
            new PuzzleElement { name = "Gravity Zone", type = ElementType.GravityZone, complexityScore = 60, sizeRange = new Vector2(3, 5) },
            new PuzzleElement { name = "Temporal Rift", type = ElementType.TemporalRift, complexityScore = 70, sizeRange = new Vector2(2, 3) },
            new PuzzleElement { name = "Void Region", type = ElementType.VoidRegion, complexityScore = 80, sizeRange = new Vector2(4, 6) },
            new PuzzleElement { name = "Checkpoint", type = ElementType.Checkpoint, complexityScore = 20, sizeRange = new Vector2(1, 1) }
        };
    }

    /// <summary>
    /// Initialize default anomalies
    /// </summary>
    private void InitializeDefaultAnomalies()
    {
        availableAnomalies = new List<Anomaly>
        {
            new Anomaly { name = "Anti-Gravity Field", type = AnomalyType.AntiGravity, difficultyLevel = 20, effectStrength = 0.5f },
            new Anomaly { name = "Time Dilation", type = AnomalyType.TimeSlowdown, difficultyLevel = 30, effectStrength = 0.7f },
            new Anomaly { name = "Temporal Acceleration", type = AnomalyType.TimeSpeedup, difficultyLevel = 40, effectStrength = 1.5f },
            new Anomaly { name = "Control Reversal", type = AnomalyType.ReverseControls, difficultyLevel = 50, effectStrength = 1.0f },
            new Anomaly { name = "Phase Shift", type = AnomalyType.InvisiblePlatforms, difficultyLevel = 60, effectStrength = 0.8f },
            new Anomaly { name = "Void Surge", type = AnomalyType.MovingVoids, difficultyLevel = 70, effectStrength = 1.2f },
            new Anomaly { name = "Temporal Echo", type = AnomalyType.TemporalEcho, difficultyLevel = 80, effectStrength = 1.0f },
            new Anomaly { name = "Spatial Distortion", type = AnomalyType.SpatialDistortion, difficultyLevel = 90, effectStrength = 1.5f }
        };
    }

    /// <summary>
    /// Get current puzzle data
    /// </summary>
    public PuzzleData GetCurrentPuzzle()
    {
        return currentPuzzle;
    }

    /// <summary>
    /// Regenerate puzzle with new parameters
    /// </summary>
    public void RegeneratePuzzle(WorldType world, int skillLevel)
    {
        currentWorld = world;
        playerSkillLevel = skillLevel;
        randomSeed = System.DateTime.Now.Millisecond;
        rng = new System.Random(randomSeed);
        
        GeneratePuzzle();
    }
}
