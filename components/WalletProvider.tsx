"use client";

import { createContext, useContext, useState, useEffect, ReactNode } from "react";

interface WalletContextType {
  connected: boolean;
  address: string | null;
  walletName: string | null;
  connect: () => Promise<void>;
  disconnect: () => void;
}

const WalletContext = createContext<WalletContextType | undefined>(undefined);

export function WalletProvider({ children }: { children: ReactNode }) {
  const [connected, setConnected] = useState(false);
  const [address, setAddress] = useState<string | null>(null);
  const [walletName, setWalletName] = useState<string | null>(null);

  const connect = async () => {
    try {
      if (typeof window === "undefined") return;

      console.log("=== Starting Wallet Connection ===");
      
      // Log ALL window properties to find the wallet
      console.log("All window properties:", Object.keys(window).filter(key => 
        key.toLowerCase().includes('wallet') || 
        key.toLowerCase().includes('sui') ||
        key.toLowerCase().includes('one')
      ));

      // Wait longer for wallet injection
      await new Promise(resolve => setTimeout(resolve, 2000));

      // Check all possible wallet locations
      const walletChecks = {
        suiWallet: (window as any).suiWallet,
        oneWallet: (window as any).oneWallet,
        wallet: (window as any).wallet,
        sui: (window as any).sui,
        ethereum: (window as any).ethereum, // Some wallets use this
        wallets: (window as any).wallets, // Wallet Standard
      };

      console.log("Wallet availability:", walletChecks);
      console.log("Wallet types:", Object.entries(walletChecks).map(([k, v]) => [k, typeof v]));

      // Check Wallet Standard (new standard for Sui wallets)
      if ((window as any).wallets) {
        console.log("Found wallets array:", (window as any).wallets);
        const walletStandard = (window as any).wallets;
        if (walletStandard && walletStandard.length > 0) {
          console.log("Using Wallet Standard, found wallets:", walletStandard);
          const oneWallet = walletStandard.find((w: any) => 
            w.name?.toLowerCase().includes('one') || 
            w.name?.toLowerCase().includes('sui')
          );
          if (oneWallet) {
            console.log("Found OneWallet via Wallet Standard:", oneWallet);
            // Use the wallet standard wallet
            const accounts = await oneWallet.features['standard:connect'].connect();
            if (accounts && accounts.accounts && accounts.accounts.length > 0) {
              const addr = accounts.accounts[0].address;
              setAddress(addr);
              setConnected(true);
              setWalletName(oneWallet.name || "OneWallet");
              localStorage.setItem("walletConnected", "true");
              localStorage.setItem("walletAddress", addr);
              console.log("=== Connected via Wallet Standard! ===");
              return;
            }
          }
        }
      }

      // Find the first available wallet (legacy method)
      const activeWallet = walletChecks.suiWallet || 
                          walletChecks.oneWallet || 
                          walletChecks.wallet ||
                          walletChecks.sui;

      if (!activeWallet) {
        console.error("No wallet found!");
        console.error("Please check:");
        console.error("1. Is OneWallet extension installed?");
        console.error("2. Is OneWallet enabled in your browser?");
        console.error("3. Try refreshing the page");
        
        // Show a more helpful error
        const allKeys = Object.keys(window).slice(0, 50); // First 50 keys
        console.error("Sample window properties:", allKeys);
        
        alert("OneWallet not detected!\n\nPlease:\n1. Make sure OneWallet extension is installed\n2. Make sure OneWallet is enabled\n3. Refresh the page\n4. Check browser console (F12) for details");
        return;
      }

      console.log("Found wallet:", activeWallet);
      console.log("Wallet methods:", Object.keys(activeWallet));

      // Try different connection methods
      let accounts: any[] = [];
      
      try {
        // Method 1: Standard Wallet API (most common for Sui wallets)
        if (typeof activeWallet.requestPermissions === "function") {
          console.log("Trying requestPermissions...");
          const result = await activeWallet.requestPermissions();
          console.log("requestPermissions result:", result);
          accounts = result?.accounts || [];
        }
        // Method 2: Direct connect
        else if (typeof activeWallet.connect === "function") {
          console.log("Trying connect...");
          const result = await activeWallet.connect();
          console.log("connect result:", result);
          accounts = result?.accounts || [result] || [];
        }
        // Method 3: getAccounts (if already connected)
        else if (typeof activeWallet.getAccounts === "function") {
          console.log("Trying getAccounts...");
          accounts = await activeWallet.getAccounts();
          console.log("getAccounts result:", accounts);
        }
        // Method 4: Check if accounts already exist
        else if (activeWallet.accounts) {
          console.log("Using existing accounts...");
          accounts = activeWallet.accounts;
        }
      } catch (err: any) {
        console.error("Connection method error:", err);
        throw new Error(`Connection failed: ${err.message}`);
      }

      console.log("Final accounts:", accounts);

      if (!accounts || accounts.length === 0) {
        throw new Error("No accounts returned. Please unlock OneWallet and try again.");
      }

      // Extract address from account
      const account = accounts[0];
      let addr: string;

      if (typeof account === "string") {
        addr = account;
      } else if (account.address) {
        addr = account.address;
      } else if (account.publicKey) {
        // Derive address from public key if needed
        addr = account.publicKey;
      } else {
        throw new Error("Could not extract address from account");
      }

      console.log("Connected address:", addr);
      
      setAddress(addr);
      setConnected(true);
      setWalletName("OneWallet");
      
      // Store in localStorage
      localStorage.setItem("walletConnected", "true");
      localStorage.setItem("walletAddress", addr);

      console.log("=== Connection Successful! ===");
    } catch (error: any) {
      console.error("=== Connection Error ===");
      console.error(error);
      alert(`Failed to connect wallet:\n\n${error.message || "Unknown error"}\n\nPlease:\n1. Make sure OneWallet is unlocked\n2. Refresh the page\n3. Try again`);
    }
  };

  const disconnect = () => {
    setConnected(false);
    setAddress(null);
    setWalletName(null);
    localStorage.removeItem("walletConnected");
    localStorage.removeItem("walletAddress");
  };

  // Auto-connect on mount if previously connected
  useEffect(() => {
    const wasConnected = localStorage.getItem("walletConnected");
    const savedAddress = localStorage.getItem("walletAddress");
    
    if (wasConnected && savedAddress) {
      setConnected(true);
      setAddress(savedAddress);
      setWalletName("OneWallet");
    }

    // Log wallet availability for debugging
    if (typeof window !== "undefined") {
      setTimeout(() => {
        console.log("=== Wallet Detection ===");
        console.log("window.suiWallet:", typeof (window as any).suiWallet);
        console.log("window.oneWallet:", typeof (window as any).oneWallet);
        console.log("window.wallet:", typeof (window as any).wallet);
        
        // List all wallet-related properties
        const walletProps = Object.keys(window).filter(key => 
          key.toLowerCase().includes('wallet') || key.toLowerCase().includes('sui')
        );
        console.log("Wallet-related properties:", walletProps);
        console.log("======================");
      }, 1000);
    }
  }, []);

  return (
    <WalletContext.Provider value={{ connected, address, walletName, connect, disconnect }}>
      {children}
    </WalletContext.Provider>
  );
}

export function useWallet() {
  const context = useContext(WalletContext);
  if (context === undefined) {
    throw new Error("useWallet must be used within a WalletProvider");
  }
  return context;
}
