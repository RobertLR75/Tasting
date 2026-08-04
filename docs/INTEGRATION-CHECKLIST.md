# Frontend-Backend Integration Checklist

**Purpose:** Comprehensive checklist for integrating the completed frontend tracks with the backend API  
**Status:** Pre-integration (Tracks B-F still in progress)  
**Target Date:** 2026-08-06 (after Tracks C-E complete)

---

## Pre-Integration Setup (Blocking Items)

### Backend Environment
- [ ] PostgreSQL installed and running locally
- [ ] Connection string configured in `appsettings.Development.json`
- [ ] Database migrations applied successfully
- [ ] Test user data seeded (use `SeedData.cs`)
- [ ] Backend API running on `http://localhost:5000` or configured port
- [ ] CORS enabled (already configured ✅)
- [ ] JWT secret key configured (production length recommended)

### Frontend Environment
- [ ] Node.js installed (for frontend build)
- [ ] Frontend build succeeds without errors
- [ ] Frontend running on different port (e.g., `http://localhost:3000`)
- [ ] All feature branches merged to `setup-parallel-frontend-dev`

### Network & Debugging
- [ ] Backend CORS headers properly configured for frontend origin
- [ ] Browser dev tools open to check network requests
- [ ] API documentation available at `/scalar/v1`
- [ ] JWT token validation working in browser console

---

## Authentication Flow (Track A + Backend)

### Login Endpoint (POST /api/v1/users/login)
- [ ] Backend login endpoint responds at correct URL
- [ ] Accepts email + password in request body
- [ ] Returns 200 with JWT token on success
- [ ] Returns 401 "Invalid email or password" for wrong credentials
- [ ] Returns 403 "Only administrators can access" for non-admin users
- [ ] Password hashing verified with BCrypt

### Frontend AuthService Integration
- [ ] LoginPage form submits to backend endpoint
- [ ] JWT token received and stored in sessionStorage
- [ ] Token appears in browser DevTools → Application → Session Storage
- [ ] AuthService reads token on page reload
- [ ] AuthGuard component redirects to /login if no token
- [ ] AuthGuard allows navigation if token present

### Token Usage in Protected Endpoints
- [ ] Authorization header sent: `Authorization: Bearer <token>`
- [ ] All protected endpoints accept valid token
- [ ] Protected endpoints reject request with no token (401)
- [ ] Protected endpoints reject expired token (401)
- [ ] Token claims (email, role, etc) available in AuthService

### Logout Flow
- [ ] Logout button clears token from session storage
- [ ] Logout redirects to login page
- [ ] Subsequent requests to protected endpoints return 401

---

## Track C: Users Slice Integration

### API Contract (Backend Endpoints)
- [ ] POST `/api/v1/users` - Create user
  - Request: `{ email, firstName, lastName, password, role }`
  - Response: 201 with UserResponse
  - Error: 400 (validation), 409 (duplicate email)
- [ ] GET `/api/v1/users` - List users
  - Query params: search, skip, take
  - Response: 200 with paginated UserResponse[]
- [ ] GET `/api/v1/users/{id}` - Get single user
  - Response: 200 with UserResponse
  - Error: 404 if not found
- [ ] PUT `/api/v1/users/{id}` - Update user
  - Request: `{ email, firstName, lastName, password }`
  - Response: 200 with updated UserResponse
- [ ] PATCH `/api/v1/users/{id}/role` - Change user role
  - Request: `{ role }`
  - Response: 200 with updated UserResponse
- [ ] PATCH `/api/v1/users/{id}/deactivate` - Deactivate user
  - Request: `{ isActive }`
  - Response: 200 with updated UserResponse

### Frontend Pages
- [ ] UsersPage loads and displays list of users
- [ ] Search box filters users by name/email in real-time
- [ ] Add User button navigates to AddUserPage
- [ ] Add User form submits successfully
- [ ] New user appears in list immediately
- [ ] Edit button opens EditUserPage with pre-filled data
- [ ] Edit form updates user successfully
- [ ] Change Role button opens ChangeRolePage
- [ ] Role change updates in list immediately
- [ ] Change Status button toggles IsActive
- [ ] Inactive users show appropriate visual indicator
- [ ] Error alerts display on API failures
- [ ] Loading spinners show during API calls
- [ ] Confirm dialogs appear for destructive actions

