# Admin Backoffice — Wireframes

> Detaljerte low-fidelity wireframes for `Tasting.Admin`. Dekker `Login`, `Arrangements`, `Breweries` og `Users`.  
> Wireframene er implementasjonskontrakten — de uttrykker backend-regler, ikke oppfinner UI-lokal logikk.

---

## Navigasjonsoversikt

```
/login                              → Login (ingen shell)
/arrangements                       → Arrangements liste
/arrangements/{id}/edit             → Arrangement edit
/arrangements/{id}/beers            → Arrangement add beers
/arrangements/{id}/participants     → Arrangement add participants
/arrangements/{id}/status           → Arrangement change status
/breweries                          → Breweries liste
/breweries/new                      → Brewery add
/breweries/{id}/beers               → Brewery beers liste
/breweries/{id}/beers/new           → Brewery add beer
/users                              → Users liste
/users/new                          → User add
/users/{id}/edit                    → User edit
/users/{id}/role                    → User edit role
/users/{id}/status                  → User edit status
```

**Shell-mønster** (alle autentiserte sider):

```
┌──────────────────────────────────────────────┐
│  Tasting Admin                    [AppBar]   │
├────────────┬─────────────────────────────────┤
│ Dashboard  │                                 │
│ Arrangements│         [Innholdsområde]        │
│ Users      │                                 │
│ Breweries  │                                 │
│ Beers      │                                 │
│ Ratings    │                                 │
│ Results    │                                 │
│            │                                 │
└────────────┴─────────────────────────────────┘
```

Aktiv navigasjonslenke vises highlighted. Login er eneste skjerm uten shell.

---

## Wireframe-mal

Hver seksjon følger:

| Felt | Innhold |
|---|---|
| **Formål** | Hva siden gjør |
| **Skisse** | ASCII-wireframe |
| **Primære handlinger** | Hva brukeren kan gjøre |
| **Tilstandsregler** | Disabled states, gating, tomtilstander |
| **Backend-feil som vises** | Feilmeldinger som siden må håndtere |

---

## 1. Login

**Formål:** Autentiserer adminbrukeren. Eneste skjerm uten shell.

**Skisse:**

```
┌──────────────────────────────────────┐
│                                      │
│           Tasting Admin              │
│                                      │
│  E-post                              │
│  ┌────────────────────────────────┐  │
│  │                                │  │
│  └────────────────────────────────┘  │
│                                      │
│  Passord                             │
│  ┌────────────────────────────────┐  │
│  │                                │  │
│  └────────────────────────────────┘  │
│                                      │
│  ┌────────────────────────────────┐  │
│  │           Logg inn             │  │
│  └────────────────────────────────┘  │
│                                      │
│  ⚠ Ugyldig e-post eller passord.    │  ← vises ved feil
│                                      │
└──────────────────────────────────────┘
```

**Primære handlinger:**
- Fyll inn e-post og passord → klikk «Logg inn»

**Tilstandsregler:**
- Login-knapp er aktiv så lenge feltene ikke er tomme
- Feilmelding er skjult inntil backend avviser innloggingen

**Backend-feil som vises:**
- Alle avvisninger (ukjent e-post, feil passord, ikke-admin, inaktiv bruker) vises som én generisk melding: «Ugyldig e-post eller passord.»
- Ingen spesifisering av årsaken (sikkerhetshensyn)

---

## 2. Arrangements liste

**Formål:** Gir oversikt over alle arrangementer og tilgang til relevante handlinger per status.

**Skisse:**

```
┌──────────────────────────────────────────────────────────────────────┐
│ Arrangements                                [+ Nytt arrangement]     │
├──────────────────────────────────────────────────────────────────────┤
│ Navn              │ Dato       │ Status    │ Handlinger              │
├───────────────────┼────────────┼───────────┼─────────────────────────┤
│ Høsttasting 2025  │ 2025-10-12 │ Created   │ [Edit] [Beers] [Participants] [Status] │
│ Vintertasting     │ 2025-12-01 │ Started   │ [Status]                │
│ Vår 2025          │ 2025-04-05 │ Completed │ —                       │
│ Avlyst runde      │ 2025-06-01 │ Canceled  │ [Status]                │
└──────────────────────────────────────────────────────────────────────┘
```

