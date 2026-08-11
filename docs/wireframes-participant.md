# Participant App — Wireframes

> Detaljerte low-fidelity wireframes for `Tasting.App`. Dekker hele deltaker-flyten: login → bli med i arrangement → vurder øl → se resultater.  
> Wireframene er implementasjonskontrakten — de uttrykker backend-regler, ikke oppfinner UI-lokal logikk.

---

## Navigasjonsoversikt

```
/login                                          → Screen 1: Login (ingen shell)
/register                                       → Screen 2: Create User (ingen shell)
/arrangements                                   → Screen 3: List Arrangements
/arrangements/{id}/lobby                        → Screen 4: Arrangement Lobby
/arrangements/{id}/beers/{beerIndex}            → Screen 5: Beer Info (loop per øl)
/arrangements/{id}/beers/{beerIndex}/rate       → Screen 6: Rating for Beer X (loop per øl)
/arrangements/{id}/beers/{beerIndex}/results    → Screen 7: Results for Beer X (loop per øl)
/arrangements/{id}/results                      → Screen 8: Arrangement Results
/arrangements/{id}/results/beers/{beerId}       → Screen 9: Beer X with Result
```

**Shell-mønster** (alle autentiserte sider):

```
┌──────────────────────────────────────────────┐
│  Tasting                          [AppBar]   │
│                                              │
│         [Innholdsområde]                     │
│                                              │
└──────────────────────────────────────────────┘
```

Login og Create User er eneste skjermer uten shell.

Screens 5–7 gjentas én gang per øl i arrangementet (beerIndex = 1..N).

---

## Wireframe-mal

Hvert wireframe følger dette mønsteret:

```
┌──────────────────────────────────────────────┐
│  [Tittel / AppBar]                           │
├──────────────────────────────────────────────┤
│                                              │
│  [Innholdsområde]                            │
│                                              │
│  [Handlingsknapper]                          │
└──────────────────────────────────────────────┘
```

---

## Screen 1 — Login

**Rute:** `/login`  
**Shell:** Ingen (standalone-skjerm)

```
┌──────────────────────────────────────────────┐
│                                              │
│             Tasting                          │
│                                              │
│  E-post                                      │
│  ┌────────────────────────────────────────┐  │
│  │                                        │  │
│  └────────────────────────────────────────┘  │
│                                              │
│  Passord                                     │
│  ┌────────────────────────────────────────┐  │
│  │ ••••••••                               │  │
│  └────────────────────────────────────────┘  │
│                                              │
│  [Feilmelding — kun synlig ved feil]         │
│  «Ugyldig e-post eller passord.»             │
│                                              │
│  ┌────────────────────────────────────────┐  │
│  │             Logg inn                   │  │
│  └────────────────────────────────────────┘  │
│                                              │
│       Opprett bruker →                       │
└──────────────────────────────────────────────┘
```

| Element | Regel |
|---|---|
| «Logg inn»-knapp | Aktivert kun når begge felt er ikke-tomme |
| «Logg inn»-knapp | Deaktivert mens innlogging pågår |
| Feilmelding | Vises ved enhver avvisning fra backend (ukjent e-post, feil passord, inaktiv bruker) |
| Feilmelding | Generisk tekst — ingen grunn avsløres |
| «Opprett bruker»-lenke | Navigerer til Screen 2 |
| Etter innlogging | Navigerer til Screen 3 |

**Backend-feil:**

| Feil | Melding til bruker |
|---|---|
| Ukjent e-post / feil passord / inaktiv bruker | «Ugyldig e-post eller passord.» |

**SignalR:** —

---

## Screen 2 — Create User

**Rute:** `/register`  
**Shell:** Ingen (standalone-skjerm)

```
┌──────────────────────────────────────────────┐
│                                              │
│             Opprett bruker                   │
│                                              │
│  E-post                                      │
│  ┌────────────────────────────────────────┐  │
│  │                                        │  │
│  └────────────────────────────────────────┘  │
│  [Feltfeil]                                  │
│                                              │
│  Fornavn                                     │
│  ┌────────────────────────────────────────┐  │
│  │                                        │  │
│  └────────────────────────────────────────┘  │
│  [Feltfeil]                                  │
│                                              │
│  Etternavn                                   │
│  ┌────────────────────────────────────────┐  │
│  │                                        │  │
│  └────────────────────────────────────────┘  │
│  [Feltfeil]                                  │
│                                              │
│  Passord                                     │
│  ┌────────────────────────────────────────┐  │
│  │ ••••••••                               │  │
│  └────────────────────────────────────────┘  │
│  [Feltfeil]                                  │
│                                              │
│  ┌──────────────┐  ┌──────────────────────┐  │
│  │   Avbryt     │  │       Opprett         │  │
│  └──────────────┘  └──────────────────────┘  │
│                                              │
└──────────────────────────────────────────────┘
```

