# 🚀 ChronoRift Universe - Deployment Guide

Complete step-by-step guide to deploy ChronoRift Universe to production.

---

## 📋 Table of Contents

1. [Prerequisites](#prerequisites)
2. [OneChain Setup](#onechain-setup)
3. [Smart Contract Deployment](#smart-contract-deployment)
4. [Unity Build](#unity-build)
5. [Web Deployment](#web-deployment)
6. [Configuration](#configuration)
7. [Testing](#testing)
8. [Troubleshooting](#troubleshooting)

---

## 1. Prerequisites

### Required Software
- ✅ Unity 2021.3 or later
- ✅ OneChain CLI (`one`)
- ✅ Rust & Cargo (for Move contracts)
- ✅ Git
- ✅ Web browser with OneWallet extension

### Required Accounts
- ✅ OneChain wallet with testnet OCT
- ✅ GitHub account (for repository)
- ✅ Web hosting (Netlify, Vercel, or similar)

---

## 2. OneChain Setup

### Step 2.1: Install OneChain CLI

**Linux/macOS:**
```bash
curl --proto '=https' --tlsv1.2 -sSf https://sh.rustup.rs | sh
cargo install --locked --git https://github.com/one-chain-labs/onechain.git one_chain --features tracing
mv ~/.cargo/bin/one_chain ~/.cargo/bin/one
```

**Verify Installation:**
```bash
one --version
```

### Step 2.2: Configure OneChain Client

```bash
# Initialize client
one client

# Connect to testnet (press Enter when prompted)
# Select key scheme: 0 (ed25519)

# Save your recovery phrase!
```

### Step 2.3: Get Testnet Tokens

**Option A: CLI**
```bash
one client faucet
```

**Option B: cURL**
```bash
curl --location --request POST 'https://faucet-testnet.onelabs.cc/v1/gas' \
--header 'Content-Type: application/json' \
--data-raw '{
    "FixedAmountRequest": {
        "recipient": "<YOUR_ADDRESS>"
    }
}'
```

**Verify Balance:**
```bash
one client gas
```

---

## 3. Smart Contract Deployment

### Step 3.1: Build Move Contracts

```bash
cd move
one move build
```

**Expected Output:**
```
BUILDING chronorift_universe
BUILDING Sui
BUILDING MoveStdlib
```

### Step 3.2: Publish Contracts

```bash
one client publish --gas-budget 100000000
```

**Save the output!** You'll need:
- 📦 **Package ID**: `0x...`
- 🏦 **CRU Treasury ID**: `0x...`
- 📊 **Staking Pool ID**: `0x...`
- 🏆 **Leaderboard ID**: `0x...`

### Step 3.3: Initialize Contracts

**Create Staking Pool:**
```bash
one client call \
  --package <PACKAGE_ID> \
  --module staking_pool \
  --function create_staking_pool \
  --args 1000 \
  --gas-budget 10000000
```

**Create Leaderboard:**
```bash
one client call \
  --package <PACKAGE_ID> \
  --module leaderboard \
  --function create_leaderboard \
  --gas-budget 10000000
```

**Create Player Progress (for testing):**
```bash
one client call \
  --package <PACKAGE_ID> \
  --module level_nft \
  --function create_player_progress \
  --gas-budget 10000000
```

---

## 4. Unity Build

### Step 4.1: Open Project in Unity

1. Open Unity Hub
2. Click "Add" → "Add project from disk"
3. Select `chronorift-universe` folder
4. Open with Unity 2021.3+

### Step 4.2: Configure OneChain Settings

1. Open `integration scripts/OneChainConfig.cs`
2. Update the following:

```csharp
[Header("Deployed Contract Addresses")]
public string packageId = "0xYOUR_PACKAGE_ID";
public string cruTreasuryId = "0xYOUR_TREASURY_ID";
public string stakingPoolId = "0xYOUR_STAKING_POOL_ID";
public string leaderboardId = "0xYOUR_LEADERBOARD_ID";
```

### Step 4.3: Build for WebGL

1. **File** → **Build Settings**
2. Select **WebGL** platform
3. Click **Switch Platform**
4. Click **Player Settings**
5. Configure:
   - **Company Name**: Your name
   - **Product Name**: ChronoRift Universe
   - **WebGL Template**: Default or Custom
6. Click **Build**
7. Choose output folder: `WebGL-Build`

**Build Time:** 10-30 minutes depending on your machine

---

## 5. Web Deployment

### Option A: Netlify (Recommended)

#### Step 5.1: Prepare Files

```bash
# Create deployment folder
mkdir deploy
cp -r WebGL-Build/Build deploy/
cp index.html deploy/
cp -r TemplateData deploy/
cp OneWalletBridge.jslib deploy/
```

#### Step 5.2: Deploy to Netlify

**Via Netlify CLI:**
```bash
npm install -g netlify-cli
netlify login
netlify deploy --prod --dir=deploy
```

**Via Netlify UI:**
1. Go to [netlify.com](https://netlify.com)
2. Drag & drop `deploy` folder
3. Wait for deployment
4. Get your URL: `https://your-site.netlify.app`

### Option B: Vercel

```bash
npm install -g vercel
vercel --prod
```

### Option C: GitHub Pages

```bash
# Create gh-pages branch
git checkout -b gh-pages
git add deploy/*
git commit -m "Deploy to GitHub Pages"
git push origin gh-pages

# Enable GitHub Pages in repository settings
```

---

## 6. Configuration

### Step 6.1: Update index.html

Open `index.html` and verify:

```html
<!-- OneChain Configuration -->
<script>
    window.oneChainConfig = {
        rpcUrl: 'https://rpc-testnet.onelabs.cc:443',
        chainId: 'onechain-testnet',
        networkType: 'testnet'
    };
</script>
```

### Step 6.2: Update OneWalletBridge.jslib

Ensure the bridge is included in your WebGL build:

```javascript
// Check if OneWallet is available
IsOneWalletAvailable: function() {
    return typeof window.suiWallet !== 'undefined';
}
```

### Step 6.3: Test Configuration

1. Open deployed URL
2. Open browser console (F12)
3. Check for errors
4. Verify OneChain config is loaded

---

## 7. Testing

### Step 7.1: Connect Wallet

1. Install OneWallet extension
2. Create/import wallet
3. Switch to OneChain Testnet
4. Click "Connect Wallet" in game
5. Approve connection

### Step 7.2: Test Core Features

**Level Completion:**
```
1. Start a level
2. Complete puzzle
3. Check if CRU tokens are minted
4. Verify NFT creation
```

**Staking:**
```
1. Go to Staking page
2. Enter amount
3. Click "Stake"
4. Approve transaction
5. Verify staked balance
```

**Leaderboard:**
```
1. Open leaderboard
2. Check if data loads
3. Verify your rank appears
```

### Step 7.3: Test Multiplayer

```
1. Create room
2. Get room code
3. Open game in another browser/device
4. Join with room code
5. Start game
```

---

## 8. Troubleshooting

### Issue: "OneWallet not found"

**Solution:**
- Install OneWallet browser extension
- Refresh page
- Check browser console for errors

### Issue: "Transaction failed"

**Solution:**
```bash
# Check gas balance
one client gas

# Request more testnet tokens
one client faucet
```

### Issue: "Contract not found"

**Solution:**
- Verify package ID in `OneChainConfig.cs`
- Check if contracts are published:
```bash
one client object <PACKAGE_ID>
```

### Issue: "WebGL build fails"

**Solution:**
- Check Unity version (2021.3+)
- Clear cache: `Edit → Preferences → GI Cache → Clear Cache`
- Rebuild: `File → Build Settings → Clean Build`

### Issue: "CORS errors"

**Solution:**
- Ensure HTTPS is enabled
- Add CORS headers to web server
- Use proper hosting (Netlify/Vercel handles this)

### Issue: "Slow loading"

**Solution:**
- Enable compression in Unity:
  - `Player Settings → Publishing Settings → Compression Format → Brotli`
- Use CDN for faster delivery
- Optimize assets

---

## 9. Production Checklist

Before going live, verify:

- [ ] All smart contracts deployed
- [ ] Contract addresses updated in config
- [ ] Unity build completed successfully
- [ ] Web deployment successful
- [ ] OneWallet connection works
- [ ] Token minting works
- [ ] NFT minting works
- [ ] Staking works
- [ ] Leaderboard loads
- [ ] Multiplayer works
- [ ] Mobile responsive
- [ ] HTTPS enabled
- [ ] Analytics setup (optional)
- [ ] Error tracking setup (optional)

---

## 10. Monitoring

### Check Smart Contract Activity

```bash
# View recent transactions
one client transactions --address <YOUR_ADDRESS>

# Check object ownership
one client objects <YOUR_ADDRESS>

# View specific object
one client object <OBJECT_ID>
```

### Monitor Token Supply

```bash
one client call \
  --package <PACKAGE_ID> \
  --module cru_token \
  --function get_circulating_supply \
  --args <TREASURY_ID>
```

---

## 11. Maintenance

### Update Smart Contracts

```bash
# Build new version
cd move
one move build

# Publish upgrade
one client publish --gas-budget 100000000

# Update package ID in Unity config
```

### Update Unity Build

```bash
# Make changes in Unity
# Rebuild WebGL
# Redeploy to hosting
```

---

## 12. Support

If you encounter issues:

1. **Check Logs**: Browser console (F12)
2. **Verify Config**: Double-check all IDs
3. **Test Network**: Ensure testnet is accessible
4. **Check Balance**: Ensure sufficient OCT for gas

**Get Help:**
- OneChain Discord
- GitHub Issues
- OneChain Documentation

---

## 🎉 Congratulations!

Your ChronoRift Universe game is now deployed and live on OneChain!

**Next Steps:**
- Share your game URL
- Invite players to test
- Monitor analytics
- Gather feedback
- Iterate and improve

---

<div align="center">

**Built with ❤️ on OneChain**

🌌 **Happy Deploying!** 🚀

</div>