**Primære handlinger:**
- «+ Nytt arrangement» → opprettelsesflyt (ikke i scope for disse wireframene, men lenke finnes)
- **Edit** → `/arrangements/{id}/edit` (bare synlig for `Created`)
- **Beers** → `/arrangements/{id}/beers` (bare synlig for `Created`)
- **Participants** → `/arrangements/{id}/participants` (bare synlig for `Created`)
- **Status** → `/arrangements/{id}/status` (synlig for `Created`, `Started`, `Canceled`; ikke for `Completed`)

**Tilstandsregler:**
- `Created`: viser Edit, Beers, Participants, Status
- `Started`: viser bare Status
- `Completed`: ingen handlingslenker (viser —)
- `Canceled`: viser bare Status
- Tom liste: viser melding «Ingen arrangementer registrert.»

**Backend-feil som vises:**
- Generisk lastingsfeil hvis GET-kallet feiler

---

## 3. Arrangement edit

**Formål:** Endre navn, dato og beskrivelse for et arrangement. Bare tilgjengelig når status er `Created`.

**Skisse:**

```
┌──────────────────────────────────────────┐
│ Rediger arrangement                       │
├──────────────────────────────────────────┤
│ Navn *                                   │
│ ┌──────────────────────────────────────┐ │
│ │ Høsttasting 2025                     │ │
│ └──────────────────────────────────────┘ │
│                                          │
│ Dato *                                   │
│ ┌──────────────────────────────────────┐ │
│ │ 2025-10-12                           │ │
│ └──────────────────────────────────────┘ │
│                                          │
│ Beskrivelse                              │
│ ┌──────────────────────────────────────┐ │
│ │                                      │ │
│ │                                      │ │
│ └──────────────────────────────────────┘ │
│                                          │
│ [Avbryt]              [Lagre endringer]  │
│                                          │
│ ⚠ Arrangementet kan ikke lenger         │ ← ved 409
│   redigeres (status er ikke Created).   │
└──────────────────────────────────────────┘
```

**Primære handlinger:**
- Fyll inn felter → «Lagre endringer»
- «Avbryt» → tilbake til arrangements liste

**Tilstandsregler:**
- Siden er bare tilgjengelig fra arrangements-listen når status er `Created`
- Hvis status har endret seg siden siden ble lastet, returnerer backend 422/409 og feilmeldingen vises
- «Lagre endringer»-knapp er disabled mens lagring pågår

**Backend-feil som vises:**
- Arrangement kan ikke redigeres (status != `Created`): «Arrangementet kan ikke lenger redigeres.»
- Valideringsfeil (f.eks. tomt navn): feltnivå-melding

---

## 4. Arrangement add beers

**Formål:** Legg til aktive beers fra katalogen i et arrangement. Bare tilgjengelig når status er `Created`.

**Skisse:**

```
┌──────────────────────────────────────────────────────┐
│ Legg til beers — Høsttasting 2025                    │
├──────────────────────────────────────────────────────┤
│ Søk på beer-navn                                     │
│ ┌────────────────────────────────┐ [Søk]            │
│ │                                │                  │
│ └────────────────────────────────┘                  │
├──────────────────────────────────────────────────────┤
│ □  Urquell Pilsner      │ Pilsner Urquell  │ Lager  │
│ □  Duvel                │ Duvel Moortgat  │ Ale    │
│ ☑  Punk IPA  (allerede lagt til)                    │  ← disabled
│ □  Westmalle Tripel     │ Westmalle       │ Ale    │
├──────────────────────────────────────────────────────┤
│ Valgte: 2                                            │
│ [Avbryt]                      [Legg til valgte]     │
└──────────────────────────────────────────────────────┘
```

