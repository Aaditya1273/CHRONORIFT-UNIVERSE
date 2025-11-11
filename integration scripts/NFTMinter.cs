using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NFTMinter : MonoBehaviour
{
    [Header("Wallet Connector Reference")]
    public WalletStore walletStore;

    [Header("Minting Settings")]
    [Tooltip("Level ID for minting")]
    public string levelIdToMint;
    [Tooltip("World name (Desert, Lava, Space)")]
    public string worldName;
    [Tooltip("Score achieved")]
    public string scoreAchieved;
    [Tooltip("Metadata URI for minting")]
    public string metadataURIToMint;

    [Header("OneChain Contract Settings")]
    [Tooltip("OneChain RPC endpoint")]
    public string rpcURL = "https://rpc-testnet.onelabs.cc:443";
    [Tooltip("Package ID (deployed Move contracts)")]
    public string packageId = "";
    [Tooltip("Player Progress Object ID")]
    public string playerProgressId = "";

    [Header("Optional UI Elements")]
    public Button mintNFTButton;      // Optionally call minting from a UI button.
    public Button fetchNFTsButton;    // Optionally call fetching from a UI button.
    public Transform nftListView;     // Container for NFT images if needed.

    // JavaScript interop for OneChain transactions
    [DllImport("__Internal")]
    private static extern void SendOneChainTransaction(string gameObjectName, string txData, string callbackMethod, string errorMethod);

    void Start()
    {
        if (mintNFTButton != null)
            mintNFTButton.onClick.AddListener(() => MintNFT());
        if (fetchNFTsButton != null)
            fetchNFTsButton.onClick.AddListener(() => FetchPlayerNFTs());

        walletStore = FindFirstObjectByType<WalletStore>();
    }

    public void MintNFTWithParams(string levelId, string world, string score, string metadataURI)
    {
        levelIdToMint = levelId;
        worldName = world;
        scoreAchieved = score;
        metadataURIToMint = metadataURI;
        MintNFT();
    }

    /// <summary>
    /// Mints an NFT on OneChain by calling the Move contract
    /// </summary>
    public void MintNFT()
    {
        if (string.IsNullOrEmpty(packageId))
        {
            Debug.LogError("Package ID not set! Deploy Move contracts first.");
            return;
        }

        if (string.IsNullOrEmpty(playerProgressId))
        {
            Debug.LogError("Player Progress ID not set! Create player progress first.");
            return;
        }

        // Build Move transaction for minting NFT
        string txData = BuildMintTransaction();
        
#if UNITY_WEBGL && !UNITY_EDITOR
        SendOneChainTransaction(gameObject.name, txData, nameof(OnMintSuccess), nameof(OnMintError));
#else
        Debug.Log("Minting is only available on WebGL builds");
        Debug.Log("Transaction data: " + txData);
#endif
    }

    private string BuildMintTransaction()
    {
        // Build transaction to call level_nft::mint_level_nft
        string tx = @"{
            ""kind"": ""moveCall"",
            ""data"": {
                ""packageObjectId"": """ + packageId + @""",
                ""module"": ""level_nft"",
                ""function"": ""mint_level_nft"",
                ""arguments"": [
                    """ + playerProgressId + @""",
                    " + levelIdToMint + @",
                    """ + worldName + @""",
                    " + scoreAchieved + @",
                    """ + metadataURIToMint + @"""
                ],
                ""gasBudget"": 10000000
            }
        }";
        
        return tx;
    }

    public void OnMintSuccess(string result)
    {
        Debug.Log("NFT Minted Successfully!");
        Debug.Log("Transaction Result: " + result);
        // You can parse the result and update UI here
    }

    public void OnMintError(string error)
    {
        Debug.LogError("Minting Failed: " + error);
    }

    /// <summary>
    /// Fetches player's NFTs from OneChain
    /// </summary>
    public void FetchPlayerNFTs()
    {
        if (walletStore == null || string.IsNullOrEmpty(walletStore.PermaWalletAddressText.text))
        {
            Debug.LogError("Wallet not connected!");
            return;
        }

        string playerAddress = walletStore.PermaWalletAddressText.text;
        Debug.Log("Fetching NFTs for player: " + playerAddress);
        
        // Query OneChain for player's NFTs
        // This would use the OneChain RPC to query owned objects
        // Implementation depends on OneChain SDK
        
        Debug.Log("NFT fetching feature - coming soon!");
    }

    /// <summary>
    /// Create player progress on OneChain (first time setup)
    /// </summary>
    public void CreatePlayerProgress()
    {
        if (string.IsNullOrEmpty(packageId))
        {
            Debug.LogError("Package ID not set!");
            return;
        }

        string tx = @"{
            ""kind"": ""moveCall"",
            ""data"": {
                ""packageObjectId"": """ + packageId + @""",
                ""module"": ""level_nft"",
                ""function"": ""create_player_progress"",
                ""arguments"": [],
                ""gasBudget"": 10000000
            }
        }";

#if UNITY_WEBGL && !UNITY_EDITOR
        SendOneChainTransaction(gameObject.name, tx, nameof(OnProgressCreated), nameof(OnMintError));
#else
        Debug.Log("Create progress is only available on WebGL builds");
#endif
    }

    public void OnProgressCreated(string result)
    {
        Debug.Log("Player Progress Created!");
        Debug.Log("Result: " + result);
        // Parse result to get the playerProgressId and store it
    }
}
