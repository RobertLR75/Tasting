## Arrangement
**Definition:** En tasting-sesjon som har identitet, navn, beskrivelse, status, dato og medlemskap (beers + participants).  
**Used in:** Arrangement-endepunkter, rating/resultater knyttet til arrangement.

## Participant
**Definition:** Deltakerrolle i arrangement som refererer til en `User` (ikke eget datasnapshot).  
**Used in:** Arrangement-medlemskap, senere rating/resultatflyt.

## Beer
**Definition:** Øl-objekt som inngår i et arrangement, identifisert med `Id` og `Name`.  
**Used in:** Arrangement-medlemskap, grunnlag for rating/resultater.

## User
**Definition:** Primær brukerentitet i systemet; source of truth for identitet (f.eks. navn/e-post).  
**Used in:** Participant-referanse i arrangement og tilgangsstyring.

## Arrangement status
**Definition:** Livssyklusverdi (`Created`, `Canceled`, `Started`, `Completed`) som styrer hvilke operasjoner som er lov.  
**Used in:** Validering av oppdatering og medlemskapsendringer.

## Membership uniqueness invariant
**Definition:** Innenfor ett arrangement skal hver `User` og hver `Beer` forekomme maks én gang.  
**Used in:** Add participant/beer-endepunkter, konfliktrespons `409 Conflict`.

## Optimistic concurrency
**Definition:** Versjonsbasert write-kontroll der mutasjoner krever match på kjent versjon (ETag/RowVersion), ellers `409 Conflict`.  
**Used in:** Start/update/add participant/add beer-endepunkter.

## Rating window
**Definition:** Tids-/statusvindu der rating er gyldig; i dette domenet kun mens arrangement er `Started`.  
**Used in:** Rating create/update-regler og validering mot arrangementstatus.

## Result score
**Definition:** Primær rangeringsverdi for beer i results; her definert som gjennomsnitt av ratings.  
**Used in:** Resultliste og sortering av beers.

## Tie-breakers
**Definition:** Deterministiske regler ved lik score: flest ratings, deretter lavest standardavvik, deretter `BeerId`.  
**Used in:** Stabil rangering i results.

## Beer
**Definition:** Domeneobjekt for øl med identitet, navn, beskrivelse, volum, alkoholprosent og stil/type-klassifisering.  
**Used in:** Arrangement-medlemskap, ratinggrunnlag og resultatrangering.

## Beer style
**Definition:** Klassifisering av beer som beskriver stil innenfor en beer type.  
**Used in:** Berikelse/filtrering av beer-katalog.

## Beer type
**Definition:** Overordnet kategori for beer styles.  
**Used in:** Taksonomi for beer style og katalogstruktur.

## Brewery
**Definition:** Produsententitet med metadata (navn, land, kontakt) og tilhørende beers.  
**Used in:** Beer-oppretting, katalog og domenevisning.

## Soft delete
**Definition:** Entitet beholdes i databasen men markeres inaktiv i stedet for fysisk sletting.  
**Used in:** `Brewery` livssyklus (`Inactive`) for å bevare historikk.

## Rating identity key
**Definition:** Entydig identifikasjon av rating med kombinasjonen `ArrangementId + ParticipantId + BeerId`.  
**Used in:** Upsert/update av rating og unikhetskontroll i PostgreSQL.

## Rating scale
**Definition:** Gyldig skala for delscore: 0 til 10 i steg på 0.5.  
**Used in:** Inputvalidering i rating-endepunkter.

## Result model
**Definition:** Aggregert resultat knyttet til ett `ArrangementId` og ett `BeerId`, med totalscore og deltakergrunnlag.  
**Used in:** Result-endepunkter og rangering per beer i arrangement.

## Taxonomy reference
**Definition:** Lagring av klassifisering via ID-referanser (`BeerTypeId`, `BeerStyleId`) i stedet for full embedding.  
**Used in:** Beer-modell i PostgreSQL med relasjonelle referanser.

## Metadata snapshot
**Definition:** Frosset kopi av visningsmetadata som brukes for historisk konsistens i et arrangement.  
**Used in:** Resultatvisning etter at arrangement er startet/fullført.

## Rounding policy
**Definition:** Fast regel for scoreavrunding: `decimal`, 2 desimaler, `MidpointRounding.AwayFromZero`.  
**Used in:** Beregning av `TotalRating` og aggregert resultatsnitt.

## Beer uniqueness invariant
**Definition:** Innen samme brewery må beer-navn være unikt case-insensitivt.  
**Used in:** Beer create/update-validering og `409 Conflict` ved duplikat.

## Inactive propagation
**Definition:** Regel der statusendring på parent (`Brewery`) setter child-entiteter (`Beer`) til inaktiv for ny bruk.  
**Used in:** Katalogstyring og validering ved oppretting/oppdatering av arrangement.

## Server-generated ID
**Definition:** Entitetsidentifikator generert av API/server, ikke av klient.  
**Used in:** Oppretting av Beer, Rating og Result.

## Active catalog default
**Definition:** Standardlesing returnerer kun aktive katalogelementer; inaktive krever eksplisitt admin-forespørsel.  
**Used in:** Beer-liste-endepunkt med `includeInactive`-semantikk.

## Unified error contract
**Definition:** Felles struktur for API-feilrespons med `code`, `message` og `correlationId`.  
**Used in:** Konsistent håndtering av 4xx/konfliktfeil på tvers av alle endepunkter.

## API versioning
**Definition:** Kontraktsstyring via versjonerte endepunkter (f.eks. `/api/v1/...`) for bakoverkompatibilitet.  
**Used in:** Stabil klientintegrasjon når API-kontrakter utvikles.

## User role exclusivity
**Definition:** Hver bruker kan ha nøyaktig én rolle (`Admin` eller `User`).  
**Used in:** User create/update-endepunkter og autorisasjonspolicy.

## User email uniqueness
**Definition:** E-post er globalt unik case-insensitivt for alle brukere.  
**Used in:** Oppretting av brukere og konfliktrespons `409`.

## Immediate access block
**Definition:** Når `IsActive=false`, mister brukeren tilgang umiddelbart.  
**Used in:** Innlogging/autorisering for alle beskyttede endepunkter.
