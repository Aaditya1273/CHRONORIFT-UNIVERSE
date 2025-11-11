using UnityEngine;

/// <summary>
/// OneChain Configuration for ChronoRift Universe
/// Store all contract addresses and RPC endpoints here
/// </summary>
public class OneChainConfig : MonoBehaviour
{
    [Header("OneChain Network")]
    [Tooltip("OneChain RPC Endpoint")]
    public string rpcEndpoint = "https://rpc-testnet.onelabs.cc:443";
    
    [Tooltip("Chain ID")]
    public string chainId = "onechain-testnet";

    [Header("Deployed Contract Addresses")]
    [Tooltip("Package ID (after deploying Move contracts)")]
    public string packageId = "";
    
    [Tooltip("CRU Token Treasury Object ID")]
    public string cruTreasuryId = "";
    
    [Tooltip("Staking Pool Object ID")]
    public string stakingPoolId = "";
    
    [Tooltip("Leaderboard Object ID")]
    public string leaderboardId = "";

    [Header("Player Data")]
    [Tooltip("Player Progress Object ID (created per player)")]
    public string playerProgressId = "";
    
    [Tooltip("Player Stats Object ID (created per player)")]
    public string playerStatsId = "";

    [Header("Game Settings")]
    [Tooltip("CRU reward per level completion")]
    public ulong cruRewardPerLevel = 100000000; // 0.1 CRU (9 decimals)
    
    [Tooltip("Staking reward rate")]
    public ulong stakingRewardRate = 1000; // 0.1% per epoch

    // Singleton instance
    private static OneChainConfig _instance;
    public static OneChainConfig Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<OneChainConfig>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("OneChainConfig");
                    _instance = go.AddComponent<OneChainConfig>();
                    DontDestroyOnLoad(go);
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

    /// <summary>
    /// Check if all required contract addresses are set
    /// </summary>
    public bool IsConfigured()
    {
        return !string.IsNullOrEmpty(packageId) &&
               !string.IsNullOrEmpty(cruTreasuryId) &&
               !string.IsNullOrEmpty(stakingPoolId) &&
               !string.IsNullOrEmpty(leaderboardId);
    }

    /// <summary>
    /// Log current configuration
    /// </summary>
    public void LogConfiguration()
    {
        Debug.Log("=== OneChain Configuration ===");
        Debug.Log("RPC Endpoint: " + rpcEndpoint);
        Debug.Log("Chain ID: " + chainId);
        Debug.Log("Package ID: " + (string.IsNullOrEmpty(packageId) ? "NOT SET" : packageId));
        Debug.Log("CRU Treasury: " + (string.IsNullOrEmpty(cruTreasuryId) ? "NOT SET" : cruTreasuryId));
        Debug.Log("Staking Pool: " + (string.IsNullOrEmpty(stakingPoolId) ? "NOT SET" : stakingPoolId));
        Debug.Log("Leaderboard: " + (string.IsNullOrEmpty(leaderboardId) ? "NOT SET" : leaderboardId));
        Debug.Log("Player Progress: " + (string.IsNullOrEmpty(playerProgressId) ? "NOT SET" : playerProgressId));
        Debug.Log("============================");
    }
}
