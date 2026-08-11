# 0030. Optimistic concurrency on rating updates

**Status:** Accepted

## Context
Samtidige writes på samme rating kan ellers gi siste-write-vinner uten kontroll og skjulte overskrivinger.

## Decision
Ratingens domenemodell og v1-kontrakter er uten persistensversjon. En egen persistensmodell beholder den eksisterende `row_version`-kolonnen som backend-internt concurrency token, og mapper eksplisitt til domenet.

Persistenslaget oppdager både konkurrerende oppretting av samme rating identity key og konkurrerende oppdatering mellom backendens lesing og lagring. Én write vinner; taperen returnerer `409 Conflict` gjennom Unified error contract uten automatisk retry. Klienter sender ingen versjonsverdi.

## Consequences
- Forutsigbar konfliktatferd ved parallelle rating-endringer.
- Klient kan vise konfliktmeldingen, hente ferske data og sende en ny request, men kjenner ikke concurrency-tokenet.
- Krever versjonskolonne og betinget oppdateringsmønster i PostgreSQL.
- En klientvisning som ble gammel før requesten startet kan ikke oppdages uten klienttoken; en senere gyldig request kan derfor overskrive tidligere feltverdier. Dette er en akseptert avgrensning.
- Verifiseres med handler-/persistenstester og faktiske samtidige API-writes for både create/upsert og update.
