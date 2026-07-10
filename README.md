# Dotnet Authentication System

A modern, full-stack authentication system built with **ASP.NET Core 10** and **Angular 21**, featuring JWT-based authentication, Redis caching, PostgreSQL persistence, rate limiting, and comprehensive health checks.

## Project Overview

This project demonstrates a production-ready authentication system designed to showcase enterprise-level software engineering practices. It implements secure user authentication, token management, and API rate limiting with a responsive Angular frontend.

## Architectural Decisions

### Secure Token Storage: HttpOnly Cookies vs localStorage

JWT tokens are stored exclusively in HttpOnly, Secure, SameSite cookies rather than localStorage to prevent Cross-Site Scripting (XSS) vulnerabilities. This architectural choice provides:

- **XSS Protection**: HttpOnly flag prevents JavaScript access to tokens, eliminating a major attack vector where compromised script could steal tokens from localStorage.
- **CSRF Prevention**: SameSite=Strict flag ensures cookies are not sent in cross-site requests, mitigating Cross-Site Request Forgery attacks.
- **Automatic Transmission**: Cookies are automatically included in requests to the same origin, eliminating the need for manual header injection that could expose tokens in code.
- **Secure Flag**: Ensures tokens are only transmitted over HTTPS, preventing man-in-the-middle interception.

This approach trades slight frontend complexity for significantly improved security posture compared to localStorage token storage.

### Distributed Rate Limiting: Token Bucket Algorithm with Redis and Lua

Rate limiting is implemented using the token bucket algorithm backed by Redis with Lua scripting instead of built-in ASP.NET Core rate limiting:

**Token Bucket vs. Fixed-Window and Sliding-Window Algorithms:**

- **Fixed-Window Problem**: Rate resets at window boundaries, allowing double the intended rate when requests span two windows. Example: 100 req/min limit resets at :00 and :60, allowing 200 requests between :59-:01 of adjacent windows.
- **Sliding-Window Problem**: Computationally expensive and still exhibits rate spike at boundaries due to window sliding mechanics.
- **Token Bucket Solution**: Continuously replenishes tokens at a steady rate (e.g., 100 tokens per minute). Requests consume tokens; when tokens are depleted, requests are rejected. Provides smooth rate enforcement without boundary-based spikes.

**Redis with Lua Scripting vs. In-Process Rate Limiting:**

Native ASP.NET Core rate limiting (middleware-based or in-process) is insufficient for distributed systems because:

- **Per-Instance Limits**: Each server instance tracks its own rate limit state independently. Three instances with 100 req/min limits = 300 req/min system limit, allowing clients to bypass limits by distributing requests across instances.
- **Inconsistent State**: Rate limit decisions depend on server affinity; load balancers without sticky sessions create unpredictable behavior.
- **No Coordination**: In-process counters cannot coordinate across service replicas.

Redis solves this by:

- **Centralized State**: Single source of truth for rate limit counters across all service instances ensures consistent enforcement regardless of load balancing.
- **Atomic Operations with Lua**: Lua scripts execute atomically on the Redis server, preventing race conditions between distributed instances checking and updating bucket state simultaneously. The EVAL command guarantees the entire token bucket check-and-decrement operation completes without interruption, even under concurrent load.
- **Sub-Millisecond Performance**: Redis in-memory operations provide rate limit decisions in microseconds, negligible overhead for API requests.
- **Horizontal Scalability**: Adding service instances does not increase rate limit capacity; the global Redis instance enforces limits consistently.

The Lua script atomicity is critical: without it, multiple requests could simultaneously read an insufficient token count but still proceed to decrement, violating the rate limit contract.

### Structured Logging with Serilog

Serilog provides context-enriched, structured logging output instead of unstructured string concatenation:

- **Contextual Enrichment**: Automatically includes request IDs, user IDs, and correlation IDs across distributed operations.
- **Performance**: Context-aware formatting is compiled at startup; at runtime, property extraction and serialization are highly optimized.
- **Pretty Console Output**: Development logs are human-readable with colors and structured fields; production logs are JSON-serialized for aggregation systems (ELK, Splunk).
- **Filtering**: Granular control over log levels per component; noisy Microsoft.AspNetCore logs suppressed while business logic is logged verbosely.
- **Sink Flexibility**: Seamless switching between console, file, and cloud logging backends without code changes.

## Technology Stack

### Backend (ASP.NET Core 10)

- **Secure JWT Authentication**: Cookies-only JWT storage prevents XSS vulnerabilities and cross-origin token theft compared to localStorage. Tokens are transmitted securely with HttpOnly, Secure, and SameSite flags.
- **Refresh Token System**: Long-lived refresh tokens enable seamless user sessions with automatic access token rotation, improving both security and user experience.
- **Password Security**: Industry-standard bcrypt password hashing via ASP.NET Core Identity ensures strong protection against brute-force attacks and rainbow table exploits.
- **Advanced Rate Limiting**: Token bucket algorithm implemented in Redis with Lua scripting provides superior rate limiting compared to fixed-window or sliding-window approaches. Eliminates the rate spike boundary issue found in sliding-window implementations where double the rate is briefly allowed. Lua atomicity ensures distributed consistency without race conditions across multiple service instances.
- **Health Checks**: Comprehensive endpoint monitoring for database and cache enables proactive system health verification and load balancer integration.
- **Structured Logging**: Serilog integration provides context-enriched, performance-efficient logging with structured output for better debugging and production monitoring.
- **CORS Support**: Granular cross-origin resource sharing configuration for secure frontend communication.
- **API Documentation**: OpenAPI (Swagger) support enables API exploration and integration testing.

