using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Procedural Level Generator
/// Generates complete levels with platforms, hazards, and collectibles
/// Uses AI-driven algorithms for balanced, playable level design
/// </summary>
public class ProceduralLevelGenerator : MonoBehaviour
{
    [Header("Level Settings")]
    public int levelSeed = 0;
    public int levelLength = 100; // meters
    public int levelWidth = 20; // meters
    public int levelHeight = 30; // meters

    [Header("Generation Parameters")]
    [Range(0f, 1f)]
    public float platformDensity = 0.3f;
    
    [Range(0f, 1f)]
    public float hazardDensity = 0.15f;
    
    [Range(0f, 1f)]
    public float collectibleDensity = 0.1f;

    [Header("AI References")]
    public AIPuzzleGenerator puzzleGenerator;
    public DynamicDifficultyAI difficultyAI;

    // Level data
    private LevelData currentLevel;
    private System.Random rng;

    public class LevelData
    {
        public string levelId;
        public int seed;
        public Vector3 startPosition;
        public Vector3 endPosition;
        public List<Platform> platforms;
        public List<Hazard> hazards;
        public List<Collectible> collectibles;
        public List<Checkpoint> checkpoints;
        public AIPuzzleGenerator.WorldType worldType;
        public int difficulty;
        public string levelNarrative;
    }

    [System.Serializable]
    public class Platform
    {
        public Vector3 position;
        public Vector3 size;
        public PlatformType type;
        public float movementSpeed;
        public Vector3 movementPath;
    }

    public enum PlatformType
    {
        Static,
        Moving,
        Breakable,
        Disappearing,
        Rotating
    }

    [System.Serializable]
    public class Hazard
    {
        public Vector3 position;
        public HazardType type;
        public float damageAmount;
        public float effectRadius;
    }

    public enum HazardType
    {
        Lava,
        Void,
        Spikes,
        MovingObstacle,
        TemporalAnomaly
    }

    [System.Serializable]
    public class Collectible
    {
        public Vector3 position;
        public CollectibleType type;
        public int value;
    }

    public enum CollectibleType
    {
        BristoniteQuartz,
        BristoniteSolar,
        BristoniteAether,
        BristoniteNova,
        BristonitePrime,
        HealthOrb,
        TimeBonus
    }

    [System.Serializable]
    public class Checkpoint
    {
        public Vector3 position;
        public int checkpointNumber;
    }

    void Start()
    {
        puzzleGenerator = FindObjectOfType<AIPuzzleGenerator>();
        difficultyAI = FindObjectOfType<DynamicDifficultyAI>();

        // Generate initial level
        GenerateLevel();
    }

    /// <summary>
    /// Generate a complete procedural level
    /// </summary>
    public LevelData GenerateLevel()
    {
        Debug.Log("🏗️ Generating Procedural Level...");

        // Initialize RNG
        if (levelSeed == 0)
            levelSeed = System.DateTime.Now.Millisecond;
        
        rng = new System.Random(levelSeed);

        // Get difficulty from AI
        int targetDifficulty = difficultyAI != null ? 
            difficultyAI.GetRecommendedDifficulty() : 50;

        // Initialize level data
        currentLevel = new LevelData
        {
            levelId = System.Guid.NewGuid().ToString(),
            seed = levelSeed,
            startPosition = Vector3.zero,
            endPosition = new Vector3(levelLength, 0, 0),
            platforms = new List<Platform>(),
            hazards = new List<Hazard>(),
            collectibles = new List<Collectible>(),
            checkpoints = new List<Checkpoint>(),
            worldType = GetRandomWorldType(),
            difficulty = targetDifficulty
        };

        // Generate level components
        GeneratePlatforms();
        GenerateHazards();
        GenerateCollectibles();
        GenerateCheckpoints();
        GenerateNarrative();

        Debug.Log("✅ Level Generated: " + currentLevel.levelId);
        Debug.Log("Platforms: " + currentLevel.platforms.Count);
        Debug.Log("Hazards: " + currentLevel.hazards.Count);
        Debug.Log("Collectibles: " + currentLevel.collectibles.Count);

        return currentLevel;
    }

