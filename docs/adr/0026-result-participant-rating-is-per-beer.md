# 0026. Result participant rating is scoped to the Result BeerId

**Status:** Accepted

## Context
`Result.Participants[].Rating` var tvetydig: score per beer eller totalscore for hele arrangementet.

## Decision
`Result.Participants[].Rating` representerer deltakerens rating for den aktuelle `BeerId` i resultatraden.

## Consequences
- Resultatraden forblir entydig per beer.
- Aggregatfelt (`TotalRating`) kan beregnes direkte fra participant-listen i samme rad.
- Separat modell trengs hvis man senere vil vise participant-total for hele arrangementet.
- Skal implementeres i `IRequestHandler`-laget og verifiseres med unit tests for handlerlogikken og integrasjonstester for endepunktene.
