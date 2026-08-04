# Tasting Admin Application - Development Guide

This guide covers setup, development, and deployment of the Tasting Admin application.

## Project Structure

```
Tasting/
├── src/
│   ├── Backend/
│   │   └── Tasting.Api/              # .NET 8 API backend
│   ├── Frontend/
│   │   └── Tasting.Admin/            # Blazor Server frontend
│   └── Shared/
│       └── SharedLibrary/            # Shared code (DTOs, interfaces, utilities)
├── docs/
│   ├── IMPLEMENTATION-PLAN-STATUS.md # Current implementation status
│   ├── BACKEND-API-REFERENCE.md      # API documentation
│   ├── BACKEND-AUTH-TESTING.md       # Testing guide with curl examples
│   ├── INTEGRATION-CHECKLIST.md      # End-to-end integration tests
│   ├── FRONTEND-PARALLEL-PLAN.md     # Frontend development strategy
│   └── TRACK-COORDINATION.md         # Cross-track coordination guide
└── README.md                          # This file
```

## Tech Stack

**Backend:**
- .NET 8 with FastEndpoints
- Entity Framework Core with PostgreSQL
- JWT Authentication with BCrypt password hashing
- FluentMigrator for database migrations
- MediatR for CQRS pattern

**Frontend:**
- Blazor Server with .NET 8
- MudBlazor UI components
- Razor components for pages and shared components
- HttpClient for API communication

## Prerequisites

### Local Development
- **Backend:** .NET 8 SDK
- **Frontend:** Same .NET 8 SDK (Blazor Server)
- **Database:** PostgreSQL 13+
- **Tools:** git, Visual Studio Code or Visual Studio

### Docker (Optional)
- Docker Desktop with PostgreSQL container

## Getting Started

### 1. Database Setup

```bash
# Install PostgreSQL locally or via Docker
docker run --name postgres -e POSTGRES_PASSWORD=postgres -p 5432:5432 -d postgres:15

# Create database
createdb tasting_dev

# Configure connection string
# Edit: src/Backend/Tasting.Api/appsettings.Development.json
```

**appsettings.Development.json:**
```json
{
  "ConnectionStrings": {
    "TastingDb": "Host=localhost;Port=5432;Database=tasting_dev;Username=postgres;Password=postgres"
  }
}
```

### 2. Backend Setup

```bash
cd src/Backend/Tasting.Api

# Restore dependencies
dotnet restore

# Build
dotnet build

# Run (migrations apply automatically on startup)
dotnet run

# Backend available at: http://localhost:5000
# API docs: http://localhost:5000/scalar/v1
```

### 3. Frontend Setup

```bash
cd src/Frontend/Tasting.Admin

# Restore dependencies
dotnet restore

# Build
dotnet build

# Run
dotnet run

# Frontend available at: http://localhost:5002 (or next available port)
```

### 4. Test Login

**Default Admin User:**
- Email: `admin@example.com`
- Password: `admin@123`

(Set in `src/Backend/Tasting.Api/Infrastructure/Migrations/SeedData.cs`)

Navigate to `http://localhost:5002` and login.

## Development Workflow

### Adding a New Feature (Vertical Slice)

1. **Create feature folder** under `Features/{Domain}/{FeatureName}/`
2. **Implement vertical slice:**
   - `{FeatureName}Command.cs` - MediatR command
   - `{FeatureName}Handler.cs` - Command handler
   - `{FeatureName}Request.cs` - API request model
   - `{FeatureName}Response.cs` - API response model
   - `{FeatureName}Endpoint.cs` - FastEndpoints endpoint
   - `{FeatureName}Mapper.cs` - Request/response mapping
3. **Register in DI container** in appropriate extension class
4. **Create database migration** if needed
5. **Write tests** in `Tests/` folder
6. **Document in API reference**

### Running Tests

```bash
# Backend tests
cd src/Backend/Tasting.Api
dotnet test

# Frontend tests (bUnit)
cd src/Frontend/Tasting.Admin
dotnet test
```

### Database Migrations

```bash
# Migrations apply automatically on application startup
# To manually review migrations:
cd src/Backend/Tasting.Api
ls Infrastructure/Migrations/

# To add new migration:
# 1. Create migration class in Infrastructure/Migrations/
# 2. Application auto-runs Up() on startup
# 3. Down() used for rollback
```

## Parallel Development Tracks

The frontend is organized in 6 parallel development tracks to avoid blocking dependencies:

### Track A: Shell & Auth ✅
**Branch:** `feature/admin-shell-auth`  
**Status:** Complete  
- Login page and authentication
- Main app shell with navigation
- Route guards for protected pages

### Track B: Shared UI Components 🔄
**Branch:** `feature/admin-shared-ui`  
**Status:** In Progress  
- Reusable MudBlazor components
- SearchBar, DataTable, FormField, etc.
- Other tracks depend on this

### Track C: Users Management 🔄
**Branch:** `feature/admin-users-slice`  
**Status:** In Progress  
- User listing with search
- Create/edit/delete users
- Role and status management

### Track D: Breweries & Beers 🔄
**Branch:** `feature/admin-breweries-slice`  
**Status:** In Progress  
- Brewery management
- Beer catalog per brewery
- Create/edit/deactivate

### Track E: Arrangements 🔄
**Branch:** `feature/admin-arrangements-slice`  
**Status:** In Progress  
- Complex arrangement management
- Multi-select participants and beers
- Status machine (Created → Started → Completed)