    /// <summary>
    /// Generate platforms for traversal
    /// </summary>
    private void GeneratePlatforms()
    {
        Vector3 currentPos = currentLevel.startPosition;
        int platformCount = Mathf.RoundToInt(levelLength * platformDensity);

        for (int i = 0; i < platformCount; i++)
        {
            Platform platform = new Platform
            {
                position = currentPos,
                size = GetRandomPlatformSize(),
                type = GetRandomPlatformType(),
                movementSpeed = 0f,
                movementPath = Vector3.zero
            };

            // Configure moving platforms
            if (platform.type == PlatformType.Moving)
            {
                platform.movementSpeed = Random.Range(1f, 3f);
                platform.movementPath = new Vector3(
                    Random.Range(-5f, 5f),
                    Random.Range(-2f, 2f),
                    0
                );
            }

            currentLevel.platforms.Add(platform);

            // Move to next platform position
            currentPos += new Vector3(
                Random.Range(3f, 8f),
                Random.Range(-3f, 3f),
                Random.Range(-2f, 2f)
            );

            // Keep within bounds
            currentPos.y = Mathf.Clamp(currentPos.y, 0, levelHeight);
            currentPos.z = Mathf.Clamp(currentPos.z, -levelWidth / 2, levelWidth / 2);
        }

        // Ensure path to end
        EnsurePathToEnd();
    }

    private Vector3 GetRandomPlatformSize()
    {
        float width = Random.Range(2f, 5f);
        float height = Random.Range(0.5f, 1f);
        float depth = Random.Range(2f, 4f);
        
        return new Vector3(width, height, depth);
    }

    private PlatformType GetRandomPlatformType()
    {
        // Weight platform types based on difficulty
        float rand = (float)rng.NextDouble();
        
        if (currentLevel.difficulty < 30)
        {
            return rand < 0.8f ? PlatformType.Static : PlatformType.Moving;
        }
        else if (currentLevel.difficulty < 60)
        {
            if (rand < 0.5f) return PlatformType.Static;
            else if (rand < 0.8f) return PlatformType.Moving;
            else return PlatformType.Breakable;
        }
        else
        {
            if (rand < 0.3f) return PlatformType.Static;
            else if (rand < 0.5f) return PlatformType.Moving;
            else if (rand < 0.7f) return PlatformType.Breakable;
            else if (rand < 0.9f) return PlatformType.Disappearing;
            else return PlatformType.Rotating;
        }
    }

    private void EnsurePathToEnd()
    {
        // Add platforms to guarantee reachable end
        Vector3 lastPlatformPos = currentLevel.platforms[currentLevel.platforms.Count - 1].position;
        
        while (Vector3.Distance(lastPlatformPos, currentLevel.endPosition) > 10f)
        {
            lastPlatformPos += new Vector3(5f, 0, 0);
            
            Platform bridgePlatform = new Platform
            {
                position = lastPlatformPos,
                size = new Vector3(3f, 0.5f, 3f),
                type = PlatformType.Static,
                movementSpeed = 0f,
                movementPath = Vector3.zero
            };
            
            currentLevel.platforms.Add(bridgePlatform);
        }
    }

    /// <summary>
    /// Generate hazards
    /// </summary>
    private void GenerateHazards()
    {
        int hazardCount = Mathf.RoundToInt(levelLength * hazardDensity);

        for (int i = 0; i < hazardCount; i++)
        {
            Vector3 hazardPos = new Vector3(
                Random.Range(5f, levelLength - 5f),
                Random.Range(0f, levelHeight * 0.5f),
                Random.Range(-levelWidth / 2, levelWidth / 2)
            );

            Hazard hazard = new Hazard
            {
                position = hazardPos,
                type = GetRandomHazardType(),
                damageAmount = Random.Range(10f, 30f),
                effectRadius = Random.Range(2f, 5f)
            };

            currentLevel.hazards.Add(hazard);
        }
    }

    private HazardType GetRandomHazardType()
    {
        switch (currentLevel.worldType)
        {
            case AIPuzzleGenerator.WorldType.Desert:
                return HazardType.Spikes;
            
            case AIPuzzleGenerator.WorldType.Lava:
                return Random.value < 0.7f ? HazardType.Lava : HazardType.MovingObstacle;
            
            case AIPuzzleGenerator.WorldType.Void:
                return Random.value < 0.6f ? HazardType.Void : HazardType.TemporalAnomaly;
            
            case AIPuzzleGenerator.WorldType.Nebula:
                return (HazardType)Random.Range(0, System.Enum.GetValues(typeof(HazardType)).Length);
            
            default:
                return HazardType.Spikes;
        }
    }

