# Tasting.Api — Implementeringsplan

## Arkitekturavgjørelser

| Beslutning | Valg |
|---|---|
| Backend-form | Enkelt `Tasting.Api`-prosjekt |
| Features-struktur | Subdomene-grupperinger (Identity / Catalog / Arrangement / Rating) |
| Participant-info | Snapshot av FirstName+LastName ved `Started` |
| Redis v1 | Utelates — tas inn ved målte behov |
| Feilkontrakt (ADR-0031) | Global exception-handler i `SharedLibrary.FastEndpoints` |
| Arkitekturmønster | Modulær monolitt med 4 bounded contexts |
| Bounded contexts | Identity / Catalog / Arrangement / Rating+Result |
| Cross-context validering | `IArrangementService`-abstraksjon (service layer boundary) |
| Autentisering | Ekstern OIDC + JWT-validering i Tasting.Api |
| Leveransesekvens | Identity → Catalog → Arrangement → Rating → Result |

---

## Mappestruktur

```
src/Backend/Tasting.Api/
├── Features/
│   ├── Identity/
│   │   └── Users/
│   │       ├── CreateUser/        (Request, Handler, Endpoint, Mapper)
│   │       ├── GetUser/
│   │       ├── ListUsers/
│   │       ├── UpdateUser/
│   │       └── DeactivateUser/
│   ├── Catalog/
│   │   ├── Breweries/             (Create, Get, List, Update, Deactivate)
│   │   ├── Beers/                 (Create, Get, List, Update, Deactivate)
│   │   ├── BeerStyles/            (Create, Get, List)
│   │   └── BeerTypes/             (Create, Get, List)
│   ├── Arrangement/
│   │   ├── Arrangements/          (Create, Get, List, Update, Start, Cancel, Complete)
│   │   ├── Participants/          (Add, Remove)
│   │   └── Beers/                 (Add, Remove)
│   └── Rating/
│       ├── Ratings/               (Submit — upsert semantikk)
│       └── Results/               (GetResults)
├── Infrastructure/
│   ├── Identity/
│   │   ├── IdentityDbContext.cs
│   │   └── Migrations/
│   ├── Catalog/
│   │   ├── CatalogDbContext.cs
│   │   └── Migrations/
│   ├── Arrangement/
│   │   ├── ArrangementDbContext.cs
│   │   └── Migrations/
│   └── Rating/
│       ├── RatingDbContext.cs
│       └── Migrations/
├── Contracts/
│   └── IArrangementService.cs     (brukes av Rating-context for cross-context validering)
└── Program.cs
```

---

## Fase 0 — Fundament

**Mål:** Prosjektskjelett med alt på plass, men ingen features enda.

### Oppgaver

1. **Opprett `Tasting.Api`** — ASP.NET Core (.NET 10), legg til i `Tasting.sln` under `Backend`
2. **Prosjektreferanser:** `SharedLibrary`, `SharedLibrary.FastEndpoints`, `SharedLibrary.PostgreSql.EntityFramework`, `SharedLibrary.FluentMigration`, `SharedLibrary.Services`
3. **Aspire AppHost:** Registrer `Tasting.Api` som service i `Tasting.AppHost`
4. **Global feilkontrakt (ADR-0031):** Implementer i `SharedLibrary.FastEndpoints`
   - `ErrorResponse` record: `string Code`, `string Message`, `string CorrelationId`
   - Fjern kommentert kode i `FastEndPointsExtensions.UseEndpoints`
   - Legg til FastEndpoints global exception-handler som mapper:
     - `ServiceNotFoundException` → 404 + `not_found`
     - `ConflictException` → 409 + `conflict`
     - `ForbiddenException` → 403 + `forbidden`
     - `ValidationException` → 400 + `validation_error`
     - Uventede exceptions → 500 + `internal_error`
   - Legg til `CorrelationId` middleware (generer Guid per request, legg i header + response)
