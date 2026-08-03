# 0012. Results scoring and deterministic tie-breakers

**Status:** Accepted

## Context
Resultatberegning manglet presis regel for rangering og tie-break.

## Decision
Rangering per beer beregnes med:
1. Primærscore: gjennomsnitt (mean) av ratings
2. Tie-break 1: flest antall ratings vinner
3. Tie-break 2: lavest standardavvik vinner (mest konsensus)
4. Tie-break 3: `BeerId` stigende for deterministisk rekkefølge

## Consequences
- Stabil og reproduserbar ranking selv ved like scorer.
- Krever at ratingtelling og standardavvik beregnes i samme read-model.
- Klient kan stole på deterministisk sortering uten flapping mellom kall.
- Skal implementeres i `IRequestHandler`-laget og verifiseres med unit tests for handlerlogikken og integrasjonstester for endepunktene.
- Resultatvisninger kan caches i Redis når målinger viser gevinst og invalidasjon følger rating-/resultatoppdateringer.
