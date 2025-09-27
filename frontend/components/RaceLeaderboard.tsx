'use client'

import { Trophy, Clock, MapPin, Zap } from 'lucide-react'
import { RaceLiveStatusDto } from '@/types'

interface RaceLeaderboardProps {
  raceId: string
  liveStatus?: RaceLiveStatusDto | null
}

export default function RaceLeaderboard({ raceId, liveStatus }: RaceLeaderboardProps) {
  if (!liveStatus) {
    return (
      <div className="racing-card">
        <h3 className="text-xl font-bold mb-4">Leaderboard</h3>
        <div className="space-y-3">
          {[1, 2, 3].map((i) => (
            <div key={i} className="leaderboard-item animate-pulse">
              <div className="flex items-center space-x-3">
                <div className="w-8 h-8 bg-race-gray rounded-full"></div>
                <div className="flex-1">
                  <div className="h-4 bg-race-gray rounded w-24 mb-1"></div>
                  <div className="h-3 bg-race-gray rounded w-16"></div>
                </div>
              </div>
              <div className="h-4 bg-race-gray rounded w-12"></div>
            </div>
          ))}
        </div>
      </div>
    )
  }

  const sortedRacers = [...liveStatus.racers].sort((a, b) => a.rank - b.rank)

  const getRankColor = (rank: number) => {
    switch (rank) {
      case 1: return 'text-race-primary'
      case 2: return 'text-race-accent'
      case 3: return 'text-race-warning'
      default: return 'text-gray-400'
    }
  }

  const getRankIcon = (rank: number) => {
    if (rank <= 3) {
      return <Trophy className={`w-5 h-5 ${getRankColor(rank)}`} />
    }
    return <span className={`font-bold ${getRankColor(rank)}`}>#{rank}</span>
  }

  const formatTime = (timeSpan?: string) => {
    if (!timeSpan) return null
    const seconds = Math.floor(parseFloat(timeSpan) / 10000000) // Convert from ticks
    const minutes = Math.floor(seconds / 60)
    const remainingSeconds = seconds % 60
    return `${minutes}:${remainingSeconds.toString().padStart(2, '0')}`
  }

  const formatDistance = (distance?: number) => {
    if (!distance) return 'Starting...'
    if (distance < 1000) return `${Math.round(distance)}m`
    return `${(distance / 1000).toFixed(1)}km`
  }

  const formatSpeed = (speed?: number) => {
    if (!speed) return null
    return `${Math.round(speed)} km/h`
  }

  return (
    <div className="racing-card">
      <div className="flex items-center justify-between mb-4">
        <h3 className="text-xl font-bold">Live Leaderboard</h3>
        <div className="flex items-center space-x-1 text-race-primary">
          <Zap className="w-4 h-4" />
          <span className="text-sm font-semibold">LIVE</span>
        </div>
      </div>

      <div className="space-y-2">
        {sortedRacers.map((racer) => (
          <div
            key={racer.userId}
            className={`leaderboard-item ${racer.finished ? 'opacity-90' : ''}`}
          >
            <div className="flex items-center space-x-3 flex-1">
              {/* Rank */}
              <div className="flex items-center justify-center w-8 h-8">
                {getRankIcon(racer.rank)}
              </div>

              {/* Profile */}
              <div className="flex items-center space-x-2 flex-1">
                <div className="w-8 h-8 bg-race-primary rounded-full flex items-center justify-center">
                  <span className="text-xs font-bold text-race-dark">
                    {racer.username.charAt(0).toUpperCase()}
                  </span>
                </div>
                <div className="flex-1 min-w-0">
                  <div className="font-semibold truncate">{racer.username}</div>
                  <div className="text-xs text-gray-400 flex items-center space-x-2">
                    {racer.finished ? (
                      <span className="text-race-secondary font-semibold">FINISHED</span>
                    ) : (
                      <>
                        <MapPin className="w-3 h-3" />
                        <span>{formatDistance(racer.distanceAlongRoute)}</span>
                        {racer.lastLocation?.speed && (
                          <>
                            <span>•</span>
                            <span>{formatSpeed(racer.lastLocation.speed)}</span>
                          </>
                        )}
                      </>
                    )}
                  </div>
                </div>
              </div>
            </div>

            {/* Time */}
            <div className="text-right">
              {racer.finished && racer.finishTime ? (
                <div className="flex items-center space-x-1 text-race-secondary">
                  <Clock className="w-4 h-4" />
                  <span className="font-bold">{formatTime(racer.finishTime)}</span>
                </div>
              ) : (
                <div className={`w-3 h-3 rounded-full racing-pulse ${
                  racer.lastLocation ? 'bg-race-primary' : 'bg-gray-500'
                }`}></div>
              )}
            </div>
          </div>
        ))}
      </div>

      {sortedRacers.length === 0 && (
        <div className="text-center py-8 text-gray-400">
          <Trophy className="w-12 h-12 mx-auto mb-3 opacity-50" />
          <p>No racers yet</p>
        </div>
      )}

      {liveStatus && (
        <div className="mt-4 pt-4 border-t border-race-primary/20 text-xs text-gray-400">
          Race Status: {liveStatus.status.toUpperCase()}
          <br />
          Last Update: {new Date(liveStatus.lastUpdated).toLocaleTimeString()}
        </div>
      )}
    </div>
  )
}