5. **OIDC/JWT-auth:** Konfigurer i `Program.cs` med `OpenIdConnectSettings` fra `SharedLibrary`
   - JWT Bearer-validering mot OIDC-provider
   - Claim-mapping: `sub` → UserId, `role` → Role
6. **API-versjonering (ADR-0032):** Sett route prefix `/api/v1` via FastEndpoints config
7. **FluentMigration runner:** Registrer i `Program.cs` med DbContext per context
8. **Felles unntak i SharedLibrary.Services:**
   - `ConflictException` (409)
   - `ForbiddenException` (403)
   - `BusinessRuleException` (422)

---

## Fase 1 — Identity Context

**ADR-er:** 0033, 0002, 0031, 0032

### Datamodell

```csharp
// User entity (IdentityDbContext)
User {
    Guid Id
    string Email           // Unik, case-insensitiv (lowercase-indeks i PG)
    string FirstName
    string LastName
    bool IsActive
    Role Role              // enum: Admin | User
    DateTimeOffset CreatedAt
    DateTimeOffset? UpdatedAt
}
```

### Migrasjoner (FluentMigration)

- `Users`-tabell med unik indeks på `LOWER(email)` — ADR-0033
- `CHECK`-constraint: `Role IN ('Admin', 'User')`

### Features

| Feature | Route | Auth | ADR |
|---|---|---|---|
| `CreateUser` | POST `/api/v1/users` | Admin (for Admin-opprettelse), autentisert for User | 0033 |
| `GetUser` | GET `/api/v1/users/{id}` | Autentisert | — |
| `ListUsers` | GET `/api/v1/users` | Admin | 0028 |
| `UpdateUser` | PUT `/api/v1/users/{id}` | Admin | — |
| `DeactivateUser` | PATCH `/api/v1/users/{id}/deactivate` | Admin | 0033 |

**Handler-regler:**
- `CreateUser`: Hvis `Role=Admin` i request, valider at caller er `Admin` (409 ellers) — ADR-0033
- `CreateUser`: Sjekk e-post unikhet (case-insensitiv) — returner `409 Conflict` ved konflikt
- `DeactivateUser`: Sett `IsActive=false` — blokkerer videre autentisering umiddelbart

### Tests

- Unit: handler-logikk (rollesjekk, e-postunikhet, deaktivering)
- Integration: alle 5 endepunkter

---

## Fase 2 — Catalog Context

**ADR-er:** 0014, 0015, 0020, 0021, 0024, 0025, 0028, 0029

### Datamodell

```csharp
Brewery {
    Guid Id; string Name; bool IsActive; DateTimeOffset CreatedAt; DateTimeOffset? UpdatedAt
}

BeerStyle { Guid Id; string Name; string? Description; DateTimeOffset CreatedAt }
BeerType  { Guid Id; string Name; string? Description; DateTimeOffset CreatedAt }

Beer {
    Guid Id
    Guid BreweryId         // FK → Brewery, required — ADR-0015
    Guid BeerStyleId       // FK → BeerStyle — ADR-0020
    Guid BeerTypeId        // FK → BeerType — ADR-0020
    string Name            // Unik per Brewery, case-insensitiv — ADR-0024
    bool IsActive
    DateTimeOffset CreatedAt
    DateTimeOffset? UpdatedAt
}
```

### Migrasjoner

- `Breweries`, `BeerStyles`, `BeerTypes`, `Beers`
- FK: `Beers.BreweryId → Breweries.Id`, `BeerStyleId → BeerStyles.Id`, `BeerTypeId → BeerTypes.Id`
- Unik indeks på `(BreweryId, LOWER(Name))` i Beers — ADR-0024

### Features

