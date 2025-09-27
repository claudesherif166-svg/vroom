'use client'

import { useEffect, useState } from 'react'
import { useParams } from 'next/navigation'
import { ArrowLeft, Users, Clock, MapPin, Flag } from 'lucide-react'
import Link from 'next/link'
import MapView from '@/components/MapView'
import RaceLeaderboard from '@/components/RaceLeaderboard'
import { useRaceRealtime } from '@/hooks/useRaceRealtime'
import { RaceDto } from '@/types'

export default function RacePage() {
  const params = useParams()
  const raceId = params.id as string
  const [race, setRace] = useState<RaceDto | null>(null)
  const [loading, setLoading] = useState(true)

  // Mock access token function - replace with actual auth
  const getAccessToken = () => null // TODO: Implement Keycloak auth

  const { status, sendLocation, connectionState } = useRaceRealtime(raceId, getAccessToken)

  useEffect(() => {
    // Mock race data - replace with actual API call
    const mockRace: RaceDto = {
      id: raceId,
      hostId: '123e4567-e89b-12d3-a456-426614174000',
      name: 'Downtown Speed Challenge',
      startLatitude: 40.7128,
      startLongitude: -74.0060,
      endLatitude: 40.7589,
      endLongitude: -73.9851,
      status: 'in_progress',
      scheduledAt: new Date().toISOString(),
      startedAt: new Date(Date.now() - 300000).toISOString(), // Started 5 minutes ago
      finishedAt: null,
      createdAt: new Date().toISOString(),
      racers: [
        {
          id: '123e4567-e89b-12d3-a456-426614174001',
          userId: '123e4567-e89b-12d3-a456-426614174001',
          username: 'SpeedDemon',
          profilePictureUrl: null,
          startOrder: 1,
          vehicleId: '123e4567-e89b-12d3-a456-426614174010',
          status: 'racing',
          joinedAt: new Date(Date.now() - 600000).toISOString(),
          finishTime: null,
          finalRank: null
        },
        {
          id: '123e4567-e89b-12d3-a456-426614174002',
          userId: '123e4567-e89b-12d3-a456-426614174002',
          username: 'RoadRunner',
          profilePictureUrl: null,
          startOrder: 2,
          vehicleId: '123e4567-e89b-12d3-a456-426614174011',
          status: 'racing',
          joinedAt: new Date(Date.now() - 600000).toISOString(),
          finishTime: null,
          finalRank: null
        }
      ]
    }

    setRace(mockRace)
    setLoading(false)
  }, [raceId])

  // Mock GPS tracking
  useEffect(() => {
    if (race?.status === 'in_progress') {
      const interval = setInterval(() => {
        // Simulate GPS location updates
        const mockLocation = {
          latitude: 40.7128 + (Math.random() - 0.5) * 0.01,
          longitude: -74.0060 + (Math.random() - 0.5) * 0.01,
          speed: Math.random() * 60 + 20, // 20-80 km/h
          heading: Math.random() * 360,
          recordedAt: new Date().toISOString()
        }
        
        sendLocation(mockLocation)
      }, 2000) // Send location every 2 seconds

      return () => clearInterval(interval)
    }
  }, [race?.status, sendLocation])

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