using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Social Features System
/// Player profiles, friends, achievements, and social interactions
/// </summary>
public class SocialFeatures : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject profilePanel;
    public GameObject friendsPanel;
    public GameObject achievementsPanel;
    public GameObject chatPanel;

    [Header("Profile UI")]
    public TMP_Text playerNameText;
    public TMP_Text playerAddressText;
    public TMP_Text playerLevelText;
    public TMP_Text totalScoreText;
    public TMP_Text totalWinsText;
    public TMP_Text cruEarnedText;
    public Image playerAvatarImage;

    [Header("Friends UI")]
    public Transform friendsListContent;
    public GameObject friendEntryPrefab;
    public TMP_InputField addFriendInput;
    public Button addFriendButton;

    [Header("Achievements UI")]
    public Transform achievementsContent;
    public GameObject achievementPrefab;

    [Header("Settings")]
    public WalletStore walletStore;
    public LeaderboardUI leaderboardUI;

    // Player data
    private PlayerProfile currentProfile;
    private List<Friend> friendsList = new List<Friend>();
    private List<Achievement> achievements = new List<Achievement>();

    [System.Serializable]
    public class PlayerProfile
    {
        public string address;
        public string displayName;
        public int level;
        public int totalScore;
        public int totalWins;
        public ulong cruEarned;
        public int nftsOwned;
        public int achievementsUnlocked;
        public long accountCreated;
        public long lastPlayed;
    }

    [System.Serializable]
    public class Friend
    {
        public string address;
        public string displayName;
        public bool isOnline;
        public int level;
        public long lastSeen;
    }

    [System.Serializable]
    public class Achievement
    {
        public string id;
        public string name;
        public string description;
        public string icon;
        public bool unlocked;
        public long unlockedAt;
        public AchievementRarity rarity;
    }

    public enum AchievementRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    void Start()
    {
        walletStore = FindObjectOfType<WalletStore>();
        leaderboardUI = FindObjectOfType<LeaderboardUI>();

        if (addFriendButton != null)
            addFriendButton.onClick.AddListener(AddFriend);

        // Initialize achievements
        InitializeAchievements();

        // Load player profile
        LoadPlayerProfile();
    }

    /// <summary>
    /// Load player profile
    /// </summary>
    public void LoadPlayerProfile()
    {
        if (walletStore == null || string.IsNullOrEmpty(walletStore.PermaWalletAddressText.text))
        {
            Debug.LogWarning("Wallet not connected!");
            return;
        }

        Debug.Log("Loading player profile...");

        // In production, fetch from blockchain
        // For now, create mock profile
        currentProfile = new PlayerProfile
        {
            address = walletStore.PermaWalletAddressText.text,
            displayName = "ChronoRift Player",
            level = Random.Range(1, 50),
            totalScore = Random.Range(1000, 50000),
            totalWins = Random.Range(5, 100),
            cruEarned = (ulong)Random.Range(100, 5000) * 1000000000,
            nftsOwned = Random.Range(1, 20),
            achievementsUnlocked = Random.Range(5, 30),
            accountCreated = System.DateTimeOffset.UtcNow.AddDays(-Random.Range(1, 365)).ToUnixTimeSeconds(),
            lastPlayed = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };

        UpdateProfileUI();
    }

    private void UpdateProfileUI()
    {
        if (currentProfile == null) return;

        if (playerNameText != null)
            playerNameText.text = currentProfile.displayName;

        if (playerAddressText != null)
            playerAddressText.text = FormatAddress(currentProfile.address);

        if (playerLevelText != null)
            playerLevelText.text = "Level " + currentProfile.level;

        if (totalScoreText != null)
            totalScoreText.text = "Score: " + currentProfile.totalScore.ToString("N0");

        if (totalWinsText != null)
            totalWinsText.text = "Wins: " + currentProfile.totalWins;

        if (cruEarnedText != null)
        {
            float cru = currentProfile.cruEarned / 1000000000f;
            cruEarnedText.text = "CRU Earned: " + cru.ToString("F2");
        }
    }

    private string FormatAddress(string address)
    {
        if (string.IsNullOrEmpty(address) || address.Length < 10)
            return address;

        return address.Substring(0, 6) + "..." + address.Substring(address.Length - 4);
    }

    /// <summary>
    /// Add friend
    /// </summary>
    public void AddFriend()
    {
        if (addFriendInput == null || string.IsNullOrEmpty(addFriendInput.text))
        {
            Debug.LogWarning("Enter friend address!");
            return;
        }

        string friendAddress = addFriendInput.text.Trim();

        // Check if already friends
        if (friendsList.Exists(f => f.address == friendAddress))
        {
            Debug.LogWarning("Already friends!");
            return;
        }

        // Add friend
        Friend newFriend = new Friend
        {
            address = friendAddress,
            displayName = "Player " + Random.Range(1000, 9999),
            isOnline = Random.value > 0.5f,
            level = Random.Range(1, 50),
            lastSeen = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };

        friendsList.Add(newFriend);
        addFriendInput.text = "";

        Debug.Log("Friend added: " + newFriend.displayName);

        UpdateFriendsUI();
    }

    private void UpdateFriendsUI()
    {
        if (friendsListContent == null) return;

        // Clear existing
        foreach (Transform child in friendsListContent)
        {
            Destroy(child.gameObject);
        }

        // Create friend entries
        foreach (var friend in friendsList)
        {
            CreateFriendEntry(friend);
        }
    }

    private void CreateFriendEntry(Friend friend)
    {
        if (friendEntryPrefab == null) return;

        GameObject entryObj = Instantiate(friendEntryPrefab, friendsListContent);

        TMP_Text nameText = entryObj.transform.Find("NameText")?.GetComponent<TMP_Text>();
        TMP_Text statusText = entryObj.transform.Find("StatusText")?.GetComponent<TMP_Text>();
        Image statusIndicator = entryObj.transform.Find("StatusIndicator")?.GetComponent<Image>();

        if (nameText != null)
            nameText.text = friend.displayName + " (Lv." + friend.level + ")";

        if (statusText != null)
            statusText.text = friend.isOnline ? "Online" : "Offline";

        if (statusIndicator != null)
            statusIndicator.color = friend.isOnline ? Color.green : Color.gray;
    }

    /// <summary>
    /// Initialize achievements
    /// </summary>
    private void InitializeAchievements()
    {
        achievements = new List<Achievement>
        {
            new Achievement { id = "first_level", name = "First Steps", description = "Complete your first level", rarity = AchievementRarity.Common, unlocked = true },
            new Achievement { id = "desert_master", name = "Desert Master", description = "Complete all Desert levels", rarity = AchievementRarity.Uncommon, unlocked = false },
            new Achievement { id = "lava_survivor", name = "Lava Survivor", description = "Complete all Lava levels", rarity = AchievementRarity.Rare, unlocked = false },
            new Achievement { id = "void_walker", name = "Void Walker", description = "Complete all Void levels", rarity = AchievementRarity.Epic, unlocked = false },
            new Achievement { id = "nebula_conqueror", name = "Nebula Conqueror", description = "Complete all Nebula levels", rarity = AchievementRarity.Legendary, unlocked = false },
            new Achievement { id = "speed_demon", name = "Speed Demon", description = "Complete a level in under 30 seconds", rarity = AchievementRarity.Rare, unlocked = false },
            new Achievement { id = "collector", name = "Bristonite Collector", description = "Collect 100 Bristonite shards", rarity = AchievementRarity.Uncommon, unlocked = false },
            new Achievement { id = "rich", name = "CRU Millionaire", description = "Earn 1000 CRU tokens", rarity = AchievementRarity.Epic, unlocked = false },
            new Achievement { id = "social", name = "Social Butterfly", description = "Add 10 friends", rarity = AchievementRarity.Common, unlocked = false },
            new Achievement { id = "champion", name = "Champion", description = "Reach #1 on the leaderboard", rarity = AchievementRarity.Legendary, unlocked = false }
        };

        UpdateAchievementsUI();
    }

    private void UpdateAchievementsUI()
    {
        if (achievementsContent == null) return;

        // Clear existing
        foreach (Transform child in achievementsContent)
        {
            Destroy(child.gameObject);
        }

        // Create achievement entries
        foreach (var achievement in achievements)
        {
            CreateAchievementEntry(achievement);
        }
    }

    private void CreateAchievementEntry(Achievement achievement)
    {
        if (achievementPrefab == null) return;

        GameObject entryObj = Instantiate(achievementPrefab, achievementsContent);

        TMP_Text nameText = entryObj.transform.Find("NameText")?.GetComponent<TMP_Text>();
        TMP_Text descText = entryObj.transform.Find("DescriptionText")?.GetComponent<TMP_Text>();
        Image lockIcon = entryObj.transform.Find("LockIcon")?.GetComponent<Image>();

        if (nameText != null)
        {
            nameText.text = achievement.name;
            nameText.color = GetRarityColor(achievement.rarity);
        }

        if (descText != null)
            descText.text = achievement.description;

        if (lockIcon != null)
            lockIcon.enabled = !achievement.unlocked;

        // Gray out if locked
        if (!achievement.unlocked)
        {
            var canvasGroup = entryObj.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = entryObj.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0.5f;
        }
    }

    private Color GetRarityColor(AchievementRarity rarity)
    {
        switch (rarity)
        {
            case AchievementRarity.Common: return Color.white;
            case AchievementRarity.Uncommon: return Color.green;
            case AchievementRarity.Rare: return Color.blue;
            case AchievementRarity.Epic: return new Color(0.6f, 0f, 1f); // Purple
            case AchievementRarity.Legendary: return new Color(1f, 0.5f, 0f); // Orange
            default: return Color.white;
        }
    }

    /// <summary>
    /// Unlock achievement
    /// </summary>
    public void UnlockAchievement(string achievementId)
    {
        var achievement = achievements.Find(a => a.id == achievementId);
        if (achievement != null && !achievement.unlocked)
        {
            achievement.unlocked = true;
            achievement.unlockedAt = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            
            Debug.Log("🏆 Achievement Unlocked: " + achievement.name);
            
            // Show notification
            ShowAchievementNotification(achievement);
            
            UpdateAchievementsUI();
        }
    }

    private void ShowAchievementNotification(Achievement achievement)
    {
        Debug.Log("🎉 " + achievement.name + " - " + achievement.description);
        // In production, show UI notification
    }

    /// <summary>
    /// Show panel
    /// </summary>
    public void ShowPanel(string panelName)
    {
        if (profilePanel != null) profilePanel.SetActive(panelName == "profile");
        if (friendsPanel != null) friendsPanel.SetActive(panelName == "friends");
        if (achievementsPanel != null) achievementsPanel.SetActive(panelName == "achievements");
        if (chatPanel != null) chatPanel.SetActive(panelName == "chat");
    }

    /// <summary>
    /// Get player profile
    /// </summary>
    public PlayerProfile GetProfile()
    {
        return currentProfile;
    }

    /// <summary>
    /// Get friends count
    /// </summary>
    public int GetFriendsCount()
    {
        return friendsList.Count;
    }

    /// <summary>
    /// Get unlocked achievements count
    /// </summary>
    public int GetUnlockedAchievementsCount()
    {
        return achievements.Count(a => a.unlocked);
    }
}
