using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

/// <summary>
/// On-Chain Leaderboard UI
/// Fetches and displays player rankings from OneChain blockchain
/// </summary>
public class LeaderboardUI : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject leaderboardPanel;
    public Transform leaderboardContent;
    public GameObject leaderboardEntryPrefab;
    public TMP_Text titleText;
    public Button refreshButton;
    public TMP_Dropdown filterDropdown;

    [Header("Player Highlight")]
    public TMP_Text myRankText;
    public TMP_Text myScoreText;
    public GameObject myEntryHighlight;

    [Header("Settings")]
    public OneChainConfig config;
    public WalletStore walletStore;
    
    [Header("Leaderboard Settings")]
    public int maxEntries = 100;
    public float refreshInterval = 30f; // seconds

    // Leaderboard data
    private List<LeaderboardEntry> leaderboardData = new List<LeaderboardEntry>();
    private LeaderboardFilter currentFilter = LeaderboardFilter.AllTime;

    public enum LeaderboardFilter
    {
        AllTime,
        ThisWeek,
        Today,
        ByWorld
    }

    [System.Serializable]
    public class LeaderboardEntry
    {
        public int rank;
        public string playerAddress;
        public string playerName;
        public int totalScore;
        public int levelsCompleted;
        public int totalWins;
        public ulong cruEarned;
        public float averageTime;
        public long lastPlayed;
    }

    // JavaScript interop
    [DllImport("__Internal")]
    private static extern void QueryOneChain(string gameObjectName, string queryData, string callbackMethod, string errorMethod);

    void Start()
    {
        config = OneChainConfig.Instance;
        walletStore = FindObjectOfType<WalletStore>();

        if (refreshButton != null)
            refreshButton.onClick.AddListener(RefreshLeaderboard);

        if (filterDropdown != null)
        {
            filterDropdown.onValueChanged.AddListener(OnFilterChanged);
        }

        // Auto-refresh leaderboard
        InvokeRepeating(nameof(RefreshLeaderboard), 0f, refreshInterval);
    }

    /// <summary>
    /// Refresh leaderboard data from blockchain
    /// </summary>
    public void RefreshLeaderboard()
    {
        Debug.Log("🏆 Refreshing Leaderboard...");

        if (config == null || string.IsNullOrEmpty(config.leaderboardId))
        {
            Debug.LogError("Leaderboard not configured!");
            return;
        }

        // Query blockchain for leaderboard events
        string query = BuildLeaderboardQuery();

#if UNITY_WEBGL && !UNITY_EDITOR
        QueryOneChain(gameObject.name, query, nameof(OnLeaderboardDataReceived), nameof(OnLeaderboardError));
#else
        Debug.Log("Fetching leaderboard data (Editor Mode)");
        // Simulate data in editor
        GenerateMockLeaderboardData();
        UpdateLeaderboardUI();
#endif
    }

    private string BuildLeaderboardQuery()
    {
        // Query for LevelCompleted events from leaderboard module
        string query = @"{
            ""method"": ""suix_queryEvents"",
            ""params"": [{
                ""MoveEventType"": """ + config.packageId + @"::leaderboard::LevelCompletedEvent""
            }, null, " + maxEntries + @", false]
        }";
        
        return query;
    }

    public void OnLeaderboardDataReceived(string result)
    {
        Debug.Log("Leaderboard data received: " + result);

        try
        {
            // Parse blockchain events
            ParseLeaderboardEvents(result);
            
            // Sort and rank players
            RankPlayers();
            
            // Update UI
            UpdateLeaderboardUI();
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to parse leaderboard data: " + e.Message);
        }
    }

    public void OnLeaderboardError(string error)
    {
        Debug.LogError("Failed to fetch leaderboard: " + error);
    }

    /// <summary>
    /// Parse blockchain events into leaderboard entries
    /// </summary>
    private void ParseLeaderboardEvents(string eventsJson)
    {
        // This is a simplified parser
        // In production, use a proper JSON library like Newtonsoft.Json
        
        leaderboardData.Clear();

        // Parse events and aggregate player data
        // For now, use mock data
        GenerateMockLeaderboardData();
    }

    /// <summary>
    /// Generate mock leaderboard data for testing
    /// </summary>
    private void GenerateMockLeaderboardData()
    {
        leaderboardData.Clear();

        string[] names = new string[] 
        {
            "ChronoMaster", "RiftWalker", "TemporalKing", "VoidSeeker", "EpochHunter",
            "TimeWeaver", "NebulaNinja", "BristoniteCollector", "QuantumLeaper", "ParadoxSolver"
        };

        for (int i = 0; i < 10; i++)
        {
            LeaderboardEntry entry = new LeaderboardEntry
            {
                rank = i + 1,
                playerAddress = "0x" + Random.Range(1000, 9999).ToString("X") + "...",
                playerName = names[i],
                totalScore = Random.Range(5000, 50000),
                levelsCompleted = Random.Range(10, 100),
                totalWins = Random.Range(5, 50),
                cruEarned = (ulong)Random.Range(100, 1000) * 1000000000,
                averageTime = Random.Range(30f, 120f),
                lastPlayed = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds() - Random.Range(0, 86400)
            };

            leaderboardData.Add(entry);
        }

        // Add current player if wallet connected
        if (walletStore != null && !string.IsNullOrEmpty(walletStore.PermaWalletAddressText.text))
        {
            LeaderboardEntry myEntry = new LeaderboardEntry
            {
                rank = 11,
                playerAddress = walletStore.PermaWalletAddressText.text,
                playerName = "You",
                totalScore = Random.Range(1000, 10000),
                levelsCompleted = Random.Range(5, 30),
                totalWins = Random.Range(2, 15),
                cruEarned = (ulong)Random.Range(50, 500) * 1000000000,
                averageTime = Random.Range(40f, 100f),
                lastPlayed = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            leaderboardData.Add(myEntry);
        }
    }

    /// <summary>
    /// Rank players based on total score
    /// </summary>
    private void RankPlayers()
    {
        // Sort by total score descending
        leaderboardData = leaderboardData.OrderByDescending(e => e.totalScore).ToList();

        // Assign ranks
        for (int i = 0; i < leaderboardData.Count; i++)
        {
            leaderboardData[i].rank = i + 1;
        }
    }

    /// <summary>
    /// Update leaderboard UI
    /// </summary>
    private void UpdateLeaderboardUI()
    {
        // Clear existing entries
        foreach (Transform child in leaderboardContent)
        {
            Destroy(child.gameObject);
        }

        // Create new entries
        foreach (var entry in leaderboardData)
        {
            CreateLeaderboardEntry(entry);
        }

        // Update player's rank display
        UpdateMyRank();
    }

    private void CreateLeaderboardEntry(LeaderboardEntry entry)
    {
        if (leaderboardEntryPrefab == null)
        {
            Debug.LogWarning("Leaderboard entry prefab not set!");
            return;
        }

        GameObject entryObj = Instantiate(leaderboardEntryPrefab, leaderboardContent);
        
        // Find UI elements in prefab
        TMP_Text rankText = entryObj.transform.Find("RankText")?.GetComponent<TMP_Text>();
        TMP_Text nameText = entryObj.transform.Find("NameText")?.GetComponent<TMP_Text>();
        TMP_Text scoreText = entryObj.transform.Find("ScoreText")?.GetComponent<TMP_Text>();
        TMP_Text levelsText = entryObj.transform.Find("LevelsText")?.GetComponent<TMP_Text>();
        TMP_Text cruText = entryObj.transform.Find("CRUText")?.GetComponent<TMP_Text>();

        // Set values
        if (rankText != null)
        {
            rankText.text = GetRankDisplay(entry.rank);
        }

        if (nameText != null)
        {
            nameText.text = entry.playerName;
        }

        if (scoreText != null)
        {
            scoreText.text = entry.totalScore.ToString("N0");
        }

        if (levelsText != null)
        {
            levelsText.text = entry.levelsCompleted.ToString();
        }

        if (cruText != null)
        {
            float cru = entry.cruEarned / 1000000000f;
            cruText.text = cru.ToString("F2") + " CRU";
        }

        // Highlight current player
        if (walletStore != null && entry.playerAddress == walletStore.PermaWalletAddressText.text)
        {
            Image bg = entryObj.GetComponent<Image>();
            if (bg != null)
            {
                bg.color = new Color(1f, 0.8f, 0f, 0.3f); // Gold highlight
            }
        }
    }

    private string GetRankDisplay(int rank)
    {
        if (rank == 1) return "🥇 1st";
        else if (rank == 2) return "🥈 2nd";
        else if (rank == 3) return "🥉 3rd";
        else return "#" + rank;
    }

    private void UpdateMyRank()
    {
        if (walletStore == null || string.IsNullOrEmpty(walletStore.PermaWalletAddressText.text))
        {
            if (myRankText != null) myRankText.text = "Not Connected";
            if (myScoreText != null) myScoreText.text = "-";
            return;
        }

        var myEntry = leaderboardData.FirstOrDefault(e => e.playerAddress == walletStore.PermaWalletAddressText.text);

        if (myEntry != null)
        {
            if (myRankText != null)
            {
                myRankText.text = "Your Rank: " + GetRankDisplay(myEntry.rank);
            }

            if (myScoreText != null)
            {
                myScoreText.text = "Score: " + myEntry.totalScore.ToString("N0");
            }
        }
        else
        {
            if (myRankText != null) myRankText.text = "Not Ranked";
            if (myScoreText != null) myScoreText.text = "Play to get ranked!";
        }
    }

    /// <summary>
    /// Filter changed
    /// </summary>
    private void OnFilterChanged(int filterIndex)
    {
        currentFilter = (LeaderboardFilter)filterIndex;
        RefreshLeaderboard();
    }

    /// <summary>
    /// Get leaderboard data
    /// </summary>
    public List<LeaderboardEntry> GetLeaderboardData()
    {
        return leaderboardData;
    }

    /// <summary>
    /// Get player rank
    /// </summary>
    public int GetPlayerRank(string playerAddress)
    {
        var entry = leaderboardData.FirstOrDefault(e => e.playerAddress == playerAddress);
        return entry != null ? entry.rank : -1;
    }
}
