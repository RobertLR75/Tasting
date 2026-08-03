# 0019. Single Result row per Arrangement and Beer

**Status:** Accepted

## Context
Resultpersistens måtte avklares for å unngå tvetydig datamodell og duplikater.

## Decision
Det lagres nøyaktig én `Result`-rad per `(ArrangementId, BeerId)`.
Raden oppdateres fortløpende mens arrangement er `Started`, og fryses ved `Completed`.

## Consequences
- Enkelt lesemønster for resultattabeller/rangering.
- Krever unik constraint eller unik indeks på `(ArrangementId, BeerId)` i PostgreSQL, opprettet via FluentMigration.
- Oppdateringer må være idempotente og concurrency-sikre.
- Skal implementeres i `IRequestHandler`-laget og verifiseres med unit tests for handlerlogikken og integrasjonstester for endepunktene.