    /// <summary>
    /// Generate collectibles
    /// </summary>
    private void GenerateCollectibles()
    {
        int collectibleCount = Mathf.RoundToInt(levelLength * collectibleDensity);

        for (int i = 0; i < collectibleCount; i++)
        {
            // Place near platforms
            if (currentLevel.platforms.Count > 0)
            {
                int platformIndex = Random.Range(0, currentLevel.platforms.Count);
                Vector3 platformPos = currentLevel.platforms[platformIndex].position;
                
                Vector3 collectiblePos = platformPos + new Vector3(
                    Random.Range(-2f, 2f),
                    Random.Range(2f, 4f),
                    Random.Range(-1f, 1f)
                );

                Collectible collectible = new Collectible
                {
                    position = collectiblePos,
                    type = GetRandomCollectibleType(),
                    value = GetCollectibleValue()
                };

                currentLevel.collectibles.Add(collectible);
            }
        }
    }

    private CollectibleType GetRandomCollectibleType()
    {
        float rand = Random.value;
        
        if (rand < 0.4f) return CollectibleType.BristoniteQuartz;
        else if (rand < 0.65f) return CollectibleType.BristoniteSolar;
        else if (rand < 0.8f) return CollectibleType.BristoniteAether;
        else if (rand < 0.9f) return CollectibleType.BristoniteNova;
        else if (rand < 0.95f) return CollectibleType.BristonitePrime;
        else if (rand < 0.98f) return CollectibleType.HealthOrb;
        else return CollectibleType.TimeBonus;
    }

    private int GetCollectibleValue()
    {
        return Random.Range(10, 100);
    }

    /// <summary>
    /// Generate checkpoints
    /// </summary>
    private void GenerateCheckpoints()
    {
        int checkpointCount = Mathf.Max(1, levelLength / 25); // Every 25 meters

        for (int i = 0; i < checkpointCount; i++)
        {
            float progress = (i + 1) / (float)(checkpointCount + 1);
            Vector3 checkpointPos = Vector3.Lerp(currentLevel.startPosition, currentLevel.endPosition, progress);
            
            // Find nearest platform
            float nearestDist = float.MaxValue;
            Vector3 nearestPlatformPos = checkpointPos;
            
            foreach (var platform in currentLevel.platforms)
            {
                float dist = Vector3.Distance(platform.position, checkpointPos);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearestPlatformPos = platform.position;
                }
            }

            Checkpoint checkpoint = new Checkpoint
            {
                position = nearestPlatformPos + Vector3.up * 2f,
                checkpointNumber = i + 1
            };

            currentLevel.checkpoints.Add(checkpoint);
        }
    }

    /// <summary>
    /// Generate level narrative
    /// </summary>
    private void GenerateNarrative()
    {
        string[] narratives = new string[]
        {
            "Sector {0}: The temporal fabric weakens. Collect Bristonite shards to stabilize the rift.",
            "ChronoZone {0}: Reality bends in this region. Navigate carefully through the anomalies.",
            "Epoch {0}: The Shattered Timeline reveals its secrets. Will you claim them?",
            "Rift {0}: Temporal storms rage ahead. Only the skilled survive the ChronoRift."
        };

        int index = rng.Next(narratives.Length);
        currentLevel.levelNarrative = string.Format(narratives[index], currentLevel.levelId.Substring(0, 4));
    }

    private AIPuzzleGenerator.WorldType GetRandomWorldType()
    {
        // Weight worlds based on difficulty
        if (currentLevel.difficulty < 25)
            return AIPuzzleGenerator.WorldType.Desert;
        else if (currentLevel.difficulty < 50)
            return Random.value < 0.5f ? AIPuzzleGenerator.WorldType.Desert : AIPuzzleGenerator.WorldType.Lava;
        else if (currentLevel.difficulty < 75)
            return (AIPuzzleGenerator.WorldType)Random.Range(0, 3);
        else
            return (AIPuzzleGenerator.WorldType)Random.Range(0, 4);
    }

    /// <summary>
    /// Get current level data
    /// </summary>
    public LevelData GetCurrentLevel()
    {
        return currentLevel;
    }

    /// <summary>
    /// Regenerate level with new seed
    /// </summary>
    public void RegenerateLevel(int newSeed = 0)
    {
        levelSeed = newSeed == 0 ? System.DateTime.Now.Millisecond : newSeed;
        GenerateLevel();
    }
}
