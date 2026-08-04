# Tasting.Admin + Tasting.Api — Implementeringsplan

## Mål

Lever admin-backoffice for `Login`, `Arrangement`, `Breweries` og `Users` som parallelle vertical slices i `Tasting.Admin`, med all kanonisk forretningslogikk håndhevet i `Tasting.Api`-handlers.

## Styrende beslutninger

| Beslutning | Valg |
|---|---|
| Admin-auth | Bare `Admin` får tilgang til admin-frontend |
| Login-feil | Generisk feilmelding for feil legitimasjon og ikke-admin |
| Arrangement-redigering | Bare `Created` kan redigeres |
| Arrangement-medlemskap | Beers og participants kan bare legges til/fjernes i `Created` |
| Arrangement-status | `Created -> Started`, `Created -> Canceled`, `Canceled -> Created`, `Started -> Completed` |
| Reopen semantics | `Canceled -> Created` beholder beers og participants |
| Arrangement-lister | Full liste med status som standard |
| Arrangement beer-picking | Bare aktive breweries/beers, globalt beersøk, multi-select på tvers av breweries |
| Arrangement participant-picking | Bare aktive users, eksisterende participants vises disabled |
| Brewery uniqueness | `(Name, Country)` er unik kombinasjon |
| Brewery search | Søker bare på brewery-navn |
| Beer uniqueness | Beer-navn er unikt per brewery også mot inactive beers |
| User lists | Viser både aktive og inactive users |
| User search | Case-insensitivt deltreff på navn og e-post |
| User role/status safety | Rolleendring krever aktiv user; siste aktive admin kan ikke deaktiveres eller nedgraderes |
| Backend ownership | Forretningsregler implementeres i `IRequestHandler`-laget |

---

## Arbeidsstrømmer som kan utvikles i parallell

| Spor | Avhenger av | Leveranse |
|---|---|---|
| A. Backend auth + identity rules | Ingen | Login-kontrakter, user-søk, rolle/status-regler |
| B. Backend arrangement rules | Ingen | Arrangement CRUD, statusendringer, add/remove beer/participant |
| C. Backend catalog rules | Ingen | Brewery/beer-søk, create-flow, aktive katalogregler |
| D. Frontend shell + login | A | Ruting, auth-guard, login-side |
| E. Frontend users slice | A + D | Users list/add/edit/role/status |
| F. Frontend breweries slice | C + D | Breweries list/add + beers under brewery |
| G. Frontend arrangements slice | B + C + A + D | Arrangement list/edit/status/add beers/add participants |
| H. Testspor | Løper sammen med hvert spor | Handler-, integrasjons- og bUnit-tester |

---

## Spor A — Backend auth + identity rules

### API-endringer

1. **Autentisering for admin-frontend**
   - Verifiser eksisterende auth-oppsett i `Tasting.Api`.
   - Innfør/login-endre endpoint eller auth-adapter som returnerer generisk feil ved:
     - ukjent e-post
     - feil passord
     - gyldig bruker uten `Admin`
     - inaktiv bruker
   - Sørg for at admin-frontenden kun får gyldig sesjon/token for `Admin`.

2. **Users list/search**
   - Utvid `ListUsers` med fritekstsøk på `Email`, `FirstName`, `LastName`.
   - Returner både aktive og inactive users som standard.

3. **Create user**
   - Håndhev global unikhet på e-post case-insensitivt, også mot inactive users.
   - Nye users opprettes som `IsActive=true`.

4. **Update user**
   - Tillat endring av navn og e-post.
   - Håndhev samme e-postunikhet som ved create.

5. **Change user role**
   - Lag eksplisitt operasjon/endepunkt for rolleendring hvis det ikke finnes.
   - Avvis rolleendring hvis target user er inactive.
   - Avvis nedgradering av siste aktive admin.

6. **Change user status**
   - Lag eksplisitt operasjon/endepunkt for aktivering/deaktivering hvis det ikke finnes.
   - Avvis deaktivering av siste aktive admin.

### Tester

- Handler-tests for:
  - generisk login-nekt
  - e-postunikhet mot inactive users
  - søk på navn/e-post
  - blokkert rolleendring for inactive user
  - blokkert deaktivering/nedgradering av siste aktive admin
- Integrasjonstester for alle user-flyter

---

## Spor B — Backend arrangement rules

### API-endringer