| Feature | Route | Auth | ADR |
|---|---|---|---|
| `CreateBrewery` | POST `/api/v1/breweries` | Admin | 0021 |
| `GetBrewery` | GET `/api/v1/breweries/{id}` | Autentisert | — |
| `ListBreweries` | GET `/api/v1/breweries` | Autentisert | 0028 |
| `UpdateBrewery` | PUT `/api/v1/breweries/{id}` | Admin | 0021 |
| `DeactivateBrewery` | PATCH `/api/v1/breweries/{id}/deactivate` | Admin | 0015, 0025 |
| `CreateBeerStyle` | POST `/api/v1/beer-styles` | Admin | 0021 |
| `GetBeerStyle` | GET `/api/v1/beer-styles/{id}` | Autentisert | — |
| `ListBeerStyles` | GET `/api/v1/beer-styles` | Autentisert | 0028 |
| `CreateBeerType` | POST `/api/v1/beer-types` | Admin | 0021 |
| `GetBeerType` | GET `/api/v1/beer-types/{id}` | Autentisert | — |
| `ListBeerTypes` | GET `/api/v1/beer-types` | Autentisert | 0028 |
| `CreateBeer` | POST `/api/v1/beers` | Admin | 0021 |
| `GetBeer` | GET `/api/v1/beers/{id}` | Autentisert | — |
| `ListBeers` | GET `/api/v1/beers?includeInactive=false` | Autentisert (Admin for includeInactive=true) | 0029 |
| `UpdateBeer` | PUT `/api/v1/beers/{id}` | Admin | 0021 |
| `DeactivateBeer` | PATCH `/api/v1/beers/{id}/deactivate` | Admin | 0029 |

**Handler-regler:**
- `DeactivateBrewery`: Transaksjonelt sett alle tilknyttede `Beer.IsActive=false` — ADR-0025
- `CreateBeer`: Valider `BreweryId` er aktiv og eksisterer — ADR-0015
- `CreateBeer` / `UpdateBeer`: Sjekk `(BreweryId, LOWER(Name))` unikhet — ADR-0024
- `ListBeers`: Default filter `IsActive=true`; `Admin` kan sende `includeInactive=true` — ADR-0029

### Tests

- Unit: brewery-deaktiverings-kaskade, beer-navneunikhet, inaktiv-brewery-validering
- Integration: alle endepunkter

---

## Fase 3 — Arrangement Context

**ADR-er:** 0001, 0003, 0004, 0006, 0007, 0008, 0013, 0022

### Datamodell

```csharp
Arrangement {
    Guid Id
    string Name
    string? Description
    ArrangementStatus Status     // enum: Created | Started | Canceled | Completed
    uint RowVersion              // Optimistic concurrency — ADR-0006
    DateTimeOffset CreatedAt
    DateTimeOffset? UpdatedAt
}

ArrangementParticipant {
    Guid Id
    Guid ArrangementId
    Guid UserId
    string FirstNameSnapshot     // Snapshot ved Started — ADR-0022 + grilling-beslutning
    string LastNameSnapshot
    DateTimeOffset CreatedAt
}

ArrangementBeer {
    Guid Id
    Guid ArrangementId
    Guid BeerId
    string NameSnapshot          // Snapshot ved Started — ADR-0022
    string BreweryNameSnapshot
    string BeerStyleSnapshot
    string BeerTypeSnapshot
    DateTimeOffset CreatedAt
}
```

### Migrasjoner

- `Arrangements`, `ArrangementParticipants`, `ArrangementBeers`
- Unik constraint: `(ArrangementId, UserId)` i ArrangementParticipants — ADR-0004
- Unik constraint: `(ArrangementId, BeerId)` i ArrangementBeers — ADR-0004
- `RowVersion` / `xmin` i Arrangements — ADR-0006

### `IArrangementService` (Contracts/)

```csharp
public interface IArrangementService {
    Task<ArrangementStatus> GetStatusAsync(Guid arrangementId, CancellationToken ct);
    Task<bool> IsParticipantAsync(Guid arrangementId, Guid userId, CancellationToken ct);
    Task<bool> IsBeerInArrangementAsync(Guid arrangementId, Guid beerId, CancellationToken ct);
}
```

### Features

