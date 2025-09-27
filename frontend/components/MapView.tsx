'use client'

import { useEffect, useRef, useState } from 'react'
import { Map, NavigationControl, Marker, Source, Layer } from 'react-map-gl'
import { MapPin, Flag } from 'lucide-react'
import { RaceDto, RaceLiveStatusDto } from '@/types'
import 'maplibre-gl/dist/maplibre-gl.css'

interface MapViewProps {
  race: RaceDto
  liveStatus?: RaceLiveStatusDto | null
  className?: string
}

export default function MapView({ race, liveStatus, className }: MapViewProps) {
  const mapRef = useRef<any>(null)
  const [viewState, setViewState] = useState({
    longitude: (race.startLongitude + race.endLongitude) / 2,
    latitude: (race.startLatitude + race.endLatitude) / 2,
    zoom: 13
  })

  // Calculate bounds to fit start and end points
  useEffect(() => {
    if (mapRef.current && race) {
      const bounds = [
        [Math.min(race.startLongitude, race.endLongitude), Math.min(race.startLatitude, race.endLatitude)],
        [Math.max(race.startLongitude, race.endLongitude), Math.max(race.startLatitude, race.endLatitude)]
      ]
      
      mapRef.current.fitBounds(bounds, {
        padding: 50,
        duration: 1000
      })
    }
  }, [race])

  // Route line data
  const routeData = {
    type: 'Feature' as const,
    geometry: {
      type: 'LineString' as const,
      coordinates: [
        [race.startLongitude, race.startLatitude],
        [race.endLongitude, race.endLatitude]
      ]
    }
  }

  const routeLayerStyle = {
    id: 'route',
    type: 'line' as const,
    paint: {
      'line-color': '#00ff88',
      'line-width': 4,
      'line-opacity': 0.8
    }
  }

  return (
    <div className={`relative ${className}`}>
      <Map
        {...viewState}
        onMove={evt => setViewState(evt.viewState)}
        mapStyle="https://basemaps.cartocdn.com/gl/dark-matter-gl-style/style.json"
        ref={mapRef}
        attributionControl={false}
      >
        <NavigationControl position="top-right" />

        {/* Route Line */}
        <Source id="route" type="geojson" data={routeData}>
          <Layer {...routeLayerStyle} />
        </Source>

        {/* Start Point */}
        <Marker
          longitude={race.startLongitude}
          latitude={race.startLatitude}
          anchor="bottom"
        >
          <div className="flex items-center justify-center w-10 h-10 bg-race-primary rounded-full racing-pulse">
            <MapPin className="w-6 h-6 text-race-dark" />
          </div>
        </Marker>

        {/* Finish Point */}
        <Marker
          longitude={race.endLongitude}
          latitude={race.endLatitude}
          anchor="bottom"
        >
          <div className="flex items-center justify-center w-10 h-10 bg-race-secondary rounded-full racing-pulse">
            <Flag className="w-6 h-6 text-white" />
          </div>
        </Marker>

        {/* Live Racer Positions */}
        {liveStatus?.racers.map((racer) => (
          racer.lastLocation && (
            <Marker
              key={racer.userId}
              longitude={racer.lastLocation.longitude}
              latitude={racer.lastLocation.latitude}
              anchor="center"
            >
              <div className="relative">
                {/* Racer Marker */}
                <div className={`w-8 h-8 rounded-full border-2 ${
                  racer.rank === 1 ? 'bg-race-primary border-race-primary' :
                  racer.rank === 2 ? 'bg-race-accent border-race-accent' :
                  racer.rank === 3 ? 'bg-race-warning border-race-warning' :
                  'bg-race-gray border-gray-400'
                } flex items-center justify-center racing-pulse`}>
                  <span className="text-xs font-bold text-white">{racer.rank}</span>
                </div>
                
                {/* Username Label */}
                <div className="absolute -bottom-8 left-1/2 transform -translate-x-1/2 bg-race-dark/80 text-white px-2 py-1 rounded text-xs whitespace-nowrap">
                  {racer.username}
                </div>
                
                {/* Direction Indicator */}
                {racer.lastLocation.heading && (
                  <div 
                    className="absolute top-0 left-1/2 w-1 h-6 bg-race-primary transform -translate-x-1/2 -translate-y-full origin-bottom"
                    style={{
                      transform: `translateX(-50%) translateY(-100%) rotate(${racer.lastLocation.heading}deg)`
                    }}
                  />
                )}
              </div>
            </Marker>
          )
        ))}
      </Map>

      {/* Map Legend */}
      <div className="absolute bottom-4 left-4 bg-race-dark/90 backdrop-blur-sm rounded-lg p-3 text-sm">
        <h4 className="font-bold mb-2 text-race-primary">Legend</h4>
        <div className="space-y-2">
          <div className="flex items-center space-x-2">
            <div className="w-4 h-4 bg-race-primary rounded-full"></div>
            <span>Start Point</span>
          </div>
          <div className="flex items-center space-x-2">
            <div className="w-4 h-4 bg-race-secondary rounded-full"></div>
            <span>Finish Line</span>
          </div>
          <div className="flex items-center space-x-2">
            <div className="w-4 h-4 bg-race-accent rounded-full"></div>
            <span>Live Racers</span>
          </div>
        </div>
      </div>

      {/* Race Stats Overlay */}
      {liveStatus && (
        <div className="absolute top-4 left-4 bg-race-dark/90 backdrop-blur-sm rounded-lg p-4">
          <div className="flex items-center space-x-2 mb-2">
            <div className="w-3 h-3 bg-race-primary rounded-full racing-pulse"></div>
            <span className="font-bold text-race-primary">LIVE</span>
          </div>
          <div className="text-sm space-y-1">
            <div>Active Racers: {liveStatus.racers.filter(r => !r.finished).length}</div>
            <div>Finished: {liveStatus.racers.filter(r => r.finished).length}</div>
            <div className="text-xs text-gray-400">
              Updated: {new Date(liveStatus.lastUpdated).toLocaleTimeString()}
            </div>
          </div>
        </div>
      )}
    </div>
  )
}