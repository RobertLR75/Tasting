# Architecture — Tasting (C4 Level 2 Container Diagram)

```mermaid
graph TD
    Admin(["👤 Admin"])
    Participant(["👤 Deltaker"])

    subgraph ACA["☁️ Azure Container Apps"]
        AdminFrontend["🖥️ Admin Frontend\nBlazor"]
        ParticipantFrontend["📱 Deltaker Frontend\nTBD · SPA"]
        API["⚙️ Tasting API\nC# .NET 9 · FastEndpoints\nVertical Slice Architecture\nJWT Auth"]
        PostgreSQL[("🗄️ PostgreSQL\nArrangements · Ratings · Results\nUsers · Breweries · Beers · Taxonomy")]
        Redis[("⚡ Redis\nCaching")]
    end

    KeyVault["🔑 Azure Key Vault\nHemmeligheter"]
    Monitor["📊 Azure Monitor\nApplication Insights\nOpenTelemetry OTLP"]

    Admin -->|HTTPS| AdminFrontend
    Participant -->|HTTPS| ParticipantFrontend
    AdminFrontend -->|REST / JSON| API
    ParticipantFrontend -->|REST / JSON| API
    API -->|EF Core + SQL| PostgreSQL
    API -->|StackExchange.Redis| Redis
    API -->|Azure SDK – oppstart| KeyVault
    API -->|OTLP traces/metrics/logs| Monitor
```

## Komponenter

| Container | Teknologi | Ansvar |
|---|---|---|
| **Admin Frontend** | Blazor | Administrasjon av arrangementer, brukere og katalogdata |
| **Deltaker Frontend** | TBD (SPA) | Deltakergrensesnitt for tasting-flyt og vurderinger |
| **Tasting API** | C# .NET 9, FastEndpoints, Vertical Slice | REST API, JWT auth, domenelogikk |
| **PostgreSQL** | Azure Database for PostgreSQL, EF Core, FluentMigration | All persistent domenedata |
| **Redis** | Azure Cache for Redis | Caching |
| **Azure Key Vault** | Azure SDK | Hemmeligheter (connection strings, signing key) |
| **Azure Monitor** | OpenTelemetry OTLP | Traces, metrics, logger |

## Nøkkelbeslutninger

- **[ADR-0014](adr/0014-fastendpoints-mongodb-and-shared-service-pattern.md)** — FastEndpoints + shared service pattern
- **[ADR-0034](adr/0034-postgresql-replaces-mongodb.md)** — PostgreSQL erstatter MongoDB for all lagring
- **[ADR-0032](adr/0032-api-versioning-from-day-one.md)** — API-versjonering fra dag én (`/api/v1/...`)
- **[ADR-0031](adr/0031-unified-error-contract-across-endpoints.md)** — Felles feilkontrakt på tvers av alle endepunkter
- **[ADR-0001](adr/0001-arrangement-lifecycle-gates.md)** — Arrangement livssyklus-porter

## Lokalt utviklingsmiljø

.NET Aspire (`Tasting.AppHost`) orkestrerer alle containere lokalt med service discovery og Aspire Dashboard for observabilitet.
