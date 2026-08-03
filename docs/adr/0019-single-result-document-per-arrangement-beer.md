# 0019. Single Result document per Arrangement and Beer

**Status:** Accepted

## Context
Resultpersistens måtte avklares for å unngå tvetydig datamodell og duplikater.

## Decision
Det lagres nøyaktig ett `Result`-dokument per `(ArrangementId, BeerId)`.
Dokumentet oppdateres fortløpende mens arrangement er `Started`, og fryses ved `Completed`.

## Consequences
- Enkelt lesemønster for resultattabeller/rangering.
- Krever unik indeks på `(ArrangementId, BeerId)` i MongoDB.
- Oppdateringer må være idempotente og concurrency-sikre.