**Primære handlinger:**
- Fritekstsøk på beer-navn på tvers av alle aktive breweries
- Multi-select av beers
- «Legg til valgte» → sender valgte beers til arrangementet
- «Avbryt» → tilbake til arrangements liste

**Tilstandsregler:**
- Kun aktive beers fra aktive breweries vises
- Beers som allerede er knyttet til arrangementet vises disabled med markering «allerede lagt til»
- Tom søkeresultat: «Ingen beers matcher søket.»
- «Legg til valgte»-knapp er disabled hvis ingen er valgt

**Backend-feil som vises:**
- Beer allerede i arrangement (`409`): «En eller flere beers er allerede lagt til.»
- Arrangement ikke lenger `Created`: «Arrangementet kan ikke lenger endres.»

---

## 5. Arrangement add participants

**Formål:** Legg til aktive users som participants i et arrangement. Bare tilgjengelig når status er `Created`.

**Skisse:**

```
┌──────────────────────────────────────────────────────┐
│ Legg til participants — Høsttasting 2025             │
├──────────────────────────────────────────────────────┤
│ Søk på navn eller e-post                             │
│ ┌────────────────────────────────┐ [Søk]            │
│ │                                │                  │
│ └────────────────────────────────┘                  │
├──────────────────────────────────────────────────────┤
│ □  Ola Nordmann     │ ola@example.com   │ User      │
│ ☑  Kari Hansen  (allerede participant)              │  ← disabled
│ □  Erik Larsen      │ erik@example.com  │ Admin     │
├──────────────────────────────────────────────────────┤
│ Valgte: 1                                            │
│ [Avbryt]                      [Legg til valgte]     │
└──────────────────────────────────────────────────────┘
```

**Primære handlinger:**
- Fritekstsøk på navn og e-post (case-insensitivt deltreff)
- Multi-select av users
- «Legg til valgte» → legger til som participants
- «Avbryt» → tilbake til arrangements liste

**Tilstandsregler:**
- Kun aktive users vises
- Users som allerede er participants vises disabled med markering
- Tom søkeresultat: «Ingen brukere matcher søket.»
- «Legg til valgte» er disabled hvis ingen er valgt

**Backend-feil som vises:**
- Participant allerede i arrangement (`409`): «En eller flere brukere er allerede participants.»
- User er inaktiv: «Bruker er ikke aktiv og kan ikke legges til.»
- Arrangement ikke lenger `Created`: «Arrangementet kan ikke lenger endres.»

---

## 6. Arrangement change status

**Formål:** Utfør en statusovergang for arrangementet. Viser bare gyldige neste statuser.

**Skisse — fra Created:**

```
┌──────────────────────────────────────────┐
│ Endre status — Høsttasting 2025          │
├──────────────────────────────────────────┤
│ Nåværende status: Created               │
│                                          │
│ Velg ny status:                          │
│                                          │
│ ○ Started                               │
│   Start arrangementet. Beer-metadata     │
│   fryses som snapshot. Ratings åpnes.   │
│                                          │
│ ○ Canceled                              │
│   Kanseller arrangementet. Kan          │
│   gjenåpnes til Created igjen.          │
│                                          │
│ [Avbryt]              [Bekreft endring]  │
└──────────────────────────────────────────┘
```

**Skisse — fra Started:**

```
┌──────────────────────────────────────────┐
│ Endre status — Vintertasting             │
├──────────────────────────────────────────┤
│ Nåværende status: Started               │
│                                          │
│ Velg ny status:                          │
│                                          │
│ ○ Completed                             │
│   Fullfør arrangementet. Ratings fryses.│
│   Denne handlingen kan ikke reverseres. │
│                                          │
│ [Avbryt]              [Bekreft endring]  │
└──────────────────────────────────────────┘
```

