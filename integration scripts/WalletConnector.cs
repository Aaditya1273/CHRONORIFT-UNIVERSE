using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WalletConnector : MonoBehaviour
{
    [Header("Wallet UI Elements")]
    public Button walletConnectButton;
    public TMP_Text accountSelectedLabel;  // Displays the connected account
    public TMP_Text errorLabel;            // Displays error messages
    public TMP_Text walletAddressTMPText;  // Also displays the connected account

    [Header("Panels")]
    public GameObject connectPanel;
    public GameObject successPanel;

    // Internal state variables
    private string selectedAccountAddress;
    public bool isWalletInitialised = false;
    public string currentChainId = "onechain-testnet";

    private WalletStore walletStore;

    void Awake()
    {
        walletStore = FindAnyObjectByType<WalletStore>();
    }

    // JavaScript interop for OneWallet
    [DllImport("__Internal")]
    private static extern void ConnectOneWallet(string gameObjectName, string callbackMethod, string errorMethod);

    [DllImport("__Internal")]
    private static extern void GetOneWalletAddress(string gameObjectName, string callbackMethod);

    [DllImport("__Internal")]
    private static extern bool IsOneWalletAvailable();

    [DllImport("__Internal")]
    private static extern string GetPreConnectedWallet();

    void Start()
    {
        if (walletConnectButton != null)
            walletConnectButton.onClick.AddListener(() => ConnectWallet());

        // Check if wallet is already connected from Next.js
        CheckPreConnectedWallet();
    }

    void CheckPreConnectedWallet()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        try
        {
            string preConnectedAddress = GetPreConnectedWallet();
            if (!string.IsNullOrEmpty(preConnectedAddress))
            {
                Debug.Log("Wallet already connected from web page: " + preConnectedAddress);
                // Automatically connect with the pre-connected wallet
                WalletConnected(preConnectedAddress);
            }
            else
            {
                Debug.Log("No pre-connected wallet found, showing connection UI");
            }
        }
        catch (Exception e)
        {
            Debug.Log("Error checking pre-connected wallet: " + e.Message);
        }
#endif
    }

    // This method can be called from JavaScript to set wallet address
    public void OnWalletConnected(string address)
    {
        Debug.Log("Wallet connected from JavaScript: " + address);
        WalletConnected(address);
    }

    public void ConnectWallet()
    {
        if (errorLabel != null)
            errorLabel.gameObject.SetActive(false);

#if UNITY_WEBGL && !UNITY_EDITOR
        if (IsOneWalletAvailable())
        {
            ConnectOneWallet(gameObject.name, nameof(WalletConnected), nameof(DisplayError));
        }
        else
        {
            DisplayError("OneWallet is not available, please install it");
        }
#else
        DisplayError("ConnectWallet is only available on WebGL builds.");
#endif
    }

    public void WalletConnected(string addressSelected)
    {
        if (!isWalletInitialised)
        {
            isWalletInitialised = true;
        }
        NewAccountSelected(addressSelected);
    }

    public void ChainChanged(string chainId)
    {
        Debug.Log("OneChain Network: " + chainId);
        currentChainId = chainId;
    }

    public void NewAccountSelected(string accountAddress)
    {
        selectedAccountAddress = accountAddress;
        if (accountSelectedLabel != null)
        {
            accountSelectedLabel.text = accountAddress;
            accountSelectedLabel.gameObject.SetActive(true);
        }
        if (walletAddressTMPText != null)
        {
            walletAddressTMPText.text = accountAddress;
            walletAddressTMPText.gameObject.SetActive(true);
        }

        if (connectPanel != null)
            connectPanel.SetActive(false);
        if (successPanel != null)
            successPanel.SetActive(true);

        if (errorLabel != null)
            errorLabel.gameObject.SetActive(false);

        if(walletStore!= null)
            walletStore.StoreWalletInfo();

    }

    public void DisplayError(string errorMessage)
    {
        if (errorLabel != null)
        {
            errorLabel.text = errorMessage;
            errorLabel.gameObject.SetActive(true);
        }
    }

    // Public property to allow other scripts to retrieve the wallet address.
    public string WalletAddress
    {
        get { return selectedAccountAddress; }
    }
}
