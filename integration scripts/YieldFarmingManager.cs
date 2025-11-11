using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// DeFi Yield Farming Manager
/// Earn passive income by providing liquidity or staking
/// </summary>
public class YieldFarmingManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text apyText;
    public TMP_Text myYieldText;
    public TMP_Text totalValueLockedText;
    public Button harvestButton;
    public GameObject yieldPanel;

    [Header("Settings")]
    public WalletStore walletStore;
    public OneChainConfig config;
    public StakingManager stakingManager;
    public OneDEXIntegration dexIntegration;

    [Header("Yield Settings")]
    [Tooltip("Base APY for staking")]
    public float baseStakingAPY = 15f; // 15% APY
    
    [Tooltip("Bonus APY for liquidity providers")]
    public float liquidityBonusAPY = 25f; // 25% APY
    
    [Tooltip("Compound frequency (days)")]
    public int compoundFrequency = 7; // Weekly compounding

    private ulong totalValueLocked = 0;
    private ulong myStakedAmount = 0;
    private ulong pendingYield = 0;
    private ulong lastHarvestTime = 0;

    // JavaScript interop
    [DllImport("__Internal")]
    private static extern void SendOneChainTransaction(string gameObjectName, string txData, string callbackMethod, string errorMethod);

    void Start()
    {
        walletStore = FindObjectOfType<WalletStore>();
        config = OneChainConfig.Instance;
        stakingManager = FindObjectOfType<StakingManager>();
        dexIntegration = FindObjectOfType<OneDEXIntegration>();

        if (harvestButton != null)
            harvestButton.onClick.AddListener(HarvestYield);

        // Calculate yield every minute
        InvokeRepeating(nameof(CalculateYield), 60f, 60f);
        
        // Fetch initial data
        FetchYieldData();
    }

    /// <summary>
    /// Calculate pending yield based on staked amount and time
    /// </summary>
    public void CalculateYield()
    {
        if (myStakedAmount == 0)
        {
            pendingYield = 0;
            UpdateUI();
            return;
        }

        // Calculate time elapsed since last harvest (in seconds)
        ulong currentTime = (ulong)System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        ulong timeElapsed = lastHarvestTime > 0 ? currentTime - lastHarvestTime : 0;

        if (timeElapsed == 0)
        {
            lastHarvestTime = currentTime;
            return;
        }

        // Calculate yield
        // Formula: (stakedAmount * APY * timeElapsed) / (365 * 24 * 60 * 60)
        float totalAPY = baseStakingAPY;
        
        // Add bonus APY if providing liquidity
        if (dexIntegration != null && dexIntegration.GetTotalLiquidity() > 0)
        {
            totalAPY += liquidityBonusAPY;
        }

        float secondsInYear = 365f * 24f * 60f * 60f;
        float yieldRate = totalAPY / 100f / secondsInYear;
        ulong calculatedYield = (ulong)(myStakedAmount * yieldRate * timeElapsed);

        pendingYield += calculatedYield;

        Debug.Log("Yield calculated: " + (calculatedYield / 1000000000f) + " CRU");
        Debug.Log("Total pending yield: " + (pendingYield / 1000000000f) + " CRU");

        UpdateUI();
    }

    /// <summary>
    /// Harvest accumulated yield
    /// </summary>
    public void HarvestYield()
    {
        if (pendingYield == 0)
        {
            Debug.LogWarning("No yield to harvest!");
            return;
        }

        Debug.Log("Harvesting " + (pendingYield / 1000000000f) + " CRU yield");

        string tx = BuildHarvestTransaction();

#if UNITY_WEBGL && !UNITY_EDITOR
        SendOneChainTransaction(gameObject.name, tx, nameof(OnHarvestSuccess), nameof(OnHarvestError));
#else
        Debug.Log("Harvest transaction: " + tx);
        OnHarvestSuccess("{\"status\":\"success\",\"amount\":" + pendingYield + "}");
#endif
    }

    private string BuildHarvestTransaction()
    {
        // Claim rewards from staking pool
        string tx = @"{
            ""kind"": ""moveCall"",
            ""data"": {
                ""packageObjectId"": """ + config.packageId + @""",
                ""module"": ""staking_pool"",
                ""function"": ""claim_rewards"",
                ""arguments"": [
                    """ + config.stakingPoolId + @"""
                ],
                ""gasBudget"": 10000000
            }
        }";
        return tx;
    }

    public void OnHarvestSuccess(string result)
    {
        Debug.Log("Yield Harvested Successfully!");
        Debug.Log("Result: " + result);

        ulong harvestedAmount = pendingYield;
        
        // Reset pending yield
        pendingYield = 0;
        lastHarvestTime = (ulong)System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // Show notification
        ShowHarvestNotification(harvestedAmount);

        UpdateUI();
    }

    public void OnHarvestError(string error)
    {
        Debug.LogError("Harvest failed: " + error);
    }

    /// <summary>
    /// Auto-compound yield (reinvest into staking)
    /// </summary>
    public void AutoCompound()
    {
        if (pendingYield == 0)
        {
            Debug.LogWarning("No yield to compound!");
            return;
        }

        Debug.Log("Auto-compounding " + (pendingYield / 1000000000f) + " CRU");

        // Harvest and immediately restake
        if (stakingManager != null)
        {
            // This would harvest and restake in one transaction
            Debug.Log("Auto-compound feature - implementation needed");
        }
    }

    /// <summary>
    /// Fetch yield farming data
    /// </summary>
    public void FetchYieldData()
    {
        // Get staked amount from staking manager
        if (stakingManager != null)
        {
            myStakedAmount = (ulong)(stakingManager.GetTotalStaked() * 1000000000);
        }

        // Get total value locked
        totalValueLocked = myStakedAmount; // Simplified

        // Calculate initial yield
        CalculateYield();

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (apyText != null)
        {
            float totalAPY = baseStakingAPY;
            if (dexIntegration != null && dexIntegration.GetTotalLiquidity() > 0)
            {
                totalAPY += liquidityBonusAPY;
            }
            apyText.text = "APY: " + totalAPY.ToString("F1") + "%";
        }

        if (myYieldText != null)
        {
            float yield = pendingYield / 1000000000f;
            myYieldText.text = "Pending Yield: " + yield.ToString("F4") + " CRU";
        }

        if (totalValueLockedText != null)
        {
            float tvl = totalValueLocked / 1000000000f;
            totalValueLockedText.text = "TVL: " + tvl.ToString("F2") + " CRU";
        }

        if (harvestButton != null)
        {
            harvestButton.interactable = pendingYield > 0;
        }
    }

    private void ShowHarvestNotification(ulong amount)
    {
        float cruAmount = amount / 1000000000f;
        Debug.Log("🌾 Harvested: " + cruAmount.ToString("F4") + " CRU!");
    }

    /// <summary>
    /// Calculate projected earnings
    /// </summary>
    public float CalculateProjectedEarnings(float stakedAmount, int days)
    {
        float totalAPY = baseStakingAPY + liquidityBonusAPY;
        float dailyRate = totalAPY / 365f / 100f;
        float projectedEarnings = stakedAmount * dailyRate * days;
        
        return projectedEarnings;
    }

    /// <summary>
    /// Get current APY
    /// </summary>
    public float GetCurrentAPY()
    {
        float totalAPY = baseStakingAPY;
        if (dexIntegration != null && dexIntegration.GetTotalLiquidity() > 0)
        {
            totalAPY += liquidityBonusAPY;
        }
        return totalAPY;
    }

    /// <summary>
    /// Get pending yield amount
    /// </summary>
    public float GetPendingYield()
    {
        return pendingYield / 1000000000f;
    }
}
