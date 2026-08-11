# Entity Framework and Dapper share one persistence boundary

PostgreSQL remains the single store and FluentMigration remains its sole schema owner, but operators may select Entity Framework or Dapper once at process startup. Both providers implement the same `IPersistenceService<TEntity>` and provider-neutral Specification boundary so domains can migrate incrementally without leaking ORM, SQL, or table concepts into application code; mixed providers, hot switching, and provider-owned migrations are rejected.

## Considered Options

- **Keep Entity Framework as the only provider** — rejected because it prevents the requested operational provider choice.
- **Expose provider-specific query APIs** — rejected because it would couple domains to infrastructure and make behavioral parity difficult to verify.
- **One startup-selected provider behind shared contracts** — chosen because it keeps the schema and application semantics stable while allowing incremental domain migration.
