# 0018. Result model aligns to Arrangement and Beer dimensions

**Status:** Accepted

## Context
Opprinnelig `ResultModel` brukte `RoundId` (string), som ikke matchet domenet ellers (`ArrangementId: Guid`).

## Decision
`ResultModel` skal inneholde:
- `ArrangementId`
- `BeerId`
- `TotalRating`
- Liste av participants

`RoundId` fjernes fra modellen.

## Consequences
- Results blir konsistent med rating- og arrangementdomene.
- Mapping mellom rating og resultater blir direkte og entydig.
- Krever presis definisjon av hva participant-listen representerer (råscore, aggregat, eller begge).
