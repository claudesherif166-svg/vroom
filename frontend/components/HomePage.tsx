'use client'

import Link from 'next/link'
import { useAuth } from '@/hooks/useAuth'
import { MapPin, Zap, Users, Trophy } from 'lucide-react'

export default function HomePage() {
  const { isAuthenticated, user, login } = useAuth()

  return (
    <div className="min-h-screen bg-gradient-to-br from-race-dark via-race-gray to-race-dark">
      {/* Navigation */}
      <nav className="flex items-center justify-between p-6 backdrop-blur-sm bg-race-dark/50">
        <div className="flex items-center space-x-2">
          <Zap className="h-8 w-8 text-race-primary" />
          <span className="text-2xl font-racing font-bold text-race-primary">StreetRacer</span>
        </div>
        
        <div className="hidden md:flex items-center space-x-8">
          <Link href="/races" className="text-white hover:text-race-primary transition-colors">
            Races
          </Link>
          <Link href="/events" className="text-white hover:text-race-primary transition-colors">
            Events
          </Link>
          <Link href="/vehicles" className="text-white hover:text-race-primary transition-colors">
            Vehicles
          </Link>
          {isAuthenticated && (
            <Link href="/profile" className="text-white hover:text-race-primary transition-colors">
              Profile
            </Link>
          )}
        </div>
        
        <div className="flex items-center space-x-4">
          {isAuthenticated ? (
            <div className="flex items-center space-x-4">
              <span className="text-race-primary">Welcome, {user?.username}</span>
              <Link href="/feed" className="racing-button">
                Dashboard
              </Link>
            </div>
          ) : (
            <>
              <button 
                onClick={login}
                className="text-white hover:text-race-primary transition-colors"
              >
                Login
              </button>
              <button 
                onClick={login}
                className="racing-button"
              >
                Get Started
              </button>
            </>
          )}
        </div>
      </nav>

      {/* Hero Section */}
      <main className="container mx-auto px-6 py-20">
        <div className="text-center mb-16">
          <h1 className="text-6xl md:text-8xl font-racing font-black mb-6 bg-gradient-to-r from-race-primary via-race-accent to-race-secondary bg-clip-text text-transparent">
            RACE
            <br />
            LIVE
          </h1>
          <p className="text-xl md:text-2xl text-gray-300 mb-8 max-w-3xl mx-auto">
            Join real-time GPS races with racers worldwide. Track your position, 
            compete for rankings, and become the ultimate street racer.
          </p>
          <div className="flex flex-col sm:flex-row gap-4 justify-center">
            {isAuthenticated ? (
              <Link href="/races/new" className="racing-button text-lg px-8 py-4">
                Start Racing Now
              </Link>
            ) : (
              <button onClick={login} className="racing-button text-lg px-8 py-4">
                Start Racing Now
              </button>
            )}
            <Link href="/map" className="racing-button bg-transparent border-2 border-race-primary text-race-primary hover:bg-race-primary hover:text-race-dark text-lg px-8 py-4">
              Explore Map
            </Link>
          </div>
        </div>

        {/* Features Grid */}
        <div className="grid md:grid-cols-3 gap-8 mb-20">
          <div className="racing-card text-center group hover:racing-glow transition-all duration-300">
            <MapPin className="h-16 w-16 text-race-primary mx-auto mb-4 group-hover:scale-110 transition-transform" />
            <h3 className="text-2xl font-bold mb-4">Real-Time GPS</h3>
            <p className="text-gray-300">
              Live location tracking with sub-second updates. See your position 
              and competitors in real-time on the race course.
            </p>
          </div>
          
          <div className="racing-card text-center group hover:racing-glow transition-all duration-300">
            <Users className="h-16 w-16 text-race-accent mx-auto mb-4 group-hover:scale-110 transition-transform" />
            <h3 className="text-2xl font-bold mb-4">Social Racing</h3>
            <p className="text-gray-300">
              Connect with racers, create events, share achievements, 
              and build your racing community.
            </p>
          </div>
          
          <div className="racing-card text-center group hover:racing-glow transition-all duration-300">
            <Trophy className="h-16 w-16 text-race-secondary mx-auto mb-4 group-hover:scale-110 transition-transform" />
            <h3 className="text-2xl font-bold mb-4">Competitive</h3>
            <p className="text-gray-300">
              Anti-cheat system, verified rankings, achievements, 
              and leaderboards for fair competition.
            </p>
          </div>
        </div>

        {/* Live Stats */}
        <div className="racing-card p-8 text-center">
          <h2 className="text-3xl font-bold mb-8">Platform Stats</h2>
          <div className="grid grid-cols-2 md:grid-cols-4 gap-8">
            <div>
              <div className="text-4xl font-racing font-bold text-race-primary mb-2">1,247</div>
              <div className="text-gray-400">Active Racers</div>
            </div>
            <div>
              <div className="text-4xl font-racing font-bold text-race-accent mb-2">89</div>
              <div className="text-gray-400">Live Races</div>
            </div>
            <div>
              <div className="text-4xl font-racing font-bold text-race-secondary mb-2">15,632</div>
              <div className="text-gray-400">Races Completed</div>
            </div>
            <div>
              <div className="text-4xl font-racing font-bold text-race-warning mb-2">24/7</div>
              <div className="text-gray-400">Racing Action</div>
            </div>
          </div>
        </div>
      </main>

      {/* Footer */}
      <footer className="bg-race-dark/80 border-t border-race-primary/20 py-12">
        <div className="container mx-auto px-6">
          <div className="grid md:grid-cols-4 gap-8">
            <div>
              <div className="flex items-center space-x-2 mb-4">
                <Zap className="h-6 w-6 text-race-primary" />
                <span className="text-xl font-racing font-bold text-race-primary">StreetRacer</span>
              </div>
              <p className="text-gray-400">
                The ultimate real-time racing platform for competitive racers worldwide.
              </p>
            </div>
            
            <div>
              <h4 className="font-bold mb-4">Racing</h4>
              <ul className="space-y-2 text-gray-400">
                <li><Link href="/races" className="hover:text-race-primary transition-colors">Live Races</Link></li>
                <li><Link href="/events" className="hover:text-race-primary transition-colors">Events</Link></li>
                <li><Link href="/leaderboard" className="hover:text-race-primary transition-colors">Leaderboard</Link></li>
                <li><Link href="/achievements" className="hover:text-race-primary transition-colors">Achievements</Link></li>
              </ul>
            </div>
            
            <div>
              <h4 className="font-bold mb-4">Community</h4>
              <ul className="space-y-2 text-gray-400">
                <li><Link href="/feed" className="hover:text-race-primary transition-colors">Social Feed</Link></li>
                <li><Link href="/chat" className="hover:text-race-primary transition-colors">Chat</Link></li>
                <li><Link href="/stories" className="hover:text-race-primary transition-colors">Stories</Link></li>
                <li><Link href="/vehicles" className="hover:text-race-primary transition-colors">Garage</Link></li>
              </ul>
            </div>
            
            <div>
              <h4 className="font-bold mb-4">Support</h4>
              <ul className="space-y-2 text-gray-400">
                <li><Link href="/help" className="hover:text-race-primary transition-colors">Help Center</Link></li>
                <li><Link href="/privacy" className="hover:text-race-primary transition-colors">Privacy</Link></li>
                <li><Link href="/terms" className="hover:text-race-primary transition-colors">Terms</Link></li>
                <li><Link href="/contact" className="hover:text-race-primary transition-colors">Contact</Link></li>
              </ul>
            </div>
          </div>
          
          <div className="border-t border-race-primary/20 mt-8 pt-8 text-center text-gray-400">
            <p>&copy; 2025 StreetRacer Platform. Built for the racing community.</p>
          </div>
        </div>
      </footer>
    </div>
  )
}