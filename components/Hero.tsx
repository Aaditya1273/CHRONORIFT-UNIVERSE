"use client";

import { useWallet } from "./WalletProvider";
import { Gamepad2, Wallet, Sparkles } from "lucide-react";

export function Hero() {
  const { connected, address, connect } = useWallet();

  const shortAddress = address
    ? `${address.slice(0, 6)}...${address.slice(-4)}`
    : null;

  return (
    <section className="relative min-h-screen flex items-center justify-center overflow-hidden px-4">
      {/* Animated background elements */}
      <div className="absolute inset-0 overflow-hidden">
        <div className="absolute w-96 h-96 bg-purple-500/20 rounded-full blur-3xl animate-float top-20 left-10"></div>
        <div className="absolute w-96 h-96 bg-blue-500/20 rounded-full blur-3xl animate-float top-40 right-10 animation-delay-2000"></div>
        <div className="absolute w-96 h-96 bg-pink-500/20 rounded-full blur-3xl animate-float bottom-20 left-1/2 animation-delay-4000"></div>
      </div>

      {/* Content */}
      <div className="relative z-10 max-w-6xl mx-auto text-center">
        {/* Logo/Title */}
        <div className="mb-8 animate-slide-up">
          <h1 className="text-6xl md:text-8xl font-bold mb-4 gradient-text">
            CHRONORIFT
          </h1>
          <h2 className="text-3xl md:text-5xl font-bold text-white/90 mb-6">
            UNIVERSE
          </h2>
          <p className="text-xl md:text-2xl text-purple-300 font-semibold">
            Shattered Epoch Quest
          </p>
        </div>

        {/* Description */}
        <p className="text-lg md:text-xl text-gray-300 max-w-3xl mx-auto mb-12 animate-fade-in">
          AI-Powered Blockchain Puzzle Odyssey on OneChain. Navigate temporal rifts,
          collect Bristonite NFTs, earn $CRU tokens, and compete globally in this
          revolutionary Play-to-Earn experience.
        </p>

        {/* Features badges */}
        <div className="flex flex-wrap justify-center gap-4 mb-12 animate-fade-in">
          <div className="glass px-6 py-3 rounded-full flex items-center gap-2">
            <Sparkles className="w-5 h-5 text-yellow-400" />
            <span className="text-sm font-semibold">AI-Generated Puzzles</span>
          </div>
          <div className="glass px-6 py-3 rounded-full flex items-center gap-2">
            <Gamepad2 className="w-5 h-5 text-green-400" />
            <span className="text-sm font-semibold">Dynamic Difficulty</span>
          </div>
          <div className="glass px-6 py-3 rounded-full flex items-center gap-2">
            <Wallet className="w-5 h-5 text-purple-400" />
            <span className="text-sm font-semibold">Play-to-Earn</span>
          </div>
        </div>

        {/* CTA Buttons */}
        <div className="flex flex-col sm:flex-row gap-4 justify-center items-center animate-slide-up">
          {!connected ? (
            <button
              onClick={connect}
              className="group relative px-8 py-4 bg-gradient-to-r from-purple-600 to-pink-600 rounded-xl font-bold text-lg hover:scale-105 transition-all duration-300 shadow-lg hover:shadow-purple-500/50 flex items-center gap-3"
            >
              <Wallet className="w-6 h-6" />
              Connect Wallet
              <div className="absolute inset-0 rounded-xl bg-white/20 opacity-0 group-hover:opacity-100 transition-opacity"></div>
            </button>
          ) : (
            <div className="glass px-8 py-4 rounded-xl flex items-center gap-3">
              <div className="w-3 h-3 bg-green-400 rounded-full animate-pulse"></div>
              <span className="font-semibold">{shortAddress}</span>
            </div>
          )}

          <button
            onClick={() => {
              if (connected) {
                window.location.href = "/game";
              } else {
                alert("Please connect your wallet first!");
              }
            }}
            className={`px-8 py-4 rounded-xl font-bold text-lg transition-all duration-300 flex items-center gap-3 ${
              connected
                ? "bg-gradient-to-r from-green-600 to-blue-600 hover:scale-105 shadow-lg hover:shadow-green-500/50"
                : "bg-gray-700 cursor-not-allowed opacity-50"
            }`}
            disabled={!connected}
          >
            <Gamepad2 className="w-6 h-6" />
            Play Now
          </button>
        </div>

        {/* Stats */}
        <div className="mt-16 grid grid-cols-2 md:grid-cols-4 gap-6 max-w-4xl mx-auto">
          <div className="glass p-6 rounded-xl">
            <div className="text-3xl font-bold text-purple-400 mb-2">4</div>
            <div className="text-sm text-gray-400">Unique Worlds</div>
          </div>
          <div className="glass p-6 rounded-xl">
            <div className="text-3xl font-bold text-blue-400 mb-2">∞</div>
            <div className="text-sm text-gray-400">AI Puzzles</div>
          </div>
          <div className="glass p-6 rounded-xl">
            <div className="text-3xl font-bold text-green-400 mb-2">$CRU</div>
            <div className="text-sm text-gray-400">Earn Tokens</div>
          </div>
          <div className="glass p-6 rounded-xl">
            <div className="text-3xl font-bold text-pink-400 mb-2">NFT</div>
            <div className="text-sm text-gray-400">Collectibles</div>
          </div>
        </div>
      </div>

      {/* Scroll indicator */}
      <div className="absolute bottom-8 left-1/2 transform -translate-x-1/2 animate-bounce">
        <div className="w-6 h-10 border-2 border-white/30 rounded-full flex justify-center">
          <div className="w-1 h-3 bg-white/50 rounded-full mt-2 animate-pulse"></div>
        </div>
      </div>
    </section>
  );
}