### Frontend (Angular 21)

- **Reactive Forms**: Declarative form validation and state management with RxJS observables.
- **Route Protection**: Auth guards and guest guards enforce authorization at the routing level before component instantiation.
- **HTTP Interceptors**: Automatic JWT cookie inclusion in requests and token refresh handling without manual header management.
- **Authentication Service**: Centralized auth state management with RxJS subjects for reactive component updates.
- **PrimeNG UI Components**: Enterprise-grade, accessible UI component library with built-in keyboard navigation.
- **Tailwind CSS**: Utility-first styling framework for rapid, maintainable responsive design.
- **Server-Side Rendering**: Angular SSR improves Time to First Contentful Paint and SEO without sacrificing interactivity.
- **Responsive Design**: Mobile-first approach with adaptive layouts for all device sizes.

## Technology Stack

### Backend

- **Runtime**: .NET 10.0
- **Framework**: ASP.NET Core Web API
- **Authentication**: JWT Bearer, System.IdentityModel.Tokens.Jwt
- **Database**: PostgreSQL 16 with Entity Framework Core
- **Caching**: Redis (StackExchange.Redis)
- **Logging**: Serilog
- **Security**: ASP.NET Core Identity (PasswordHasher)
- **Rate Limiting**: Custom implementation with Lua scripting

### Frontend

- **Framework**: Angular 21.2.0
- **Language**: TypeScript 5.9.2
- **Package Manager**: npm 10.8.2
- **UI Library**: PrimeNG 21.1.5
- **CSS Framework**: Tailwind CSS 4.1.12
- **Styling**: PostCSS
- **Testing**: Vitest
- **Code Quality**: Prettier

### Infrastructure (Supporting Containers)

- **Container Orchestration**: Docker Compose
- **Database**: PostgreSQL 16 Alpine (containerized)
- **Cache**: Redis 7 Alpine (containerized)
- **Monitoring**: RedisInsight (containerized)

## Project Structure

```
dotnet-auth-system/
├── auth-service/                 # ASP.NET Core Backend
│   ├── Modules/
│   │   ├── Auth/                # Authentication module
│   │   │   ├── AuthController.cs
│   │   │   ├── AuthService.cs
│   │   │   ├── JwtService.cs
│   │   │   ├── RefreshTokenService.cs
│   │   │   ├── User.cs
│   │   │   ├── RefreshToken.cs
│   │   │   └── AuthDTO.cs
│   │   ├── RateLimiter/         # Rate limiting module
│   │   │   ├── RateLimiterService.cs
│   │   │   ├── RateLimiterMiddleware.cs
│   │   │   └── token_bucket.lua
│   │   └── Book/                # Sample module for protected endpoints
│   ├── Infrastructures/
│   │   └── StartupChecks/       # Health checks on startup
│   ├── Data/
│   │   └── AppDbContext.cs      # Entity Framework Core context
│   ├── Options/                 # Configuration DTOs
│   ├── Migrations/              # Database migrations
│   ├── Program.cs               # Application startup
│   ├── auth-service.csproj      # Project file
│   └── appsettings.json         # Configuration
├── auth-frontend/               # Angular Frontend
│   ├── src/
│   │   ├── app/
│   │   │   ├── features/
│   │   │   │   ├── auth/
│   │   │   │   │   ├── auth.service.ts
│   │   │   │   │   ├── auth.guard.ts
│   │   │   │   │   ├── guest.guard.ts
│   │   │   │   │   ├── auth.interceptor.ts
│   │   │   │   │   ├── pages/
│   │   │   │   │   │   ├── login/
│   │   │   │   │   │   └── register/
│   │   │   │   │   └── auth.routes.ts
│   │   │   │   └── dashboard/
│   │   │   ├── app.ts           # Root component
│   │   │   ├── app.routes.ts    # Application routes
│   │   │   └── app.config.ts    # Application config
│   │   ├── environments/        # Environment configs
│   │   ├── main.ts              # Bootstrap
│   │   └── index.html
│   ├── angular.json             # Angular CLI config
│   ├── tailwind.config.js       # Tailwind configuration
│   └── package.json
├── docker-compose.yml           # Infrastructure setup
└── .gitignore
```

## Authentication Flow

