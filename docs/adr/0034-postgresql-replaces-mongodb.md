# PostgreSQL replaces MongoDB for all storage

ADR-0014 chose MongoDB for Arrangements, Ratings and Results. After further evaluation we decided to use PostgreSQL for all storage domains (Arrangements, Ratings, Results, Users, Breweries, Beers and Taxonomy). A single relational store with Entity Framework Core and FluentMigration simplifies the data model, enables cross-entity queries and transactions, and eliminates the operational overhead of maintaining two separate databases. Redis is retained as a dedicated caching layer.

## Considered Options

- **Keep MongoDB for documents (Arrangements/Ratings/Results) + PostgreSQL for catalog** — rejected because the split adds operational complexity without sufficient benefit at this scale.
- **PostgreSQL for everything** — chosen.

## Consequences

- `SharedLibrary.MongoDB` remains in the repo but will not be used by application projects; it can be removed in a follow-up cleanup.
- All entities must be mapped as relational models; document-style embedding (e.g. snapshot metadata) becomes a JSON column in PostgreSQL instead.
