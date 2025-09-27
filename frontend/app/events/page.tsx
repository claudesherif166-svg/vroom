'use client'

import { useState, useEffect } from 'react'
import { useAuth } from '@/hooks/useAuth'
import { apiClient } from '@/lib/api'
import { EventDto, PagedResult } from '@/types'
import { useRouter } from 'next/navigation'
import { Zap, Calendar, MapPin, Users, Plus } from 'lucide-react'
import Link from 'next/link'

export default function EventsPage() {
  const { isAuthenticated, isLoading, user } = useAuth()
  const router = useRouter()
  const [events, setEvents] = useState<EventDto[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [nextCursor, setNextCursor] = useState<string | null>(null)
  const [hasMore, setHasMore] = useState(false)

  useEffect(() => {
    if (!isLoading && !isAuthenticated) {
      router.push('/auth/login?returnTo=/events')
      return
    }

    if (isAuthenticated) {
      loadEvents()
    }
  }, [isAuthenticated, isLoading, router])

  const loadEvents = async (cursor?: string) => {
    try {
      setError(null)
      const result = await apiClient.getEvents(cursor)
      
      if (cursor) {
        setEvents(prev => [...prev, ...result.items])
      } else {
        setEvents(result.items)
      }
      
      setNextCursor(result.nextCursor || null)
      setHasMore(result.hasMore)
    } catch (error) {
      console.error('Failed to load events:', error)
      setError(error instanceof Error ? error.message : 'Failed to load events')
    } finally {
      setLoading(false)
    }
  }

  const loadMore = () => {
    if (nextCursor && !loading) {
      setLoading(true)
      loadEvents(nextCursor)
    }
  }

  if (isLoading) {
    return (
      <div className="min-h-screen bg-race-dark flex items-center justify-center">
        <div className="racing-card p-8 text-center">
          <div className="animate-spin w-16 h-16 border-4 border-race-primary border-t-transparent rounded-full mx-auto mb-4"></div>
          <p className="text-race-primary">Loading...</p>
        </div>
      </div>
    )
  }

  return (
    <div className="min-h-screen bg-race-dark">
      {/* Header */}
      <header className="bg-race-gray/50 backdrop-blur-sm border-b border-race-primary/20 p-4">
        <div className="container mx-auto flex items-center justify-between">
          <div className="flex items-center space-x-4">
            <Link href="/" className="flex items-center space-x-2">
              <Zap className="h-8 w-8 text-race-primary" />
              <span className="text-2xl font-racing font-bold text-race-primary">StreetRacer</span>
            </Link>
          </div>

          <div className="flex items-center space-x-4">
            <span className="text-race-primary">Welcome, {user?.username}</span>
            <Link href="/events/new" className="racing-button flex items-center space-x-2">
              <Plus className="w-4 h-4" />
              <span>New Event</span>
            </Link>
          </div>
        </div>
      </header>

      {/* Main Content */}
      <div className="container mx-auto p-4 max-w-4xl">
        <div className="mb-8">
          <h1 className="text-3xl font-bold mb-2">Racing Events</h1>
          <p className="text-gray-400">Discover and join racing events in your area</p>
        </div>

        {error && (
          <div className="racing-card p-4 mb-6 border-red-500/50 bg-red-500/10">
            <p className="text-red-400">{error}</p>
            <button 
              onClick={() => loadEvents()}
              className="mt-2 text-race-primary hover:text-race-accent transition-colors"
            >
              Try again
            </button>
          </div>
        )}

        {/* Events Grid */}
        <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-6">
          {events.length === 0 && !loading ? (
            <div className="col-span-full racing-card p-8 text-center">
              <Calendar className="w-16 h-16 text-race-primary/50 mx-auto mb-4" />
              <h3 className="text-xl font-bold mb-2">No events yet</h3>
              <p className="text-gray-400 mb-4">
                Be the first to create a racing event!
              </p>
              <Link href="/events/new" className="racing-button">
                Create Event
              </Link>
            </div>
          ) : (
            events.map((event) => (
              <div key={event.id} className="racing-card hover:racing-glow transition-all duration-300">
                {/* Event Header */}
                <div className="flex items-start justify-between mb-4">
                  <div className="flex-1">
                    <h3 className="text-xl font-bold mb-1">{event.title}</h3>
                    <p className="text-sm text-gray-400">by {event.hostUsername}</p>
                  </div>
                  <div className="text-xs text-race-primary bg-race-primary/10 px-2 py-1 rounded">
                    {event.visibility}
                  </div>
                </div>

                {/* Event Description */}
                {event.description && (
                  <p className="text-gray-300 mb-4 line-clamp-3">{event.description}</p>
                )}

                {/* Event Details */}
                <div className="space-y-2 mb-4">
                  {event.startAt && (
                    <div className="flex items-center space-x-2 text-sm text-gray-400">
                      <Calendar className="w-4 h-4" />
                      <span>{new Date(event.startAt).toLocaleDateString()}</span>
                      <span>{new Date(event.startAt).toLocaleTimeString()}</span>
                    </div>
                  )}
                  
                  {event.locationLatitude && event.locationLongitude && (
                    <div className="flex items-center space-x-2 text-sm text-gray-400">
                      <MapPin className="w-4 h-4" />
                      <span>
                        {event.locationLatitude.toFixed(4)}, {event.locationLongitude.toFixed(4)}
                      </span>
                    </div>
                  )}

                  <div className="flex items-center space-x-2 text-sm text-gray-400">
                    <Users className="w-4 h-4" />
                    <span>{event.contributors.length} contributors</span>
                  </div>
                </div>

                {/* Event Actions */}
                <div className="flex items-center justify-between pt-4 border-t border-race-primary/20">
                  <Link 
                    href={`/events/${event.id}`}
                    className="text-race-primary hover:text-race-accent transition-colors font-semibold"
                  >
                    View Details
                  </Link>
                  
                  <div className="flex items-center space-x-2">
                    <button className="text-sm px-3 py-1 border border-race-primary/30 text-race-primary rounded hover:bg-race-primary/10 transition-colors">
                      Join
                    </button>
                  </div>
                </div>
              </div>
            ))
          )}
        </div>

        {/* Load More */}
        {hasMore && (
          <div className="mt-8 text-center">
            <button
              onClick={loadMore}
              disabled={loading}
              className="racing-button disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {loading ? 'Loading...' : 'Load More'}
            </button>
          </div>
        )}
      </div>
    </div>
  )
}