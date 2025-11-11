using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Tournament Prize Pool Manager
/// Players contribute to prize pool, winners claim rewards
/// </summary>
public class PrizePoolManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text prizePoolText;
    public TMP_Text entryFeeText;
    public TMP_Text participantsText;
    public TMP_Text myContributionText;
    public Button enterTournamentButton;
    public Button claimPrizeButton;
    public GameObject tournamentPanel;

    [Header("Settings")]
    public WalletStore walletStore;
    public OneChainConfig config;
    public CRUTokenManager tokenManager;

    [Header("Tournament Settings")]
    [Tooltip("Entry fee in CRU")]
    public float entryFeeCRU = 0.1f;
    
    [Tooltip("Winner takes percentage")]
    public float winnerPercentage = 90f;
    
    [Tooltip("Burn percentage (deflationary)")]
    public float burnPercentage = 10f;

    private ulong totalPrizePool = 0;
    private int totalParticipants = 0;
    private ulong myContribution = 0;
    private bool hasEntered = false;
    private string prizePoolObjectId = "";

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

        if (enterTournamentButton != null)
            enterTournamentButton.onClick.AddListener(EnterTournament);
        
        if (claimPrizeButton != null)
            claimPrizeButton.onClick.AddListener(ClaimPrize);

        // Initialize prize pool object ID (should be created once)
        InitializePrizePool();
        
        // Fetch current prize pool data
        FetchPrizePoolData();
    }

    /// <summary>
    /// Initialize prize pool (one-time setup)
    /// </summary>
    public void InitializePrizePool()
    {
        if (!string.IsNullOrEmpty(prizePoolObjectId))
        {
            Debug.Log("Prize pool already initialized");
            return;
        }

        // In production, this would be called once by admin
        // For now, we'll use a placeholder ID
        prizePoolObjectId = config.stakingPoolId; // Reuse staking pool for demo
    }

    /// <summary>
    /// Enter tournament by paying entry fee
    /// </summary>
    public void EnterTournament()
    {
        if (hasEntered)
        {
            Debug.LogWarning("Already entered tournament!");
            return;
        }

        ulong entryFeeAmount = (ulong)(entryFeeCRU * 1000000000); // Convert to smallest unit

        string tx = BuildEnterTournamentTransaction(entryFeeAmount);

#if UNITY_WEBGL && !UNITY_EDITOR
        SendOneChainTransaction(gameObject.name, tx, nameof(OnTournamentEntered), nameof(OnTournamentError));
#else
        Debug.Log("Entering tournament with fee: " + entryFeeCRU + " CRU");
        Debug.Log("Transaction: " + tx);
        OnTournamentEntered("{\"status\":\"success\"}");
#endif
    }

    private string BuildEnterTournamentTransaction(ulong entryFee)
    {
        // Build transaction to add to prize pool
        string tx = @"{
            ""kind"": ""moveCall"",
            ""data"": {
                ""packageObjectId"": """ + config.packageId + @""",
                ""module"": ""staking_pool"",
                ""function"": ""stake_tokens"",
                ""arguments"": [
                    """ + prizePoolObjectId + @""",
                    {""type"": ""coin"", ""amount"": " + entryFee + @"}
                ],
                ""gasBudget"": 10000000
            }
        }";
        return tx;
    }

    public void OnTournamentEntered(string result)
    {
        Debug.Log("Tournament Entered Successfully!");
        Debug.Log("Result: " + result);
        
        hasEntered = true;
        ulong entryFeeAmount = (ulong)(entryFeeCRU * 1000000000);
        myContribution += entryFeeAmount;
        
        // Update UI
        FetchPrizePoolData();
        
        // Update token balance
        if (tokenManager != null)
            tokenManager.FetchBalance();
        
        // Show success message
        ShowNotification("Entered Tournament! Entry Fee: " + entryFeeCRU + " CRU");
    }

    public void OnTournamentError(string error)
    {
        Debug.LogError("Tournament entry failed: " + error);
        ShowNotification("Failed to enter tournament: " + error);
    }

    /// <summary>
    /// Claim prize after winning
    /// </summary>
    public void ClaimPrize()
    {
        if (!hasEntered)
        {
            Debug.LogWarning("Must enter tournament first!");
            return;
        }

        // Calculate prize amount (90% of pool)
        ulong prizeAmount = (ulong)(totalPrizePool * (winnerPercentage / 100f));

        string tx = BuildClaimPrizeTransaction();

#if UNITY_WEBGL && !UNITY_EDITOR
        SendOneChainTransaction(gameObject.name, tx, nameof(OnPrizeClaimed), nameof(OnClaimError));
#else
        Debug.Log("Claiming prize: " + (prizeAmount / 1000000000f) + " CRU");
        OnPrizeClaimed("{\"status\":\"success\",\"amount\":" + prizeAmount + "}");
#endif
    }

    private string BuildClaimPrizeTransaction()
    {
        string tx = @"{
            ""kind"": ""moveCall"",
            ""data"": {
                ""packageObjectId"": """ + config.packageId + @""",
                ""module"": ""staking_pool"",
                ""function"": ""claim_rewards"",
                ""arguments"": [
                    """ + prizePoolObjectId + @"""
                ],
                ""gasBudget"": 10000000
            }
        }";
        return tx;
    }

    public void OnPrizeClaimed(string result)
    {
        Debug.Log("Prize Claimed Successfully!");
        Debug.Log("Result: " + result);
        
        try
        {
            // Parse prize amount from result
            ulong prizeAmount = (ulong)(totalPrizePool * (winnerPercentage / 100f));
            float cruAmount = prizeAmount / 1000000000f;
            
            ShowNotification("🏆 Prize Won: " + cruAmount.ToString("F2") + " CRU!");
            
            // Reset tournament state
            hasEntered = false;
            myContribution = 0;
            
            // Refresh data
            FetchPrizePoolData();
            
            if (tokenManager != null)
                tokenManager.FetchBalance();
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to parse prize result: " + e.Message);
        }
    }

    public void OnClaimError(string error)
    {
        Debug.LogError("Prize claim failed: " + error);
        ShowNotification("Failed to claim prize: " + error);
    }

    /// <summary>
    /// Fetch current prize pool data
    /// </summary>
    public void FetchPrizePoolData()
    {
        if (string.IsNullOrEmpty(prizePoolObjectId))
        {
            Debug.LogError("Prize pool not initialized!");
            return;
        }

        string query = @"{
            ""method"": ""sui_getObject"",
            ""params"": [
                """ + prizePoolObjectId + @""",
                {""showContent"": true}
            ]
        }";

