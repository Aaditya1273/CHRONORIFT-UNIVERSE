"use client";

import { ConnectButton, useCurrentAccount } from "@mysten/dapp-kit";
import { Gamepad2, Sparkles, Trophy, Zap, Shield, Coins, Users, Target } from "lucide-react";

export function Hero() {
  const account = useCurrentAccount();

  return (
    <section className="relative min-h-screen flex items-center justify-center overflow-hidden">
      {/* Epic Animated Background Grid */}
      <div className="absolute inset-0 overflow-hidden">
        {/* Animated Grid */}
        <div className="absolute inset-0 bg-grid-pattern opacity-20"></div>
        
        {/* Floating Orbs */}
        <div className="absolute w-[500px] h-[500px] bg-purple-500/30 rounded-full blur-3xl animate-float top-20 -left-20"></div>
        <div className="absolute w-[500px] h-[500px] bg-blue-500/30 rounded-full blur-3xl animate-float-delayed top-40 -right-20"></div>
        <div className="absolute w-[400px] h-[400px] bg-pink-500/30 rounded-full blur-3xl animate-float-slow bottom-20 left-1/2 -translate-x-1/2"></div>
        
        {/* Particles */}
        <div className="absolute inset-0">
          {[...Array(20)].map((_, i) => (
            <div
              key={i}
              className="absolute w-1 h-1 bg-white/30 rounded-full animate-particle"
              style={{
                left: `${Math.random() * 100}%`,
                top: `${Math.random() * 100}%`,
                animationDelay: `${Math.random() * 5}s`,
                animationDuration: `${5 + Math.random() * 10}s`,
              }}
            ></div>
          ))}
        </div>
      </div>

      {/* Main Content */}
      <div className="relative z-10 w-full max-w-7xl mx-auto px-4 py-20">
        {/* Top Badge */}
        <div className="flex justify-center mb-8 animate-fade-in-down">
          <div className="glass-strong px-6 py-3 rounded-full border border-purple-500/30 flex items-center gap-3">
            <div className="w-2 h-2 bg-green-400 rounded-full animate-pulse"></div>
            <span className="text-sm font-bold tracking-wider text-purple-300">POWERED BY ONECHAIN</span>
          </div>
        </div>

        {/* Main Title - Symmetrical */}
        <div className="text-center mb-12 space-y-6">
          <h1 className="text-7xl md:text-9xl font-black mb-4 animate-title-glow">
            <span className="gradient-text-epic tracking-tight">CHRONORIFT</span>
          </h1>
          <div className="flex items-center justify-center gap-4">
            <div className="h-px w-20 bg-gradient-to-r from-transparent via-purple-500 to-transparent"></div>
            <h2 className="text-3xl md:text-5xl font-bold text-white/90 tracking-wide">
              UNIVERSE
            </h2>
            <div className="h-px w-20 bg-gradient-to-r from-transparent via-purple-500 to-transparent"></div>
          </div>
          <p className="text-xl md:text-2xl text-purple-400 font-bold tracking-widest animate-pulse-slow">
            SHATTERED EPOCH QUEST
          </p>
        </div>

        {/* Epic Description */}
        <p className="text-center text-lg md:text-xl text-gray-300 max-w-4xl mx-auto mb-12 leading-relaxed animate-fade-in">
          Experience the ultimate <span className="text-purple-400 font-bold">AI-Powered Blockchain Gaming</span> odyssey. 
          Navigate temporal rifts, collect legendary <span className="text-blue-400 font-bold">Bristonite NFTs</span>, 
          earn <span className="text-green-400 font-bold">$CRU tokens</span>, and dominate the global leaderboard.
        </p>

        {/* Feature Pills - Symmetrical Grid */}
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4 max-w-5xl mx-auto mb-12">
          <div className="glass-strong p-4 rounded-xl border border-yellow-500/30 hover:border-yellow-500/60 transition-all duration-300 hover:scale-105 animate-fade-in-up" style={{animationDelay: '0.1s'}}>
            <Sparkles className="w-6 h-6 text-yellow-400 mx-auto mb-2" />
            <span className="text-sm font-bold block text-center">AI Puzzles</span>
          </div>
          <div className="glass-strong p-4 rounded-xl border border-green-500/30 hover:border-green-500/60 transition-all duration-300 hover:scale-105 animate-fade-in-up" style={{animationDelay: '0.2s'}}>
            <Zap className="w-6 h-6 text-green-400 mx-auto mb-2" />
            <span className="text-sm font-bold block text-center">Dynamic Difficulty</span>
          </div>
          <div className="glass-strong p-4 rounded-xl border border-blue-500/30 hover:border-blue-500/60 transition-all duration-300 hover:scale-105 animate-fade-in-up" style={{animationDelay: '0.3s'}}>
            <Coins className="w-6 h-6 text-blue-400 mx-auto mb-2" />
            <span className="text-sm font-bold block text-center">Play-to-Earn</span>
          </div>
          <div className="glass-strong p-4 rounded-xl border border-purple-500/30 hover:border-purple-500/60 transition-all duration-300 hover:scale-105 animate-fade-in-up" style={{animationDelay: '0.4s'}}>
            <Shield className="w-6 h-6 text-purple-400 mx-auto mb-2" />
            <span className="text-sm font-bold block text-center">NFT Rewards</span>
          </div>
        </div>

        {/* CTA Buttons - Symmetrical */}
        <div className="flex flex-col items-center gap-6 mb-12 animate-fade-in-up" style={{animationDelay: '0.5s'}}>
          <div className="flex flex-col sm:flex-row gap-4 items-center">
            {/* Connect Wallet Button */}
            <div className="sui-connect-button-wrapper transform hover:scale-105 transition-transform duration-300">
              <ConnectButton />
            </div>

            {/* Play Button */}
            <button
              onClick={() => {
                if (account) {
                  window.location.href = "/game";
                } else {
                  alert("Please connect your wallet first!");
                }
              }}
              className={`group relative px-10 py-5 rounded-xl font-bold text-xl transition-all duration-300 flex items-center gap-3 overflow-hidden ${
                account
                  ? "bg-gradient-to-r from-green-600 via-emerald-600 to-green-600 hover:scale-105 shadow-2xl hover:shadow-green-500/50"
                  : "bg-gray-700/50 cursor-not-allowed opacity-50"
              }`}
              disabled={!account}
            >
              <div className="absolute inset-0 bg-gradient-to-r from-transparent via-white/20 to-transparent translate-x-[-200%] group-hover:translate-x-[200%] transition-transform duration-1000"></div>
              <Gamepad2 className="w-7 h-7 relative z-10" />
              <span className="relative z-10">PLAY NOW</span>
            </button>
          </div>

          {/* Connected Status */}
          {account && (
            <div className="glass-strong px-8 py-4 rounded-xl border border-green-500/50 animate-fade-in">
              <div className="flex items-center gap-4">
                <div className="relative">
                  <div className="w-4 h-4 bg-green-400 rounded-full animate-pulse"></div>
                  <div className="absolute inset-0 w-4 h-4 bg-green-400 rounded-full animate-ping"></div>
                </div>
                <div>
                  <div className="text-xs text-gray-400 mb-1">CONNECTED</div>
                  <div className="font-mono font-bold text-green-400">
                    {account.address.slice(0, 8)}...{account.address.slice(-6)}
                  </div>
                </div>
              </div>
            </div>
          )}
        </div>

        {/* Stats Grid - Perfectly Symmetrical */}
        <div className="grid grid-cols-2 md:grid-cols-4 gap-6 max-w-6xl mx-auto">
          <div className="glass-strong p-8 rounded-2xl border border-purple-500/30 hover:border-purple-500/60 transition-all duration-300 hover:scale-105 group animate-fade-in-up" style={{animationDelay: '0.6s'}}>
            <Trophy className="w-10 h-10 text-purple-400 mx-auto mb-4 group-hover:scale-110 transition-transform" />
            <div className="text-5xl font-black text-purple-400 mb-2 text-center">4</div>
            <div className="text-sm text-gray-400 text-center font-semibold">UNIQUE WORLDS</div>
          </div>
          
          <div className="glass-strong p-8 rounded-2xl border border-blue-500/30 hover:border-blue-500/60 transition-all duration-300 hover:scale-105 group animate-fade-in-up" style={{animationDelay: '0.7s'}}>
            <Target className="w-10 h-10 text-blue-400 mx-auto mb-4 group-hover:scale-110 transition-transform" />
            <div className="text-5xl font-black text-blue-400 mb-2 text-center">∞</div>
            <div className="text-sm text-gray-400 text-center font-semibold">AI PUZZLES</div>
          </div>
          
          <div className="glass-strong p-8 rounded-2xl border border-green-500/30 hover:border-green-500/60 transition-all duration-300 hover:scale-105 group animate-fade-in-up" style={{animationDelay: '0.8s'}}>
            <Coins className="w-10 h-10 text-green-400 mx-auto mb-4 group-hover:scale-110 transition-transform" />
            <div className="text-5xl font-black text-green-400 mb-2 text-center">$CRU</div>
            <div className="text-sm text-gray-400 text-center font-semibold">EARN TOKENS</div>
          </div>
          
          <div className="glass-strong p-8 rounded-2xl border border-pink-500/30 hover:border-pink-500/60 transition-all duration-300 hover:scale-105 group animate-fade-in-up" style={{animationDelay: '0.9s'}}>
            <Users className="w-10 h-10 text-pink-400 mx-auto mb-4 group-hover:scale-110 transition-transform" />
            <div className="text-5xl font-black text-pink-400 mb-2 text-center">1K+</div>
            <div className="text-sm text-gray-400 text-center font-semibold">PLAYERS</div>
          </div>
        </div>
      </div>

      {/* Scroll Indicator */}
      <div className="absolute bottom-8 left-1/2 transform -translate-x-1/2 animate-bounce-slow">
        <div className="w-8 h-12 border-2 border-purple-500/50 rounded-full flex justify-center p-2">
          <div className="w-1.5 h-3 bg-purple-400 rounded-full animate-scroll-indicator"></div>
        </div>
      </div>
    </section>
  );
}
