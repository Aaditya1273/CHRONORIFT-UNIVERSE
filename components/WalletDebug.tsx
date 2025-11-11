"use client";

import { useEffect, useState } from "react";

export function WalletDebug() {
  const [walletInfo, setWalletInfo] = useState<any>(null);

  useEffect(() => {
    if (typeof window === "undefined") return;

    const checkWallets = () => {
      const info = {
        suiWallet: typeof (window as any).suiWallet,
        oneWallet: typeof (window as any).oneWallet,
        wallet: typeof (window as any).wallet,
        allWalletProps: Object.keys(window).filter(key => 
          key.toLowerCase().includes('wallet') || key.toLowerCase().includes('sui')
        ),
        userAgent: navigator.userAgent,
        extensions: (window as any).chrome?.runtime ? "Chrome extensions available" : "No chrome runtime"
      };
      
      setWalletInfo(info);
    };

    // Check immediately and after delay
    checkWallets();
    setTimeout(checkWallets, 2000);
  }, []);

  if (!walletInfo) return null;

  return (
    <div className="fixed bottom-4 right-4 bg-black/90 text-white p-4 rounded-lg text-xs max-w-md z-50">
      <h3 className="font-bold mb-2">🔍 Wallet Debug Info</h3>
      <div className="space-y-1">
        <div>window.suiWallet: <span className="text-green-400">{walletInfo.suiWallet}</span></div>
        <div>window.oneWallet: <span className="text-green-400">{walletInfo.oneWallet}</span></div>
        <div>window.wallet: <span className="text-green-400">{walletInfo.wallet}</span></div>
        <div className="mt-2 pt-2 border-t border-gray-700">
          <div className="font-semibold">Wallet Properties Found:</div>
          <div className="text-gray-400">{walletInfo.allWalletProps.join(", ") || "None"}</div>
        </div>
        <div className="mt-2 pt-2 border-t border-gray-700 text-gray-400 text-[10px]">
          {walletInfo.extensions}
        </div>
      </div>
      <button 
        onClick={() => {
          console.log("Full window object keys:", Object.keys(window));
          console.log("window.suiWallet:", (window as any).suiWallet);
          console.log("window.oneWallet:", (window as any).oneWallet);
        }}
        className="mt-2 text-blue-400 hover:text-blue-300 text-xs"
      >
        Log to Console
      </button>
    </div>
  );
}
