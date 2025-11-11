using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Real World Asset (RWA) Manager
/// Tokenize Bristonite shards and in-game assets as NFTs
/// </summary>
public class RWAManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text rwaBalanceText;
    public GameObject rwaPanel;

    [Header("Settings")]
    public WalletStore walletStore;
    public OneChainConfig config;

    [Header("RWA Types")]
    public enum RWAType
    {
        BristoniteQuartz,   // Common - 🪙
        BristoniteSolar,    // Uncommon - 🟡
        BristoniteAether,   // Rare - 🟢
        BristoniteNova,     // Epic - 🔴
        BristonitePrime     // Legendary - 💠
    }

    // Track player's RWA holdings
    private int quartzCount = 0;
    private int solarCount = 0;
    private int aetherCount = 0;
    private int novaCount = 0;
    private int primeCount = 0;

    // JavaScript interop
    [DllImport("__Internal")]
    private static extern void SendOneChainTransaction(string gameObjectName, string txData, string callbackMethod, string errorMethod);

    void Start()
    {
        walletStore = FindObjectOfType<WalletStore>();
        config = OneChainConfig.Instance;
        
        FetchRWABalance();
    }

    /// <summary>
    /// Mint RWA NFT for collected Bristonite
    /// </summary>
    public void MintBristoniteRWA(RWAType type, int quantity)
    {
        if (config == null || string.IsNullOrEmpty(config.packageId))
        {
            Debug.LogError("Package not configured!");
            return;
        }

        string rwaName = GetRWAName(type);
        string metadata = BuildRWAMetadata(type, quantity);

        string tx = BuildMintRWATransaction(rwaName, metadata, quantity);

#if UNITY_WEBGL && !UNITY_EDITOR
        SendOneChainTransaction(gameObject.name, tx, nameof(OnRWAMinted), nameof(OnRWAError));
#else
        Debug.Log("Minting RWA: " + rwaName + " x" + quantity);
        Debug.Log("Transaction: " + tx);
        OnRWAMinted("{\"status\":\"success\",\"type\":\"" + type.ToString() + "\",\"quantity\":" + quantity + "}");
#endif
    }

    private string BuildMintRWATransaction(string name, string metadata, int quantity)
    {
        // This would call a custom RWA minting function in Move
        string tx = @"{
            ""kind"": ""moveCall"",
            ""data"": {
                ""packageObjectId"": """ + config.packageId + @""",
                ""module"": ""level_nft"",
                ""function"": ""mint_level_nft"",
                ""arguments"": [
                    """ + config.playerProgressId + @""",
                    999,
                    """ + name + @""",
                    " + quantity + @",
                    """ + metadata + @"""
                ],
                ""gasBudget"": 10000000
            }
        }";
        return tx;
    }

    private string GetRWAName(RWAType type)
    {
        switch (type)
        {
            case RWAType.BristoniteQuartz: return "Bristonite Quartz Shard";
            case RWAType.BristoniteSolar: return "Bristonite Solar Crystal";
            case RWAType.BristoniteAether: return "Bristonite Aether Fragment";
            case RWAType.BristoniteNova: return "Bristonite Nova Core";
            case RWAType.BristonitePrime: return "Bristonite Prime Essence";
            default: return "Unknown Bristonite";
        }
    }

    private string BuildRWAMetadata(RWAType type, int quantity)
    {
        // Build IPFS metadata for RWA
        string rarity = GetRarity(type);
        string description = GetDescription(type);
        
        string metadata = @"{
            ""name"": """ + GetRWAName(type) + @""",
            ""description"": """ + description + @""",
            ""image"": ""ipfs://QmBristonite" + type.ToString() + @""",
            ""attributes"": [
                {""trait_type"": ""Rarity"", ""value"": """ + rarity + @"""},
                {""trait_type"": ""Quantity"", ""value"": " + quantity + @"},
                {""trait_type"": ""Type"", ""value"": ""RWA""},
                {""trait_type"": ""Asset_Class"", ""value"": ""Bristonite""},
                {""trait_type"": ""Tradeable"", ""value"": true}
            ]
        }";
        
        return metadata;
    }

    private string GetRarity(RWAType type)
    {
        switch (type)
        {
            case RWAType.BristoniteQuartz: return "Common";
            case RWAType.BristoniteSolar: return "Uncommon";
            case RWAType.BristoniteAether: return "Rare";
            case RWAType.BristoniteNova: return "Epic";
            case RWAType.BristonitePrime: return "Legendary";
            default: return "Unknown";
        }
    }

    private string GetDescription(RWAType type)
    {
        switch (type)
        {
            case RWAType.BristoniteQuartz:
                return "Basic temporal energy shard. Used for simple epoch manipulations.";
            case RWAType.BristoniteSolar:
                return "Solar-charged crystal. Enhances temporal warp capabilities.";
            case RWAType.BristoniteAether:
                return "Ethereal fragment from the void. Grants anti-gravity anomaly resistance.";
            case RWAType.BristoniteNova:
                return "Nova core from collapsed timelines. Unlocks advanced epoch mechanics.";
            case RWAType.BristonitePrime:
                return "Legendary essence of pure chronorift energy. Ultimate temporal power.";
            default:
                return "Unknown Bristonite type.";
        }
    }

    public void OnRWAMinted(string result)
    {
        Debug.Log("RWA NFT Minted Successfully!");
        Debug.Log("Result: " + result);
        
        // Parse result to update counts
        try
        {
            if (result.Contains("BristoniteQuartz"))
                quartzCount++;
            else if (result.Contains("BristoniteSolar"))
                solarCount++;
            else if (result.Contains("BristoniteAether"))
                aetherCount++;
            else if (result.Contains("BristoniteNova"))
                novaCount++;
            else if (result.Contains("BristonitePrime"))
                primeCount++;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to parse RWA result: " + e.Message);
        }
        
        UpdateRWADisplay();
    }

    public void OnRWAError(string error)
    {
        Debug.LogError("RWA minting failed: " + error);
    }

    /// <summary>
    /// Fetch player's RWA balance
    /// </summary>
    public void FetchRWABalance()
    {
        if (walletStore == null || string.IsNullOrEmpty(walletStore.PermaWalletAddressText.text))
        {
            Debug.LogError("Wallet not connected!");
            return;
        }

        Debug.Log("Fetching RWA balance...");
        
        // In a real implementation, query OneChain for RWA NFTs
        // For now, use stored counts
        UpdateRWADisplay();
    }

    private void UpdateRWADisplay()
    {
        if (rwaBalanceText != null)
        {
            string display = "Bristonite Assets:\n";
            display += "🪙 Quartz: " + quartzCount + "\n";
            display += "🟡 Solar: " + solarCount + "\n";
            display += "🟢 Aether: " + aetherCount + "\n";
            display += "🔴 Nova: " + novaCount + "\n";
            display += "💠 Prime: " + primeCount;
            
            rwaBalanceText.text = display;
        }
    }

    /// <summary>
    /// Trade RWA on OneDEX (placeholder)
    /// </summary>
    public void TradeRWAOnDEX(RWAType type, int quantity, ulong pricePerUnit)
    {
        Debug.Log("Trading " + quantity + "x " + type.ToString() + " on OneDEX for " + pricePerUnit + " CRU each");
        
        // This would integrate with OneDEX
        // Create a listing or execute a swap
        
        Debug.Log("OneDEX integration - coming soon!");
    }

    /// <summary>
    /// Convert RWA to CRU tokens (burn mechanism)
    /// </summary>
    public void ConvertRWAToCRU(RWAType type, int quantity)
    {
        ulong cruAmount = GetRWAValue(type) * (ulong)quantity;
        
        Debug.Log("Converting " + quantity + "x " + type.ToString() + " to " + cruAmount + " CRU");
        
        // This would burn the RWA NFT and mint CRU tokens
        // Deflationary mechanism
        
        Debug.Log("RWA conversion - coming soon!");
    }

    private ulong GetRWAValue(RWAType type)
    {
        // Value in CRU (smallest unit)
        switch (type)
        {
            case RWAType.BristoniteQuartz: return 10000000; // 0.01 CRU
            case RWAType.BristoniteSolar: return 50000000; // 0.05 CRU
            case RWAType.BristoniteAether: return 100000000; // 0.1 CRU
            case RWAType.BristoniteNova: return 500000000; // 0.5 CRU
            case RWAType.BristonitePrime: return 1000000000; // 1 CRU
            default: return 0;
        }
    }

    /// <summary>
    /// Get total RWA count
    /// </summary>
    public int GetTotalRWACount()
    {
        return quartzCount + solarCount + aetherCount + novaCount + primeCount;
    }
}