| Feature | Route | Auth | ADR |
|---|---|---|---|
| `CreateArrangement` | POST `/api/v1/arrangements` | Admin | 0008 |
| `GetArrangement` | GET `/api/v1/arrangements/{id}` | Autentisert | — |
| `ListArrangements` | GET `/api/v1/arrangements` | Autentisert | 0028 |
| `UpdateArrangement` | PUT `/api/v1/arrangements/{id}` | Admin | 0007, 0008 |
| `StartArrangement` | POST `/api/v1/arrangements/{id}/start` | Admin | 0003, 0008 |
| `CancelArrangement` | POST `/api/v1/arrangements/{id}/cancel` | Admin | 0003, 0008 |
| `CompleteArrangement` | POST `/api/v1/arrangements/{id}/complete` | Admin | 0003, 0008 |
| `AddParticipant` | POST `/api/v1/arrangements/{id}/participants` | Admin | 0001, 0004, 0008 |
| `RemoveParticipant` | DELETE `/api/v1/arrangements/{id}/participants/{userId}` | Admin | 0013 |
| `AddBeer` | POST `/api/v1/arrangements/{id}/beers` | Admin | 0001, 0004, 0008 |
| `RemoveBeer` | DELETE `/api/v1/arrangements/{id}/beers/{beerId}` | Admin | 0013 |

**Handler-regler:**
- `UpdateArrangement`: Avvis hvis status ≠ `Created` → 409 — ADR-0007
- `AddParticipant` / `AddBeer` / `RemoveParticipant` / `RemoveBeer`: Status må være `Created` — ADR-0001, 0013
- Alle mutasjoner på Arrangement: Bruk optimistic concurrency med `RowVersion` → 409 ved konflikt — ADR-0006
- Status-transisjonsmatrise (ADR-0003):
  - `Created → Started` ✓ (trigger snapshot-taking: ADR-0022)
  - `Created → Canceled` ✓
  - `Started → Completed` ✓
  - Alt annet → 409
- `StartArrangement`: Ta snapshot av navn fra User og beer-metadata fra Catalog (via service-grensesnitt)
- `AddParticipant` / `AddBeer`: Sjekk duplikat → 409 — ADR-0004

### Tests

- Unit: state machine, snapshot-logikk, concurrency-konflikt, duplikatsjekk
- Integration: alle 11 endepunkter, inkludert concurrency-scenario

---

## Fase 4+5 — Rating+Result Context

**ADR-er:** 0005, 0009, 0010, 0011, 0012, 0016, 0017, 0018, 0019, 0023, 0026, 0027, 0030

### Datamodell

```csharp
Rating {
    Guid Id                      // Server-generert — ADR-0027
    Guid ArrangementId
    Guid ParticipantId           // UserId
    Guid BeerId                  // ADR-0016
    decimal Visibility           // 0-10, steg 0.5 — ADR-0017
    decimal Smell
    decimal Taste
    decimal Toast
    decimal TotalRating          // Server-beregnet — ADR-0017
    uint RowVersion              // Optimistic concurrency — ADR-0030
    DateTimeOffset CreatedAt
    DateTimeOffset? UpdatedAt
}

Result {
    Guid Id                      // Server-generert — ADR-0027
    Guid ArrangementId
    Guid BeerId                  // ADR-0018, 0019
    string BeerNameSnapshot      // Fra arrangement-snapshot
    decimal TotalRating          // Avrundet til 2dp — ADR-0023
    int RatingCount              // For tie-break — ADR-0012
    decimal StandardDeviation    // Intern (ikke avrundet) — ADR-0023
    int Rank
    DateTimeOffset CreatedAt
    DateTimeOffset? UpdatedAt
}

ResultParticipant {
    Guid Id
    Guid ResultId
    Guid ParticipantId
    string ParticipantNameSnapshot
    decimal Rating               // Deltakerens score for denne beer — ADR-0026
}
```

### Migrasjoner

- `Ratings`, `Results`, `ResultParticipants`
- Unik constraint: `(ArrangementId, ParticipantId, BeerId)` i Ratings — ADR-0016
- Unik constraint: `(ArrangementId, BeerId)` i Results — ADR-0019
- `RowVersion` i Ratings — ADR-0030

