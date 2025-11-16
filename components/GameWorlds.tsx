"use client";

import { Clock, Zap, Mountain, Skull } from "lucide-react";

export function GameWorlds() {
  const worlds = [
    {
      id: 1,
      name: "TEMPORAL NEXUS",
      icon: Clock,
      description: "Navigate through time rifts and solve chronological puzzles in this mind-bending dimension.",
      color: "from-blue-500 to-cyan-500",
      borderColor: "border-blue-500/30 hover:border-blue-500/60",
      bgGradient: "from-blue-900/20 to-cyan-900/20",
    },
    {
      id: 2,
      name: "QUANTUM REALM",
      icon: Zap,
      description: "Master quantum mechanics and probability-based challenges in this electrifying world.",
      color: "from-purple-500 to-pink-500",
      borderColor: "border-purple-500/30 hover:border-purple-500/60",
      bgGradient: "from-purple-900/20 to-pink-900/20",
    },
    {
      id: 3,
      name: "CRYSTAL PEAKS",
      icon: Mountain,
      description: "Climb through crystalline mountains and solve geometric puzzles in this frozen landscape.",
      color: "from-emerald-500 to-teal-500",
      borderColor: "border-emerald-500/30 hover:border-emerald-500/60",
      bgGradient: "from-emerald-900/20 to-teal-900/20",
    },
    {
      id: 4,
      name: "VOID ABYSS",
      icon: Skull,
      description: "Face the ultimate challenge in this dark dimension where only the bravest survive.",
      color: "from-red-500 to-orange-500",
      borderColor: "border-red-500/30 hover:border-red-500/60",
      bgGradient: "from-red-900/20 to-orange-900/20",
    },
  ];

  return (
    <section className="relative py-32 px-4 overflow-hidden">
      {/* Background Effects */}
      <div className="absolute inset-0">
        <div className="absolute inset-0 bg-gradient-to-b from-transparent via-blue-900/10 to-transparent"></div>
        <div className="absolute w-96 h-96 bg-blue-500/10 rounded-full blur-3xl top-20 left-20 animate-float"></div>
        <div className="absolute w-96 h-96 bg-purple-500/10 rounded-full blur-3xl bottom-20 right-20 animate-float-delayed"></div>
      </div>

      <div className="relative z-10 max-w-7xl mx-auto">
        {/* Section Header */}
        <div className="text-center mb-20">
          <div className="inline-block mb-4">
            <div className="glass-strong px-6 py-2 rounded-full border border-blue-500/30">
              <span className="text-sm font-bold tracking-wider text-blue-300">EXPLORE WORLDS</span>
            </div>
          </div>
          <h2 className="text-5xl md:text-7xl font-black mb-6 gradient-text-epic">
            4 UNIQUE DIMENSIONS
          </h2>
          <p className="text-xl text-gray-400 max-w-3xl mx-auto">
            Each world offers unique challenges, puzzles, and rewards. Master them all to become a ChronoRift legend.
          </p>
        </div>

        {/* Worlds Grid */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
          {worlds.map((world, index) => (
            <div
              key={world.id}
              className={`group relative glass-strong rounded-3xl border ${world.borderColor} overflow-hidden transition-all duration-500 hover:scale-105 hover:-translate-y-2 animate-fade-in-up`}
              style={{ animationDelay: `${index * 0.15}s` }}
            >
              {/* Background Gradient */}
              <div className={`absolute inset-0 bg-gradient-to-br ${world.bgGradient} opacity-0 group-hover:opacity-100 transition-opacity duration-500`}></div>

              {/* Content */}
              <div className="relative p-8">
                {/* World Number */}
                <div className="absolute top-4 right-4 text-6xl font-black text-white/5 group-hover:text-white/10 transition-colors">
                  {world.id}
                </div>

                {/* Icon */}
                <div className={`w-20 h-20 rounded-2xl bg-gradient-to-br ${world.color} p-5 mb-6 group-hover:scale-110 group-hover:rotate-6 transition-all duration-300`}>
                  <world.icon className="w-full h-full text-white" />
                </div>

                {/* World Name */}
                <h3 className="text-3xl font-black mb-4 text-white group-hover:text-transparent group-hover:bg-gradient-to-r group-hover:bg-clip-text group-hover:from-white group-hover:via-purple-300 group-hover:to-white transition-all duration-300">
                  {world.name}
                </h3>

                {/* Description */}
                <p className="text-gray-400 leading-relaxed mb-6">
                  {world.description}
                </p>

                {/* Stats */}
                <div className="flex gap-6 text-sm">
                  <div>
                    <div className="text-gray-500 mb-1">Levels</div>
                    <div className="font-bold text-white">∞</div>
                  </div>
                  <div>
                    <div className="text-gray-500 mb-1">Difficulty</div>
                    <div className="font-bold text-white">Dynamic</div>
                  </div>
                  <div>
                    <div className="text-gray-500 mb-1">Rewards</div>
                    <div className="font-bold text-white">$CRU + NFT</div>
                  </div>
                </div>

                {/* Hover Line */}
                <div className={`h-1 w-0 group-hover:w-full bg-gradient-to-r ${world.color} rounded-full mt-6 transition-all duration-500`}></div>
              </div>
            </div>
          ))}
        </div>

        {/* Bottom CTA */}
        <div className="text-center mt-16">
          <div className="glass-strong inline-block px-8 py-4 rounded-xl border border-blue-500/30">
            <p className="text-gray-300">
              <span className="font-bold text-blue-400">Procedurally Generated</span> • 
              <span className="font-bold text-purple-400 ml-2">AI-Powered</span> • 
              <span className="font-bold text-green-400 ml-2">Infinite Replayability</span>
            </p>
          </div>
        </div>
      </div>
    </section>
  );
}
