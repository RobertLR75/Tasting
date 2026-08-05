# 45. Test Data Seeding in Development

Date: 2026-08-05

## Status

Accepted

## Context

To effectively develop and test the application locally, developers need a realistic baseline of data. Without a consistent dataset, manual testing of UI components, API endpoints, and database queries becomes tedious and error-prone. We need test data for the core domains: `Arrangement`, `Breweries`, `Beers`, and `Users`. 

We must also ensure that this test data is only applied in the development environment and never accidentally deployed to production.

## Decision

We will implement an automated test data seeding mechanism (e.g., Entity Framework Core `IHostedService` data seeder or specific Development migrations) that provisions a realistic dataset for development.

1.  **Scope of Data:** The seeder will populate `Users`, `Breweries`, `Beers`, and `Arrangement` (including tying users/beers to an arrangement to simulate a real-world scenario).
2.  **Environment Isolation:** The seeding logic will explicitly check the environment (`ASPNETCORE_ENVIRONMENT == "Development"`) before executing to prevent test data from leaking into staging or production databases.
3.  **Idempotency:** The seeding script will be written to be idempotent. It will check if the default test entities exist before attempting to insert them, allowing developers to run the application multiple times without causing primary key or unique constraint violations.

## Consequences

*   **Positive:** Developers can check out the repository, run the Aspire AppHost, and immediately have a fully populated, usable system without manual data entry.
*   **Positive:** Consistent data makes it easier to write and run automated end-to-end and integration tests.
*   **Negative:** The seeding logic must be maintained alongside the schema. When domain models change, the test data factory must also be updated.
