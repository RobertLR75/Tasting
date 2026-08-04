# Backend API Reference

**Environment:** Development  
**Base URL:** `http://localhost:5000/api/v1`  
**Documentation:** Available at `http://localhost:5000/scalar/v1`  
**Authentication:** JWT Bearer Token in `Authorization` header

---

## Quick Start

### 1. Login
```bash
curl -X POST http://localhost:5000/api/v1/users/login \
  -H "Content-Type: application/json" \
  -d '{"email": "admin@example.com", "password": "admin@123"}'
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "email": "admin@example.com",
  "firstName": "Admin",
  "lastName": "User",
  "role": "Admin"
}
```

### 2. Use Token
```bash
curl -X GET http://localhost:5000/api/v1/users \
  -H "Authorization: Bearer <TOKEN>"
```

---

## Authentication

### Login
**POST** `/users/login`

**Access:** Public (AllowAnonymous)

**Request Body:**
```json
{
  "email": "admin@example.com",
  "password": "admin@123"
}
```

**Response:** 200 OK
```json
{
  "token": "string (JWT)",
  "email": "string",
  "firstName": "string",
  "lastName": "string",
  "role": "Admin | User"
}
```

**Errors:**
- 401: Invalid email or password
- 403: Only administrators can access

---

## Users

### List Users
**GET** `/users`

**Access:** Admin only

**Query Parameters:**
- `search` (string, optional) - Filter by name or email
- `skip` (integer, optional, default: 0) - Pagination offset
- `take` (integer, optional, default: 20) - Pagination limit

**Response:** 200 OK
```json
{
  "users": [
    {
      "id": "uuid",
      "email": "user@example.com",
      "emailNormalized": "user@example.com",
      "firstName": "John",
      "lastName": "Doe",
      "isActive": true,
      "role": "User",
      "createdAt": "2026-08-04T12:00:00Z",
      "updatedAt": null
    }
  ]
}
```

### Get User
**GET** `/users/{id}`

**Access:** Admin only

**Path Parameters:**
- `id` (guid) - User ID

**Response:** 200 OK
```json
{
  "id": "uuid",
  "email": "user@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "isActive": true,
  "role": "User",
  "createdAt": "2026-08-04T12:00:00Z",
  "updatedAt": null
}
```

**Errors:**
- 404: User not found

### Create User
**POST** `/users`

**Access:** Admin only

**Request Body:**
```json
{
  "email": "newuser@example.com",
  "firstName": "Jane",
  "lastName": "Smith",
  "password": "SecurePassword123!",
  "role": "User"
}
```

**Response:** 201 Created
```json
{
  "id": "uuid",
  "email": "newuser@example.com",
  "firstName": "Jane",
  "lastName": "Smith",
  "isActive": true,
  "role": "User",
  "createdAt": "2026-08-04T12:00:00Z",
  "updatedAt": null
}
```

**Errors:**
- 400: Validation error (email format, password strength)
- 409: Email already exists

### Update User
**PUT** `/users/{id}`

**Access:** Admin only

**Path Parameters:**
- `id` (guid) - User ID

**Request Body:**
```json
{
  "email": "updated@example.com",
  "firstName": "Jane",
  "lastName": "Smith",
  "password": "NewPassword123!"
}
```

**Response:** 200 OK
```json
{
  "id": "uuid",
  "email": "updated@example.com",
  "firstName": "Jane",
  "lastName": "Smith",
  "isActive": true,
  "role": "User",
  "updatedAt": "2026-08-04T13:00:00Z"
}
```

**Errors:**
- 400: Validation error
- 404: User not found
- 409: Email conflict

### Change User Role
**PATCH** `/users/{id}/role`

**Access:** Admin only

**Path Parameters:**
- `id` (guid) - User ID

**Request Body:**
```json
{
  "role": "Admin"
}
```

**Response:** 200 OK

**Errors:**
- 400: Invalid role
- 403: Cannot change own role / Cannot demote self
- 404: User not found

### Deactivate User
**PATCH** `/users/{id}/deactivate`

**Access:** Admin only

**Path Parameters:**
- `id` (guid) - User ID

**Request Body:**
```json
{
  "isActive": false
}
```

**Response:** 200 OK
```json
{
  "id": "uuid",
  "email": "user@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "isActive": false,
  "role": "User",
  "updatedAt": "2026-08-04T13:00:00Z"
}
```

---

## Breweries

### List Breweries
**GET** `/breweries`

**Access:** Admin only

