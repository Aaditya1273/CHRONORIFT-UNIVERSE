"use client";

import { useEffect, useState } from "react";
import { useCurrentAccount } from "@mysten/dapp-kit";
import { useRouter } from "next/navigation";
import { Loader2 } from "lucide-react";

export default function GamePage() {
  const account = useCurrentAccount();
  const router = useRouter();
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [walletChecking, setWalletChecking] = useState(true);

  useEffect(() => {
    // Wait a moment for wallet to initialize
    const timer = setTimeout(() => {
      setWalletChecking(false);
      if (!account) {
        console.log("No wallet connected, redirecting...");
        router.push("/");
        return;
      }
    }, 1500);

    // Don't load game until we have account
    if (!account) {
      return () => clearTimeout(timer);
    }

    setWalletChecking(false);
    clearTimeout(timer);

    // Load Unity game
    const loadUnityGame = async () => {
      try {
        setLoading(true);
        
        // Set wallet address in window BEFORE Unity loads
        if (account?.address) {
          (window as any).preConnectedWalletAddress = account.address;
          console.log("Set pre-connected wallet address:", account.address);
        }
        
        // Check if Unity build files exist
        const buildUrl = "/Build/Temporal Odyssey.loader.js";
        
        // Create Unity container
        const container = document.getElementById("unity-container");
        if (!container) {
          throw new Error("Unity container not found");
        }

        // Load Unity loader script
        const script = document.createElement("script");
        script.src = buildUrl;
        script.async = true;
        
        script.onload = () => {
          // Initialize Unity
          if (typeof (window as any).createUnityInstance === "function") {
            const canvas = document.getElementById("unity-canvas") as HTMLCanvasElement;
            
            (window as any).createUnityInstance(canvas, {
              dataUrl: "/Build/Temporal Odyssey.data",
              frameworkUrl: "/Build/Temporal Odyssey.framework.js",
              codeUrl: "/Build/Temporal Odyssey.wasm",
              streamingAssetsUrl: "StreamingAssets",
              companyName: "ChronoRift",
              productName: "Temporal Odyssey",
              productVersion: "1.0",
            }).then((unityInstance: any) => {
              setLoading(false);
              console.log("Unity game loaded successfully!");
              
              // Store unity instance globally
              (window as any).unityInstance = unityInstance;
              
              // Send wallet address to Unity immediately
              if (account?.address) {
                console.log("Sending wallet address to Unity:", account.address);
                
                // Try multiple game objects that might handle wallet connection
                setTimeout(() => {
                  try {
                    unityInstance.SendMessage("WalletConnector", "OnWalletConnected", account.address);
                    unityInstance.SendMessage("GameManager", "OnWalletConnected", account.address);
                    unityInstance.SendMessage("WalletManager", "SetWalletAddress", account.address);
                    console.log("Wallet address sent to Unity");
                  } catch (e) {
                    console.log("Error sending to Unity:", e);
                  }
                }, 1000);
              }
            }).catch((error: any) => {
              console.error("Unity initialization error:", error);
              setError("Failed to initialize game. Please refresh and try again.");
              setLoading(false);
            });
          } else {
            throw new Error("Unity loader not found");
          }
        };

        script.onerror = () => {
          setError("Game files not found. Please make sure the Unity build is in the /public/Build folder.");
          setLoading(false);
        };

        document.body.appendChild(script);

        return () => {
          document.body.removeChild(script);
        };
      } catch (err: any) {
        console.error("Error loading game:", err);
        setError(err.message || "Failed to load game");
        setLoading(false);
      }
    };

    loadUnityGame();
  }, [account, router]);

  // Show loading while checking wallet
  if (walletChecking) {
    return (
      <div className="min-h-screen bg-black flex items-center justify-center">
        <div className="text-center">
          <Loader2 className="w-16 h-16 text-purple-400 animate-spin mx-auto mb-4" />
          <p className="text-xl text-white font-bold">Checking wallet connection...</p>
        </div>
      </div>
    );
  }

  if (!account) {
    return null; // Will redirect
  }

  return (
    <div className="min-h-screen bg-black flex flex-col">
      {/* Header */}
      <div className="bg-gradient-to-r from-purple-900/50 to-blue-900/50 backdrop-blur-xl border-b border-purple-500/20 p-4">
        <div className="max-w-7xl mx-auto flex justify-between items-center">
          <div>
            <h1 className="text-2xl font-black gradient-text-epic">CHRONORIFT</h1>
            <p className="text-sm text-gray-400">Shattered Epoch Quest</p>
          </div>
          <div className="flex items-center gap-4">
            <div className="glass-strong px-4 py-2 rounded-lg border border-green-500/30">
              <div className="flex items-center gap-2">
                <div className="w-2 h-2 bg-green-400 rounded-full animate-pulse"></div>
                <span className="text-sm font-mono text-green-400">
                  {account.address.slice(0, 6)}...{account.address.slice(-4)}
                </span>
              </div>
            </div>
            <button
              onClick={() => router.push("/")}
              className="px-4 py-2 bg-purple-600 hover:bg-purple-700 rounded-lg font-semibold transition-colors"
            >
              Exit Game
            </button>
          </div>
        </div>
      </div>

      {/* Game Container */}
      <div className="flex-1 flex items-center justify-center p-4">
        {loading && (
          <div className="text-center">
            <Loader2 className="w-16 h-16 text-purple-400 animate-spin mx-auto mb-4" />
            <p className="text-xl text-white font-bold mb-2">Loading ChronoRift Universe...</p>
            <p className="text-gray-400">Initializing game engine</p>
          </div>
        )}

        {error && (
          <div className="text-center max-w-2xl">
            <div className="glass-strong p-8 rounded-2xl border border-red-500/30">
              <h2 className="text-2xl font-bold text-red-400 mb-4">⚠️ Game Loading Error</h2>
              <p className="text-gray-300 mb-6">{error}</p>
              <div className="space-y-4 text-left text-sm text-gray-400">
                <p><strong className="text-white">To fix this:</strong></p>
                <ol className="list-decimal list-inside space-y-2 ml-4">
                  <li>Make sure your Unity WebGL build is in the <code className="bg-black/50 px-2 py-1 rounded">/public/Build</code> folder</li>
                  <li>The build should include these files:
                    <ul className="list-disc list-inside ml-6 mt-2">
                      <li>Temporal Odyssey.loader.js</li>
                      <li>Temporal Odyssey.data</li>
                      <li>Temporal Odyssey.framework.js</li>
                      <li>Temporal Odyssey.wasm</li>
                    </ul>
                  </li>
                  <li>Refresh the page after adding the files</li>
                </ol>
              </div>
              <button
                onClick={() => router.push("/")}
                className="mt-6 px-6 py-3 bg-purple-600 hover:bg-purple-700 rounded-lg font-semibold transition-colors"
              >
                Return to Home
              </button>
            </div>
          </div>
        )}

        {/* Unity Canvas */}
        <div id="unity-container" className={`w-full h-full ${loading || error ? 'hidden' : ''}`}>
          <canvas
            id="unity-canvas"
            className="w-full h-full"
            style={{ background: "#000" }}
          ></canvas>
        </div>
      </div>
    </div>
  );
}
