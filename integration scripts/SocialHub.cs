using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Social Hub - Central hub for all social features
/// Integrates leaderboard, friends, achievements, and multiplayer
/// </summary>
public class SocialHub : MonoBehaviour
{
    [Header("Main Panels")]
    public GameObject socialHubPanel;
    public GameObject leaderboardTab;
    public GameObject friendsTab;
    public GameObject achievementsTab;
    public GameObject multiplayerTab;
    public GameObject profileTab;

    [Header("Navigation Buttons")]
    public Button leaderboardButton;
    public Button friendsButton;
    public Button achievementsButton;
    public Button multiplayerButton;
    public Button profileButton;
    public Button closeButton;

    [Header("Quick Stats")]
    public TMP_Text playerRankText;
    public TMP_Text friendsCountText;
    public TMP_Text achievementsCountText;
    public TMP_Text onlinePlayersText;

    [Header("Managers")]
    public LeaderboardUI leaderboardUI;
    public SocialFeatures socialFeatures;
    public MultiplayerManager multiplayerManager;

    // Current active tab
    private string currentTab = "leaderboard";

    void Start()
    {
        // Find managers
        leaderboardUI = FindObjectOfType<LeaderboardUI>();
        socialFeatures = FindObjectOfType<SocialFeatures>();
        multiplayerManager = FindObjectOfType<MultiplayerManager>();

        // Setup navigation
        if (leaderboardButton != null)
            leaderboardButton.onClick.AddListener(() => ShowTab("leaderboard"));

        if (friendsButton != null)
            friendsButton.onClick.AddListener(() => ShowTab("friends"));

        if (achievementsButton != null)
            achievementsButton.onClick.AddListener(() => ShowTab("achievements"));

        if (multiplayerButton != null)
            multiplayerButton.onClick.AddListener(() => ShowTab("multiplayer"));

        if (profileButton != null)
            profileButton.onClick.AddListener(() => ShowTab("profile"));

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseSocialHub);

        // Show default tab
        ShowTab("leaderboard");

        // Update quick stats periodically
        InvokeRepeating(nameof(UpdateQuickStats), 0f, 5f);
    }

    /// <summary>
    /// Show social hub
    /// </summary>
    public void ShowSocialHub()
    {
        if (socialHubPanel != null)
            socialHubPanel.SetActive(true);

        UpdateQuickStats();
    }

    /// <summary>
    /// Close social hub
    /// </summary>
    public void CloseSocialHub()
    {
        if (socialHubPanel != null)
            socialHubPanel.SetActive(false);
    }

    /// <summary>
    /// Show specific tab
    /// </summary>
    public void ShowTab(string tabName)
    {
        currentTab = tabName;

        // Hide all tabs
        if (leaderboardTab != null) leaderboardTab.SetActive(false);
        if (friendsTab != null) friendsTab.SetActive(false);
        if (achievementsTab != null) achievementsTab.SetActive(false);
        if (multiplayerTab != null) multiplayerTab.SetActive(false);
        if (profileTab != null) profileTab.SetActive(false);

        // Show selected tab
        switch (tabName.ToLower())
        {
            case "leaderboard":
                if (leaderboardTab != null) leaderboardTab.SetActive(true);
                if (leaderboardUI != null) leaderboardUI.RefreshLeaderboard();
                break;

            case "friends":
                if (friendsTab != null) friendsTab.SetActive(true);
                if (socialFeatures != null) socialFeatures.ShowPanel("friends");
                break;

            case "achievements":
                if (achievementsTab != null) achievementsTab.SetActive(true);
                if (socialFeatures != null) socialFeatures.ShowPanel("achievements");
                break;

            case "multiplayer":
                if (multiplayerTab != null) multiplayerTab.SetActive(true);
                break;

            case "profile":
                if (profileTab != null) profileTab.SetActive(true);
                if (socialFeatures != null) socialFeatures.ShowPanel("profile");
                break;
        }

        Debug.Log("Showing tab: " + tabName);
    }

    /// <summary>
    /// Update quick stats display
    /// </summary>
    private void UpdateQuickStats()
    {
        // Player rank
        if (playerRankText != null && leaderboardUI != null)
        {
            var leaderboardData = leaderboardUI.GetLeaderboardData();
            if (leaderboardData != null && leaderboardData.Count > 0)
            {
                playerRankText.text = "Rank: #" + Random.Range(1, 100);
            }
            else
            {
                playerRankText.text = "Rank: --";
            }
        }

        // Friends count
        if (friendsCountText != null && socialFeatures != null)
        {
            int friendsCount = socialFeatures.GetFriendsCount();
            friendsCountText.text = "Friends: " + friendsCount;
        }

        // Achievements count
        if (achievementsCountText != null && socialFeatures != null)
        {
            int unlockedCount = socialFeatures.GetUnlockedAchievementsCount();
            achievementsCountText.text = "Achievements: " + unlockedCount;
        }

        // Online players (simulated)
        if (onlinePlayersText != null)
        {
            int onlinePlayers = Random.Range(500, 2000);
            onlinePlayersText.text = "Online: " + onlinePlayers;
        }
    }

    /// <summary>
    /// Get comprehensive social summary
    /// </summary>
    public string GetSocialSummary()
    {
        string summary = "=== ChronoRift Universe Social Hub ===\n";
        
        if (leaderboardUI != null)
        {
            summary += "Leaderboard Entries: " + leaderboardUI.GetLeaderboardData().Count + "\n";
        }

        if (socialFeatures != null)
        {
            summary += "Friends: " + socialFeatures.GetFriendsCount() + "\n";
            summary += "Achievements Unlocked: " + socialFeatures.GetUnlockedAchievementsCount() + "\n";
        }

        if (multiplayerManager != null)
        {
            var room = multiplayerManager.GetCurrentRoom();
            if (room != null)
            {
                summary += "In Multiplayer Room: " + room.roomCode + "\n";
                summary += "Players: " + room.currentPlayers + "/" + room.maxPlayers + "\n";
            }
        }

        summary += "====================================";

        return summary;
    }

    /// <summary>
    /// Quick action - Challenge friend
    /// </summary>
    public void ChallengeFriend(string friendAddress)
    {
        Debug.Log("Challenging friend: " + friendAddress);
        
        // Create PvP room
        if (multiplayerManager != null)
        {
            multiplayerManager.CreateRoom();
        }

        ShowTab("multiplayer");
    }

    /// <summary>
    /// Quick action - Share achievement
    /// </summary>
    public void ShareAchievement(string achievementId)
    {
        Debug.Log("Sharing achievement: " + achievementId);
        
        // In production, share to social media or friends
        string shareText = "I just unlocked an achievement in ChronoRift Universe! 🏆";
        Debug.Log("Share text: " + shareText);
    }

    /// <summary>
    /// Quick action - View player profile
    /// </summary>
    public void ViewPlayerProfile(string playerAddress)
    {
        Debug.Log("Viewing profile: " + playerAddress);
        
        // Load player profile
        ShowTab("profile");
    }

    /// <summary>
    /// Toggle social hub
    /// </summary>
    public void ToggleSocialHub()
    {
        if (socialHubPanel != null)
        {
            bool isActive = socialHubPanel.activeSelf;
            socialHubPanel.SetActive(!isActive);

            if (!isActive)
            {
                UpdateQuickStats();
            }
        }
    }
}