| Element | Regel |
|---|---|
| «Opprett»-knapp | Aktivert kun når alle felt er ikke-tomme |
| Brukernavn | Settes automatisk til e-post-adressen |
| «Avbryt»-knapp | Navigerer tilbake til Screen 1 (Login) |
| Etter opprettelse | Navigerer til Screen 3 |

**Backend-feil:**

| Feil | Melding til bruker |
|---|---|
| Duplikat e-post | «E-posten er allerede i bruk.» |
| Valideringsfeil | Feltspesifikke feilmeldinger under hvert felt |

**SignalR:** —

---

## Screen 3 — List Arrangements

**Rute:** `/arrangements`  
**Shell:** App shell med topplinje

```
┌──────────────────────────────────────────────┐
│  Tasting                          [AppBar]   │
├──────────────────────────────────────────────┤
│                                              │
│  Aktive arrangementer                        │
│                                              │
│  ┌──────────────────────────────────────┐    │
│  │ Navn       │ Dato       │ Status     │    │
│  ├────────────┼────────────┼────────────┤    │
│  │ Sommerfest │ 2026-08-01 │ Created    │    │
│  │ Høsttest   │ 2026-09-15 │ Started    │    │
│  └──────────────────────────────────────┘    │
│                                              │
│  [Tom liste] «Ingen aktive arrangementer.»   │
│  (kun vist når listen er tom)                │
│                                              │
└──────────────────────────────────────────────┘
```

| Element | Regel |
|---|---|
| Grid | Viser kun synlige arrangementer med status `Active` |
| «Bli med»-knapp | Selvregistrerer autentisert bruker som Participant; klienten sender ikke UserId |
| «Bli med»-knapp | Navigerer til Screen 4 (Lobby) etter vellykket backend-respons |
| «Gå til lobby»-knapp | Vises når brukeren allerede er Participant og navigerer uten ny join |
| Tom liste | Viser «Ingen aktive arrangementer.» |

**Backend-feil:**

| Feil | Melding til bruker |
|---|---|
| Lastfeil | Generisk feilmelding |

**SignalR:** —

---

## Screen 4 — Arrangement Lobby

**Rute:** `/arrangements/{id}/lobby`  
**Shell:** App shell med topplinje

```
┌──────────────────────────────────────────────┐
│  Tasting                          [AppBar]   │
├──────────────────────────────────────────────┤
│                                              │
│  Sommerfest — Lobby                          │
│                                              │
│  Øl i arrangementet:                         │
│  ┌──────────────────────────────────────┐    │
│  │ Navn       │ Bryggeri   │ Stil       │    │
│  ├────────────┼────────────┼────────────┤    │
│  │ Mørk Lager │ Hansa      │ Lager      │    │
│  │ Pale Ale   │ Nøgne Ø    │ Pale Ale   │    │
│  └──────────────────────────────────────┘    │
│                                              │
│  ⏳ Venter på at arrangementet starter…      │
│                                              │
└──────────────────────────────────────────────┘
```

| Element | Regel |
|---|---|
| Ølgrid | Viser beer snapshot-data (fryst ved arrangementstart) |
| Ventemelding | Vises mens `Arrangement.Status = Created` |
| Navigasjon | Blokkert manuelt — kun SignalR kan utløse viderekobling |
| Viderekobling | Automatisk navigasjon til Screen 5 (beer[0]) når SignalR sender `ArrangementStatusChanged` med `Started` |

**Backend-feil:**

| Feil | Melding til bruker |
|---|---|
| Lastfeil | Generisk feilmelding |

**SignalR:**

| Event | Handling |
|---|---|
| `ArrangementStatusChanged` → `Started` | Naviger automatisk til Screen 5 for beerIndex = 1 |

---

## Screen 5 — Beer Info (loop, øl-indeks 1–N)

**Rute:** `/arrangements/{id}/beers/{beerIndex}`  
**Shell:** App shell med topplinje