### Data Integrity
- [ ] Form validations match backend (email format, length limits)
- [ ] Duplicate email validation works
- [ ] Admin role enforcement works (non-admin can't set Admin role)
- [ ] Password hashing happens on backend (not frontend)

---

## Track D: Breweries Slice Integration

### API Contract (Backend Endpoints)
- [ ] POST `/api/v1/breweries` - Create brewery
  - Request: `{ name, country, region, description }`
  - Response: 201 with BreweryResponse
- [ ] GET `/api/v1/breweries` - List breweries
  - Query params: search, skip, take
  - Response: 200 with paginated BreweryResponse[]
- [ ] GET `/api/v1/breweries/{id}` - Get single brewery
  - Response: 200 with BreweryResponse
- [ ] PUT `/api/v1/breweries/{id}` - Update brewery
  - Response: 200 with updated BreweryResponse
- [ ] PATCH `/api/v1/breweries/{id}/deactivate` - Deactivate brewery
  - Response: 200 with updated BreweryResponse

### Beer Endpoints
- [ ] POST `/api/v1/beers` - Create beer
  - Request: `{ name, breweryId, style, type, abv, ibu, description }`
  - Response: 201 with BeerResponse
- [ ] GET `/api/v1/beers` - List beers
  - Query params: breweryId, search, skip, take
  - Response: 200 with paginated BeerResponse[]
- [ ] GET `/api/v1/beers/{id}` - Get single beer
  - Response: 200 with BeerResponse
- [ ] PUT `/api/v1/beers/{id}` - Update beer
  - Response: 200 with updated BeerResponse
- [ ] PATCH `/api/v1/beers/{id}/deactivate` - Deactivate beer
  - Response: 200 with updated BeerResponse

### Frontend Pages
- [ ] BreweriesPage loads and displays list
- [ ] Search/filter works for brewery names
- [ ] Add Brewery form submits successfully
- [ ] Brewery list updates immediately
- [ ] Edit brewery works correctly
- [ ] View Beers button shows beers for that brewery
- [ ] Add Beer form in BreweryBeersPage works
- [ ] Beers appear in list grouped by brewery
- [ ] Error handling and loading states work
- [ ] Confirm dialogs for delete/deactivate

---

## Track E: Arrangements Slice Integration (Complex)

### Arrangement Endpoints
- [ ] POST `/api/v1/arrangements` - Create arrangement
  - Request: `{ name, description, date, location }`
  - Response: 201 with ArrangementResponse
- [ ] GET `/api/v1/arrangements` - List arrangements
  - Response: 200 with status-filtered list
- [ ] GET `/api/v1/arrangements/{id}` - Get single arrangement
  - Response: 200 with full arrangement data
- [ ] PUT `/api/v1/arrangements/{id}` - Update arrangement (only if Created status)
  - Response: 200 with updated ArrangementResponse
- [ ] PATCH `/api/v1/arrangements/{id}/status` or similar - Update status
  - Response: 200 with updated status
  - Only valid transitions allowed

### Participant Endpoints
- [ ] POST `/api/v1/arrangements/{id}/participants` - Add participant
  - Request: `{ userId }`
  - Response: 201
- [ ] GET `/api/v1/arrangements/{id}/participants` - List participants
  - Response: 200 with ParticipantResponse[]
- [ ] DELETE `/api/v1/arrangements/{id}/participants/{userId}` - Remove participant
  - Response: 204

### Beer Endpoints
- [ ] POST `/api/v1/arrangements/{id}/beers` - Add beer to arrangement
  - Request: `{ beerId }`
  - Response: 201
- [ ] GET `/api/v1/arrangements/{id}/beers` - List beers
  - Response: 200 with BeerResponse[]
- [ ] DELETE `/api/v1/arrangements/{id}/beers/{beerId}` - Remove beer
  - Response: 204

### Status Machine Validation
- [ ] Created arrangement can transition to: Started, Canceled
- [ ] Started arrangement can transition to: Completed, Canceled
- [ ] Completed/Canceled arrangements are read-only
- [ ] Invalid status transitions return 400 error
- [ ] UI buttons show only valid next statuses

### Frontend Multi-Select Components
- [ ] AddBeersPage multi-select works correctly
  - Search across all breweries' beers
  - Checkboxes select/deselect beers
  - Already-added beers disabled in selector
  - Submit only works if changes made
  - Selected beers appear in arrangement

- [ ] AddParticipantsPage multi-select works correctly
  - Search users by name/email
  - Only active users available
  - Checkboxes select/deselect users
  - Already-added users disabled
  - Selected users appear in arrangement

### Status Transition UI
- [ ] ArrangementsPage shows status badges with correct colors
- [ ] Action buttons conditional on arrangement status
- [ ] StatusChangePage shows only valid next statuses
- [ ] Confirm dialog before status change
- [ ] Status updates immediately in list view
- [ ] Status history/timeline displays (if implemented)

### Error Scenarios
- [ ] Cannot add participant if already added
- [ ] Cannot add beer if already added
- [ ] Cannot add non-existent participant/beer
- [ ] Status changes enforce business rules
- [ ] Proper error messages for all failure cases

---

## Track F: Testing Infrastructure Validation

### Test Execution
- [ ] All bUnit tests pass locally
- [ ] GitHub Actions workflow runs tests on PR
- [ ] Test coverage >= 80%
- [ ] No failing tests block merge

### Test Coverage
- [ ] Track C: Users CRUD + role/status changes
- [ ] Track D: Breweries CRUD + Beer management
- [ ] Track E: Arrangement status machine + multi-select
- [ ] Integration tests for cross-track data (C↔E users, D↔E beers)

### API Contract Verification
- [ ] Endpoint URLs match documentation
- [ ] Request/response shapes validated
- [ ] Error responses handled correctly
- [ ] Missing fields detected early

---

## End-to-End Flow Verification

### Complete User Journey
```
1. Open app → redirects to /login (not authenticated)
2. Enter admin credentials → receives JWT token
3. Redirect to /shell with navigation menu
4. Click Users → shows user list
5. Click Add User → create new user → returns to list
6. Click Breweries → shows brewery list
7. Add Beer to brewery → beer appears in list
8. Click Arrangements → shows arrangement list
9. Create Arrangement:
   - Fill basic info
   - Add beers (multi-select from breweries)
   - Add participants (multi-select users)
   - Change status: Created → Started → Completed
10. Logout → redirects to login, token cleared
```

### Run Through:
- [ ] All pages load without errors
- [ ] No JavaScript errors in console
- [ ] Network requests all return 200 (or expected status)
- [ ] Authentication header present on protected endpoints
- [ ] CORS headers present in responses
- [ ] Performance acceptable (< 2s page loads)

---

## Data Integrity & Validation

### Frontend Validations
- [ ] Email format validation
- [ ] Required field validation
- [ ] String length limits enforced
- [ ] Numeric ranges validated
- [ ] Date/time inputs handled correctly

### Backend Validations
- [ ] Backend rejects invalid data
- [ ] Duplicate detection works (e.g., email)
- [ ] Business rule enforcement (e.g., status transitions)
- [ ] Authorization checks pass/fail correctly

### Sync Between Frontend & Backend
- [ ] Create operations return full object data
- [ ] List operations reflect recent changes
- [ ] Edit operations persist correctly
- [ ] Delete/deactivate operations immediate
- [ ] No stale data in UI after operations

---

## Security Checklist

### Authentication & Authorization
- [ ] Login only works for Admin users
- [ ] Non-admin users get 403 rejection
- [ ] Token includes correct role claim
- [ ] Expired tokens rejected properly
- [ ] Invalid tokens rejected with 401

### API Security
- [ ] POST/PUT/PATCH require authentication
- [ ] GET endpoints allow anonymous (review as needed)
- [ ] Admin-only operations check role
- [ ] Cross-origin requests work with CORS
- [ ] No sensitive data in JWT token payload (avoid passwords, etc)

### Data Protection
- [ ] Passwords never sent in response body
- [ ] Password hashing verified (BCrypt)
- [ ] HTTPS recommended for production (not enforced locally)
- [ ] Session tokens secure (httpOnly in production)

---

## Performance & Stress Testing

### Load Testing (Optional, if time permits)
- [ ] List endpoints handle 1000+ items
- [ ] Search/filter performance acceptable
- [ ] Multi-select with 100+ items responsive
- [ ] No timeout errors on normal operations
- [ ] Memory usage stable over time

### Browser Compatibility (Minimum)
- [ ] Chrome latest
- [ ] Firefox latest
- [ ] Edge latest
- [ ] Safari (if on Mac)

---

## Post-Integration Sign-Off

### Functional Completeness
- [ ] All 5 feature slices (A-E) working end-to-end
- [ ] All CRUD operations functional
- [ ] Complex multi-select working correctly
- [ ] Status machine enforced properly
- [ ] Error handling user-friendly

### Quality Metrics
- [ ] 80%+ test coverage
- [ ] 0 blocking bugs
- [ ] <10 minor issues
- [ ] All tracked issues closed
- [ ] Code review approved

### Documentation
- [ ] README updated with setup instructions
- [ ] API documentation (Scalar) accurate
- [ ] Frontend components documented
- [ ] Integration guide written
- [ ] Known issues / limitations documented

### Ready for Deployment
- [ ] All tests passing
- [ ] No console errors/warnings
- [ ] Performance benchmarks acceptable
- [ ] Security audit passed
- [ ] Merge to main approved

---

## Rollback Plan (If Issues Found)

1. **If Backend Issue:** Revert `implement-implementation-plan` branch
2. **If Frontend Track Issue:** Revert specific feature branch
3. **If Merge Conflict:** Resolve in temporary branch, test thoroughly
4. **If Data Corruption:** Restore from database backup, reseed test data

---

## Notes for Integration Testing

- Run tests in order: Track C → Track D → Track E (dependencies)
- Have API documentation (`/scalar/v1`) open in second browser tab
- Keep browser DevTools Network tab open to watch API calls
- Document any differences between expected and actual behavior
- If API contract mismatch found, prioritize fixing backend
- Track F tests should be run continuously during integration

**Integration Testing Lead:** Coordinate cross-track issues  
**Backend Lead:** Fix endpoint contract issues  
**Frontend Leads:** Ensure pages consume APIs correctly  

---

Last Updated: 2026-08-04  
Next Review: When Tracks C-F report completion
