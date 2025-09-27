# StreetRacer - Real-Time Racing Platform

A mobile-first, map-based social racing platform with real-time GPS tracking, live leaderboards, and social features.

## Architecture

- **Frontend**: Next.js 14 (TypeScript) + Tailwind CSS + MapLibre GL JS
- **Backend**: ASP.NET Core 8 (Clean Architecture)
- **Database**: PostgreSQL + PostGIS for spatial data
- **Real-time**: SignalR for live race updates
- **Search**: Elasticsearch for fast queries and analytics
- **Cache**: Redis for live rankings and session management
- **Storage**: MinIO for media files
- **Auth**: Keycloak OIDC integration

## Quick Start

### Prerequisites

- Docker & Docker Compose
- .NET 8 SDK
- Node.js 20+

### Local Development

1. **Start Infrastructure Services**
```bash
docker-compose up -d postgres redis elasticsearch minio keycloak rabbitmq
```

2. **Setup Database**
```bash
cd backend
dotnet ef database update
```

3. **Start Backend**
```bash
cd backend/src/StreetRacer.Api
dotnet run
```

4. **Start Frontend**
```bash
cd frontend
npm install
npm run dev
```

5. **Access Services**
- Frontend: http://localhost:3000
- Backend API: http://localhost:5000
- Keycloak: http://localhost:8080 (admin/admin)
- MinIO: http://localhost:9001 (minio/minio123)
- Elasticsearch: http://localhost:9200

## Key Features

### Real-Time Racing
- GPS location streaming at 1-2Hz via SignalR
- Server-side authoritative ranking with anti-cheat
- Live leaderboards with sub-second updates
- PostGIS-powered route projection and distance calculations

### Social Features
- User profiles and follower system
- Race events with contributor management
- Social feed with posts, likes, and comments
- Stories with 24-hour expiration

### Anti-Cheat System
- Speed validation per vehicle type
- Teleportation detection
- Clock skew validation
- Audit logging for suspicious activities

## API Documentation

The API follows RESTful conventions with cursor-based pagination:

### Authentication
- Keycloak OIDC flow
- JWT Bearer tokens
- Automatic user mapping from Keycloak sub

### Core Endpoints
- `GET /api/v1/users/me` - Current user profile
- `POST /api/v1/races` - Create new race
- `POST /api/v1/races/{id}/invite` - Invite participants
- `GET /api/v1/races/{id}/live` - Live race status
- `GET /api/v1/feed` - Personalized social feed

### Real-Time Events (SignalR)
- `/hubs/race` - Race-specific updates
- `/hubs/chat` - Chat messages
- `/hubs/notifications` - Push notifications

## Development Guidelines

### Backend Structure
```
backend/
├── src/
│   ├── StreetRacer.Api/          # Controllers & SignalR Hubs
│   ├── StreetRacer.Application/  # Services & DTOs
│   ├── StreetRacer.Domain/       # Entities & Value Objects
│   ├── StreetRacer.Infrastructure/ # EF Core & External Services
│   └── StreetRacer.Workers/      # Background Services
└── tests/                        # Unit & Integration Tests
```

### Frontend Structure
```
frontend/
├── app/                  # Next.js 14 App Router
├── components/           # Reusable UI Components
├── hooks/                # Custom React Hooks
├── lib/                  # Utilities & API Client
└── types/                # TypeScript Definitions
```

## Deployment

### Production Docker Build
```bash
docker build -t streetracer-backend ./backend
docker build -t streetracer-frontend ./frontend
```

### Environment Variables
See `.env.example` files in backend and frontend directories.

## Testing

### Backend Tests
```bash
cd backend
dotnet test
```

### Frontend Tests
```bash
cd frontend
npm test
```

### E2E Tests
```bash
npm run test:e2e
```

## Monitoring & Observability

- **Metrics**: Prometheus + Grafana
- **Logging**: Structured JSON to Elasticsearch
- **Tracing**: OpenTelemetry with correlation IDs
- **Health Checks**: Built-in ASP.NET Core health checks

## Contributing

1. Fork the repository
2. Create feature branch
3. Follow coding standards (EditorConfig)
4. Add tests for new features
5. Submit pull request

## License

MIT License - see LICENSE file for details.