### Track F: Testing Infrastructure 🔄
**Branch:** `feature/admin-frontend-tests`  
**Status:** In Progress  
- bUnit test suite
- Test data builders
- API contract verification
- CI/CD integration

**Coordination:** See `docs/TRACK-COORDINATION.md`

## API Endpoints

All endpoints prefixed with `/api/v1`

### Authentication
- `POST /users/login` - Get JWT token

### Users
- `GET /users` - List users
- `GET /users/{id}` - Get user
- `POST /users` - Create user
- `PUT /users/{id}` - Update user
- `PATCH /users/{id}/role` - Change role
- `PATCH /users/{id}/deactivate` - Toggle active status

### Breweries & Beers
- `GET /breweries`, `POST /breweries` - Brewery CRUD
- `GET /beers`, `POST /beers` - Beer CRUD
- Search and filtering available via query parameters

### Arrangements
- `GET /arrangements`, `POST /arrangements` - Arrangement CRUD
- `POST /arrangements/{id}/participants` - Add participant
- `POST /arrangements/{id}/beers` - Add beer
- Status transitions: Created → Started → Completed/Canceled

**Full API Reference:** `docs/BACKEND-API-REFERENCE.md`

## Testing

### Manual API Testing

```bash
# See docs/BACKEND-AUTH-TESTING.md for detailed examples
curl -X POST http://localhost:5000/api/v1/users/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@example.com","password":"admin@123"}'
```

### Integration Testing

After all frontend tracks complete:

1. Run `docs/INTEGRATION-CHECKLIST.md`
2. Test all CRUD operations end-to-end
3. Verify error handling
4. Check performance under load

## Debugging

### Backend
- Logs output to console
- Debug with VS Code or Visual Studio
- Use Scalar API docs at `/scalar/v1` to test endpoints

### Frontend
- Browser DevTools (F12)
- Check Network tab for API calls
- Check Application tab for stored JWT token
- Console for JavaScript errors

## Environment Configuration

### appsettings.json

**Backend settings:**

```json
{
  "Jwt": {
    "SecretKey": "your-secret-key-min-32-chars",
    "Issuer": "TastingApi",
    "Audience": "TastingAdmin",
    "ExpirationMinutes": 480
  }
}
```

**Important:** Change `SecretKey` to a secure random string in production (min 32 characters).

## Deployment

### Docker Build

```bash
# Build backend Docker image
docker build -f src/Backend/Tasting.Api/Dockerfile -t tasting-api:latest .

# Run with PostgreSQL
docker-compose up
```

### Azure Deployment

Recommended: Use Azure App Service + Azure Database for PostgreSQL

1. Create resource group
2. Create App Service (.NET 8)
3. Create PostgreSQL database
4. Configure connection string in App Settings
5. Deploy via git push or GitHub Actions

### CI/CD

GitHub Actions workflows (if implemented):
- Run tests on PR
- Build on merge to main
- Deploy to staging/production

## Security Considerations

### Development
- JWT secret is placeholder (change for production!)
- CORS allows any origin (tighten for production)
- AllowAnonymous on /login endpoint only
- Password hashing with BCrypt (never plaintext)

### Production Checklist
- [ ] Use HTTPS only
- [ ] Secure JWT secret (32+ random characters)
- [ ] Restrict CORS to specific frontend domain
- [ ] Enable HTTPS redirect
- [ ] Configure firewall rules
- [ ] Regular security audits
- [ ] Dependency updates (keep NuGet packages current)
- [ ] Database backups enabled
- [ ] Monitoring & alerting configured
- [ ] Rate limiting implemented
- [ ] Password policies enforced (min complexity)

## Troubleshooting

### Backend won't start
1. Check PostgreSQL connection string
2. Verify database exists
3. Check .NET SDK version: `dotnet --version` (needs 8.x)
4. Run: `dotnet restore` && `dotnet clean`

### Frontend can't connect to backend
1. Verify backend running on correct port (5000)
2. Check CORS headers in Network tab
3. Check JWT token in Application → Session Storage
4. Verify Authorization header sent: `Authorization: Bearer <token>`

### Database migration issues
1. Check connection string in appsettings.Development.json
2. Verify PostgreSQL running: `psql -U postgres`
3. Check migration folder: `src/Backend/Tasting.Api/Infrastructure/Migrations/`

### Login fails
1. Verify user exists in database
2. Check password hash (should not be plaintext)
3. Verify role is Admin (non-admin users get 403)
4. Check JWT configuration in appsettings.json

## Resources

**Documentation:**
- `docs/BACKEND-API-REFERENCE.md` - Full API docs
- `docs/BACKEND-AUTH-TESTING.md` - Testing examples
- `docs/INTEGRATION-CHECKLIST.md` - Integration test guide
- `docs/FRONTEND-PARALLEL-PLAN.md` - Frontend strategy
- `docs/IMPLEMENTATION-PLAN-STATUS.md` - Current progress

**External:**
- [FastEndpoints](https://fast-endpoints.com/)
- [MudBlazor](https://mudblazor.com/)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [Blazor Documentation](https://learn.microsoft.com/en-us/aspnet/core/blazor/)

## Contributing

1. Create feature branch from `main`
2. Follow vertical slice architecture
3. Add tests for new features
4. Run full test suite before PR
5. Update documentation if needed
6. Link to related issues
7. Get code review approval
8. Merge to main

## Questions?

Check the documentation in `docs/` folder or review the code in the corresponding feature folder.

---

**Last Updated:** 2026-08-04  
**Status:** Active Development - Parallel Frontend Tracks in Progress