**Query Parameters:**
- `search` (string, optional) - Filter by name
- `skip` (integer, optional)
- `take` (integer, optional)

**Response:** 200 OK
```json
{
  "breweries": [
    {
      "id": "uuid",
      "name": "Local Brewery",
      "country": "Norway",
      "region": "Eastern",
      "description": "Craft brewery description",
      "isActive": true,
      "createdAt": "2026-08-04T12:00:00Z",
      "updatedAt": null
    }
  ]
}
```

### Get Brewery
**GET** `/breweries/{id}`

**Response:** 200 OK

### Create Brewery
**POST** `/breweries`

**Access:** Admin only

**Request Body:**
```json
{
  "name": "New Brewery",
  "country": "Norway",
  "region": "Western",
  "description": "A new craft brewery"
}
```

**Response:** 201 Created

### Update Brewery
**PUT** `/breweries/{id}`

**Access:** Admin only

**Response:** 200 OK

### Deactivate Brewery
**PATCH** `/breweries/{id}/deactivate`

**Access:** Admin only

---

## Beers

### List Beers
**GET** `/beers`

**Access:** Admin only

**Query Parameters:**
- `breweryId` (guid, optional) - Filter by brewery
- `search` (string, optional) - Filter by name or style
- `skip` (integer, optional)
- `take` (integer, optional)

**Response:** 200 OK
```json
{
  "beers": [
    {
      "id": "uuid",
      "breweryId": "uuid",
      "name": "IPA",
      "style": "India Pale Ale",
      "type": "Ale",
      "abv": 6.5,
      "ibu": 60,
      "description": "Hoppy IPA with citrus notes",
      "isActive": true,
      "createdAt": "2026-08-04T12:00:00Z",
      "updatedAt": null
    }
  ]
}
```

### Get Beer
**GET** `/beers/{id}`

**Response:** 200 OK

### Create Beer
**POST** `/beers`

**Access:** Admin only

**Request Body:**
```json
{
  "breweryId": "uuid",
  "name": "Summer Lager",
  "style": "Lager",
  "type": "Lager",
  "abv": 4.8,
  "ibu": 20,
  "description": "Crisp and refreshing summer lager"
}
```

**Response:** 201 Created

### Update Beer
**PUT** `/beers/{id}`

**Access:** Admin only

**Response:** 200 OK

### Deactivate Beer
**PATCH** `/beers/{id}/deactivate`

**Access:** Admin only

---

## Arrangements

### List Arrangements
**GET** `/arrangements`

**Access:** Admin only

**Query Parameters:**
- `status` (string, optional) - Filter by status: Created, Started, Completed, Canceled
- `skip` (integer, optional)
- `take` (integer, optional)

**Response:** 200 OK
```json
{
  "arrangements": [
    {
      "id": "uuid",
      "name": "Summer Tasting 2026",
      "description": "Annual summer beer tasting event",
      "date": "2026-08-15T18:00:00Z",
      "location": "The Beer Hall",
      "status": "Created",
      "createdAt": "2026-08-04T12:00:00Z",
      "updatedAt": null
    }
  ]
}
```

### Get Arrangement
**GET** `/arrangements/{id}`

**Response:** 200 OK

### Create Arrangement
**POST** `/arrangements`

**Access:** Admin only

**Request Body:**
```json
{
  "name": "Fall Tasting",
  "description": "Fall seasonal beer tasting",
  "date": "2026-09-21T18:00:00Z",
  "location": "The Beer Hall"
}
```

**Response:** 201 Created

### Update Arrangement
**PUT** `/arrangements/{id}`

**Access:** Admin only, Status = Created

**Request Body:**
```json
{
  "name": "Updated Name",
  "description": "Updated description",
  "date": "2026-09-22T18:00:00Z",
  "location": "New Location"
}
```

**Response:** 200 OK

### Start Arrangement
**PATCH** `/arrangements/{id}` with status=Started or similar

**Access:** Admin only, Status = Created

**Response:** 200 OK
- Status transitions: Created → Started

### Complete Arrangement
**PATCH** `/arrangements/{id}` with status=Completed

**Access:** Admin only, Status = Started

**Response:** 200 OK
- Status transitions: Started → Completed

### Cancel Arrangement
**PATCH** `/arrangements/{id}` with status=Canceled

**Access:** Admin only, Status = Created or Started

**Response:** 200 OK

---

## Arrangement Participants

### Add Participant
**POST** `/arrangements/{id}/participants`

**Access:** Admin only

**Path Parameters:**
- `id` (guid) - Arrangement ID

