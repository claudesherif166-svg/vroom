'use client'

import { useState, useEffect } from 'react'
import { useAuth } from '@/hooks/useAuth'
import { apiClient } from '@/lib/api'
import { VehicleDto, PagedResult } from '@/types'
import { useRouter } from 'next/navigation'
import { Zap, Car, Plus, Settings } from 'lucide-react'
import Link from 'next/link'

export default function VehiclesPage() {
  const { isAuthenticated, isLoading, user } = useAuth()
  const router = useRouter()
  const [vehicles, setVehicles] = useState<VehicleDto[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [nextCursor, setNextCursor] = useState<string | null>(null)
  const [hasMore, setHasMore] = useState(false)

  useEffect(() => {
    if (!isLoading && !isAuthenticated) {
      router.push('/auth/login?returnTo=/vehicles')
      return
    }

    if (isAuthenticated && user) {
      loadVehicles()
    }
  }, [isAuthenticated, isLoading, router, user])

  const loadVehicles = async (cursor?: string) => {
    if (!user) return

    try {
      setError(null)
      const result = await apiClient.getVehicles(user.id, cursor)
      
      if (cursor) {
        setVehicles(prev => [...prev, ...result.items])
      } else {
        setVehicles(result.items)
      }
      
      setNextCursor(result.nextCursor || null)
      setHasMore(result.hasMore)
    } catch (error) {
      console.error('Failed to load vehicles:', error)
      setError(error instanceof Error ? error.message : 'Failed to load vehicles')
    } finally {
      setLoading(false)
    }
  }

  const loadMore = () => {
    if (nextCursor && !loading) {
      setLoading(true)
      loadVehicles(nextCursor)
    }
  }

  const getVehicleIcon = (type: string) => {
    switch (type.toLowerCase()) {
      case 'bike':
        return '🏍️'
      case 'truck':
        return '🚛'
      default:
        return '🏎️'
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
            <Link href="/vehicles/new" className="racing-button flex items-center space-x-2">
              <Plus className="w-4 h-4" />
              <span>Add Vehicle</span>
            </Link>
          </div>
        </div>
      </header>

      {/* Main Content */}
      <div className="container mx-auto p-4 max-w-6xl">
        <div className="mb-8">
          <h1 className="text-3xl font-bold mb-2">My Garage</h1>
          <p className="text-gray-400">Manage your racing vehicles and customize their appearance</p>
        </div>

        {error && (
          <div className="racing-card p-4 mb-6 border-red-500/50 bg-red-500/10">
            <p className="text-red-400">{error}</p>
            <button 
              onClick={() => loadVehicles()}
              className="mt-2 text-race-primary hover:text-race-accent transition-colors"
            >
              Try again
            </button>
          </div>
        )}

        {/* Vehicles Grid */}
        <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-6">
          {vehicles.length === 0 && !loading ? (
            <div className="col-span-full racing-card p-8 text-center">
              <Car className="w-16 h-16 text-race-primary/50 mx-auto mb-4" />
              <h3 className="text-xl font-bold mb-2">No vehicles yet</h3>
              <p className="text-gray-400 mb-4">
                Add your first vehicle to start racing!
              </p>
              <Link href="/vehicles/new" className="racing-button">
                Add Vehicle
              </Link>
            </div>
          ) : (
            vehicles.map((vehicle) => (
              <div key={vehicle.id} className="racing-card hover:racing-glow transition-all duration-300">
                {/* Vehicle Preview */}
                <div className="aspect-video bg-race-gray/50 rounded-lg mb-4 flex items-center justify-center">
                  {vehicle.model3DUrl ? (
                    <img 
                      src={vehicle.model3DUrl} 
                      alt={vehicle.name || 'Vehicle'}
                      className="w-full h-full object-cover rounded-lg"
                    />
                  ) : (
                    <div className="text-6xl">
                      {getVehicleIcon(vehicle.type)}
                    </div>
                  )}
                </div>

                {/* Vehicle Info */}
                <div className="mb-4">
                  <div className="flex items-center justify-between mb-2">
                    <h3 className="text-xl font-bold">
                      {vehicle.name || `${vehicle.type.charAt(0).toUpperCase() + vehicle.type.slice(1)}`}
                    </h3>
                    <div className="text-xs text-race-accent bg-race-accent/10 px-2 py-1 rounded">
                      {vehicle.type.toUpperCase()}
                    </div>
                  </div>
                  
                  <div className="text-sm text-gray-400 space-y-1">
                    <div>Max Speed: {vehicle.maxSpeed} km/h</div>
                    <div>Added: {new Date(vehicle.createdAt).toLocaleDateString()}</div>
                  </div>
                </div>

                {/* Vehicle Actions */}
                <div className="flex items-center justify-between pt-4 border-t border-race-primary/20">
                  <Link 
                    href={`/vehicles/${vehicle.id}`}
                    className="text-race-primary hover:text-race-accent transition-colors font-semibold"
                  >
                    View Details
                  </Link>
                  
                  <div className="flex items-center space-x-2">
                    <Link 
                      href={`/vehicles/${vehicle.id}/edit`}
                      className="p-2 text-race-primary hover:text-race-accent transition-colors"
                    >
                      <Settings className="w-4 h-4" />
                    </Link>
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