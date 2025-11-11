using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// OneDEX Integration for ChronoRift Universe
/// Trade CRU tokens and RWA NFTs on decentralized exchange
/// </summary>
public class OneDEXIntegration : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text cruPriceText;
    public TMP_Text liquidityText;
    public TMP_InputField swapAmountInput;
    public TMP_Dropdown swapFromDropdown;
    public TMP_Dropdown swapToDropdown;
    public Button swapButton;
    public Button addLiquidityButton;

    [Header("Settings")]
    public WalletStore walletStore;
    public OneChainConfig config;
    public CRUTokenManager tokenManager;

    [Header("OneDEX Settings")]
    [Tooltip("OneDEX Router Contract")]
    public string oneDEXRouter = "";
    
    [Tooltip("CRU/OCT Liquidity Pool")]
    public string cruOctPoolId = "";

    private float cruPriceInOCT = 0.001f; // 1 CRU = 0.001 OCT
    private ulong totalLiquidity = 0;

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

        if (swapButton != null)
            swapButton.onClick.AddListener(ExecuteSwap);
        
        if (addLiquidityButton != null)
            addLiquidityButton.onClick.AddListener(AddLiquidity);

        // Fetch OneDEX data
        FetchDEXData();
    }

    /// <summary>
    /// Execute token swap on OneDEX
    /// </summary>
    public void ExecuteSwap()
    {
        if (swapAmountInput == null || string.IsNullOrEmpty(swapAmountInput.text))
        {
            Debug.LogError("Please enter swap amount!");
            return;
        }

        if (!float.TryParse(swapAmountInput.text, out float amount))
        {
            Debug.LogError("Invalid amount!");
            return;
        }

        string fromToken = swapFromDropdown != null ? swapFromDropdown.options[swapFromDropdown.value].text : "CRU";
        string toToken = swapToDropdown != null ? swapToDropdown.options[swapToDropdown.value].text : "OCT";

        Debug.Log("Swapping " + amount + " " + fromToken + " to " + toToken);

        ulong amountIn = (ulong)(amount * 1000000000); // Convert to smallest unit
        
        string tx = BuildSwapTransaction(fromToken, toToken, amountIn);

#if UNITY_WEBGL && !UNITY_EDITOR
        SendOneChainTransaction(gameObject.name, tx, nameof(OnSwapSuccess), nameof(OnSwapError));
#else
        Debug.Log("Swap transaction: " + tx);
        OnSwapSuccess("{\"status\":\"success\"}");
#endif
    }

    private string BuildSwapTransaction(string fromToken, string toToken, ulong amountIn)
    {
        // Build OneDEX swap transaction
        // This is a simplified version - actual implementation would be more complex
        
        string tx = @"{
            ""kind"": ""moveCall"",
            ""data"": {
                ""packageObjectId"": """ + oneDEXRouter + @""",
                ""module"": ""router"",
                ""function"": ""swap_exact_input"",
                ""typeArguments"": [
                    """ + GetTokenType(fromToken) + @""",
                    """ + GetTokenType(toToken) + @"""
                ],
                ""arguments"": [
                    """ + cruOctPoolId + @""",
                    {""type"": ""coin"", ""amount"": " + amountIn + @"},
                    0
                ],
                ""gasBudget"": 10000000
            }
        }";
        return tx;
    }

    private string GetTokenType(string token)
    {
        if (token == "CRU")
            return config.packageId + "::cru_token::CRU";
        else if (token == "OCT")
            return "0x2::sui::SUI"; // OneChain native token
        else
            return "";
    }

    public void OnSwapSuccess(string result)
    {
        Debug.Log("Swap Successful!");
        Debug.Log("Result: " + result);
        
        // Clear input
        if (swapAmountInput != null)
            swapAmountInput.text = "";
        
        // Update balances
        if (tokenManager != null)
            tokenManager.FetchBalance();
        
        FetchDEXData();
        
        ShowNotification("Swap completed successfully!");
    }

    public void OnSwapError(string error)
    {
        Debug.LogError("Swap failed: " + error);
        ShowNotification("Swap failed: " + error);
    }

    /// <summary>
    /// Add liquidity to CRU/OCT pool
    /// </summary>
    public void AddLiquidity()
    {
        Debug.Log("Adding liquidity to CRU/OCT pool");
        
        // This would add liquidity to the pool
        // User provides both CRU and OCT tokens
        // Receives LP tokens in return
        
        string tx = BuildAddLiquidityTransaction(100000000, 100000); // 0.1 CRU + 0.0001 OCT

#if UNITY_WEBGL && !UNITY_EDITOR
        SendOneChainTransaction(gameObject.name, tx, nameof(OnLiquidityAdded), nameof(OnLiquidityError));
#else
        Debug.Log("Add liquidity transaction");
        OnLiquidityAdded("{\"status\":\"success\"}");
#endif
    }

    private string BuildAddLiquidityTransaction(ulong cruAmount, ulong octAmount)
    {
        string tx = @"{
            ""kind"": ""moveCall"",
            ""data"": {
                ""packageObjectId"": """ + oneDEXRouter + @""",
                ""module"": ""router"",
                ""function"": ""add_liquidity"",
                ""typeArguments"": [
                    """ + config.packageId + @"::cru_token::CRU"",
                    ""0x2::sui::SUI""
                ],
                ""arguments"": [
                    """ + cruOctPoolId + @""",
                    {""type"": ""coin"", ""amount"": " + cruAmount + @"},
                    {""type"": ""coin"", ""amount"": " + octAmount + @"},
                    0,
                    0
                ],
                ""gasBudget"": 10000000
            }
        }";
        return tx;
    }

    public void OnLiquidityAdded(string result)
    {
        Debug.Log("Liquidity Added Successfully!");
        Debug.Log("Result: " + result);
        
        FetchDEXData();
        
        if (tokenManager != null)
            tokenManager.FetchBalance();
        
        ShowNotification("Liquidity added successfully!");
    }

    public void OnLiquidityError(string error)
    {
        Debug.LogError("Add liquidity failed: " + error);
        ShowNotification("Failed to add liquidity: " + error);
    }

    /// <summary>
    /// Fetch OneDEX data (price, liquidity)
    /// </summary>
    public void FetchDEXData()
    {
        if (string.IsNullOrEmpty(cruOctPoolId))
        {
            Debug.LogWarning("OneDEX pool not configured");
            return;
        }

        string query = @"{
            ""method"": ""sui_getObject"",
            ""params"": [
                """ + cruOctPoolId + @""",
                {""showContent"": true}
            ]
        }";

#if UNITY_WEBGL && !UNITY_EDITOR
        QueryOneChain(gameObject.name, query, nameof(OnDEXDataFetched), nameof(OnFetchError));
#else
        Debug.Log("Fetching OneDEX data");
        // Simulate data
        cruPriceInOCT = 0.001f;
        totalLiquidity = 10000000000; // 10 CRU equivalent
        UpdateUI();
#endif
    }

    public void OnDEXDataFetched(string result)
    {
        Debug.Log("OneDEX data fetched: " + result);
        
        try
        {
            // Parse pool data
            // Extract reserves, calculate price
            
            UpdateUI();
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to parse DEX data: " + e.Message);
        }
    }

    public void OnFetchError(string error)
    {
        Debug.LogError("Failed to fetch DEX data: " + error);
    }

    private void UpdateUI()
    {
        if (cruPriceText != null)
        {
            cruPriceText.text = "1 CRU = " + cruPriceInOCT.ToString("F6") + " OCT";
        }

        if (liquidityText != null)
        {
            float liquidity = totalLiquidity / 1000000000f;
            liquidityText.text = "Total Liquidity: " + liquidity.ToString("F2") + " CRU";
        }
    }

    private void ShowNotification(string message)
    {
        Debug.Log("Notification: " + message);
    }

    /// <summary>
    /// Calculate output amount for swap
    /// </summary>
    public float CalculateSwapOutput(float inputAmount, bool cruToOct)
    {
        if (cruToOct)
        {
            return inputAmount * cruPriceInOCT;
        }
        else
        {
            return inputAmount / cruPriceInOCT;
        }
    }

    /// <summary>
    /// Get current CRU price in OCT
    /// </summary>
    public float GetCRUPrice()
    {
        return cruPriceInOCT;
    }

    /// <summary>
    /// Get total liquidity in pool
    /// </summary>
    public float GetTotalLiquidity()
    {
        return totalLiquidity / 1000000000f;
    }
}