### Features

| Feature | Route | Auth | ADR |
|---|---|---|---|
| `SubmitRating` | POST `/api/v1/arrangements/{id}/ratings` | Participant (autentisert) | 0005, 0009, 0010 |
| `GetResults` | GET `/api/v1/arrangements/{id}/results` | Autentisert | 0011, 0012 |

**SubmitRating handler-regler (ADR-0005, 0009, 0010, 0011, 0016, 0017, 0023, 0027, 0030):**

1. Hent arrangement-status via `IArrangementService.GetStatusAsync` → avvis (409) hvis ≠ `Started`
2. Valider at caller er participant via `IArrangementService.IsParticipantAsync` → 403 hvis ikke
3. Valider at beerId er i arrangementet via `IArrangementService.IsBeerInArrangementAsync` → 404 hvis ikke
4. Valider sub-scores: hvert felt i [0, 10] med steg 0.5 → 400 hvis ugyldig
5. Beregn `TotalRating = (Visibility + Smell + Taste + Toast) / 4` server-side
6. Avrund til 2 desimaler med `MidpointRounding.AwayFromZero` — ADR-0023
7. Upsert: sjekk `(ArrangementId, ParticipantId, BeerId)` — oppdater hvis eksisterer, opprett hvis ikke — ADR-0010
8. Bruk optimistic concurrency ved update → 409 ved konflikt — ADR-0030
9. Auto-opprett/oppdater `Result`-rad for `(ArrangementId, BeerId)` — ADR-0011
10. Oppdater `ResultParticipant` for denne deltakeren

**GetResults handler-regler (ADR-0012, 0023):**

1. Hent alle Result-rader for arrangementet
2. Beregn rangering:
   - Primær: `TotalRating` DESC (mean av alle ratings, avrundet 2dp)
   - Tie-break 1: `RatingCount` DESC
   - Tie-break 2: `StandardDeviation` ASC (uten avrunding internt)
   - Tie-break 3: `BeerId` ASC (deterministisk)
3. Returner rangert liste med participant-ratings per beer

**Result-frysing (ADR-0011, 0019):**  
Når arrangement går til `Completed` (via `CompleteArrangement` i Arrangement-context), fryses Result-rader. `SubmitRating`-handler blokkeres av status-sjekk i steg 1.

### Tests

- Unit: scoring-logikk, validering (range + steps), tie-break, upsert-semantikk, concurrency-konflikt
- Integration: submit + get results, full flyt fra Created → Started → rating → Completed

---

## Tverrgående krav

### Feilkontrakt (ADR-0031)

Alle handlers kaster domenespesifikke exceptions. Global handler i `SharedLibrary.FastEndpoints` mapper til:

```json
{
  "code": "conflict",
  "message": "En beer med dette navnet finnes allerede for dette bryggeriet.",
  "correlationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

HTTP-koder: 400 (validation), 403 (forbidden), 404 (not_found), 409 (conflict), 500 (internal_error)

### Optimistic concurrency (ADR-0006, 0030)

- `Arrangement.RowVersion`: ETag/`If-Match`-header eller inlinei request body
- `Rating.RowVersion`: Samme mønster
- Conflict → 409 med `code: "concurrency_conflict"`

### Autentisering (ADR-0033)

- `IsActive=false` → 403 på alle kall (middleware/policy)
- `Role=Admin` claim kreves for admin-operasjoner
- Participant-validering: `sub`-claim i JWT = `UserId` i Participants-tabell

---

## Teststruktur

```
tests/
├── Tasting.Api.UnitTests/
│   ├── Identity/
│   ├── Catalog/
│   ├── Arrangement/
│   └── Rating/
└── Tasting.Api.IntegrationTests/
    ├── Identity/
    ├── Catalog/
    ├── Arrangement/
    └── Rating/
```

- Unit: xUnit + Moq/NSubstitute, in-memory DbContext
- Integration: xUnit + `WebApplicationFactory<Program>` + Testcontainers (PostgreSQL)
