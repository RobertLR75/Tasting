# 0025. Inactive Brewery forces inactive Beers

**Status:** Accepted

## Context
Det var uklart om beers fra inaktive breweries fortsatt kunne brukes i nye arrangement.

## Decision
Når et `Brewery` settes `Inactive`, skal tilknyttede beers også settes `Inactive`.

## Consequences
- Nye arrangement kan blokkere bruk av beers fra inaktive breweries.
- Krever konsistent statuspropagering fra brewery til beers.
- Krever transaksjonell oppdatering av berørte beer-rader i PostgreSQL.
- Historiske arrangementdata beholdes, men ny bruk av inaktive beers stoppes.
- Skal implementeres i `IRequestHandler`-laget og verifiseres med unit tests for handlerlogikken og integrasjonstester for endepunktene.
