import type { Metadata } from 'next'
import './globals.css'

export const metadata: Metadata = {
  title: 'StreetRacer - Real-Time Racing Platform',
  description: 'Join live GPS races and compete with racers worldwide',
  keywords: 'racing, GPS, real-time, competition, social',
  authors: [{ name: 'StreetRacer Team' }],
  openGraph: {
    title: 'StreetRacer - Real-Time Racing Platform',
    description: 'Join live GPS races and compete with racers worldwide',
    type: 'website',
  },
}

export default function RootLayout({
  children,
}: {
  children: React.ReactNode
}) {
  return (
    <html lang="en">
      <body className="min-h-screen bg-race-dark text-white">
        {children}
      </body>
    </html>
  )
}