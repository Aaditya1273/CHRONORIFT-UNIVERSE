using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Deflationary Tokenomics System
/// Auto-burn mechanism to reduce CRU supply and increase scarcity
/// </summary>
public class DeflationarySystem : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text totalSupplyText;
    public TMP_Text circulatingSupplyText;
    public TMP_Text totalBurnedText;
    public TMP_Text burnRateText;
    public GameObject burnNotificationPanel;
    public TMP_Text burnNotificationText;

    [Header("Settings")]
    public OneChainConfig config;
    public CRUTokenManager tokenManager;

    [Header("Burn Settings")]
    [Tooltip("Percentage of transactions to burn")]
    [Range(0f, 20f)]
    public float transactionBurnRate = 2f; // 2% burn on transactions
    
    [Tooltip("Percentage of prize pool to burn")]
    [Range(0f, 50f)]
    public float prizePoolBurnRate = 10f; // 10% of prize pool burned
    
    [Tooltip("Auto-burn threshold (when to trigger auto-burn)")]
    public ulong autoBurnThreshold = 1000000000000; // 1000 CRU

    private ulong totalMinted = 0;
    private ulong totalBurned = 0;
    private ulong circulatingSupply = 0;

    // JavaScript interop
    [DllImport("__Internal")]
    private static extern void SendOneChainTransaction(string gameObjectName, string txData, string callbackMethod, string errorMethod);

    [DllImport("__Internal")]
    private static extern void QueryOneChain(string gameObjectName, string queryData, string callbackMethod, string errorMethod);

    void Start()
    {
        config = OneChainConfig.Instance;
        tokenManager = FindObjectOfType<CRUTokenManager>();

        // Fetch token supply data
        FetchSupplyData();
        
        // Start auto-burn check (every 5 minutes)
        InvokeRepeating(nameof(CheckAutoBurn), 300f, 300f);
    }

    /// <summary>
    /// Burn CRU tokens (deflationary mechanism)
    /// </summary>
    public void BurnTokens(ulong amount, string reason)
    {
        if (amount == 0)
        {
            Debug.LogWarning("Cannot burn 0 tokens");
            return;
        }

        Debug.Log("Burning " + (amount / 1000000000f) + " CRU. Reason: " + reason);

        string tx = BuildBurnTransaction(amount);

#if UNITY_WEBGL && !UNITY_EDITOR
        SendOneChainTransaction(gameObject.name, tx, nameof(OnBurnSuccess), nameof(OnBurnError));
#else
        Debug.Log("Burn transaction: " + tx);
        OnBurnSuccess("{\"status\":\"success\",\"amount\":" + amount + ",\"reason\":\"" + reason + "\"}");
#endif
    }

    private string BuildBurnTransaction(ulong amount)
    {
        string tx = @"{
            ""kind"": ""moveCall"",
            ""data"": {
                ""packageObjectId"": """ + config.packageId + @""",
                ""module"": ""cru_token"",
                ""function"": ""burn_tokens"",
                ""arguments"": [
                    """ + config.cruTreasuryId + @""",
                    {""type"": ""coin"", ""amount"": " + amount + @"}
                ],
                ""gasBudget"": 10000000
            }
        }";
        return tx;
    }

    public void OnBurnSuccess(string result)
    {
        Debug.Log("Tokens Burned Successfully!");
        Debug.Log("Result: " + result);
        
        try
        {
            // Parse burn amount from result
            if (result.Contains("amount"))
            {
                // Extract amount
                string amountStr = result.Split(new string[] { "amount\":" }, System.StringSplitOptions.None)[1].Split(',')[0];
                ulong burnedAmount = ulong.Parse(amountStr);
                
                totalBurned += burnedAmount;
                circulatingSupply -= burnedAmount;
                
                // Show burn notification
                ShowBurnNotification(burnedAmount);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to parse burn result: " + e.Message);
        }
        
        // Update UI
        UpdateUI();
        FetchSupplyData();
    }

    public void OnBurnError(string error)
    {
        Debug.LogError("Burn failed: " + error);
    }

    /// <summary>
    /// Calculate burn amount for transaction
    /// </summary>
    public ulong CalculateTransactionBurn(ulong transactionAmount)
    {
        return (ulong)(transactionAmount * (transactionBurnRate / 100f));
    }

    /// <summary>
    /// Calculate burn amount for prize pool
    /// </summary>
    public ulong CalculatePrizePoolBurn(ulong prizePoolAmount)
    {
        return (ulong)(prizePoolAmount * (prizePoolBurnRate / 100f));
    }

    /// <summary>
    /// Auto-burn mechanism (triggered periodically)
    /// </summary>
    public void CheckAutoBurn()
    {
        if (circulatingSupply > autoBurnThreshold)
        {
            // Calculate auto-burn amount (1% of excess supply)
            ulong excessSupply = circulatingSupply - autoBurnThreshold;
            ulong burnAmount = excessSupply / 100;
            
            if (burnAmount > 0)
            {
                Debug.Log("Auto-burn triggered! Burning " + (burnAmount / 1000000000f) + " CRU");
                BurnTokens(burnAmount, "Auto-burn (excess supply)");
            }
        }
    }

    /// <summary>
    /// Fetch token supply data from blockchain
    /// </summary>
    public void FetchSupplyData()
    {
        if (config == null || string.IsNullOrEmpty(config.cruTreasuryId))
        {
            Debug.LogWarning("CRU Treasury not configured");
            return;
        }

        string query = @"{
            ""method"": ""sui_getObject"",
            ""params"": [
                """ + config.cruTreasuryId + @""",
                {""showContent"": true}
            ]
        }";

#if UNITY_WEBGL && !UNITY_EDITOR
        QueryOneChain(gameObject.name, query, nameof(OnSupplyDataFetched), nameof(OnFetchError));
#else
        Debug.Log("Fetching supply data");
        // Simulate data
        totalMinted = 100000000000000; // 100,000 CRU
        totalBurned = 5000000000000;   // 5,000 CRU
        circulatingSupply = totalMinted - totalBurned;
        UpdateUI();
#endif
    }

    public void OnSupplyDataFetched(string result)
    {
        Debug.Log("Supply data fetched: " + result);
        
        try
        {
            // Parse treasury data
            // Extract total_minted, total_burned
            
            circulatingSupply = totalMinted - totalBurned;
            UpdateUI();
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to parse supply data: " + e.Message);
        }
    }

    public void OnFetchError(string error)
    {
        Debug.LogError("Failed to fetch supply data: " + error);
    }

    private void UpdateUI()
    {
        if (totalSupplyText != null)
        {
            float supply = totalMinted / 1000000000f;
            totalSupplyText.text = "Total Minted: " + supply.ToString("N0") + " CRU";
        }

        if (circulatingSupplyText != null)
        {
            float supply = circulatingSupply / 1000000000f;
            circulatingSupplyText.text = "Circulating: " + supply.ToString("N0") + " CRU";
        }

        if (totalBurnedText != null)
        {
            float burned = totalBurned / 1000000000f;
            totalBurnedText.text = "Total Burned: " + burned.ToString("N0") + " CRU 🔥";
        }

        if (burnRateText != null)
        {
            float burnPercentage = totalMinted > 0 ? (totalBurned / (float)totalMinted) * 100f : 0f;
            burnRateText.text = "Burn Rate: " + burnPercentage.ToString("F2") + "%";
        }
    }

    private void ShowBurnNotification(ulong amount)
    {
        if (burnNotificationPanel != null)
        {
            burnNotificationPanel.SetActive(true);
        }

        if (burnNotificationText != null)
        {
            float cruAmount = amount / 1000000000f;
            burnNotificationText.text = "🔥 " + cruAmount.ToString("F2") + " CRU Burned!";
        }

        // Hide after 3 seconds
        Invoke(nameof(HideBurnNotification), 3f);
    }

    private void HideBurnNotification()
    {
        if (burnNotificationPanel != null)
        {
            burnNotificationPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Get burn statistics
    /// </summary>
    public (float totalMinted, float totalBurned, float circulating, float burnRate) GetBurnStats()
    {
        float minted = totalMinted / 1000000000f;
        float burned = totalBurned / 1000000000f;
        float circ = circulatingSupply / 1000000000f;
        float rate = totalMinted > 0 ? (totalBurned / (float)totalMinted) * 100f : 0f;
        
        return (minted, burned, circ, rate);
    }

    /// <summary>
    /// Predict future supply based on burn rate
    /// </summary>
    public float PredictSupplyInDays(int days)
    {
        // Simple prediction based on current burn rate
        float dailyBurnRate = (totalBurned / 1000000000f) / 30f; // Assume 30 days of data
        float futureBurn = dailyBurnRate * days;
        float futureSupply = (circulatingSupply / 1000000000f) - futureBurn;
        
        return Mathf.Max(0, futureSupply);
    }
}
