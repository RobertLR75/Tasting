# Backend Authentication Testing Guide

## Overview
This document provides instructions for testing the JWT authentication system implemented in the backend.

## Prerequisites
- PostgreSQL database running locally
- .NET 8 SDK installed
- Connection string configured in `appsettings.Development.json`

## Database Setup

1. **Configure Connection String**
   ```json
   // appsettings.Development.json
   {
     "ConnectionStrings": {
       "TastingDb": "Host=localhost;Port=5432;Database=tasting_dev;Username=postgres;Password=your_password"
     }
   }
   ```

2. **Run Database Migrations**
   ```bash
   cd src/Backend/Tasting.Api
   dotnet run
   ```
   This will auto-run migrations on startup.

3. **Seed Test Users** (Manual for now)
   ```bash
   # Use the SeedData.cs class to create test users
   # Future: Add a migration or API endpoint to seed data
   ```

## Test Credentials

After seeding, use these credentials:

| Email | Password | Role | Access |
|-------|----------|------|--------|
| admin@example.com | admin@123 | Admin | ✅ Can login |
| user@example.com | user@123 | User | ❌ Blocked (non-admin) |
| inactive@example.com | inactive@123 | User | ❌ Inactive user |

## Testing Login Endpoint

### 1. Successful Login (Admin)
```bash
curl -X POST http://localhost:5000/api/v1/users/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@example.com",
    "password": "admin@123"
  }'
```

**Expected Response (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "email": "admin@example.com",
  "firstName": "Admin",
  "lastName": "User",
  "role": "Admin"
}
```

### 2. Non-Admin User (403 Forbidden)
```bash
curl -X POST http://localhost:5000/api/v1/users/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "user@123"
  }'
```

**Expected Response (403 Forbidden):**
```
Only administrators can access this application
```

### 3. Wrong Password (401 Unauthorized)
```bash
curl -X POST http://localhost:5000/api/v1/users/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@example.com",
    "password": "wrong_password"
  }'
```

**Expected Response (401 Unauthorized):**
```
Invalid email or password
```

### 4. Inactive User (401 Unauthorized)
```bash
curl -X POST http://localhost:5000/api/v1/users/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "inactive@example.com",
    "password": "inactive@123"
  }'
```

**Expected Response (401 Unauthorized):**
```
Invalid email or password
```

## JWT Token Structure

The generated JWT contains these claims:

```json
{
  "nameidentifier": "550e8400-e29b-41d4-a716-446655440000",
  "email": "admin@example.com",
  "givenname": "Admin",
  "surname": "User",
  "role": "Admin",
  "exp": 1234567890,
  "iss": "TastingApi",
  "aud": "TastingAdmin"
}
```

## Using Token in Protected Endpoints

Once you have a token, use it for protected endpoints:

```bash
curl -X GET http://localhost:5000/api/v1/users \
  -H "Authorization: Bearer YOUR_JWT_TOKEN_HERE"
```

## Configuration

JWT settings are in `appsettings.json`:

```json
{
  "Jwt": {
    "SecretKey": "your-secret-key-change-in-production-at-least-32-characters",
    "Issuer": "TastingApi",
    "Audience": "TastingAdmin",
    "ExpirationMinutes": 480
  }
}
```

**Important:** 
- Change `SecretKey` to a secure random string (min 32 chars) in production
- Keep secret key safe - never commit to version control
- Use environment variables in production

## Troubleshooting

| Issue | Solution |
|-------|----------|
| "Invalid email or password" for correct credentials | Check password was hashed during user creation |
| No 200 response on login | Verify user exists and is active |
| "Only administrators can access" | User's role must be Admin in database |
| JWT validation fails in protected endpoints | Check token expiration and secret key match |

## Frontend Integration

The frontend `AuthService` is configured to:
1. POST to `/api/v1/users/login` with email/password
2. Extract token from `LoginResponse.Token`
3. Store in `SecureStorage` (session storage)
4. Add token to `Authorization: Bearer` header for subsequent requests

See `src/Frontend/Tasting.Admin/Features/Auth/Services/AuthService.cs` for implementation.

## Next Steps

- [ ] Implement token refresh endpoint
- [ ] Add password reset flow
- [ ] Set up CORS for frontend
- [ ] Implement JWT token storage in browser
- [ ] Add logout endpoint
- [ ] Add rate limiting for login attempts
- [ ] Implement 2FA (optional)
