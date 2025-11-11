/**
 * Sui/OneChain Wallet Selector
 * Replaces Metamask with Sui-based wallet options
 */

// Available Sui wallets
const SUI_WALLETS = {
    suiWallet: {
        name: 'Sui Wallet',
        icon: '🔷',
        check: () => typeof window.suiWallet !== 'undefined'
    },
    ethos: {
        name: 'Ethos Wallet',
        icon: '⚡',
        check: () => typeof window.ethosWallet !== 'undefined'
    },
    suiet: {
        name: 'Suiet Wallet',
        icon: '🦊',
        check: () => typeof window.suiet !== 'undefined'
    },
    martian: {
        name: 'Martian Wallet',
        icon: '🚀',
        check: () => typeof window.martian !== 'undefined'
    },
    oneWallet: {
        name: 'OneWallet',
        icon: '⭕',
        check: () => typeof window.oneWallet !== 'undefined' || typeof window.suiWallet !== 'undefined'
    }
};

class WalletSelector {
    constructor() {
        this.selectedWallet = null;
        this.connectedAddress = null;
        this.init();
    }

    init() {
        // Hide Metamask button if it exists
        this.hideMetamaskButton();
        
        // Create wallet selector UI
        this.createWalletSelectorUI();
        
        // Listen for wallet changes
        this.setupWalletListeners();
    }

    hideMetamaskButton() {
        // Find and hide any Metamask buttons
        const metamaskButtons = document.querySelectorAll('[class*="metamask"], [id*="metamask"]');
        metamaskButtons.forEach(btn => {
            btn.style.display = 'none';
        });
    }

    createWalletSelectorUI() {
        // Check if UI already exists
        if (document.getElementById('sui-wallet-selector')) {
            return;
        }

        // Create modal container
        const modal = document.createElement('div');
        modal.id = 'sui-wallet-selector';
        modal.className = 'wallet-modal';
        modal.innerHTML = `
            <div class="wallet-modal-content">
                <div class="wallet-modal-header">
                    <h2>Connect Wallet</h2>
                    <button class="wallet-modal-close">&times;</button>
                </div>
                <div class="wallet-modal-body">
                    <p class="wallet-subtitle">Choose your Sui/OneChain wallet</p>
                    <div class="wallet-list" id="wallet-list"></div>
                </div>
                <div class="wallet-modal-footer">
                    <p class="wallet-info">Don't have a wallet? <a href="https://sui.io/wallet" target="_blank">Get one here</a></p>
                </div>
            </div>
        `;

        document.body.appendChild(modal);

        // Populate wallet list
        this.populateWalletList();

        // Setup close button
        modal.querySelector('.wallet-modal-close').onclick = () => this.closeModal();
        modal.onclick = (e) => {
            if (e.target === modal) this.closeModal();
        };

        // Create connect button (replaces Metamask button)
        this.createConnectButton();
    }