1. **List arrangements**
   - Returner full liste med status.

2. **Update arrangement**
   - Håndhev at bare `Created` kan redigeres.
   - Tillat endring av `Name`, `Date`, `Description`.

3. **Status transitions**
   - Oppdater state machine til:
     - `Created -> Started`
     - `Created -> Canceled`
     - `Canceled -> Created`
     - `Started -> Completed`
   - Alle andre overganger avvises.

4. **Reopen semantics**
   - `Canceled -> Created` må ikke slette beers eller participants.

5. **Participant membership**
   - `AddParticipant` og `RemoveParticipant` bare når `Status == Created`.
   - Participant må være aktiv user.
   - Duplikater gir `409`.

6. **Beer membership**
   - `AddBeer` og `RemoveBeer` bare når `Status == Created`.
   - Beer må være aktiv og tilhøre aktivt brewery.
   - Duplikater gir `409`.

7. **Arrangement details for admin pages**
   - Sørg for at `GetArrangement` returnerer nok data til:
     - redigeringsside
     - visning av eksisterende beers
     - visning av eksisterende participants
     - statusstyring

### Tester

- Handler-tests for:
  - `Canceled -> Created`
  - medlemskap beholdes ved reopen
  - blokkert redigering når status != `Created`
  - blokkert add/remove utenfor `Created`
  - blokkert add participant for inactive user
  - blokkert add beer for inactive beer/brewery
- Integrasjonstester for hele arrangementflyten

---

## Spor C — Backend catalog rules

### API-endringer

1. **List breweries**
   - Full liste for admin.
   - Støtt søk bare på brewery-navn.

2. **Create brewery**
   - Håndhev unik kombinasjon `(Name, Country)` case-insensitivt etter repo-konvensjon.

3. **List beers for brewery**
   - Sørg for et lesekall som kan drive brewery-beers-siden.
   - Støtt søk på beer-navn.

4. **Create beer**
   - Opprett beer knyttet til valgt brewery.
   - Håndhev at navn er unikt per brewery også mot inactive beers.

5. **Catalog selection for arrangement**
   - Sørg for lesekall som støtter:
     - liste av aktive breweries
     - aktive beers
     - globalt beersøk på tvers av breweries
   - Returner markører nok til at frontend kan vise hvilket brewery et beer tilhører.

### Tester

- Handler-tests for:
  - brewery uniqueness `(Name, Country)`
  - beer uniqueness mot inactive beers
  - brewery-søk bare på navn
  - globalt beersøk bare over aktive elementer
- Integrasjonstester for brewery/beer flytene

---

## Spor D — Frontend shell + login

### UI-endringer

1. **Routing**
   - Definer sider og ruter for:
     - `/login`
     - `/arrangements`
     - `/arrangements/{id}/edit`
     - `/arrangements/{id}/beers`
     - `/arrangements/{id}/participants`
     - `/arrangements/{id}/status`
     - `/breweries`
     - `/breweries/new`
     - `/breweries/{id}/beers`
     - `/breweries/{id}/beers/new`
     - `/users`
     - `/users/new`
     - `/users/{id}/edit`
     - `/users/{id}/role`
     - `/users/{id}/status`

2. **Auth guard**
   - Uautentiserte brukere sendes til login.
   - Kun admin-sesjon får åpne backoffice-ruter.

3. **Login page**
   - Felter for e-post og passord.
   - Login-knapp.
   - Generisk feil ved avvist innlogging.

4. **HTTP-klienter og slice services**
   - Lag feature-nære klienter/modeller per slice i frontend.
   - Ikke legg domenevalidering i UI; bare inputvalidering og presentasjon.

### Tester

- bUnit for routing, guard og login-feilmelding

---

## Spor E — Frontend users slice

### UI-endringer

1. **Users page**
   - Søkefelt + søkeknapp.
   - Liste over alle users med navn, e-post, rolle, status.
   - Lenker til edit, role, status.
   - Knapp for opprett ny user.

2. **Add user page**
   - Felter for navn og e-post.
   - Eventuelt rollefelt hvis create-flowen skal støtte både `User` og `Admin`.
   - Opprett-knapp.
   - Vis backend-feil for e-postkonflikt.

3. **Edit user page**
   - Endre navn og e-post.

4. **Edit user role page**
   - Velg mellom `Admin` og `User`.
   - Håndter backend-feil når bruker er inactive eller siste aktive admin.

