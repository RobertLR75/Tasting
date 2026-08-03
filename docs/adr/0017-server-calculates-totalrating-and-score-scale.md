# 0017. Server calculates total rating; score scale is 0-10 with 0.5 steps

**Status:** Accepted

## Context
Beregning og validering av rating måtte standardiseres for å unngå klientvariasjon og manipulasjon.

## Decision
- `TotalRating` beregnes server-side.
- Delscore-felter (`Visibility`, `Smell`, `Taste`, `Toast`) har gyldig intervall `0-10`.
- Kun steg på `0.5` er tillatt.

## Consequences
- Enhetlig og sikker scoreberegning uavhengig av klient.
- Rating-endepunkt må validere range og step før lagring.
- Klient trenger ikke sende/ha autoritet over totalfelt.
- Skal implementeres i `IRequestHandler`-laget og verifiseres med unit tests for handlerlogikken og integrasjonstester for endepunktene.
