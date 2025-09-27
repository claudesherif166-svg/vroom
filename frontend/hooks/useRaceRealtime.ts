'use client'

import { useEffect, useRef, useState, useCallback } from 'react'
import { HubConnectionBuilder, HubConnectionState, LogLevel } from '@microsoft/signalr'
import { LocationDto, RaceLiveStatusDto } from '@/types'

export function useRaceRealtime(
  raceId: string,
  getAccessToken: () => string | null
) {
  const [status, setStatus] = useState<RaceLiveStatusDto | null>(null)
  const [connectionState, setConnectionState] = useState<'Disconnected' | 'Connecting' | 'Connected'>('Disconnected')
  const connectionRef = useRef<any>(null)

  const sendLocation = useCallback(async (location: LocationDto) => {
    if (connectionRef.current?.state === HubConnectionState.Connected) {
      try {
        await connectionRef.current.invoke('SendLocation', raceId, location)
        console.log('Location sent successfully')
      } catch (error) {
        console.error('Error sending location:', error)
      }
    } else {
      console.warn('Cannot send location - not connected to hub')
    }
  }, [raceId])

  const acceptInvite = useCallback(async () => {
    if (connectionRef.current?.state === HubConnectionState.Connected) {
      try {
        await connectionRef.current.invoke('AcceptInvite', raceId)
        console.log('Invite accepted successfully')
      } catch (error) {
        console.error('Error accepting invite:', error)
      }
    }
  }, [raceId])

  const requestStart = useCallback(async () => {
    if (connectionRef.current?.state === HubConnectionState.Connected) {
      try {
        await connectionRef.current.invoke('RequestStart', raceId)
        console.log('Race start requested successfully')
      } catch (error) {
        console.error('Error requesting race start:', error)
      }
    }
  }, [raceId])

  useEffect(() => {
    // For now, we'll use a mock token since Keycloak isn't set up yet
    const mockAccessToken = () => 'mock-jwt-token'

    const connection = new HubConnectionBuilder()
      .withUrl(`${process.env.NEXT_PUBLIC_API_URL}/hubs/race`, {
        accessTokenFactory: mockAccessToken // Use mock token for development
      })
      .configureLogging(LogLevel.Information)
      .withAutomaticReconnect([0, 2000, 10000, 30000])
      .build()

    connectionRef.current = connection

    // Connection state handlers
    connection.onclose((error) => {
      console.log('SignalR connection closed:', error)
      setConnectionState('Disconnected')
    })

    connection.onreconnecting(() => {
      console.log('SignalR reconnecting...')
      setConnectionState('Connecting')
    })

    connection.onreconnected(() => {
      console.log('SignalR reconnected')
      setConnectionState('Connected')
      // Rejoin race group after reconnection
      connection.invoke('JoinRace', raceId).catch(console.error)
    })

    // Race event handlers
    connection.on('RaceUpdated', (raceStatus: RaceLiveStatusDto) => {
      console.log('Race updated:', raceStatus)
      setStatus(raceStatus)
    })

    connection.on('RaceFinished', (finalResults: RaceLiveStatusDto) => {
      console.log('Race finished:', finalResults)
      setStatus(finalResults)
    })

    connection.on('ParticipantJoined', (data) => {
      console.log('Participant joined:', data)
    })

    connection.on('RaceStartRequested', (data) => {
      console.log('Race start requested:', data)
    })

    connection.on('Error', (error) => {
      console.error('SignalR error:', error)
    })

    // Start connection
    const startConnection = async () => {
      try {
        setConnectionState('Connecting')
        await connection.start()
        setConnectionState('Connected')
        console.log('SignalR connected successfully')

        // Join race group
        await connection.invoke('JoinRace', raceId)
        console.log(`Joined race group: ${raceId}`)
      } catch (error) {
        console.error('Error starting SignalR connection:', error)
        setConnectionState('Disconnected')
        
        // In development, simulate connection and data
        setTimeout(() => {
          setConnectionState('Connected')
          // Mock live race data for development
          setStatus({
            raceId,
            status: 'in_progress',
            lastUpdated: new Date().toISOString(),
            racers: [
              {
                userId: '123e4567-e89b-12d3-a456-426614174001',
                username: 'SpeedDemon',
                profilePictureUrl: null,
                rank: 1,
                lastLocation: {
                  latitude: 40.7128 + Math.random() * 0.01,
                  longitude: -74.0060 + Math.random() * 0.01,
                  speed: 45 + Math.random() * 20,
                  heading: Math.random() * 360,
                  recordedAt: new Date().toISOString()
                },
                distanceAlongRoute: 1200 + Math.random() * 500,
                finished: false,
                finishTime: undefined
              },
              {
                userId: '123e4567-e89b-12d3-a456-426614174002',
                username: 'RoadRunner',
                profilePictureUrl: null,
                rank: 2,
                lastLocation: {
                  latitude: 40.7100 + Math.random() * 0.01,
                  longitude: -74.0080 + Math.random() * 0.01,
                  speed: 40 + Math.random() * 15,
                  heading: Math.random() * 360,
                  recordedAt: new Date().toISOString()
                },
                distanceAlongRoute: 1000 + Math.random() * 400,
                finished: false,
                finishTime: undefined
              }
            ]
          })

          // Update mock data every few seconds
          const interval = setInterval(() => {
            setStatus(prev => {
              if (!prev) return null
              return {
                ...prev,
                lastUpdated: new Date().toISOString(),
                racers: prev.racers.map(racer => ({
                  ...racer,
                  lastLocation: racer.lastLocation ? {
                    ...racer.lastLocation,
                    latitude: racer.lastLocation.latitude + (Math.random() - 0.5) * 0.001,
                    longitude: racer.lastLocation.longitude + (Math.random() - 0.5) * 0.001,
                    speed: Math.max(0, racer.lastLocation.speed! + (Math.random() - 0.5) * 10),
                    heading: (racer.lastLocation.heading! + (Math.random() - 0.5) * 30) % 360,
                    recordedAt: new Date().toISOString()
                  } : null,
                  distanceAlongRoute: (racer.distanceAlongRoute || 0) + Math.random() * 50
                }))
              }
            })
          }, 3000)

          return () => clearInterval(interval)
        }, 1000)
      }
    }

    startConnection()

    // Cleanup
    return () => {
      if (connection.state === HubConnectionState.Connected) {
        connection.invoke('LeaveRace', raceId).finally(() => {
          connection.stop()
        })
      } else {
        connection.stop()
      }
    }
  }, [raceId])

  return {
    status,
    connectionState,
    sendLocation,
    acceptInvite,
    requestStart
  }
}