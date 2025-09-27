import { authService } from './auth'
import { 
  UserDto, 
  PostDto, 
  RaceDto, 
  CreateRaceDto, 
  LocationDto, 
  RaceLiveStatusDto,
  PagedResult,
  CreatePostDto,
  UpdateUserDto
  EventDto,
  CreateEventDto,
  VehicleDto,
  CreateVehicleDto,
  UpdateVehicleDto
} from '@/types'

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000'

class ApiError extends Error {
  constructor(
    message: string,
    public status: number,
    public code?: string
  ) {
    super(message)
    this.name = 'ApiError'
  }
}

class ApiClient {
  private async request<T>(
    endpoint: string,
    options: RequestInit = {}
  ): Promise<T> {
    const url = `${API_BASE_URL}${endpoint}`
    
    // Get access token
    const token = await authService.getValidAccessToken()
    
    const headers: HeadersInit = {
      'Content-Type': 'application/json',
      ...options.headers,
    }

    if (token) {
      headers.Authorization = `Bearer ${token}`
    }

    const response = await fetch(url, {
      ...options,
      headers,
    })

    if (!response.ok) {
      let errorMessage = `HTTP ${response.status}`
      let errorCode = response.status.toString()
      
      try {
        const errorData = await response.json()
        errorMessage = errorData.message || errorData.title || errorMessage
        errorCode = errorData.code || errorCode
      } catch {
        // Ignore JSON parsing errors
      }
      
      throw new ApiError(errorMessage, response.status, errorCode)
    }

    if (response.status === 204) {
      return {} as T
    }

    return response.json()
  }

  // Auth endpoints
  async syncUser(): Promise<UserDto> {
    return this.request<UserDto>('/api/v1/auth/sync', {
      method: 'POST',
    })
  }

  async getProfile(): Promise<UserDto> {
    return this.request<UserDto>('/api/v1/auth/profile')
  }

  // User endpoints
  async getCurrentUser(): Promise<UserDto> {
    return this.request<UserDto>('/api/v1/users/me')
  }

  async getUser(id: string): Promise<UserDto> {
    return this.request<UserDto>(`/api/v1/users/${id}`)
  }

  async updateCurrentUser(data: UpdateUserDto): Promise<UserDto> {
    return this.request<UserDto>('/api/v1/users/me', {
      method: 'PUT',
      body: JSON.stringify(data),
    })
  }

  // Feed endpoints
  async getFeed(cursor?: string, limit = 20): Promise<PagedResult<PostDto>> {
    const params = new URLSearchParams()
    if (cursor) params.append('cursor', cursor)
    params.append('limit', limit.toString())
    
    return this.request<PagedResult<PostDto>>(`/api/v1/feed?${params}`)
  }

  // Post endpoints
  async createPost(data: CreatePostDto): Promise<PostDto> {
    return this.request<PostDto>('/api/v1/posts', {
      method: 'POST',
      body: JSON.stringify(data),
    })
  }

  async getPost(id: string): Promise<PostDto> {
    return this.request<PostDto>(`/api/v1/posts/${id}`)
  }

  async deletePost(id: string): Promise<void> {
    return this.request<void>(`/api/v1/posts/${id}`, {
      method: 'DELETE',
    })
  }

  async likePost(id: string): Promise<void> {
    return this.request<void>(`/api/v1/posts/${id}/likes`, {
      method: 'POST',
    })
  }

  async unlikePost(id: string): Promise<void> {
    return this.request<void>(`/api/v1/posts/${id}/likes`, {
      method: 'DELETE',
    })
  }

  // Race endpoints
  async createRace(data: CreateRaceDto): Promise<RaceDto> {
    return this.request<RaceDto>('/api/v1/races', {
      method: 'POST',
      body: JSON.stringify(data),
    })
  }

  async getRace(id: string): Promise<RaceDto> {
    return this.request<RaceDto>(`/api/v1/races/${id}`)
  }

  async inviteRacers(raceId: string, racerUserIds: string[]): Promise<void> {
    return this.request<void>(`/api/v1/races/${raceId}/invite`, {
      method: 'POST',
      body: JSON.stringify({ racerUserIds }),
    })
  }

  async acceptInvite(raceId: string): Promise<void> {
    return this.request<void>(`/api/v1/races/${raceId}/accept`, {
      method: 'POST',
    })
  }

  async startRace(raceId: string): Promise<void> {
    return this.request<void>(`/api/v1/races/${raceId}/start`, {
      method: 'POST',
    })
  }

  async cancelRace(raceId: string): Promise<void> {
    return this.request<void>(`/api/v1/races/${raceId}/cancel`, {
      method: 'POST',
    })
  }

  async submitLocation(raceId: string, location: LocationDto): Promise<void> {
    return this.request<void>(`/api/v1/races/${raceId}/location`, {
      method: 'POST',
      body: JSON.stringify(location),
    })
  }

  async getRaceLiveStatus(raceId: string): Promise<RaceLiveStatusDto> {
    return this.request<RaceLiveStatusDto>(`/api/v1/races/${raceId}/live`)
  }

  async getRaceResults(raceId: string): Promise<RaceLiveStatusDto> {
    return this.request<RaceLiveStatusDto>(`/api/v1/races/${raceId}/results`)
  }

  // Vehicle endpoints
  async getVehicles(ownerId?: string, cursor?: string): Promise<PagedResult<VehicleDto>> {
    const params = new URLSearchParams()
    if (ownerId) params.append('ownerId', ownerId)
    if (cursor) params.append('cursor', cursor)
    
    return this.request<PagedResult<VehicleDto>>(`/api/v1/vehicles?${params}`)
  }

  async createVehicle(data: CreateVehicleDto): Promise<VehicleDto> {
    return this.request<VehicleDto>('/api/v1/vehicles', {
      method: 'POST',
      body: JSON.stringify(data),
    })
  }

  async getVehicle(id: string): Promise<VehicleDto> {
    return this.request<VehicleDto>(`/api/v1/vehicles/${id}`)
  }

  async updateVehicle(id: string, data: UpdateVehicleDto): Promise<VehicleDto> {
    return this.request<VehicleDto>(`/api/v1/vehicles/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    })
  }

  async deleteVehicle(id: string): Promise<void> {
    return this.request<void>(`/api/v1/vehicles/${id}`, {
      method: 'DELETE',
    })
  }

  // Event endpoints
  async getEvents(cursor?: string): Promise<PagedResult<EventDto>> {
    const params = new URLSearchParams()
    if (cursor) params.append('cursor', cursor)
    
    return this.request<PagedResult<EventDto>>(`/api/v1/events?${params}`)
  }

  async createEvent(data: CreateEventDto): Promise<EventDto> {
    return this.request<EventDto>('/api/v1/events', {
      method: 'POST',
      body: JSON.stringify(data),
    })
  }

  async getEvent(id: string): Promise<EventDto> {
    return this.request<EventDto>(`/api/v1/events/${id}`)
  }

  // Search endpoints
  async search(type: string, query: string, cursor?: string): Promise<PagedResult<any>> {
    const params = new URLSearchParams()
    params.append('type', type)
    params.append('q', query)
    if (cursor) params.append('cursor', cursor)
    
    return this.request<PagedResult<any>>(`/api/v1/search?${params}`)
  }

  // Health check
  async healthCheck(): Promise<{ status: string; timestamp: string }> {
    return this.request<{ status: string; timestamp: string }>('/health')
  }
}

export const apiClient = new ApiClient()
export { ApiError }