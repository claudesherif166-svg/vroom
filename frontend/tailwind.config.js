/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    './pages/**/*.{js,ts,jsx,tsx,mdx}',
    './components/**/*.{js,ts,jsx,tsx,mdx}',
    './app/**/*.{js,ts,jsx,tsx,mdx}',
  ],
  theme: {
    extend: {
      colors: {
        'race-primary': '#00ff88',
        'race-secondary': '#ff0088',
        'race-accent': '#00d4ff',
        'race-warning': '#ffaa00',
        'race-dark': '#0a0a0a',
        'race-gray': '#1a1a1a',
      },
      fontFamily: {
        'racing': ['Orbitron', 'monospace'],
      },
      animation: {
        'pulse-slow': 'pulse 3s cubic-bezier(0.4, 0, 0.6, 1) infinite',
        'bounce-slow': 'bounce 2s infinite',
      },
    },
  },
  plugins: [],
}