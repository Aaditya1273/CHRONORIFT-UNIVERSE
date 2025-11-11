using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages CRU token staking for epoch enclaves
/// </summary>
public class StakingManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text totalStakedText;
    public TMP_Text myStakeText;
    public TMP_Text rewardsText;
    public TMP_InputField stakeAmountInput;
    public Button stakeButton;
    public Button unstakeButton;
    public Button claimRewardsButton;

    [Header("Settings")]
    public WalletStore walletStore;
    public OneChainConfig config;
    public CRUTokenManager tokenManager;

    private ulong totalStaked = 0;
    private ulong myStake = 0;
    private ulong pendingRewards = 0;
    private string myStakeRecordId = "";

    // JavaScript interop
    [DllImport("__Internal")]
    private static extern void SendOneChainTransaction(string gameObjectName, string txData, string callbackMethod, string errorMethod);

    [DllImport("__Internal")]
    private static extern void QueryOneChain(string gameObjectName, string queryData, string callbackMethod, string errorMethod);

    void Start()
    {
        walletStore = FindObjectOfType<WalletStore>();
        config = OneChainConfig.Instance;
        tokenManager = FindObjectOfType<CRUTokenManager>();

        if (stakeButton != null)
            stakeButton.onClick.AddListener(StakeTokens);
        
        if (unstakeButton != null)
            unstakeButton.onClick.AddListener(UnstakeTokens);
        
        if (claimRewardsButton != null)
            claimRewardsButton.onClick.AddListener(ClaimRewards);

        // Fetch staking info
        FetchStakingInfo();
    }

    /// <summary>
    /// Stake CRU tokens
    /// </summary>
    public void StakeTokens()
    {
        if (config == null || string.IsNullOrEmpty(config.stakingPoolId))
        {
            Debug.LogError("Staking pool not configured!");
            return;
        }

        if (stakeAmountInput == null || string.IsNullOrEmpty(stakeAmountInput.text))
        {
            Debug.LogError("Please enter stake amount!");
            return;
        }

        // Parse amount (convert CRU to smallest unit)
        if (!float.TryParse(stakeAmountInput.text, out float cruAmount))
        {
            Debug.LogError("Invalid amount!");
            return;
        }

        ulong amount = (ulong)(cruAmount * 1000000000); // Convert to 9 decimals

        string tx = BuildStakeTransaction(amount);

#if UNITY_WEBGL && !UNITY_EDITOR
        SendOneChainTransaction(gameObject.name, tx, nameof(OnStakeSuccess), nameof(OnStakeError));
#else
        Debug.Log("Staking " + cruAmount + " CRU");
        Debug.Log("Transaction: " + tx);
        OnStakeSuccess("{\"status\":\"success\"}");
#endif
    }

    private string BuildStakeTransaction(ulong amount)
    {
        // This assumes you have CRU coins to stake
        // In reality, you'd need to split coins or use existing coin objects
        string tx = @"{
            ""kind"": ""moveCall"",
            ""data"": {
                ""packageObjectId"": """ + config.packageId + @""",
                ""module"": ""staking_pool"",
                ""function"": ""stake_tokens"",
                ""arguments"": [
                    """ + config.stakingPoolId + @""",
                    {""type"": ""coin"", ""amount"": " + amount + @"}
                ],
                ""gasBudget"": 10000000
            }
        }";
        return tx;
    }

    public void OnStakeSuccess(string result)
    {
        Debug.Log("Tokens Staked Successfully!");
        Debug.Log("Result: " + result);
        
        // Clear input
        if (stakeAmountInput != null)
            stakeAmountInput.text = "";
        
        // Refresh staking info
        FetchStakingInfo();
        
        // Update token balance
        if (tokenManager != null)
            tokenManager.FetchBalance();
    }

    public void OnStakeError(string error)
    {
        Debug.LogError("Staking failed: " + error);
    }

    /// <summary>
    /// Unstake CRU tokens
    /// </summary>
    public void UnstakeTokens()
    {
        if (string.IsNullOrEmpty(myStakeRecordId))
        {
            Debug.LogError("No stake record found!");
            return;
        }

        string tx = BuildUnstakeTransaction();

#if UNITY_WEBGL && !UNITY_EDITOR
        SendOneChainTransaction(gameObject.name, tx, nameof(OnUnstakeSuccess), nameof(OnUnstakeError));
#else
        Debug.Log("Unstaking tokens");
        OnUnstakeSuccess("{\"status\":\"success\"}");
#endif
    }

    private string BuildUnstakeTransaction()
    {
        string tx = @"{
            ""kind"": ""moveCall"",
            ""data"": {
                ""packageObjectId"": """ + config.packageId + @""",
                ""module"": ""staking_pool"",
                ""function"": ""unstake_tokens"",
                ""arguments"": [
                    """ + config.stakingPoolId + @""",
                    """ + myStakeRecordId + @"""
                ],
                ""gasBudget"": 10000000
            }
        }";
        return tx;
    }

    public void OnUnstakeSuccess(string result)
    {
        Debug.Log("Tokens Unstaked Successfully!");
        myStakeRecordId = "";
        myStake = 0;
        
        FetchStakingInfo();
        
        if (tokenManager != null)
            tokenManager.FetchBalance();
    }

    public void OnUnstakeError(string error)
    {
        Debug.LogError("Unstaking failed: " + error);
    }

    /// <summary>
    /// Claim staking rewards
    /// </summary>
    public void ClaimRewards()
    {
        if (string.IsNullOrEmpty(myStakeRecordId))
        {
            Debug.LogError("No stake record found!");
            return;
        }

        string tx = BuildClaimRewardsTransaction();

#if UNITY_WEBGL && !UNITY_EDITOR
        SendOneChainTransaction(gameObject.name, tx, nameof(OnClaimSuccess), nameof(OnClaimError));
#else
        Debug.Log("Claiming rewards");
        OnClaimSuccess("{\"status\":\"success\"}");
#endif
    }

    private string BuildClaimRewardsTransaction()
    {
        string tx = @"{
            ""kind"": ""moveCall"",
            ""data"": {
                ""packageObjectId"": """ + config.packageId + @""",
                ""module"": ""staking_pool"",
                ""function"": ""claim_rewards"",
                ""arguments"": [
                    """ + config.stakingPoolId + @""",
                    """ + myStakeRecordId + @"""
                ],
                ""gasBudget"": 10000000
            }
        }";
        return tx;
    }

    public void OnClaimSuccess(string result)
    {
        Debug.Log("Rewards Claimed Successfully!");
        pendingRewards = 0;
        UpdateUI();
        
        if (tokenManager != null)
            tokenManager.FetchBalance();
    }

    public void OnClaimError(string error)
    {
        Debug.LogError("Claim failed: " + error);
    }

    /// <summary>
    /// Fetch staking pool information
    /// </summary>
    public void FetchStakingInfo()
    {
        if (config == null || string.IsNullOrEmpty(config.stakingPoolId))
        {
            Debug.LogError("Staking pool not configured!");
            return;
        }

        // Query staking pool data
        string query = @"{
            ""method"": ""sui_getObject"",
            ""params"": [
                """ + config.stakingPoolId + @""",
                {""showContent"": true}
            ]
        }";

