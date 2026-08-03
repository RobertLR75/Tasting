# 0010. One rating per participant-beer pair, editable

**Status:** Accepted

## Context
Semantikk for gjentatt rating per participant/beer i samme arrangement måtte avklares.

## Decision
- En participant kan ikke ha flere separate ratings for samme beer i samme arrangement.
- Eksisterende rating kan endres (update), i stedet for å opprette duplikat.
- Endring er kun tillatt mens arrangement er `Started`.

## Consequences
- Krever unik constraint på `(ArrangementId, ParticipantId, BeerId)`.
- Re-submit må mappe til update-operasjon.
- Rating er låst når arrangement ikke lenger er `Started` (inkludert `Completed`).
- Skal implementeres i `IRequestHandler`-laget og verifiseres med unit tests for handlerlogikken og integrasjonstester for endepunktene.
