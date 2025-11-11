using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages $CRU token rewards and balance
/// </summary>
public class CRUTokenManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text balanceText;
    public TMP_Text rewardNotificationText;
    public GameObject rewardPanel;

    [Header("Settings")]
    public WalletStore walletStore;
    public OneChainConfig config;

    private ulong currentBalance = 0;

    // JavaScript interop
    [DllImport("__Internal")]
    private static extern void SendOneChainTransaction(string gameObjectName, string txData, string callbackMethod, string errorMethod);

    [DllImport("__Internal")]
    private static extern void QueryOneChain(string gameObjectName, string queryData, string callbackMethod, string errorMethod);

    void Start()
    {
        walletStore = FindObjectOfType<WalletStore>();
        config = OneChainConfig.Instance;

        if (balanceText != null)
        {
            UpdateBalanceDisplay();
        }
    }

    /// <summary>
    /// Mint CRU tokens as reward for completing a level
    /// </summary>
    public void MintReward(int levelId, ulong rewardAmount)
    {
        if (config == null || string.IsNullOrEmpty(config.cruTreasuryId))
        {
            Debug.LogError("CRU Treasury not configured!");
            return;
        }

        if (walletStore == null || string.IsNullOrEmpty(walletStore.PermaWalletAddressText.text))
        {
            Debug.LogError("Wallet not connected!");
            return;
        }

        string playerAddress = walletStore.PermaWalletAddressText.text;
        
        // Build transaction to mint CRU tokens
        string tx = BuildMintRewardTransaction(rewardAmount, playerAddress);

#if UNITY_WEBGL && !UNITY_EDITOR
        SendOneChainTransaction(gameObject.name, tx, nameof(OnRewardMinted), nameof(OnRewardError));
#else
        Debug.Log("Minting CRU reward: " + rewardAmount + " for level " + levelId);
        Debug.Log("Transaction: " + tx);
        // Simulate success in editor
        OnRewardMinted("{\"status\":\"success\"}");
#endif
    }

    private string BuildMintRewardTransaction(ulong amount, string recipient)
    {
        string tx = @"{
            ""kind"": ""moveCall"",
            ""data"": {
                ""packageObjectId"": """ + config.packageId + @""",
                ""module"": ""cru_token"",
                ""function"": ""mint_reward"",
                ""arguments"": [
                    """ + config.cruTreasuryId + @""",
                    " + amount + @",
                    """ + recipient + @"""
                ],
                ""gasBudget"": 10000000
            }
        }";
        return tx;
    }

    public void OnRewardMinted(string result)
    {
        Debug.Log("CRU Reward Minted Successfully!");
        Debug.Log("Result: " + result);
        
        // Update balance
        FetchBalance();
        
        // Show reward notification
        ShowRewardNotification(config.cruRewardPerLevel);
    }

    public void OnRewardError(string error)
    {
        Debug.LogError("Failed to mint CRU reward: " + error);
    }

    /// <summary>
    /// Fetch player's CRU token balance
    /// </summary>
    public void FetchBalance()
    {
        if (walletStore == null || string.IsNullOrEmpty(walletStore.PermaWalletAddressText.text))
        {
            Debug.LogError("Wallet not connected!");
            return;
        }

        string playerAddress = walletStore.PermaWalletAddressText.text;
        
        // Build query to get CRU balance
        string query = @"{
            ""method"": ""suix_getBalance"",
            ""params"": [
                """ + playerAddress + @""",
                """ + config.packageId + @"::cru_token::CRU""
            ]
        }";

#if UNITY_WEBGL && !UNITY_EDITOR
        QueryOneChain(gameObject.name, query, nameof(OnBalanceFetched), nameof(OnBalanceError));
#else
        Debug.Log("Fetching CRU balance for: " + playerAddress);
        // Simulate balance in editor
        OnBalanceFetched("{\"totalBalance\":\"1000000000\"}");
#endif
    }

    public void OnBalanceFetched(string result)
    {
        Debug.Log("Balance fetched: " + result);
        
        try
        {
            // Parse JSON result
            // This is a simplified version - you'd use a proper JSON parser
            if (result.Contains("totalBalance"))
            {
                // Extract balance value
                string balanceStr = result.Split(new string[] { "totalBalance\":\"" }, System.StringSplitOptions.None)[1].Split('\"')[0];
                currentBalance = ulong.Parse(balanceStr);
                UpdateBalanceDisplay();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to parse balance: " + e.Message);
        }
    }

    public void OnBalanceError(string error)
    {
        Debug.LogError("Failed to fetch balance: " + error);
    }

    private void UpdateBalanceDisplay()
    {
        if (balanceText != null)
        {
            // Convert from smallest unit (9 decimals) to CRU
            float cruAmount = currentBalance / 1000000000f;
            balanceText.text = cruAmount.ToString("F2") + " CRU";
        }
    }

    private void ShowRewardNotification(ulong amount)
    {
        if (rewardPanel != null)
        {
            rewardPanel.SetActive(true);
        }

        if (rewardNotificationText != null)
        {
            float cruAmount = amount / 1000000000f;
            rewardNotificationText.text = "+" + cruAmount.ToString("F2") + " CRU Earned!";
        }

        // Hide notification after 3 seconds
        Invoke(nameof(HideRewardNotification), 3f);
    }

    private void HideRewardNotification()
    {
        if (rewardPanel != null)
        {
            rewardPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Get current balance
    /// </summary>
    public ulong GetBalance()
    {
        return currentBalance;
    }
}
