# 0025. Inactive Brewery forces inactive Beers

**Status:** Accepted

## Context
Det var uklart om beers fra inaktive breweries fortsatt kunne brukes i nye arrangement.

## Decision
Når et `Brewery` settes `Inactive`, skal tilknyttede beers også settes `Inactive`.

## Consequences
- Nye arrangement kan blokkere bruk av beers fra inaktive breweries.
- Krever konsistent statuspropagering fra brewery til beers.
- Historiske arrangementdata beholdes, men ny bruk av inaktive beers stoppes.
