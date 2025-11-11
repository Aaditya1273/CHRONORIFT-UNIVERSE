"use client";

import { createNetworkConfig, SuiClientProvider, WalletProvider as SuiWalletProvider } from '@mysten/dapp-kit';
import { getFullnodeUrl } from '@mysten/sui/client';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ReactNode } from 'react';

// Create QueryClient
const queryClient = new QueryClient();

// Configure OneChain network (it's Sui-based)
const { networkConfig } = createNetworkConfig({
  testnet: { 
    url: 'https://rpc-testnet.onelabs.cc:443',
  },
  mainnet: { 
    url: getFullnodeUrl('mainnet') 
  },
});

export function WalletProvider({ children }: { children: ReactNode }) {
  return (
    <QueryClientProvider client={queryClient}>
      <SuiClientProvider networks={networkConfig} defaultNetwork="testnet">
        <SuiWalletProvider autoConnect>
          {children}
        </SuiWalletProvider>
      </SuiClientProvider>
    </QueryClientProvider>
  );
}
