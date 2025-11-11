using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// GameFi Dashboard - Central hub for all DeFi features
/// Displays comprehensive financial overview
/// </summary>
public class GameFiDashboard : MonoBehaviour
{
    [Header("Dashboard Panels")]
    public GameObject overviewPanel;
    public GameObject stakingPanel;
    public GameObject yieldPanel;
    public GameObject dexPanel;
    public GameObject rwaPanel;
    public GameObject prizePoolPanel;

    [Header("Overview UI")]
    public TMP_Text totalPortfolioValueText;
    public TMP_Text cruBalanceText;
    public TMP_Text stakedBalanceText;
    public TMP_Text yieldEarnedText;
    public TMP_Text nftCountText;
    public TMP_Text rwaCountText;

    [Header("Quick Stats")]
    public TMP_Text currentAPYText;
    public TMP_Text burnRateText;
    public TMP_Text cruPriceText;
    public TMP_Text marketCapText;

    [Header("Managers")]
    public CRUTokenManager tokenManager;
    public StakingManager stakingManager;
    public YieldFarmingManager yieldManager;
    public OneDEXIntegration dexIntegration;
    public RWAManager rwaManager;
    public PrizePoolManager prizePoolManager;
    public DeflationarySystem deflationarySystem;

    [Header("Navigation Buttons")]
    public Button overviewButton;
    public Button stakingButton;
    public Button yieldButton;
    public Button dexButton;
    public Button rwaButton;
    public Button prizePoolButton;

    void Start()
    {
        // Find all managers
        tokenManager = FindObjectOfType<CRUTokenManager>();
        stakingManager = FindObjectOfType<StakingManager>();
        yieldManager = FindObjectOfType<YieldFarmingManager>();
        dexIntegration = FindObjectOfType<OneDEXIntegration>();
        rwaManager = FindObjectOfType<RWAManager>();
        prizePoolManager = FindObjectOfType<PrizePoolManager>();
        deflationarySystem = FindObjectOfType<DeflationarySystem>();

        // Setup navigation
        if (overviewButton != null)
            overviewButton.onClick.AddListener(() => ShowPanel("overview"));
        if (stakingButton != null)
            stakingButton.onClick.AddListener(() => ShowPanel("staking"));
        if (yieldButton != null)
            yieldButton.onClick.AddListener(() => ShowPanel("yield"));
        if (dexButton != null)
            dexButton.onClick.AddListener(() => ShowPanel("dex"));
        if (rwaButton != null)
            rwaButton.onClick.AddListener(() => ShowPanel("rwa"));
        if (prizePoolButton != null)
            prizePoolButton.onClick.AddListener(() => ShowPanel("prizepool"));

        // Show overview by default
        ShowPanel("overview");

        // Update dashboard every 5 seconds
        InvokeRepeating(nameof(UpdateDashboard), 0f, 5f);
    }

    /// <summary>
    /// Update all dashboard data
    /// </summary>
    public void UpdateDashboard()
    {
        UpdateOverview();
        UpdateQuickStats();
    }

    private void UpdateOverview()
    {
        // Calculate total portfolio value
        float cruBalance = tokenManager != null ? tokenManager.GetBalance() / 1000000000f : 0f;
        float stakedBalance = stakingManager != null ? stakingManager.GetTotalStaked() : 0f;
        float yieldEarned = yieldManager != null ? yieldManager.GetPendingYield() : 0f;
        int nftCount = 0; // Would fetch from NFTMinter
        int rwaCount = rwaManager != null ? rwaManager.GetTotalRWACount() : 0;

        float totalValue = cruBalance + stakedBalance + yieldEarned;

        // Update UI
        if (totalPortfolioValueText != null)
        {
            totalPortfolioValueText.text = totalValue.ToString("F2") + " CRU";
        }

        if (cruBalanceText != null)
        {
            cruBalanceText.text = "Wallet: " + cruBalance.ToString("F2") + " CRU";
        }

        if (stakedBalanceText != null)
        {
            stakedBalanceText.text = "Staked: " + stakedBalance.ToString("F2") + " CRU";
        }

        if (yieldEarnedText != null)
        {
            yieldEarnedText.text = "Yield: " + yieldEarned.ToString("F4") + " CRU";
        }

        if (nftCountText != null)
        {
            nftCountText.text = "NFTs: " + nftCount;
        }

        if (rwaCountText != null)
        {
            rwaCountText.text = "RWA: " + rwaCount;
        }
    }

