# 0004. Membership uniqueness and conflict semantics

**Status:** Proposed

## Context
Arrangement tillater adding av participants og beers i `Created`, men konfliktatferd måtte avklares.

## Decision
- Duplikater er ikke lov.
- Samme `User` kan ikke legges til flere ganger i samme arrangement.
- Samme `Beer` kan ikke legges til flere ganger i samme arrangement.
- Ved konflikt returnerer API `409 Conflict`.

## Consequences
- Krever unikhetsvalidering per arrangement + entity-id.
- API-kontrakt blir tydelig og forutsigbar for klienter.
- Bør støttes av både domenelogikk og databasekonstraints for robusthet.
- Skal implementeres i `IRequestHandler`-laget og verifiseres med unit tests for handlerlogikken og integrasjonstester for endepunktene.