5. **Edit user status page**
   - Velg `Active` eller `Inactive`.
   - Håndter backend-feil når bruker er siste aktive admin.

### Tester

- bUnit for søk, listing, navigasjon og feilvisning

---

## Spor F — Frontend breweries slice

### UI-endringer

1. **Breweries page**
   - Søkefelt på navn + søkeknapp.
   - Liste over breweries.
   - Lenke til beers for brewery.
   - Knapp for add brewery.

2. **Add brewery page**
   - Felter for navn og land.
   - Opprett-knapp.
   - Vis backend-feil for `(Name, Country)`-konflikt.

3. **Brewery beers page**
   - Søkefelt for beer-navn + søkeknapp.
   - Liste over beers knyttet til brewery.
   - Knapp for add beer.

4. **Add beer page**
   - Felter for beer-opprettelse og kobling til brewery.
   - Vis backend-feil for navnekonflikt.

### Tester

- bUnit for søk, listing, navigasjon og create-feil

---

## Spor G — Frontend arrangements slice

### UI-endringer

1. **Arrangements page**
   - Liste over alle arrangementer med status.
   - Vis kun relevante handlingslenker per status:
     - `Edit` bare for `Created`
     - `Add beers` bare for `Created`
     - `Add participants` bare for `Created`
     - `Change status` for statusene som har gyldige neste steg

2. **Edit arrangement page**
   - Felter for navn, dato, beskrivelse.
   - Oppdater-knapp.
   - Håndter backend-konflikt hvis arrangement ikke lenger er `Created`.

3. **Add beers page**
   - Listevisning for aktive breweries og beers.
   - Globalt fritekstsøk på beer-navn.
   - Multi-select på beers, inkludert beers fra flere breweries.
   - Vis allerede tilknyttede beers som disabled eller markert.

4. **Add participants page**
   - Liste over aktive users.
   - Fritekstsøk på navn/e-post.
   - Multi-select på users.
   - Eksisterende participants vises disabled/allerede lagt til.

5. **Change arrangement status page**
   - Vis current status.
   - Vis bare gyldige neste statuser:
     - fra `Created`: `Started`, `Canceled`
     - fra `Canceled`: `Created`
     - fra `Started`: `Completed`
     - fra `Completed`: ingen

### Tester

- bUnit for statusstyrt rendering, søk, disabled-elementer og konfliktvisning

---

## Spor H — Tverrgående testing og kvalitet

1. **API integration coverage**
   - Oppdater/utvid `Tasting.Api.IntegrationTests` per slice.

2. **Frontend component coverage**
   - Oppdater/utvid `Tasting.Admin.UnitTests` per page og reusable component.

3. **Contract checks**
   - Verifiser at frontendklientene matcher faktiske API-responser for:
     - users list/search
     - breweries list/search
     - beers list/search
     - arrangement details/list/status

---

## Foreslått paralleliseringsrekkefølge

1. **Start samtidig:** Spor A, B og C
2. **Når A er stabilt:** Spor D og E
3. **Når C er stabilt:** Spor F
4. **Når A+B+C+D er stabile:** Spor G
5. **Kontinuerlig hele veien:** Spor H

---

## Konkrete arbeidsoppgaver per utviklerstrøm

### Strøm 1 — Identity
- Backend auth/login
- user search/list
- user create/edit
- role/status endpoints
- identity tests

### Strøm 2 — Arrangement
- status transition updates
- reopen semantics
- arrangement edit rules
- participant/beer membership guards
- arrangement tests

### Strøm 3 — Catalog
- brewery search/create uniqueness
- beer list/create uniqueness
- active catalog selectors for arrangement flow
- catalog tests

### Strøm 4 — Frontend shell + users
- routing
- auth guard
- login page
- users pages + tests

### Strøm 5 — Frontend breweries + arrangements
- breweries pages + tests
- arrangements pages + tests
- shared admin components for list/search/form/status actions

---

## Ferdigdefinisjon

Arbeidet er ferdig når:

1. ADR-ene beskriver de nye domenereglene uten motstrid.
2. Alle backend-regler håndheves i `IRequestHandler`-laget.
3. Admin-frontendet kan gjennomføre alle beskrevne sider og arbeidsflyter kun via API-et.
4. Parallelle team kan jobbe uavhengig med tydelige kontrakter mellom sporene.
