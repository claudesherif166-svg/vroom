'use client'

import { useEffect, useState } from 'react'
import { useParams } from 'next/navigation'
import { useAuth } from '@/hooks/useAuth'
import { apiClient } from '@/lib/api'
import { ArrowLeft, Users, Clock, MapPin, Flag } from 'lucide-react'
import Link from 'next/link'
import MapView from '@/components/MapView'
import RaceLeaderboard from '@/components/RaceLeaderboard'
import { useRaceRealtime } from '@/hooks/useRaceRealtime'
import { RaceDto } from '@/types'

export default function RacePage() {
  const params = useParams()
  const raceId = params.id as string
  const { isAuthenticated, getAccessToken } = useAuth()
  const [race, setRace] = useState<RaceDto | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const { status, sendLocation, connectionState } = useRaceRealtime(raceId, getAccessToken)

  useEffect(() => {
    const loadRace = async () => {
      if (!isAuthenticated) return
      
      try {
        setError(null)
        const raceData = await apiClient.getRace(raceId)
        setRace(raceData)
      } catch (error) {
        console.error('Failed to load race:', error)
        setError(error instanceof Error ? error.message : 'Failed to load race')
      } finally {
        setLoading(false)
      }
    }

    loadRace()
  }, [raceId, isAuthenticated])

  // GPS tracking
  useEffect(() => {
    if (race?.status === 'in_progress' && isAuthenticated) {
      let watchId: number
      
      if (navigator.geolocation) {
        watchId = navigator.geolocation.watchPosition(
          (position) => {
            const location = {
              latitude: position.coords.latitude,
              longitude: position.coords.longitude,
              speed: position.coords.speed || undefined,
              heading: position.coords.heading || undefined,
              recordedAt: new Date().toISOString()
            }
            
            sendLocation(location)
          },
          (error) => {
            console.error('Geolocation error:', error)
          },
          {
            enableHighAccuracy: true,
            timeout: 10000,
            maximumAge: 1000
          }
        )
      } else {
        // Fallback: simulate GPS for development
        const interval = setInterval(() => {
          const mockLocation = {
            latitude: race.startLatitude + (Math.random() - 0.5) * 0.01,
            longitude: race.startLongitude + (Math.random() - 0.5) * 0.01,
            speed: Math.random() * 60 + 20,
            heading: Math.random() * 360,
            recordedAt: new Date().toISOString()
          }
          
          sendLocation(mockLocation)
        }, 2000)
        
        return () => clearInterval(interval)
      }
      
      return () => {
        if (watchId) {
          navigator.geolocation.clearWatch(watchId)
        }
      }
    }
  }, [race?.status, isAuthenticated, sendLocation, race?.startLatitude, race?.startLongitude])

  if (!isAuthenticated) {
    return (
      <div className="min-h-screen bg-race-dark flex items-center justify-center">
        <div className="racing-card p-8 text-center">
          <Flag className="w-16 h-16 text-race-warning mx-auto mb-4" />
          <h1 className="text-2xl font-bold mb-4">Authentication Required</h1>
          <p className="text-gray-400 mb-6">Please sign in to view this race.</p>
          <Link href="/auth/login" className="racing-button">
            Sign In
          </Link>
        </div>
      </div>
    )
  }

  if (error) {
    return (
      <div className="min-h-screen bg-race-dark flex items-center justify-center">
        <div className="racing-card p-8 text-center">
          <Flag className="w-16 h-16 text-red-500 mx-auto mb-4" />
          <h1 className="text-2xl font-bold mb-4">Error Loading Race</h1>
          <p className="text-gray-400 mb-6">{error}</p>
          <Link href="/races" className="racing-button">
            Browse Races
          </Link>
        </div>
      </div>
    )
  }
  if (loading) {
    return (
      <div className="min-h-screen bg-race-dark flex items-center justify-center">
        <div className="racing-card p-8 text-center">
          <div className="animate-spin racing-pulse w-16 h-16 border-4 border-race-primary border-t-transparent rounded-full mx-auto mb-4"></div>
          <p className="text-race-primary">Loading race...</p>
        </div>
      </div>
    )
  }

  if (!race) {
    return (
      <div className="min-h-screen bg-race-dark flex items-center justify-center">
        <div className="racing-card p-8 text-center">
          <Flag className="w-16 h-16 text-race-secondary mx-auto mb-4" />
          <h1 className="text-2xl font-bold mb-4">Race Not Found</h1>
          <p className="text-gray-400 mb-6">The race you're looking for doesn't exist.</p>
          <Link href="/races" className="racing-button">
            Browse Races
          </Link>
        </div>
      </div>
    )
  }

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'in_progress': return 'text-race-primary'
      case 'finished': return 'text-race-secondary'
      case 'cancelled': return 'text-red-500'
      default: return 'text-race-warning'
    }
  }

  const getStatusText = (status: string) => {
    switch (status) {
      case 'in_progress': return 'LIVE'
      case 'finished': return 'FINISHED'
      case 'cancelled': return 'CANCELLED'
      default: return 'STARTING SOON'
    }
  }

  return (
    <div className="min-h-screen bg-race-dark">
      {/* Header */}
      <header className="bg-race-gray/50 backdrop-blur-sm border-b border-race-primary/20 p-4">
        <div className="container mx-auto flex items-center justify-between">
          <div className="flex items-center space-x-4">
            <Link href="/races" className="text-race-primary hover:text-race-accent transition-colors">
              <ArrowLeft className="w-6 h-6" />
            </Link>
            <div>
              <h1 className="text-2xl font-bold">{race.name}</h1>
              <div className="flex items-center space-x-4 text-sm text-gray-400">
                <div className="flex items-center space-x-1">
                  <Users className="w-4 h-4" />
                  <span>{race.racers.length} Racers</span>
                </div>
                <div className="flex items-center space-x-1">
                  <Clock className="w-4 h-4" />
                  <span>
                    {race.startedAt 
                      ? `Started ${new Date(race.startedAt).toLocaleTimeString()}`
                      : 'Not started'
                    }
                  </span>
                </div>
              </div>
            </div>
          </div>

          <div className={`px-4 py-2 rounded-lg font-bold ${getStatusColor(race.status)} border border-current`}>
            {getStatusText(race.status)}
          </div>
        </div>
      </header>

      {/* Main Content */}
      <div className="container mx-auto p-4">
        <div className="grid lg:grid-cols-3 gap-6">
          {/* Map */}
          <div className="lg:col-span-2">
            <div className="racing-card p-0 overflow-hidden h-[500px]">
              <MapView 
                race={race}
                liveStatus={status}
                className="h-full w-full"
              />
            </div>
            
            {/* Race Info */}
            <div className="racing-card mt-4">
              <h3 className="text-xl font-bold mb-4">Race Route</h3>
              <div className="grid grid-cols-2 gap-4">
                <div className="flex items-center space-x-2">
                  <MapPin className="w-5 h-5 text-race-primary" />
                  <div>
                    <div className="font-semibold">Start</div>
                    <div className="text-sm text-gray-400">
                      {race.startLatitude.toFixed(4)}, {race.startLongitude.toFixed(4)}
                    </div>
                  </div>
                </div>
                <div className="flex items-center space-x-2">
                  <Flag className="w-5 h-5 text-race-secondary" />
                  <div>
                    <div className="font-semibold">Finish</div>
                    <div className="text-sm text-gray-400">
                      {race.endLatitude.toFixed(4)}, {race.endLongitude.toFixed(4)}
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          {/* Leaderboard */}
          <div className="space-y-6">
            <RaceLeaderboard 
              raceId={raceId}
              liveStatus={status}
            />
            
            {/* Connection Status */}
            <div className="racing-card">
              <h3 className="text-lg font-bold mb-2">Connection</h3>
              <div className={`flex items-center space-x-2 ${
                connectionState === 'Connected' ? 'text-race-primary' : 
                connectionState === 'Connecting' ? 'text-race-warning' : 'text-red-500'
              }`}>
                <div className={`w-3 h-3 rounded-full ${
                  connectionState === 'Connected' ? 'bg-race-primary racing-pulse' : 
                  connectionState === 'Connecting' ? 'bg-race-warning animate-pulse' : 'bg-red-500'
                }`}></div>
                <span className="font-semibold">{connectionState}</span>
              </div>
              {status && (
                <div className="text-sm text-gray-400 mt-2">
                  Last update: {new Date(status.lastUpdated).toLocaleTimeString()}
                </div>
              )}
            </div>

            {/* Race Controls */}
            {race.status === 'created' && (
              <div className="racing-card">
                <h3 className="text-lg font-bold mb-4">Race Controls</h3>
                <div className="space-y-2">
                  <button className="racing-button w-full">
                    Accept Invite
                  </button>
                  <button className="w-full py-2 px-4 border border-race-primary/30 text-race-primary rounded-lg hover:bg-race-primary/10 transition-colors">
                    Decline
                  </button>
                </div>
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  )
}