```
┌──────────────────────────────────────────────┐
│  Tasting                          [AppBar]   │
├──────────────────────────────────────────────┤
│                                              │
│  Øl 1 av 3                    [Fremdrift]    │
│                                              │
│  [Ølbilde — hvis tilgjengelig]               │
│  ┌────────────────────────────────────────┐  │
│  │                                        │  │
│  │            [Bilde]                     │  │
│  │                                        │  │
│  └────────────────────────────────────────┘  │
│                                              │
│  Mørk Lager                                  │
│  Hansa Bryggeri · Lager · Mørkt øl           │
│                                              │
│  Et klassisk mørkt lager med hint av         │
│  karamel og ristede malter.                  │
│                                              │
│                     ┌──────────────────────┐ │
│                     │        Neste →       │ │
│                     └──────────────────────┘ │
└──────────────────────────────────────────────┘
```

| Element | Regel |
|---|---|
| Fremdriftsindikator | «Øl X av N» — viser gjeldende øl-indeks og totalt antall |
| Ølbilde | Vises kun hvis tilgjengelig; utelates ellers |
| Innhold | Navn, Beskrivelse, Bryggeri (`Brewery`), Stil (`BeerStyle`), Type (`BeerType`) fra beer snapshot |
| «Neste»-knapp | Alltid aktivert |
| «Neste»-knapp | Navigerer til Screen 6 (Rating for samme øl) |

**Backend-feil:**

| Feil | Melding til bruker |
|---|---|
| Lastfeil | Generisk feilmelding |

**SignalR:** —

---

## Screen 6 — Rating for Beer X (loop)

**Rute:** `/arrangements/{id}/beers/{beerIndex}/rate`  
**Shell:** App shell med topplinje

```
┌──────────────────────────────────────────────┐
│  Tasting                          [AppBar]   │
├──────────────────────────────────────────────┤
│                                              │
│  Øl 1 av 3 — Vurdering           [Fremdrift] │
│                                              │
│  Utseende                        [0.0 – 10]  │
│  ├────────────────────────────────────────┤  │
│  │  ○──────────────────────────           │  │
│  └────────────────────────────────────────┘  │
│                                              │
│  Lukt                            [0.0 – 10]  │
│  ├────────────────────────────────────────┤  │
│  │  ○──────────────────────────           │  │
│  └────────────────────────────────────────┘  │
│                                              │
│  Smak                            [0.0 – 10]  │
│  ├────────────────────────────────────────┤  │
│  │  ○──────────────────────────           │  │
│  └────────────────────────────────────────┘  │
│                                              │
│  Skål                            [0.0 – 10]  │
│  ├────────────────────────────────────────┤  │
│  │  ○──────────────────────────           │  │
│  └────────────────────────────────────────┘  │
│                                              │
│  [Feilmelding — kun synlig ved feil]         │
│                                              │
│                     ┌──────────────────────┐ │
│                     │  Neste → (deaktivert) │ │
│                     └──────────────────────┘ │
└──────────────────────────────────────────────┘
```

| Element | Regel |
|---|---|
| Utseende | Glider 0–10, steg 0.5; viser gjeldende verdi |
| Lukt | Glider 0–10, steg 0.5; viser gjeldende verdi |
| Smak | Glider 0–10, steg 0.5; viser gjeldende verdi |
| Skål | Glider 0–10, steg 0.5; viser gjeldende verdi |
| «Neste»-knapp | **Deaktivert** inntil alle 4 gliders er eksplisitt satt av bruker |
| «Neste»-knapp | Aktivert når alle 4 gliders er eksplisitt satt (0.0 er en gyldig verdi) |
| «Neste»-knapp | Sender inn Rating ved klikk, deretter navigerer til Screen 7 |

**Backend-feil:**

| Feil | Melding til bruker |
|---|---|
| Innsendingsfeil | Inline feilmelding |
| Optimistisk samtidighetskonflikter (409) | «Ratingen ble oppdatert av en annen instans — last siden på nytt.» |

**SignalR:** —

---

## Screen 7 — Results for Beer X (loop)

**Rute:** `/arrangements/{id}/beers/{beerIndex}/results`  
**Shell:** App shell med topplinje

```
┌──────────────────────────────────────────────┐
│  Tasting                          [AppBar]   │
├──────────────────────────────────────────────┤
│                                              │
│  Øl 1 av 3 — Resultat            [Fremdrift] │
│                                              │
│  Din vurdering                               │
│  ┌────────────────────────────────────────┐  │
│  │  Total: 7.5                            │  │
│  └────────────────────────────────────────┘  │
│                                              │
│  Alle deltakeres vurdering                   │
│  ┌────────────────────────────────────────┐  │
│  │  Total: 6.83                           │  │
│  └────────────────────────────────────────┘  │
│                                              │
│  ⏳ Venter på at alle deltakere avgir sin    │
│     rating…                                  │
│  (melding skjules når alle har svart)        │
│                                              │
│                     ┌──────────────────────┐ │
│                     │  Neste → (deaktivert) │ │
│                     └──────────────────────┘ │
└──────────────────────────────────────────────┘
```