**Request Body:**
```json
{
  "userId": "uuid"
}
```

**Response:** 201 Created

**Errors:**
- 400: User already a participant
- 404: Arrangement or User not found

### List Participants
**GET** `/arrangements/{id}/participants`

**Response:** 200 OK
```json
{
  "participants": [
    {
      "id": "uuid",
      "userId": "uuid",
      "firstName": "John",
      "lastName": "Doe",
      "email": "john@example.com"
    }
  ]
}
```

### Remove Participant
**DELETE** `/arrangements/{id}/participants/{userId}`

**Access:** Admin only

**Response:** 204 No Content

---

## Arrangement Beers

### Add Beer to Arrangement
**POST** `/arrangements/{id}/beers`

**Access:** Admin only

**Path Parameters:**
- `id` (guid) - Arrangement ID

**Request Body:**
```json
{
  "beerId": "uuid"
}
```

**Response:** 201 Created

**Errors:**
- 400: Beer already in arrangement
- 404: Arrangement or Beer not found

### List Beers in Arrangement
**GET** `/arrangements/{id}/beers`

**Response:** 200 OK
```json
{
  "beers": [
    {
      "id": "uuid",
      "name": "IPA",
      "breweryId": "uuid",
      "style": "India Pale Ale",
      "abv": 6.5,
      "ibu": 60
    }
  ]
}
```

### Remove Beer from Arrangement
**DELETE** `/arrangements/{id}/beers/{beerId}`

**Access:** Admin only

**Response:** 204 No Content

---

## Rating & Results

### Submit Rating
**POST** `/ratings`

**Access:** Admin only

**Request Body:**
```json
{
  "arrangementId": "uuid",
  "beerId": "uuid",
  "userId": "uuid",
  "score": 8.5,
  "notes": "Excellent hop profile"
}
```

**Response:** 201 Created

### Get Results
**GET** `/arrangements/{id}/results`

**Access:** Admin only

**Response:** 200 OK
```json
{
  "results": [
    {
      "beerId": "uuid",
      "beerName": "IPA",
      "averageScore": 7.8,
      "numberOfRatings": 12,
      "comments": ["Great", "Excellent hops"]
    }
  ]
}
```

---

## Common Response Codes

| Code | Meaning | Common Causes |
|------|---------|---------------|
| 200 | OK | Request successful |
| 201 | Created | Resource created successfully |
| 204 | No Content | Request successful, no response body |
| 400 | Bad Request | Invalid input, validation error |
| 401 | Unauthorized | Missing or invalid token |
| 403 | Forbidden | Insufficient permissions (non-admin) |
| 404 | Not Found | Resource doesn't exist |
| 409 | Conflict | Duplicate resource (e.g., email) |
| 500 | Internal Server Error | Backend error, check logs |

---

## Error Response Format

```json
{
  "title": "Validation Error",
  "status": 400,
  "detail": "Email format is invalid",
  "errors": {
    "email": ["Email format is invalid"]
  }
}
```

---

## JWT Token Structure

The JWT token returned from `/users/login` contains these claims:

```json
{
  "nameidentifier": "550e8400-e29b-41d4-a716-446655440000",
  "email": "admin@example.com",
  "givenname": "Admin",
  "surname": "User",
  "role": "Admin",
  "iat": 1691142000,
  "exp": 1691384400,
  "iss": "TastingApi",
  "aud": "TastingAdmin"
}
```

**Token expiration:** Configurable via `appsettings.json` (default: 480 minutes = 8 hours)

---

## CORS Headers

Responses include CORS headers for cross-origin requests:

```
Access-Control-Allow-Origin: *
Access-Control-Allow-Methods: GET, POST, PUT, PATCH, DELETE, OPTIONS
Access-Control-Allow-Headers: *
```

---

## Rate Limiting

Currently: Not implemented (add in production)

---

## Performance Considerations

- List endpoints return paginated results (default 20 per page)
- Search is case-insensitive
- Use `skip`/`take` parameters for large datasets
- Avoid N+1 queries (data is pre-loaded)

---

## Development Tools

- **OpenAPI/Swagger:** Not currently available
- **Scalar API Reference:** Available at `/scalar/v1`
- **Example Requests:** See `docs/BACKEND-AUTH-TESTING.md`

---

## Support & Troubleshooting

See `docs/BACKEND-AUTH-TESTING.md` for:
- Common error scenarios
- Debug strategies
- curl examples for manual testing

---

**Last Updated:** 2026-08-04  
**API Version:** v1
