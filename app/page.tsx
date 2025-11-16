"use client";

import { Hero } from "@/components/HeroNew";
import { Features } from "@/components/Features";
import { GameWorlds } from "@/components/GameWorlds";
import { Tokenomics } from "@/components/Tokenomics";
import { Footer } from "@/components/Footer";

export default function Home() {
  return (
    <main className="min-h-screen animated-bg">
      <Hero />
      <Features />
      <GameWorlds />
      <Tokenomics />
      <Footer />
    </main>
  );
}