**Skisse — fra Canceled:**

```
┌──────────────────────────────────────────┐
│ Endre status — Avlyst runde              │
├──────────────────────────────────────────┤
│ Nåværende status: Canceled              │
│                                          │
│ Velg ny status:                          │
│                                          │
│ ○ Created                               │
│   Gjenåpne arrangementet. Eksisterende  │
│   beers og participants beholdes.        │
│                                          │
│ [Avbryt]              [Bekreft endring]  │
└──────────────────────────────────────────┘
```

**Skisse — fra Completed:**

```
┌──────────────────────────────────────────┐
│ Endre status — Vår 2025                  │
├──────────────────────────────────────────┤
│ Nåværende status: Completed             │
│                                          │
│ Ingen videre statusendringer er mulige  │
│ for dette arrangementet.                 │
│                                          │
│ [Tilbake]                               │
└──────────────────────────────────────────┘
```

**Primære handlinger:**
- Velg én av de gyldige neste statusene → «Bekreft endring»
- «Avbryt» → tilbake til arrangements liste

**Tilstandsregler:**
- `Created` → kan velge `Started` eller `Canceled`
- `Started` → kan velge `Completed`
- `Canceled` → kan velge `Created`
- `Completed` → ingen valg, bare «Tilbake»
- «Bekreft endring» er disabled til et valg er gjort

**Backend-feil som vises:**
- Ugyldig overgang: «Ugyldig statusovergang.»
- Concurrent endring (409 concurrency): «Arrangementet ble endret av noen andre. Last siden på nytt.»

---

## 7. Breweries liste

**Formål:** Oversikt over alle breweries med søk og navigasjon til beers.

**Skisse:**

```
┌──────────────────────────────────────────────────────┐
│ Breweries                          [+ Nytt bryggeri] │
├──────────────────────────────────────────────────────┤
│ Søk på navn                                          │
│ ┌────────────────────────────────┐ [Søk]            │
│ │                                │                  │
│ └────────────────────────────────┘                  │
├──────────────────────────────────────────────────────┤
│ Navn                  │ Land      │ Status │ Beers  │
├───────────────────────┼───────────┼────────┼────────┤
│ Pilsner Urquell       │ Tsjekkia  │ Active │ [Beers]│
│ Duvel Moortgat        │ Belgia    │ Active │ [Beers]│
│ Gamle Bryggeriet      │ Norge     │ Inactive│ [Beers]│
└──────────────────────────────────────────────────────┘
```

**Primære handlinger:**
- Søk på brewery-navn (kun navn, ikke land eller status)
- «+ Nytt bryggeri» → `/breweries/new`
- **Beers** → `/breweries/{id}/beers`

**Tilstandsregler:**
- Søk er bare på navn
- Alle breweries vises (aktive og inactive)
- Tom liste: «Ingen bryggerier registrert.»
- Tomt søkeresultat: «Ingen bryggerier matcher søket.»

**Backend-feil som vises:**
- Generisk lastingsfeil

---

## 8. Brewery add

**Formål:** Opprett et nytt bryggeri.

**Skisse:**

```
┌──────────────────────────────────────────┐
│ Nytt bryggeri                            │
├──────────────────────────────────────────┤
│ Navn *                                   │
│ ┌──────────────────────────────────────┐ │
│ │                                      │ │
│ └──────────────────────────────────────┘ │
│                                          │
│ Land *                                   │
│ ┌──────────────────────────────────────┐ │
│ │                                      │ │
│ └──────────────────────────────────────┘ │
│                                          │
│ [Avbryt]                  [Opprett]      │
│                                          │
│ ⚠ Et bryggeri med dette navnet og landet │ ← ved 409
│   finnes allerede.                       │
└──────────────────────────────────────────┘
```

**Primære handlinger:**
- Fyll inn navn og land → «Opprett»
- «Avbryt» → tilbake til breweries liste

