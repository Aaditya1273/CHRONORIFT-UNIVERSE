"use client";

import { Github, Twitter, MessageCircle, Mail, ExternalLink } from "lucide-react";

export function Footer() {
  const socialLinks = [
    { icon: Twitter, label: "Twitter", href: "#" },
    { icon: Github, label: "GitHub", href: "#" },
    { icon: MessageCircle, label: "Discord", href: "#" },
    { icon: Mail, label: "Email", href: "mailto:contact@chronorift.io" },
  ];

  const links = {
    game: [
      { label: "Play Now", href: "/game" },
      { label: "Leaderboard", href: "#" },
      { label: "Achievements", href: "#" },
      { label: "Marketplace", href: "#" },
    ],
    resources: [
      { label: "Documentation", href: "#" },
      { label: "Whitepaper", href: "#" },
      { label: "Tokenomics", href: "#" },
      { label: "Roadmap", href: "#" },
    ],
    community: [
      { label: "Discord", href: "#" },
      { label: "Twitter", href: "#" },
      { label: "Telegram", href: "#" },
      { label: "Medium", href: "#" },
    ],
  };

  return (
    <footer className="relative border-t border-purple-500/20 bg-black/50 backdrop-blur-xl">
      <div className="max-w-7xl mx-auto px-4 py-16">
        {/* Main Footer Content */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-5 gap-12 mb-12">
          {/* Brand Column */}
          <div className="lg:col-span-2">
            <h3 className="text-3xl font-black gradient-text-epic mb-4">CHRONORIFT</h3>
            <p className="text-gray-400 mb-6 leading-relaxed">
              The ultimate AI-powered blockchain puzzle odyssey. Built on OneChain with love for gamers worldwide.
            </p>
            {/* Social Links */}
            <div className="flex gap-4">
              {socialLinks.map((social, index) => (
                <a
                  key={index}
                  href={social.href}
                  className="w-12 h-12 glass-strong rounded-xl border border-purple-500/30 hover:border-purple-500/60 flex items-center justify-center transition-all duration-300 hover:scale-110 hover:-translate-y-1 group"
                  aria-label={social.label}
                >
                  <social.icon className="w-5 h-5 text-gray-400 group-hover:text-purple-400 transition-colors" />
                </a>
              ))}
            </div>
          </div>

          {/* Links Columns */}
          <div>
            <h4 className="text-white font-bold mb-4 text-sm uppercase tracking-wider">Game</h4>
            <ul className="space-y-3">
              {links.game.map((link, index) => (
                <li key={index}>
                  <a
                    href={link.href}
                    className="text-gray-400 hover:text-purple-400 transition-colors duration-300 text-sm flex items-center gap-2 group"
                  >
                    <span className="w-0 group-hover:w-2 h-px bg-purple-400 transition-all duration-300"></span>
                    {link.label}
                  </a>
                </li>
              ))}
            </ul>
          </div>

          <div>
            <h4 className="text-white font-bold mb-4 text-sm uppercase tracking-wider">Resources</h4>
            <ul className="space-y-3">
              {links.resources.map((link, index) => (
                <li key={index}>
                  <a
                    href={link.href}
                    className="text-gray-400 hover:text-purple-400 transition-colors duration-300 text-sm flex items-center gap-2 group"
                  >
                    <span className="w-0 group-hover:w-2 h-px bg-purple-400 transition-all duration-300"></span>
                    {link.label}
                  </a>
                </li>
              ))}
            </ul>
          </div>

          <div>
            <h4 className="text-white font-bold mb-4 text-sm uppercase tracking-wider">Community</h4>
            <ul className="space-y-3">
              {links.community.map((link, index) => (
                <li key={index}>
                  <a
                    href={link.href}
                    className="text-gray-400 hover:text-purple-400 transition-colors duration-300 text-sm flex items-center gap-2 group"
                  >
                    <span className="w-0 group-hover:w-2 h-px bg-purple-400 transition-all duration-300"></span>
                    {link.label}
                  </a>
                </li>
              ))}
            </ul>
          </div>
        </div>

        {/* Bottom Bar */}
        <div className="pt-8 border-t border-purple-500/20">
          <div className="flex flex-col md:flex-row justify-between items-center gap-4">
            <p className="text-gray-500 text-sm">
              2025 ChronoRift Universe. All rights reserved.
            </p>
            <div className="flex items-center gap-6 text-sm">
              <a href="#" className="text-gray-500 hover:text-purple-400 transition-colors">
                Privacy Policy
              </a>
              <a href="#" className="text-gray-500 hover:text-purple-400 transition-colors">
                Terms of Service
              </a>
              <a
                href="https://onelabs.cc"
                target="_blank"
                rel="noopener noreferrer"
                className="text-gray-500 hover:text-purple-400 transition-colors flex items-center gap-1"
              >
                Powered by OneChain
                <ExternalLink className="w-3 h-3" />
              </a>
            </div>
          </div>
        </div>
      </div>
    </footer>
  );
}