    createConnectButton() {
        // Find the game canvas or main container
        const container = document.querySelector('#unity-container') || document.body;
        
        // Create connect button
        const connectBtn = document.createElement('button');
        connectBtn.id = 'sui-connect-btn';
        connectBtn.className = 'sui-connect-button';
        connectBtn.innerHTML = `
            <span class="wallet-icon">🔷</span>
            <span class="wallet-text">Connect Wallet</span>
        `;
        
        connectBtn.onclick = () => this.openModal();
        
        // Position button
        connectBtn.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            z-index: 9999;
            padding: 12px 24px;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            border: none;
            border-radius: 12px;
            font-size: 16px;
            font-weight: 600;
            cursor: pointer;
            display: flex;
            align-items: center;
            gap: 8px;
            box-shadow: 0 4px 15px rgba(0, 0, 0, 0.2);
            transition: all 0.3s ease;
        `;
        
        container.appendChild(connectBtn);
    }

    populateWalletList() {
        const walletList = document.getElementById('wallet-list');
        if (!walletList) return;

        walletList.innerHTML = '';

        Object.entries(SUI_WALLETS).forEach(([key, wallet]) => {
            const isAvailable = wallet.check();
            
            const walletItem = document.createElement('div');
            walletItem.className = `wallet-item ${isAvailable ? 'available' : 'unavailable'}`;
            walletItem.innerHTML = `
                <div class="wallet-icon-large">${wallet.icon}</div>
                <div class="wallet-info">
                    <div class="wallet-name">${wallet.name}</div>
                    <div class="wallet-status">${isAvailable ? 'Detected' : 'Not Installed'}</div>
                </div>
                ${isAvailable ? '<div class="wallet-arrow">→</div>' : '<div class="wallet-install">Install</div>'}
            `;

            if (isAvailable) {
                walletItem.onclick = () => this.connectWallet(key, wallet);
            } else {
                walletItem.onclick = () => this.installWallet(wallet);
            }

            walletList.appendChild(walletItem);
        });
    }

    async connectWallet(key, wallet) {
        try {
            console.log(`Connecting to ${wallet.name}...`);
            console.log('Available window objects:', {
                suiWallet: typeof window.suiWallet,
                oneWallet: typeof window.oneWallet,
                ethos: typeof window.ethosWallet,
                suiet: typeof window.suiet,
                martian: typeof window.martian
            });
            
            let provider;
            let accounts;

            // Connect based on wallet type
            switch(key) {
                case 'suiWallet':
                    provider = window.suiWallet;
                    accounts = await provider.requestPermissions();
                    break;
                case 'ethos':
                    provider = window.ethosWallet;
                    accounts = await provider.connect();
                    break;
                case 'suiet':
                    provider = window.suiet;
                    accounts = await provider.connect();
                    break;
                case 'martian':
                    provider = window.martian;
                    accounts = await provider.connect();
                    break;
                case 'oneWallet':
                    // OneWallet uses the standard Sui Wallet API
                    provider = window.suiWallet || window.oneWallet;
                    if (provider.requestPermissions) {
                        const permissions = await provider.requestPermissions();
                        accounts = permissions.accounts || [];
                    } else if (provider.connect) {
                        const result = await provider.connect();
                        accounts = result.accounts || [result.address] || [];
                    }
                    break;
            }

            if (accounts && accounts.length > 0) {
                // Handle different account formats
                const account = accounts[0];
                this.connectedAddress = typeof account === 'string' ? account : (account.address || account);
                this.selectedWallet = wallet;
                
                console.log('Connected:', this.connectedAddress);
                
                // Update UI
                this.updateConnectedState();
                
                // Close modal
                this.closeModal();
                
                // Notify Unity
                this.notifyUnity();
                
                // Show success message
                this.showNotification(`Connected to ${wallet.name}!`, 'success');
            }
        } catch (error) {
            console.error('Connection error:', error);
            this.showNotification(`Failed to connect: ${error.message}`, 'error');
        }
    }

    installWallet(wallet) {
        const urls = {
            'Sui Wallet': 'https://chrome.google.com/webstore/detail/sui-wallet',
            'Ethos Wallet': 'https://ethoswallet.xyz',
            'Suiet Wallet': 'https://suiet.app',
            'Martian Wallet': 'https://martianwallet.xyz',
            'OneWallet': 'https://onelabs.cc'
        };
        
        const url = urls[wallet.name] || 'https://sui.io/wallet';
        window.open(url, '_blank');
    }

    updateConnectedState() {
        const connectBtn = document.getElementById('sui-connect-btn');
        if (connectBtn && this.connectedAddress) {
            const shortAddress = this.connectedAddress.slice(0, 6) + '...' + this.connectedAddress.slice(-4);
            connectBtn.innerHTML = `
                <span class="wallet-icon">${this.selectedWallet.icon}</span>
                <span class="wallet-text">${shortAddress}</span>
            `;
            connectBtn.style.background = 'linear-gradient(135deg, #11998e 0%, #38ef7d 100%)';
        }
    }

    notifyUnity() {
        // Send wallet info to Unity
        if (typeof unityInstance !== 'undefined') {
            unityInstance.SendMessage('WalletConnector', 'OnWalletConnected', this.connectedAddress);
        }
        
        // Dispatch custom event
        window.dispatchEvent(new CustomEvent('walletConnected', {
            detail: {
                address: this.connectedAddress,
                wallet: this.selectedWallet.name
            }
        }));
    }

    setupWalletListeners() {
        // Listen for wallet account changes
        if (window.suiWallet) {
            window.suiWallet.on('accountChanged', (accounts) => {
                if (accounts.length > 0) {
                    this.connectedAddress = accounts[0];
                    this.updateConnectedState();
                }
            });
        }
    }

    openModal() {
        const modal = document.getElementById('sui-wallet-selector');
        if (modal) {
            modal.style.display = 'flex';
            this.populateWalletList(); // Refresh wallet list
        }
    }

    closeModal() {
        const modal = document.getElementById('sui-wallet-selector');
        if (modal) {
            modal.style.display = 'none';
        }
    }

    showNotification(message, type = 'info') {
        const notification = document.createElement('div');
        notification.className = `wallet-notification ${type}`;
        notification.textContent = message;
        notification.style.cssText = `
            position: fixed;
            top: 80px;
            right: 20px;
            padding: 16px 24px;
            background: ${type === 'success' ? '#10b981' : '#ef4444'};
            color: white;
            border-radius: 8px;
            z-index: 10000;
            animation: slideIn 0.3s ease;
        `;
        
        document.body.appendChild(notification);
        
        setTimeout(() => {
            notification.style.animation = 'slideOut 0.3s ease';
            setTimeout(() => notification.remove(), 300);
        }, 3000);
    }

    disconnect() {
        this.connectedAddress = null;
        this.selectedWallet = null;
        
        const connectBtn = document.getElementById('sui-connect-btn');
        if (connectBtn) {
            connectBtn.innerHTML = `
                <span class="wallet-icon">🔷</span>
                <span class="wallet-text">Connect Wallet</span>
            `;
            connectBtn.style.background = 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)';
        }
        
        this.showNotification('Wallet disconnected', 'info');
    }
}

// Initialize when DOM is ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => {
        window.walletSelector = new WalletSelector();
    });
} else {
    window.walletSelector = new WalletSelector();
}

// Export for use in other scripts
window.WalletSelector = WalletSelector;
