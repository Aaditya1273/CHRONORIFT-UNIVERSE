import type { Metadata } from "next";
import { Inter } from "next/font/google";
import "./globals.css";
import "@mysten/dapp-kit/dist/index.css";
import { WalletProvider } from "@/components/WalletProviderNew";

const inter = Inter({ subsets: ["latin"] });

export const metadata: Metadata = {
  title: "ChronoRift Universe - Shattered Epoch Quest",
  description: "AI-Powered Blockchain Puzzle Odyssey on OneChain. Play-to-Earn, NFT Rewards, Dynamic AI, Multiplayer, DeFi Integration",
  keywords: ["blockchain", "gaming", "NFT", "OneChain", "puzzle", "AI", "play-to-earn"],
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en">
      <body className={inter.className}>
        <WalletProvider>
          {children}
        </WalletProvider>
      </body>
    </html>
  );
}