#if UNITY_WEBGL && !UNITY_EDITOR
        QueryOneChain(gameObject.name, query, nameof(OnPrizePoolDataFetched), nameof(OnFetchError));
#else
        Debug.Log("Fetching prize pool data");
        // Simulate data in editor
        totalPrizePool = 5000000000; // 5 CRU
        totalParticipants = 10;
        UpdateUI();
#endif
    }

    public void OnPrizePoolDataFetched(string result)
    {
        Debug.Log("Prize pool data fetched: " + result);
        
        try
        {
            // Parse prize pool data
            // Extract total_staked (prize pool), participants count
            
            UpdateUI();
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to parse prize pool data: " + e.Message);
        }
    }

    public void OnFetchError(string error)
    {
        Debug.LogError("Failed to fetch prize pool data: " + error);
    }

    private void UpdateUI()
    {
        if (prizePoolText != null)
        {
            float cruAmount = totalPrizePool / 1000000000f;
            prizePoolText.text = "Prize Pool: " + cruAmount.ToString("F2") + " CRU";
        }

        if (entryFeeText != null)
        {
            entryFeeText.text = "Entry Fee: " + entryFeeCRU.ToString("F2") + " CRU";
        }

        if (participantsText != null)
        {
            participantsText.text = "Participants: " + totalParticipants;
        }

        if (myContributionText != null)
        {
            float cruAmount = myContribution / 1000000000f;
            myContributionText.text = "My Contribution: " + cruAmount.ToString("F2") + " CRU";
        }

        // Update button states
        if (enterTournamentButton != null)
        {
            enterTournamentButton.interactable = !hasEntered;
        }

        if (claimPrizeButton != null)
        {
            claimPrizeButton.interactable = hasEntered;
        }
    }

    private void ShowNotification(string message)
    {
        Debug.Log("Notification: " + message);
        // In production, show UI notification
    }

    /// <summary>
    /// Calculate potential prize for winner
    /// </summary>
    public float GetPotentialPrize()
    {
        return (totalPrizePool / 1000000000f) * (winnerPercentage / 100f);
    }

    /// <summary>
    /// Get burn amount (deflationary)
    /// </summary>
    public float GetBurnAmount()
    {
        return (totalPrizePool / 1000000000f) * (burnPercentage / 100f);
    }
}
