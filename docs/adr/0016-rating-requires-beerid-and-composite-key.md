# 0016. Rating requires BeerId with composite uniqueness key

**Status:** Accepted

## Context
Rating måtte knyttes entydig til en beer innen arrangementet; eksisterende modell manglet `BeerId`.

## Decision
- `RatingModel` utvides med `BeerId`.
- Domeneunikhet defineres som `(ArrangementId, ParticipantId, BeerId)`.

## Consequences
- API kan validere og oppdatere korrekt rating uten tvetydighet.
- Persistens må håndheve unikhet for å unngå duplikater.
- Results-beregning får entydig kobling til beer.
