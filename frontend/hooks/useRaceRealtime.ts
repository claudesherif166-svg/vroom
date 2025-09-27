'use client'

import { useEffect, useRef, useState, useCallback } from 'react'
import { HubConnectionBuilder, HubConnectionState, LogLevel } from '@microsoft/signalr'
import { LocationDto, RaceLiveStatusDto } from '@/types'

export function useRaceRealtime(
  raceId: string,
  getAccessToken: () => Promise<string | null>
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
    const connection = new HubConnectionBuilder()
      .withUrl(`${process.env.NEXT_PUBLIC_API_URL}/hubs/race`, {
        accessTokenFactory: getAccessToken
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