1. User Registration: Email/password registration with validation
2. Password Hashing: Bcrypt hashing via ASP.NET Core Identity
3. Login: Credentials verified against hashed password
4. JWT Generation: Access token issued with user claims (sub, username, role)
5. Refresh Token: Long-lived refresh token stored in PostgreSQL
6. Token Refresh: Use refresh token to obtain new access token
7. Route Protection: Angular guards prevent unauthorized access
8. API Requests: HTTP interceptor automatically attaches JWT to requests

## Rate Limiting

The system implements token bucket rate limiting using Redis and Lua scripting:

- Configurable per-endpoint limits
- Redis-backed distributed rate limiting
- Middleware integration for transparent enforcement
- Prevents API abuse and DDoS attacks

## Health Checks

Monitor system health via `/healthz` endpoint:

```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.123",
  "checks": [
    {
      "name": "AppDbContext",
      "status": "Healthy",
      "duration": "00:00:00.050"
    },
    {
      "name": "Redis",
      "status": "Healthy",
      "duration": "00:00:00.010"
    }
  ]
}
```

## Getting Started

### Prerequisites

- .NET 10 SDK
- Node.js 20+
- npm 10.8.2+
- Docker & Docker Compose (for PostgreSQL, Redis, and RedisInsight containers)

### Quick Start

```bash
# Clone the repository
git clone https://github.com/relmoo1220/dotnet-auth-system.git
cd dotnet-auth-system

# Start supporting containers (PostgreSQL, Redis, RedisInsight)
docker-compose up -d

# Backend runs locally
cd auth-service
dotnet restore
dotnet ef database update
dotnet run

# Frontend runs locally (in another terminal)
cd auth-frontend
npm install
npm start

# Services will be available at:
# Backend: http://localhost:5000
# Frontend: http://localhost:4200
# RedisInsight: http://localhost:5540
```

### Local Development Setup

#### Backend Setup

```bash
cd auth-service

# Restore dependencies
dotnet restore

# Apply database migrations
dotnet ef database update

# Run the service
dotnet run
```

### Backend (auth-service)

Create `appsettings.Development.json`:

```json
{
  "Database": {
    "Postgres": "Host=localhost;Port=5432;Database=mydatabase;Username=myuser;Password=mypassword"
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  },
  "Jwt": {
    "Key": "your-secret-key-at-least-32-characters-long",
    "Issuer": "auth-service",
    "Audience": "auth-service-users",
    "ExpiryMinutes": 60
  }
}
```

### Frontend (auth-frontend)

Create `src/environments/environment.ts`:

```typescript
export const environment = {
  production: false,
  apiUrl: "http://localhost:5000",
};
```

## API Endpoints

### Authentication

- `POST /api/auth/register` - User registration
- `POST /api/auth/login` - User login
- `POST /api/auth/refresh` - Refresh access token
- `POST /api/auth/logout` - User logout

### Protected Routes

- `GET /api/books` - Get all books (requires authentication)

### System

- `GET /healthz` - Health check endpoint

## Testing

### Backend

```bash
cd auth-service
dotnet test
```

### Frontend

```bash
cd auth-frontend
npm test
```

## Database Schema

### Users Table

- `Id` (Guid, PK)
- `Username` (string, unique)
- `Email` (string, unique)
- `PasswordHash` (string)
- `Role` (string, default: "user")
- `CreatedAt` (DateTime)

### RefreshTokens Table

- `Id` (Guid, PK)
- `UserId` (Guid, FK)
- `Token` (string, unique)
- `ExpiresAt` (DateTime)
- `IsRevoked` (bool)
- `CreatedAt` (DateTime)

## Security Features

- Password hashing with bcrypt
- JWT token validation
- CORS policy enforcement
- Rate limiting protection
- Refresh token rotation capability
- Secure token storage
- HTTPS enforcement (in production)
- Input validation on all endpoints
- Structured logging for audit trails

## Skills Demonstrated

### Backend Development

- Enterprise-level ASP.NET Core architecture
- JWT authentication and authorization
- Entity Framework Core with migrations
- PostgreSQL database design
- Redis integration and distributed caching
- Middleware and dependency injection
- Health checks and monitoring
- Configuration management
- Structured logging with Serilog

### Frontend Development

- Modern Angular architecture with standalone components
- Reactive programming with RxJS
- Route guards and interceptors
- Form validation and error handling
- State management patterns
- Responsive UI design
- TypeScript advanced features
- Component composition and reusability

### DevOps & Infrastructure

- Docker containerization
- Docker Compose orchestration
- Multi-service coordination
- Health checks and readiness probes
- Environment configuration management
- Database migrations
- Version control best practices

### Software Engineering Practices

- Clean code architecture
- Separation of concerns (SOLID principles)
- DRY (Don't Repeat Yourself)
- RESTful API design
- Error handling and validation
- Comprehensive logging
- Modular project structure
- Production-ready configuration

## Performance Considerations

- Redis caching for rate limiting and session data
- Connection pooling for database and Redis
- Health checks for infrastructure monitoring
- Efficient JWT validation
- Structured logging without performance overhead
- Angular SSR for faster initial page load

## Contributing

This is a portfolio project showcasing full-stack authentication capabilities.