#if UNITY_WEBGL && !UNITY_EDITOR
        QueryOneChain(gameObject.name, query, nameof(OnStakingInfoFetched), nameof(OnFetchError));
#else
        Debug.Log("Fetching staking info");
        // Simulate data in editor
        totalStaked = 10000000000; // 10 CRU
        UpdateUI();
#endif
    }

    public void OnStakingInfoFetched(string result)
    {
        Debug.Log("Staking info fetched: " + result);
        
        try
        {
            // Parse staking pool data
            // This is simplified - use proper JSON parsing
            // Extract total_staked, participants, etc.
            
            UpdateUI();
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to parse staking info: " + e.Message);
        }
    }

    public void OnFetchError(string error)
    {
        Debug.LogError("Failed to fetch staking info: " + error);
    }

    private void UpdateUI()
    {
        if (totalStakedText != null)
        {
            float cruAmount = totalStaked / 1000000000f;
            totalStakedText.text = "Total Staked: " + cruAmount.ToString("F2") + " CRU";
        }

        if (myStakeText != null)
        {
            float cruAmount = myStake / 1000000000f;
            myStakeText.text = "My Stake: " + cruAmount.ToString("F2") + " CRU";
        }

        if (rewardsText != null)
        {
            float cruAmount = pendingRewards / 1000000000f;
            rewardsText.text = "Pending Rewards: " + cruAmount.ToString("F2") + " CRU";
        }
    }

    /// <summary>
    /// Get APY (Annual Percentage Yield)
    /// </summary>
    public float GetAPY()
    {
        // Calculate based on reward rate
        // This is simplified - actual calculation would be more complex
        return (config.stakingRewardRate / 10000f) * 365f * 100f; // Assuming daily epochs
    }
}