**Tilstandsregler:**
- «Opprett» er disabled mens kallet pågår
- Begge felter er påkrevd

**Backend-feil som vises:**
- Duplikat `(Name, Country)` (`409`): «Et bryggeri med dette navnet og landet finnes allerede.»

---

## 9. Brewery beers liste

**Formål:** Oversikt over beers for ett bestemt bryggeri.

**Skisse:**

```
┌──────────────────────────────────────────────────────┐
│ Beers — Pilsner Urquell               [+ Ny beer]   │
├──────────────────────────────────────────────────────┤
│ Søk på beer-navn                                     │
│ ┌────────────────────────────────┐ [Søk]            │
│ │                                │                  │
│ └────────────────────────────────┘                  │
├──────────────────────────────────────────────────────┤
│ Navn            │ Stil      │ Type   │ Status        │
├─────────────────┼───────────┼────────┼───────────────┤
│ Pilsner Urquell │ Bohemian  │ Lager  │ Active        │
│ Gammel Rezak    │ Pale Lager│ Lager  │ Inactive      │
└──────────────────────────────────────────────────────┘
```

**Primære handlinger:**
- Søk på beer-navn
- «+ Ny beer» → `/breweries/{id}/beers/new`

**Tilstandsregler:**
- Viser alle beers for bryggeriet (aktive og inactive)
- Tom liste: «Ingen beers registrert for dette bryggeriet.»
- Tomt søkeresultat: «Ingen beers matcher søket.»

**Backend-feil som vises:**
- Generisk lastingsfeil

---

## 10. Brewery add beer

**Formål:** Opprett en ny beer knyttet til valgt bryggeri.

**Skisse:**

```
┌──────────────────────────────────────────┐
│ Ny beer — Pilsner Urquell               │
├──────────────────────────────────────────┤
│ Navn *                                   │
│ ┌──────────────────────────────────────┐ │
│ │                                      │ │
│ └──────────────────────────────────────┘ │
│                                          │
│ BeerStyle *                              │
│ ┌──────────────────────────────────────┐ │
│ │ [Velg stil]                    ▼    │ │
│ └──────────────────────────────────────┘ │
│                                          │
│ BeerType *                               │
│ ┌──────────────────────────────────────┐ │
│ │ [Velg type]                    ▼    │ │
│ └──────────────────────────────────────┘ │
│                                          │
│ [Avbryt]                  [Opprett]      │
│                                          │
│ ⚠ En beer med dette navnet finnes       │ ← ved 409
│   allerede for dette bryggeriet.        │
└──────────────────────────────────────────┘
```

**Primære handlinger:**
- Fyll inn navn, velg stil og type → «Opprett»
- «Avbryt» → tilbake til brewery beers liste

**Tilstandsregler:**
- Brewery er forhåndsvalgt og ikke redigerbart (kontekst fra URL)
- Alle felter er påkrevd
- «Opprett» er disabled mens kallet pågår

**Backend-feil som vises:**
- Duplikat navn per brewery (`409`), også mot inactive beers: «En beer med dette navnet finnes allerede for dette bryggeriet.»

---

## 11. Users liste

**Formål:** Oversikt over alle users med søk og tilgang til redigering.

**Skisse:**

```
┌──────────────────────────────────────────────────────────────────────┐
│ Users                                          [+ Ny bruker]        │
├──────────────────────────────────────────────────────────────────────┤
│ Søk på navn eller e-post                                             │
│ ┌────────────────────────────────┐ [Søk]                            │
│ │                                │                                  │
│ └────────────────────────────────┘                                  │
├──────────────────────────────────────────────────────────────────────┤
│ Navn           │ E-post             │ Rolle │ Status  │ Handlinger  │
├────────────────┼────────────────────┼───────┼─────────┼─────────────┤
│ Ola Nordmann   │ ola@example.com    │ User  │ Active  │ [Edit] [Rolle] [Status] │
│ Kari Hansen    │ kari@example.com   │ Admin │ Active  │ [Edit] [Rolle] [Status] │
│ Per Olsen      │ per@example.com    │ User  │ Inactive│ [Edit] [Rolle] [Status] │
└──────────────────────────────────────────────────────────────────────┘
```

