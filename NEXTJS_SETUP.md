# 🚀 ChronoRift Universe - Next.js 15 Landing Page

Modern landing page with proper wallet connection built with Next.js 15+ and TypeScript.

---

## 📦 Installation

```bash
npm install
```

## 🎯 Run Development Server

```bash
npm run dev
```

Then open: **http://localhost:3000**

---

## 🏗️ Project Structure

```
├── app/
│   ├── layout.tsx          # Root layout with WalletProvider
│   ├── page.tsx            # Home page
│   └── globals.css         # Global styles
├── components/
│   ├── WalletProvider.tsx  # Wallet connection context
│   ├── Hero.tsx            # Hero section with wallet connect
│   ├── Features.tsx        # Features section
│   ├── GameWorlds.tsx      # Game worlds showcase
│   ├── Tokenomics.tsx      # Tokenomics info
│   ├── Roadmap.tsx         # Project roadmap
│   └── Footer.tsx          # Footer
├── package.json            # Dependencies
├── tsconfig.json           # TypeScript config
├── tailwind.config.ts      # Tailwind CSS config
└── next.config.js          # Next.js config
```

---

## ✨ Features

### **Modern Tech Stack**
- ✅ Next.js 15+ (App Router)
- ✅ TypeScript
- ✅ Tailwind CSS
- ✅ Framer Motion (animations)
- ✅ Lucide React (icons)

### **Wallet Integration**
- ✅ OneWallet support
- ✅ Sui Wallet support
- ✅ Auto-reconnect
- ✅ LocalStorage persistence
- ✅ Clean UI/UX

### **Design**
- ✅ Responsive (mobile-first)
- ✅ Animated background
- ✅ Glass morphism effects
- ✅ Gradient text
- ✅ Smooth transitions

---

## 🔗 Wallet Connection

The wallet connection is handled by `WalletProvider.tsx`:

```typescript
// Usage in any component
import { useWallet } from "@/components/WalletProvider";

function MyComponent() {
  const { connected, address, connect, disconnect } = useWallet();
  
  return (
    <button onClick={connect}>
      {connected ? address : "Connect Wallet"}
    </button>
  );
}
```

### **Supported Wallets:**
- OneWallet (primary)
- Sui Wallet
- Ethos Wallet
- Suiet Wallet
- Martian Wallet

---

## 🎨 Customization

### **Colors**
Edit `tailwind.config.ts` to change colors:

```typescript
colors: {
  background: "var(--background)",
  foreground: "var(--foreground)",
}
```

### **Animations**
Add custom animations in `tailwind.config.ts`:

```typescript
animation: {
  'custom': 'customAnim 2s ease infinite',
},
keyframes: {
  customAnim: {
    '0%, 100%': { /* styles */ },
    '50%': { /* styles */ },
  },
}
```

---

## 📱 Responsive Design

The landing page is fully responsive:
- **Mobile**: Single column layout
- **Tablet**: 2-column grid
- **Desktop**: Full multi-column layout

---

## 🚀 Deployment

### **Vercel (Recommended)**
```bash
npm install -g vercel
vercel
```

### **Netlify**
```bash
npm run build
# Upload .next folder
```

### **Custom Server**
```bash
npm run build
npm start
```

---

## 🔧 Environment Variables

Create `.env.local`:

```env
NEXT_PUBLIC_ONECHAIN_RPC=https://rpc-testnet.onelabs.cc:443
NEXT_PUBLIC_CHAIN_ID=onechain-testnet
```

---

## 📝 Scripts

- `npm run dev` - Start development server
- `npm run build` - Build for production
- `npm start` - Start production server
- `npm run lint` - Run ESLint

---

## 🎮 Integration with Unity Game

To integrate the Unity game:

1. **Build Unity WebGL**
2. **Place build files in** `public/game/`
3. **Create game page:** `app/game/page.tsx`
4. **Load Unity:**

```typescript
"use client";
import { useEffect } from "react";

export default function GamePage() {
  useEffect(() => {
    const script = document.createElement("script");
    script.src = "/game/Build/game.loader.js";
    script.onload = () => {
      createUnityInstance(/* ... */);
    };
    document.body.appendChild(script);
  }, []);
  
  return <div id="unity-container"></div>;
}
```

---

## 🐛 Troubleshooting

### **Module not found errors**
```bash
rm -rf node_modules package-lock.json
npm install
```

### **Wallet not connecting**
- Check OneWallet is installed
- Check browser console for errors
- Try refreshing the page

### **Styles not loading**
```bash
npm run dev
# Restart dev server
```

---

## 📚 Learn More

- [Next.js Documentation](https://nextjs.org/docs)
- [Tailwind CSS](https://tailwindcss.com/docs)
- [OneChain Docs](https://docs.onelabs.cc)

---

<div align="center">

**Built with ❤️ using Next.js 15 + TypeScript**

🌌 **Modern. Fast. Beautiful.** 🚀

</div>