    private void UpdateQuickStats()
    {
        // Current APY
        if (currentAPYText != null && yieldManager != null)
        {
            float apy = yieldManager.GetCurrentAPY();
            currentAPYText.text = "APY: " + apy.ToString("F1") + "%";
        }

        // Burn Rate
        if (burnRateText != null && deflationarySystem != null)
        {
            var stats = deflationarySystem.GetBurnStats();
            burnRateText.text = "Burn Rate: " + stats.burnRate.ToString("F2") + "%";
        }

        // CRU Price
        if (cruPriceText != null && dexIntegration != null)
        {
            float price = dexIntegration.GetCRUPrice();
            cruPriceText.text = "1 CRU = " + price.ToString("F6") + " OCT";
        }

        // Market Cap (simplified)
        if (marketCapText != null && deflationarySystem != null)
        {
            var stats = deflationarySystem.GetBurnStats();
            float marketCap = stats.circulating * (dexIntegration != null ? dexIntegration.GetCRUPrice() : 0.001f);
            marketCapText.text = "Market Cap: " + marketCap.ToString("F2") + " OCT";
        }
    }

    /// <summary>
    /// Show specific panel
    /// </summary>
    public void ShowPanel(string panelName)
    {
        // Hide all panels
        if (overviewPanel != null) overviewPanel.SetActive(false);
        if (stakingPanel != null) stakingPanel.SetActive(false);
        if (yieldPanel != null) yieldPanel.SetActive(false);
        if (dexPanel != null) dexPanel.SetActive(false);
        if (rwaPanel != null) rwaPanel.SetActive(false);
        if (prizePoolPanel != null) prizePoolPanel.SetActive(false);

        // Show selected panel
        switch (panelName.ToLower())
        {
            case "overview":
                if (overviewPanel != null) overviewPanel.SetActive(true);
                break;
            case "staking":
                if (stakingPanel != null) stakingPanel.SetActive(true);
                break;
            case "yield":
                if (yieldPanel != null) yieldPanel.SetActive(true);
                break;
            case "dex":
                if (dexPanel != null) dexPanel.SetActive(true);
                break;
            case "rwa":
                if (rwaPanel != null) rwaPanel.SetActive(true);
                break;
            case "prizepool":
                if (prizePoolPanel != null) prizePoolPanel.SetActive(true);
                break;
        }

        Debug.Log("Showing panel: " + panelName);
    }

    /// <summary>
    /// Get comprehensive portfolio summary
    /// </summary>
    public string GetPortfolioSummary()
    {
        float cruBalance = tokenManager != null ? tokenManager.GetBalance() / 1000000000f : 0f;
        float stakedBalance = stakingManager != null ? stakingManager.GetTotalStaked() : 0f;
        float yieldEarned = yieldManager != null ? yieldManager.GetPendingYield() : 0f;
        float totalValue = cruBalance + stakedBalance + yieldEarned;

        string summary = "=== ChronoRift Universe Portfolio ===\n";
        summary += "Total Value: " + totalValue.ToString("F2") + " CRU\n";
        summary += "Wallet Balance: " + cruBalance.ToString("F2") + " CRU\n";
        summary += "Staked: " + stakedBalance.ToString("F2") + " CRU\n";
        summary += "Pending Yield: " + yieldEarned.ToString("F4") + " CRU\n";
        
        if (yieldManager != null)
        {
            summary += "Current APY: " + yieldManager.GetCurrentAPY().ToString("F1") + "%\n";
        }

        if (deflationarySystem != null)
        {
            var stats = deflationarySystem.GetBurnStats();
            summary += "Burn Rate: " + stats.burnRate.ToString("F2") + "%\n";
        }

        summary += "====================================";

        return summary;
    }

    /// <summary>
    /// Refresh all data
    /// </summary>
    public void RefreshAll()
    {
        Debug.Log("Refreshing all GameFi data...");

        if (tokenManager != null) tokenManager.FetchBalance();
        if (stakingManager != null) stakingManager.FetchStakingInfo();
        if (yieldManager != null) yieldManager.FetchYieldData();
        if (dexIntegration != null) dexIntegration.FetchDEXData();
        if (rwaManager != null) rwaManager.FetchRWABalance();
        if (prizePoolManager != null) prizePoolManager.FetchPrizePoolData();
        if (deflationarySystem != null) deflationarySystem.FetchSupplyData();

        UpdateDashboard();
    }
}
