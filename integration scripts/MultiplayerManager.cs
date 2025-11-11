using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Multiplayer Manager
/// Handles Co-op and PvP game modes
/// </summary>
public class MultiplayerManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject multiplayerPanel;
    public GameObject lobbyPanel;
    public GameObject matchmakingPanel;
    public TMP_Text statusText;
    public Button createRoomButton;
    public Button joinRoomButton;
    public Button quickMatchButton;

    [Header("Lobby UI")]
    public Transform playersListContent;
    public GameObject playerEntryPrefab;
    public TMP_Text roomCodeText;
    public Button startGameButton;
    public Button leaveRoomButton;

    [Header("Settings")]
    public WalletStore walletStore;
    public OneChainConfig config;

    [Header("Game Modes")]
    public GameMode currentGameMode = GameMode.CoOp;

    public enum GameMode
    {
        CoOp,           // Cooperative - 2 players work together
        PvP,            // Player vs Player - 1v1 race
        TeamBattle,     // 2v2 team competition
        Tournament      // Multi-player tournament
    }

    // Multiplayer state
    private MultiplayerRoom currentRoom;
    private List<PlayerData> playersInRoom = new List<PlayerData>();
    private bool isHost = false;
    private MatchmakingState matchmakingState = MatchmakingState.Idle;

    public enum MatchmakingState
    {
        Idle,
        Searching,
        Found,
        InLobby,
        InGame
    }

    [System.Serializable]
    public class MultiplayerRoom
    {
        public string roomId;
        public string roomCode;
        public string hostAddress;
        public GameMode gameMode;
        public int maxPlayers;
        public int currentPlayers;
        public bool isPublic;
        public int difficulty;
        public long createdAt;
    }

    [System.Serializable]
    public class PlayerData
    {
        public string address;
        public string displayName;
        public int level;
        public bool isReady;
        public bool isHost;
    }

    // JavaScript interop
    [DllImport("__Internal")]
    private static extern void SendOneChainTransaction(string gameObjectName, string txData, string callbackMethod, string errorMethod);

    void Start()
    {
        walletStore = FindObjectOfType<WalletStore>();
        config = OneChainConfig.Instance;

        if (createRoomButton != null)
            createRoomButton.onClick.AddListener(CreateRoom);

        if (joinRoomButton != null)
            joinRoomButton.onClick.AddListener(JoinRoom);

        if (quickMatchButton != null)
            quickMatchButton.onClick.AddListener(QuickMatch);

        if (startGameButton != null)
            startGameButton.onClick.AddListener(StartMultiplayerGame);

        if (leaveRoomButton != null)
            leaveRoomButton.onClick.AddListener(LeaveRoom);
    }

    /// <summary>
    /// Create a new multiplayer room
    /// </summary>
    public void CreateRoom()
    {
        if (walletStore == null || string.IsNullOrEmpty(walletStore.PermaWalletAddressText.text))
        {
            Debug.LogError("Wallet not connected!");
            return;
        }

        Debug.Log("Creating multiplayer room...");

        // Generate room code
        string roomCode = GenerateRoomCode();

        currentRoom = new MultiplayerRoom
        {
            roomId = System.Guid.NewGuid().ToString(),
            roomCode = roomCode,
            hostAddress = walletStore.PermaWalletAddressText.text,
            gameMode = currentGameMode,
            maxPlayers = GetMaxPlayersForMode(currentGameMode),
            currentPlayers = 1,
            isPublic = true,
            difficulty = 50,
            createdAt = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };

        isHost = true;
        matchmakingState = MatchmakingState.InLobby;

        // Add host to players list
        PlayerData hostPlayer = new PlayerData
        {
            address = walletStore.PermaWalletAddressText.text,
            displayName = "You (Host)",
            level = Random.Range(1, 50),
            isReady = true,
            isHost = true
        };

        playersInRoom.Clear();
        playersInRoom.Add(hostPlayer);

        Debug.Log("Room created! Code: " + roomCode);

        UpdateLobbyUI();
        ShowLobby();
    }

    private string GenerateRoomCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        char[] code = new char[6];
        for (int i = 0; i < 6; i++)
        {
            code[i] = chars[Random.Range(0, chars.Length)];
        }
        return new string(code);
    }

    private int GetMaxPlayersForMode(GameMode mode)
    {
        switch (mode)
        {
            case GameMode.CoOp: return 2;
            case GameMode.PvP: return 2;
            case GameMode.TeamBattle: return 4;
            case GameMode.Tournament: return 8;
            default: return 2;
        }
    }

    /// <summary>
    /// Join existing room
    /// </summary>
    public void JoinRoom()
    {
        Debug.Log("Joining room...");

        // In production, show room code input
        // For now, simulate joining
        SimulateJoinRoom();
    }

    private void SimulateJoinRoom()
    {
        if (currentRoom == null)
        {
            Debug.LogError("No room to join!");
            return;
        }

        PlayerData newPlayer = new PlayerData
        {
            address = "0x" + Random.Range(1000, 9999).ToString("X"),
            displayName = "Player " + (playersInRoom.Count + 1),
            level = Random.Range(1, 50),
            isReady = false,
            isHost = false
        };

        playersInRoom.Add(newPlayer);
        currentRoom.currentPlayers++;

        Debug.Log("Player joined: " + newPlayer.displayName);

        UpdateLobbyUI();
    }

    /// <summary>
    /// Quick match - find random opponent
    /// </summary>
    public void QuickMatch()
    {
        Debug.Log("Searching for match...");

        matchmakingState = MatchmakingState.Searching;
        UpdateStatusText("Searching for players...");

        // Simulate matchmaking
        Invoke(nameof(MatchFound), 2f);
    }

    private void MatchFound()
    {
        Debug.Log("Match found!");

        matchmakingState = MatchmakingState.Found;
        UpdateStatusText("Match found! Joining lobby...");

        // Create room with matched players
        CreateRoom();

        // Add opponent
        SimulateJoinRoom();

        Invoke(nameof(ShowLobby), 1f);
    }

    /// <summary>
    /// Start multiplayer game
    /// </summary>
    public void StartMultiplayerGame()
    {
        if (!isHost)
        {
            Debug.LogWarning("Only host can start the game!");
            return;
        }

        if (currentRoom.currentPlayers < 2)
        {
            Debug.LogWarning("Need at least 2 players to start!");
            return;
        }

        Debug.Log("Starting multiplayer game...");

        matchmakingState = MatchmakingState.InGame;

        // Record multiplayer match on blockchain
        RecordMultiplayerMatch();

        // Load game scene
        // UnityEngine.SceneManagement.SceneManager.LoadScene("MultiplayerGame");
    }

    /// <summary>
    /// Record multiplayer match on blockchain
    /// </summary>
    private void RecordMultiplayerMatch()
    {
        if (config == null || string.IsNullOrEmpty(config.packageId))
        {
            Debug.LogError("Config not set!");
            return;
        }

        // Build transaction to record multiplayer match
        string tx = BuildMultiplayerMatchTransaction();

#if UNITY_WEBGL && !UNITY_EDITOR
        SendOneChainTransaction(gameObject.name, tx, nameof(OnMatchRecorded), nameof(OnMatchError));
#else
        Debug.Log("Recording multiplayer match (Editor Mode)");
        OnMatchRecorded("{\"status\":\"success\"}");
#endif
    }

    private string BuildMultiplayerMatchTransaction()
    {
        // Build list of player addresses
        string playersJson = "[";
        for (int i = 0; i < playersInRoom.Count; i++)
        {
            playersJson += "\"" + playersInRoom[i].address + "\"";
            if (i < playersInRoom.Count - 1)
                playersJson += ",";
        }
        playersJson += "]";

        string tx = @"{
            ""kind"": ""moveCall"",
            ""data"": {
                ""packageObjectId"": """ + config.packageId + @""",
                ""module"": ""battle_system"",
                ""function"": ""record_multiplayer_match"",
                ""arguments"": [
                    " + playersJson + @",
                    """ + currentGameMode.ToString() + @""",
                    " + currentRoom.difficulty + @"
                ],
                ""gasBudget"": 10000000
            }
        }";

        return tx;
    }

    public void OnMatchRecorded(string result)
    {
        Debug.Log("Multiplayer match recorded on blockchain!");
        Debug.Log("Result: " + result);
    }

    public void OnMatchError(string error)
    {
        Debug.LogError("Failed to record match: " + error);
    }

    /// <summary>
    /// Leave room
    /// </summary>
    public void LeaveRoom()
    {
        Debug.Log("Leaving room...");

        if (isHost)
        {
            // Host leaving - close room
            currentRoom = null;
            playersInRoom.Clear();
        }
        else
        {
            // Player leaving - remove from list
            var myPlayer = playersInRoom.Find(p => p.address == walletStore.PermaWalletAddressText.text);
            if (myPlayer != null)
            {
                playersInRoom.Remove(myPlayer);
                currentRoom.currentPlayers--;
            }
        }

        isHost = false;
        matchmakingState = MatchmakingState.Idle;

        HideLobby();
    }

    /// <summary>
    /// Update lobby UI
    /// </summary>
    private void UpdateLobbyUI()
    {
        if (roomCodeText != null && currentRoom != null)
        {
            roomCodeText.text = "Room Code: " + currentRoom.roomCode;
        }

        if (playersListContent == null) return;

        // Clear existing
        foreach (Transform child in playersListContent)
        {
            Destroy(child.gameObject);
        }

        // Create player entries
        foreach (var player in playersInRoom)
        {
            CreatePlayerEntry(player);
        }

        // Update start button
        if (startGameButton != null)
        {
            startGameButton.interactable = isHost && currentRoom.currentPlayers >= 2;
        }
    }

    private void CreatePlayerEntry(PlayerData player)
    {
        if (playerEntryPrefab == null) return;

        GameObject entryObj = Instantiate(playerEntryPrefab, playersListContent);

        TMP_Text nameText = entryObj.transform.Find("NameText")?.GetComponent<TMP_Text>();
        TMP_Text levelText = entryObj.transform.Find("LevelText")?.GetComponent<TMP_Text>();
        Image readyIndicator = entryObj.transform.Find("ReadyIndicator")?.GetComponent<Image>();

        if (nameText != null)
            nameText.text = player.displayName + (player.isHost ? " 👑" : "");

        if (levelText != null)
            levelText.text = "Lv." + player.level;

        if (readyIndicator != null)
            readyIndicator.color = player.isReady ? Color.green : Color.yellow;
    }

    private void UpdateStatusText(string status)
    {
        if (statusText != null)
            statusText.text = status;
    }

    private void ShowLobby()
    {
        if (lobbyPanel != null)
            lobbyPanel.SetActive(true);

        if (matchmakingPanel != null)
            matchmakingPanel.SetActive(false);
    }

    private void HideLobby()
    {
        if (lobbyPanel != null)
            lobbyPanel.SetActive(false);
    }

    /// <summary>
    /// Get current room
    /// </summary>
    public MultiplayerRoom GetCurrentRoom()
    {
        return currentRoom;
    }

    /// <summary>
    /// Get players in room
    /// </summary>
    public List<PlayerData> GetPlayersInRoom()
    {
        return playersInRoom;
    }

    /// <summary>
    /// Is in multiplayer game
    /// </summary>
    public bool IsInMultiplayerGame()
    {
        return matchmakingState == MatchmakingState.InGame;
    }
}
