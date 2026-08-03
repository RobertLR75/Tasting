# 0014. FastEndpoints + PostgreSQL with SharedLibrary handler pattern

**Status:** Accepted

## Context
Nye API-endepunkter for `User`, `Brewery`, `Beer`, `BeerStyle`, `BeerType`, `Arrangement`, `Rating` og `Result` skal implementeres i samme løsning og må følge valgt teknologistack og eksisterende abstrahering.

## Decision
- Endepunkter bygges med FastEndpoints via `SharedLibrary.FastEndpoints`.
- Data persisteres i PostgreSQL via `SharedLibrary.PostgreSql.EntityFramework`.
- Schemamigrasjoner håndteres med `SharedLibrary.FluentMigration`.
- All domenelogikk og applikasjonsflyt plasseres i `IRequestHandler`-implementasjoner via `SharedLibrary.Services`.
- Endepunkter begrenses til routing, auth og request/response-mapping i `SharedLibrary.FastEndpoints`.
- Hver handler skal ha unit tests, og hvert endepunkt skal ha integrasjonstester.
- Redis kan brukes via `SharedLibrary.Redis` på read-paths der målinger viser reell ytelsesgevinst.

## Consequences
- Domenelogikk kan gjenbrukes uavhengig av transportteknologi.
- Endepunktklasser holdes tynne med konsistente endpoint-kontrakter fra `SharedLibrary.FastEndpoints`.
- Persistens, join-oppslag og concurrency-kontroll blir samlet i ett relasjonelt write-store på tvers av slices.
- Redis introduseres selektivt, ikke som blanket default, for å unngå unødvendig cache-kompleksitet.