| Element | Regel |
|---|---|
| «Din vurdering» | Brukerens egne TotalRating for dette ølet |
| «Alle deltakeres vurdering» | Result score for (Arrangement, Beer), 2 desimaler |
| Ventemelding | Vises mens ikke alle deltakere har levert rating |
| «Neste»-knapp | **Deaktivert** inntil SignalR sender `AllRatingsSubmittedForBeer` |
| «Neste»-knapp | Navigerer til Screen 5 for neste øl, eller Screen 8 hvis siste øl |

**Backend-feil:**

| Feil | Melding til bruker |
|---|---|
| Lastfeil | Generisk feilmelding |

**SignalR:**

| Event | Handling |
|---|---|
| `AllRatingsSubmittedForBeer` | Aktiver «Neste»-knapp |

---

## Screen 8 — Arrangement Results

**Rute:** `/arrangements/{id}/results`  
**Shell:** App shell med topplinje

```
┌──────────────────────────────────────────────┐
│  Tasting                          [AppBar]   │
├──────────────────────────────────────────────┤
│                                              │
│  Sommerfest — Resultater                     │
│                                              │
│  ┌────────────────────────────────────────┐  │
│  │ # │ Øl-navn    │ Bryggeri  │ Rating    │  │
│  ├───┼────────────┼───────────┼───────────┤  │
│  │ 1 │ Pale Ale   │ Nøgne Ø  │ 8.20      │  │
│  │ 2 │ Mørk Lager │ Hansa    │ 6.83      │  │
│  │ 3 │ IPA        │ Lervig   │ 5.50      │  │
│  └────────────────────────────────────────┘  │
│                                              │
│  ┌────────────────────────────────────────┐  │
│  │             Ferdig →                   │  │
│  └────────────────────────────────────────┘  │
│                                              │
└──────────────────────────────────────────────┘
```

| Element | Regel |
|---|---|
| Grid | Sortering etter Result score synkende |
| Grid | Uavgjort-tiebreaker: flest vurderinger → lavest standardavvik → BeerId stigende |
| «#»-kolonne | Rangering (1, 2, 3…) |
| «Rating»-kolonne | Result score, 2 desimaler |
| «Åpne øl»-lenke | Klikk på rad navigerer til Screen 9 for det ølet |
| «Ferdig»-knapp | Navigerer til Screen 3 (tilbake til List Arrangements) |
| Grid | Alltid vist (arrangementet er Completed på dette punktet) |

**Backend-feil:**

| Feil | Melding til bruker |
|---|---|
| Lastfeil | Generisk feilmelding |

**SignalR:** —

---

## Screen 9 — Beer X with Result

**Rute:** `/arrangements/{id}/results/beers/{beerId}`  
**Shell:** App shell med topplinje

```
┌──────────────────────────────────────────────┐
│  Tasting                          [AppBar]   │
├──────────────────────────────────────────────┤
│                                              │
│  Pale Ale                                    │
│                                              │
│  [Ølbilde — hvis tilgjengelig]               │
│  ┌────────────────────────────────────────┐  │
│  │                                        │  │
│  │            [Bilde]                     │  │
│  │                                        │  │
│  └────────────────────────────────────────┘  │
│                                              │
│  Total rating: 8.20                          │
│                                              │
│  Et frisk og humlet pale ale med aromer      │
│  av sitrus og blomst.                        │
│                                              │
│  Nøgne Ø · Pale Ale · Lyst øl               │
│                                              │
│  ┌────────────────────────────────────────┐  │
│  │              ← Lukk                   │  │
│  └────────────────────────────────────────┘  │
│                                              │
└──────────────────────────────────────────────┘
```

| Element | Regel |
|---|---|
| Ølbilde | Vises kun hvis tilgjengelig |
| Total rating | Result score for dette ølet, 2 desimaler |
| Innhold | Beskrivelse, Bryggeri, Stil, Type fra beer snapshot |
| «Lukk»-knapp | Navigerer tilbake til Screen 8 |

**Backend-feil:**

| Feil | Melding til bruker |
|---|---|
| Lastfeil | Generisk feilmelding |

**SignalR:** —