**Primære handlinger:**
- Søk på navn eller e-post (case-insensitivt deltreff)
- «+ Ny bruker» → `/users/new`
- **Edit** → `/users/{id}/edit`
- **Rolle** → `/users/{id}/role`
- **Status** → `/users/{id}/status`

**Tilstandsregler:**
- Viser både aktive og inactive users
- Tom liste: «Ingen brukere registrert.»
- Tomt søkeresultat: «Ingen brukere matcher søket.»

**Backend-feil som vises:**
- Generisk lastingsfeil

---

## 12. User add

**Formål:** Opprett en ny bruker.

**Skisse:**

```
┌──────────────────────────────────────────┐
│ Ny bruker                                │
├──────────────────────────────────────────┤
│ Fornavn *                                │
│ ┌──────────────────────────────────────┐ │
│ │                                      │ │
│ └──────────────────────────────────────┘ │
│                                          │
│ Etternavn *                              │
│ ┌──────────────────────────────────────┐ │
│ │                                      │ │
│ └──────────────────────────────────────┘ │
│                                          │
│ E-post *                                 │
│ ┌──────────────────────────────────────┐ │
│ │                                      │ │
│ └──────────────────────────────────────┘ │
│                                          │
│ [Avbryt]                  [Opprett]      │
│                                          │
│ ⚠ En bruker med denne e-postadressen    │ ← ved 409
│   finnes allerede.                       │
└──────────────────────────────────────────┘
```

**Primære handlinger:**
- Fyll inn fornavn, etternavn og e-post → «Opprett»
- «Avbryt» → tilbake til users liste

**Tilstandsregler:**
- Nye users opprettes som `IsActive=true` og rolle `User` (server-side default)
- «Opprett» er disabled mens kallet pågår
- Alle felter er påkrevd

**Backend-feil som vises:**
- Duplikat e-post (`409`), også mot inactive users: «En bruker med denne e-postadressen finnes allerede.»

---

## 13. User edit

**Formål:** Endre navn og e-post for en eksisterende bruker.

**Skisse:**

```
┌──────────────────────────────────────────┐
│ Rediger bruker — Ola Nordmann            │
├──────────────────────────────────────────┤
│ Fornavn *                                │
│ ┌──────────────────────────────────────┐ │
│ │ Ola                                  │ │
│ └──────────────────────────────────────┘ │
│                                          │
│ Etternavn *                              │
│ ┌──────────────────────────────────────┐ │
│ │ Nordmann                             │ │
│ └──────────────────────────────────────┘ │
│                                          │
│ E-post *                                 │
│ ┌──────────────────────────────────────┐ │
│ │ ola@example.com                      │ │
│ └──────────────────────────────────────┘ │
│                                          │
│ [Avbryt]              [Lagre endringer]  │
│                                          │
│ ⚠ E-postadressen er allerede i bruk.    │ ← ved 409
└──────────────────────────────────────────┘
```

**Primære handlinger:**
- Endre navn og/eller e-post → «Lagre endringer»
- «Avbryt» → tilbake til users liste

**Tilstandsregler:**
- Alle felter er påkrevd
- «Lagre endringer» er disabled mens kallet pågår

**Backend-feil som vises:**
- Duplikat e-post (`409`): «E-postadressen er allerede i bruk.»

---

## 14. User edit role

**Formål:** Endre rolle for en bruker (`Admin` eller `User`).

**Skisse:**

