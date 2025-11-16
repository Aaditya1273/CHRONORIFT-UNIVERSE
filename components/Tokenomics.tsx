"use client";

import { Coins, TrendingUp, Lock, Flame, Users, Wallet } from "lucide-react";

export function Tokenomics() {
  const tokenStats = [
    {
      icon: Coins,
      label: "Total Supply",
      value: "1,000,000,000",
      suffix: "$CRU",
      color: "from-yellow-500 to-orange-500",
    },
    {
      icon: TrendingUp,
      label: "Circulating",
      value: "250,000,000",
      suffix: "$CRU",
      color: "from-green-500 to-emerald-500",
    },
    {
      icon: Lock,
      label: "Staked",
      value: "150,000,000",
      suffix: "$CRU",
      color: "from-blue-500 to-cyan-500",
    },
    {
      icon: Flame,
      label: "Burned",
      value: "50,000,000",
      suffix: "$CRU",
      color: "from-red-500 to-pink-500",
    },
  ];

  const distribution = [
    { label: "Play-to-Earn Rewards", percentage: 40, color: "bg-green-500" },
    { label: "Staking & DeFi", percentage: 25, color: "bg-blue-500" },
    { label: "Team & Development", percentage: 15, color: "bg-purple-500" },
    { label: "Marketing & Partnerships", percentage: 10, color: "bg-pink-500" },
    { label: "Liquidity Pool", percentage: 10, color: "bg-yellow-500" },
  ];

  const utilities = [
    {
      icon: Wallet,
      title: "In-Game Currency",
      description: "Use $CRU to purchase power-ups, unlock levels, and customize your character.",
    },
    {
      icon: Lock,
      title: "Staking Rewards",
      description: "Stake $CRU tokens to earn passive income and exclusive NFT drops.",
    },
    {
      icon: Users,
      title: "Governance",
      description: "Vote on game updates, new features, and community proposals.",
    },
    {
      icon: TrendingUp,
      title: "DeFi Integration",
      description: "Provide liquidity, farm yields, and trade on OneDEX marketplace.",
    },
  ];

  return (
    <section className="relative py-32 px-4 overflow-hidden">
      {/* Background Effects */}
      <div className="absolute inset-0">
        <div className="absolute inset-0 bg-gradient-to-b from-transparent via-green-900/10 to-transparent"></div>
        <div className="absolute w-96 h-96 bg-green-500/10 rounded-full blur-3xl top-20 right-20 animate-float"></div>
        <div className="absolute w-96 h-96 bg-yellow-500/10 rounded-full blur-3xl bottom-20 left-20 animate-float-delayed"></div>
      </div>

      <div className="relative z-10 max-w-7xl mx-auto">
        {/* Section Header */}
        <div className="text-center mb-20">
          <div className="inline-block mb-4">
            <div className="glass-strong px-6 py-2 rounded-full border border-green-500/30">
              <span className="text-sm font-bold tracking-wider text-green-300">$CRU TOKEN</span>
            </div>
          </div>
          <h2 className="text-5xl md:text-7xl font-black mb-6 gradient-text-epic">
            TOKENOMICS
          </h2>
          <p className="text-xl text-gray-400 max-w-3xl mx-auto">
            Deflationary tokenomics designed for sustainable growth and long-term value creation
          </p>
        </div>

        {/* Token Stats Grid */}
        <div className="grid grid-cols-2 md:grid-cols-4 gap-6 mb-20">
          {tokenStats.map((stat, index) => (
            <div
              key={index}
              className="glass-strong p-8 rounded-2xl border border-white/10 hover:border-white/20 transition-all duration-300 hover:scale-105 animate-fade-in-up"
              style={{ animationDelay: `${index * 0.1}s` }}
            >
              <div className={`w-14 h-14 rounded-xl bg-gradient-to-br ${stat.color} p-3 mb-4 mx-auto`}>
                <stat.icon className="w-full h-full text-white" />
              </div>
              <div className="text-center">
                <div className="text-3xl font-black text-white mb-1">{stat.value}</div>
                <div className="text-sm text-gray-400 mb-2">{stat.suffix}</div>
                <div className="text-xs text-gray-500 font-semibold">{stat.label}</div>
              </div>
            </div>
          ))}
        </div>

        {/* Distribution & Utilities Grid */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-12 mb-20">
          {/* Token Distribution */}
          <div className="glass-strong p-8 rounded-3xl border border-white/10">
            <h3 className="text-2xl font-black mb-8 text-white">Token Distribution</h3>
            <div className="space-y-6">
              {distribution.map((item, index) => (
                <div key={index} className="animate-fade-in-up" style={{ animationDelay: `${index * 0.1}s` }}>
                  <div className="flex justify-between mb-2">
                    <span className="text-gray-300 font-semibold">{item.label}</span>
                    <span className="text-white font-bold">{item.percentage}%</span>
                  </div>
                  <div className="h-3 bg-gray-800 rounded-full overflow-hidden">
                    <div
                      className={`h-full ${item.color} rounded-full transition-all duration-1000 ease-out`}
                      style={{ width: `${item.percentage}%` }}
                    ></div>
                  </div>
                </div>
              ))}
            </div>
          </div>

          {/* Token Utilities */}
          <div className="glass-strong p-8 rounded-3xl border border-white/10">
            <h3 className="text-2xl font-black mb-8 text-white">Token Utility</h3>
            <div className="space-y-6">
              {utilities.map((utility, index) => (
                <div
                  key={index}
                  className="flex gap-4 animate-fade-in-up"
                  style={{ animationDelay: `${index * 0.1}s` }}
                >
                  <div className="w-12 h-12 rounded-xl bg-gradient-to-br from-green-500 to-emerald-500 p-3 flex-shrink-0">
                    <utility.icon className="w-full h-full text-white" />
                  </div>
                  <div>
                    <h4 className="font-bold text-white mb-1">{utility.title}</h4>
                    <p className="text-sm text-gray-400 leading-relaxed">{utility.description}</p>
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>

        {/* Deflationary Mechanism */}
        <div className="glass-strong p-10 rounded-3xl border border-red-500/30 text-center">
          <div className="inline-flex items-center gap-3 mb-6">
            <Flame className="w-8 h-8 text-red-400" />
            <h3 className="text-3xl font-black text-white">Deflationary Mechanism</h3>
            <Flame className="w-8 h-8 text-red-400" />
          </div>
          <p className="text-gray-300 max-w-3xl mx-auto leading-relaxed">
            <span className="font-bold text-red-400">5% of all transactions</span> are automatically burned, 
            reducing total supply over time. <span className="font-bold text-orange-400">Prize pool distributions</span> also 
            include token burns, creating continuous deflationary pressure and increasing scarcity.
          </p>
        </div>

        {/* Bottom CTA */}
        <div className="text-center mt-16">
          <div className="glass-strong inline-block px-8 py-4 rounded-xl border border-green-500/30">
            <p className="text-gray-300">
              <span className="font-bold text-green-400">Audited Smart Contracts</span> • 
              <span className="font-bold text-blue-400 ml-2">Fully On-Chain</span> • 
              <span className="font-bold text-purple-400 ml-2">Community Governed</span>
            </p>
          </div>
        </div>
      </div>
    </section>
  );
}
