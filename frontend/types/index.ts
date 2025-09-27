// User Types
export interface UserDto {
  id: string
  email: string
  username: string
  profilePictureUrl?: string
  bio?: string
  isPrivate: boolean
  createdAt: string
  followersCount: number
  followingCount: number
  racesCount: number
  isFollowedByCurrentUser: boolean
}

// Race Types
export interface RaceDto {
  id: string
  hostId: string
  name?: string
  startLatitude: number
  startLongitude: number
  endLatitude: number
  endLongitude: number
  status: string
  scheduledAt?: string
  startedAt?: string
  finishedAt?: string
  createdAt: string
  racers: RaceRacerDto[]
}

export interface RaceRacerDto {
  id: string
  userId: string
  username: string
  profilePictureUrl?: string
  startOrder: number
  vehicleId?: string
  status: string
  joinedAt?: string
  finishTime?: string
  finalRank?: number
}

export interface LocationDto {
  latitude: number
  longitude: number
  speed?: number
  heading?: number
  recordedAt: string
}

export interface RaceLiveStatusDto {
  raceId: string
  status: string
  racers: RacerStatusDto[]
  lastUpdated: string
}

export interface RacerStatusDto {
  userId: string
  username: string
  profilePictureUrl?: string
  rank: number
  lastLocation?: LocationDto
  distanceAlongRoute?: number
  finished: boolean
  finishTime?: string
}

// Vehicle Types
export interface VehicleDto {
  id: string
  ownerId: string
  name?: string
  type: string
  designMeta?: any
  model3DUrl?: string
  maxSpeed: number
  createdAt: string
}

// Post Types
export interface PostDto {
  id: string
  authorId: string
  authorUsername: string
  authorProfilePictureUrl?: string
  text?: string
  visibility: string
  likesCount: number
  commentsCount: number
  isLikedByCurrentUser: boolean
  createdAt: string
  media: PostMediaDto[]
}

export interface PostMediaDto {
  id: string
  mediaUrl: string
  mediaType: string
  orderIndex: number
}

// API Response Types
export interface PagedResult<T> {
  items: T[]
  nextCursor?: string
  hasMore: boolean
  totalCount: number
}

export interface ApiError {
  message: string
  code: string
  details?: any
}

// Form Types
export interface CreateRaceDto {
  name?: string
  racerUserIds: string[]
  startLatitude: number
  startLongitude: number
  endLatitude: number
  endLongitude: number
  routeGeoJson?: any
  scheduledAt?: string
}

export interface UpdateUserDto {
  username: string
  bio?: string
  isPrivate: boolean
}

// Event Types
export interface EventDto {
  id: string
  hostId: string
  hostUsername: string
  title: string
  description?: string
  locationLatitude?: number
  locationLongitude?: number
  startAt?: string
  endAt?: string
  visibility: string
  joinPolicy: string
  createdAt: string
  contributors: EventContributorDto[]
}

export interface EventContributorDto {
  id: string
  userId: string
  username: string
  profilePictureUrl?: string
  role: string
  grantedBy: string
  createdAt: string
}

export interface CreateEventDto {
  title: string
  description?: string
  locationLatitude?: number
  locationLongitude?: number
  startAt?: string
  endAt?: string
  visibility?: string
  joinPolicy?: string
}

export interface CreatePostDto {
  text?: string
  visibility?: string
  media: CreatePostMediaDto[]
}

export interface CreatePostMediaDto {
  mediaUrl: string
  mediaType: string
  orderIndex: number
}