```
┌──────────────────────────────────────────┐
│ Endre rolle — Kari Hansen                │
├──────────────────────────────────────────┤
│ Nåværende rolle: Admin                  │
│                                          │
│ Velg ny rolle:                           │
│                                          │
│ ○ Admin                                 │
│ ● User                                  │
│                                          │
│ [Avbryt]              [Lagre rolle]      │
│                                          │
│ ⚠ Kan ikke endre rollen til siste       │ ← ved 422/409
│   aktive admin.                         │
│                                          │
│ ⚠ Kan ikke endre rollen til en inaktiv  │ ← ved 422
│   bruker.                               │
└──────────────────────────────────────────┘
```

**Primære handlinger:**
- Velg `Admin` eller `User` → «Lagre rolle»
- «Avbryt» → tilbake til users liste

**Tilstandsregler:**
- Begge alternativer vises alltid
- Nåværende rolle er forhåndsvalgt
- «Lagre rolle» er disabled hvis valgt rolle er lik nåværende rolle
- «Lagre rolle» er disabled mens kallet pågår

**Backend-feil som vises:**
- Bruker er inaktiv: «Kan ikke endre rollen til en inaktiv bruker.»
- Siste aktive admin (`409`): «Kan ikke endre rollen til siste aktive admin.»

---

## 15. User edit status

**Formål:** Aktiver eller deaktiver en bruker.

**Skisse:**

```
┌──────────────────────────────────────────┐
│ Endre status — Kari Hansen               │
├──────────────────────────────────────────┤
│ Nåværende status: Active                │
│                                          │
│ Velg ny status:                          │
│                                          │
│ ○ Active                                │
│ ● Inactive                              │
│                                          │
│ [Avbryt]              [Lagre status]     │
│                                          │
│ ⚠ Kan ikke deaktivere siste aktive      │ ← ved 409
│   admin.                                │
└──────────────────────────────────────────┘
```

**Primære handlinger:**
- Velg `Active` eller `Inactive` → «Lagre status»
- «Avbryt» → tilbake til users liste

**Tilstandsregler:**
- Begge alternativer vises alltid
- Nåværende status er forhåndsvalgt
- «Lagre status» er disabled hvis valgt status er lik nåværende
- «Lagre status» er disabled mens kallet pågår

**Backend-feil som vises:**
- Siste aktive admin kan ikke deaktiveres (`409`): «Kan ikke deaktivere siste aktive admin.»

---

## Annotasjonssammendrag — Domene- og tilstandsregler

| Regel | Kilde |
|---|---|
| Bare `Admin` kan logge inn på backoffice | ADR-0035 |
| Login-feil vises alltid som generisk melding | ADR-0035, implementation-plan |
| Arrangement kan bare redigeres i `Created` | ADR-0007, ADR-0042 |
| Add/remove beers og participants bare i `Created` | ADR-0008, ADR-0042 |
| Statusoverganger: `Created→Started`, `Created→Canceled`, `Canceled→Created`, `Started→Completed` | ADR-0003 |
| `Canceled→Created` beholder beers og participants | ADR-0003 |
| `Completed` er terminal — ingen videre handlinger | ADR-0003 |
| Bare aktive users kan legges til som participants | ADR-0042 |
| Bare aktive beers fra aktive breweries i arrangement | ADR-0042 |
| Beer snapshot tas ved overgang til `Started` | ADR-0022 |
| Participant snapshot tas ved overgang til `Started` | ADR-0022 |
| Brewery søk bare på navn | implementation-plan |
| Beer navn unikt per brewery, også mot inactive | ADR-0024 |
| Brewery unik kombinasjon `(Name, Country)` | implementation-plan |
| Users liste viser aktive og inactive | ADR-0042 |
| User søk: case-insensitivt deltreff på navn og e-post | implementation-plan |
| Rolleendring krever aktiv user | ADR-0033 |
| Siste aktive admin kan ikke deaktiveres eller nedgraderes | ADR-0033 |
| E-post globalt unik, case-insensitivt, også mot inactive | ADR-0033 |
