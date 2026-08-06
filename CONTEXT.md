# Domain Context

Et system for blindtesting av øl i arrangerte smakingssessjoner. Admin forvalter katalog og arrangement; participants setter scores; systemet rangerer øl deterministisk.

## Language

### Kvalitetsstyring

**Coverage gate**:
En håndhevet minimumsterskel for line coverage per SharedLibrary-prosjekt. I denne løsningen betyr det minst 90% line coverage for hvert prosjekt under `src/Shared/`, med eksplisitte og begrunnede ekskluderinger for kode uten reell logikk.
_Avoid_: Coverage target, global coverage, test percentage

**Test strategy**:
Valgt kombinasjon av unit tests og eventuelle integration tests for et spesifikt SharedLibrary-prosjekt, basert på bibliotekets ansvar. Unit tests er standard; integration tests brukes bare når bibliotekets verdi ligger i samspillet med eksterne rammeverk eller infrastrukturlag.
_Avoid_: Test approach, test plan, blanket testing

### Identitet

**User**:
Person i systemet med identitet og tilgangsnivå. Har nøyaktig én rolle (`Admin` eller `User`), og e-post er globalt unik (case-insensitiv).
_Avoid_: Member, Account, Profile

**Role**:
Autorisasjonsnivå for en bruker. Kun `Admin` eller `User` er gyldige; en bruker har alltid nøyaktig én.
_Avoid_: Permission, Access level

**Active user**:
Bruker med `IsActive=true` som kan autentiseres og utføre tillatte operasjoner.
_Avoid_: Enabled user

**Inactive user**:
Bruker med `IsActive=false`; tilgang blokkeres umiddelbart ved deaktivering.
_Avoid_: Disabled user, Deleted user

### Katalog

**Brewery**:
Produsentenhet som eier beers. Kan aldri slettes fysisk — settes `Inactive` via soft delete. Inaktivering kaskaderer til alle tilknyttede beers.
_Avoid_: Brand, Producer

**Beer**:
Øl-oppføring i katalogen, knyttet til ett `Brewery`, én `BeerStyle` og én `BeerType`. Navn er unikt per brewery (case-insensitiv). Inaktive beers skjules som standard.
_Avoid_: Product, Item

**BeerStyle**:
Stilklassifisering av en beer innenfor en `BeerType` (f.eks. IPA innenfor Ale).
_Avoid_: Substyle, Category

**BeerType**:
Overordnet kategori for `BeerStyle`-er (f.eks. Ale, Lager, Stout).
_Avoid_: Category, Genre

### Arrangement

**Arrangement**:
En tasting-sesjon med navn, beskrivelse og livssyklus. Eier et sett med beers og participants. Overganger mellom statuser styrer hvilke operasjoner som er tillatt.
_Avoid_: Event, Session, Round

**Arrangement status**:
Livssyklusverdi som bestemmer tillatte operasjoner: `Created → Active → Started → Completed`, eller `Created → Canceled` (reversibel til `Created`). `Active` er bekreftelsessteg der innhold (beers, participants) er låst og arrangement er forpliktet til gjennomføring, men rating-vinduet er ennå ikke åpnet. Ingen andre overganger er gyldige — spesielt er `Active` enveis (ingen rollback til `Created`) og kan ikke kanselleres.
_Avoid_: State, Phase

**Participant**:
En `User` som er lagt til i et `Arrangement` og har rett til å sette ratings. Refererer til `User` via ID — ikke et selvstendig subjekt.
_Avoid_: Member, Attendee, Voter

**Participant snapshot**:
Frossen kopi av deltakerens `FirstName` og `LastName` tatt idet arrangementet starter. Brukes i resultater for å bevare hvem som ratet, uavhengig av fremtidige brukerendringer.
_Avoid_: User copy, Cached user

**Beer snapshot**:
Frossen kopi av beer-metadata (navn, brewery, style, type) tatt idet arrangementet starter. Resultater for et arrangement viser alltid snapshot-data, aldri live-katalog.
_Avoid_: Catalog copy, Frozen beer

**Membership uniqueness invariant**:
Innen ett arrangement kan samme `User` og samme `Beer` forekomme maks én gang. Brudd gir `409 Conflict`.

### Rating og resultat

**Rating**:
En participants score på én beer i ett arrangement. Består av fire delscorer (`Visibility`, `Smell`, `Taste`, `Toast`) i skalaen 0–10 med steg 0.5. `TotalRating` beregnes server-side. Kan endres mens arrangementet er `Started`; fryses ved `Completed`.
_Avoid_: Vote, Score, Review

**Rating window**:
Perioden der rating er tillatt: kun mens arrangementet har status `Started`.

**Rating identity key**:
Den sammensatte nøkkelen `(ArrangementId, ParticipantId, BeerId)` som entydige identifiserer én rating. Duplikat-submit tolkes som oppdatering, ikke ny oppføring.

**Result**:
Aggregert rangeringsoppføring per `(ArrangementId, BeerId)`. Opprettes automatisk ved første rating, oppdateres løpende, og fryses ved `Completed`.
_Avoid_: Score summary, Ranking entry

**Result score**:
Primær rangeringsverdi: gjennomsnitt av alle ratings for en beer i arrangementet, avrundet til 2 desimaler med `MidpointRounding.AwayFromZero`.

**Tie-breakers**:
Deterministiske regler ved lik `Result score`: 1) flest antall ratings, 2) lavest standardavvik, 3) `BeerId` stigende.

### Tverrgående

**Optimistic concurrency**:
Versjonsbasert skrivekontroll der mutasjoner krever match på kjent versjon (`RowVersion`). Tapende skriv returnerer `409 Conflict`.

**Unified error contract**:
Felles feilstruktur for alle API-svar: `code`, `message`, `correlationId`. Brukt konsekvent for 400, 403, 404 og 409.

**Server-generated ID**:
Entitetsidentifikator generert av serveren. Klienter sender ikke `Id` i create-requests.

**Soft delete**:
Entitet markeres `Inactive` i stedet for fysisk sletting, for å bevare historisk integritet.
