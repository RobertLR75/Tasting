# Wireframes — Implementeringsplan

## Mål

Produser et samlet, implementerbart wireframe-dokument for admin-backoffice som dekker `Login`, `Arrangement`, `Breweries` og `Users`, og som kan brukes direkte av parallelle frontend- og backend-spor.

## Styrende beslutninger

| Beslutning | Valg |
|---|---|
| Wireframe-stil | Detaljerte low-fidelity wireframes |
| Layout | Eksisterende admin-shell med toppbar og venstremeny |
| Login | Egen avskallet skjerm uten shell |
| Dokumentstruktur | Ett samlet dokument med én seksjon per skjerm |
| Annotasjoner | Domene- og tilstandsnotater inkluderes per skjerm |
| Autoritet | Wireframene skal uttrykke backend-regler, ikke oppfinne UI-lokal forretningslogikk |

---

## Leveranser

1. **Samlet wireframe-dokument**
   - Ett dokument i repoet med alle adminskjermene
   - ASCII-/Markdown-baserte skisser eller Mermaid der det er hensiktsmessig
   - Én seksjon per skjermbilde

2. **Skjermkatalog**
   - Login
   - Arrangements liste
   - Arrangement edit
   - Arrangement add beers
   - Arrangement add participants
   - Arrangement change status
   - Breweries liste
   - Brewery add
   - Brewery beers liste
   - Brewery add beer
   - Users liste
   - User add
   - User edit
   - User edit role
   - User edit status

3. **Annotasjonsmodell**
   - Synlige handlinger
   - Disabled states
   - Tomtilstander
   - Søkeatferd
   - Backend-feil som må kunne vises
   - Statusgating og rollebegrensninger

---

## Arbeidsstrømmer som kan kjøres parallelt

| Spor | Avhenger av | Leveranse |
|---|---|---|
| A. Informasjonsarkitektur | Ingen | Endelig skjermliste, navigasjon og gruppering |
| B. Arrangement-wireframes | A | Alle arrangementskisser med statusannotasjoner |
| C. Brewery-wireframes | A | Brewery- og beer-skisser med katalogannotasjoner |
| D. User-wireframes | A | User-skisser med rolle/status-annotasjoner |
| E. Login + shell-wireframes | A | Login-skjerm og shell-mønster |
| F. Sammenstilling + konsistenssjekk | B + C + D + E | Ett samlet dokument med ensartet språk og struktur |

---

## Spor A — Informasjonsarkitektur

### Oppgaver

1. Fastslå endelig skjermliste basert på ADR-er og implementeringsplanen.
2. Definer hvordan skjermene grupperes under shell-navigasjonen:
   - Arrangements
   - Users
   - Breweries
3. Marker hvilke skjermbilder som er:
   - listevisninger
   - detalj-/editvisninger
   - valg-/medlemskapsskjermbilder
   - statusskjermbilder
4. Definer fast wireframe-mal som alle seksjoner følger:
   - formål
   - skisse
   - primære handlinger
   - tilstandsregler
   - backend-avhengigheter

### Resultat

Et felles rammeverk som de andre sporene kan fylle ut uten å divergere i struktur eller begrepsbruk.

---

## Spor B — Arrangement-wireframes

### Skjermer

1. **Arrangements liste**
   - Tabell med navn, dato, status og handlinger
   - Handlingskolonne styrt av status

2. **Arrangement edit**
   - Felter for navn, dato, beskrivelse
   - Oppdater-knapp
   - Konfliktflate når arrangement ikke lenger er `Created`

3. **Arrangement add beers**
   - Søkeområde
   - Liste over beers med brewery-tilhørighet
   - Multi-select
   - Disabled markering for beers som allerede er knyttet til arrangementet

4. **Arrangement add participants**
   - Søkeområde
   - Liste over aktive users
   - Multi-select
   - Disabled markering for eksisterende participants

5. **Arrangement change status**
   - Current status
   - Bare gyldige neste statuser
   - Varseltekst for irreversible/operative konsekvenser der relevant

### Annotasjoner som må inn

- Bare `Created` har edit/add-actions
- `Canceled -> Created` er lov
- `Completed` har ingen neste handling
- Bare aktive users kan velges
- Bare aktive breweries/beers kan velges

---

## Spor C — Brewery-wireframes

### Skjermer

1. **Breweries liste**
   - Søkeinput på navn
   - Listevisning
   - Add Brewery-knapp
   - Lenke til beers for valgt brewery

2. **Brewery add**
   - Navn
   - Land
   - Opprett-knapp
   - Konfliktmelding for duplikat `(Name, Country)`

3. **Brewery beers liste**
   - Søkeinput på beer-navn
   - Liste over beers for valgt brewery
   - Add Beer-knapp

4. **Brewery add beer**
   - Felter for beer-opprettelse
   - Knytting til valgt brewery
   - Konfliktflate ved duplikatnavn

### Annotasjoner som må inn

- Brewery-søk er bare på navn
- Beer-navn må være unikt per brewery også mot inactive beers
- Arrangement-relaterte valg skal ikke introduseres her

---

## Spor D — User-wireframes

### Skjermer

1. **Users liste**
   - Søkefelt
   - Liste over alle users
   - Handlinger for edit, role, status
   - Add User-knapp

2. **User add**
   - Navn
   - E-post
   - Eventuell rolle hvis create-flowen skal eksponere den
   - Opprett-knapp

3. **User edit**
   - Navn
   - E-post
   - Lagre-knapp

4. **User edit role**
   - Valg mellom `Admin` og `User`
   - Tilstandsnotat for blokkert rolleendring ved inactive user

5. **User edit status**
   - Valg mellom `Active` og `Inactive`
   - Tilstandsnotat for blokkert deaktivering av siste aktive admin

### Annotasjoner som må inn

- Listen viser både aktive og inactive users
- Søk matcher navn og e-post
- Rolleendring krever aktiv user
- Siste aktive admin kan ikke nedgraderes eller deaktiveres

---

## Spor E — Login + shell-wireframes

### Skjermer

1. **Login**
   - E-post
   - Passord
   - Login-knapp
   - Generisk feilflate

2. **Shell pattern**
   - Toppbar
   - Venstremeny
   - Innholdsområde
   - Aktiv navigasjonstilstand

### Annotasjoner som må inn

- Login er eneste skjerm uten shell
- Shell brukes av alle autentiserte adminskjermer
- Menyen må støtte minst Arrangements, Users og Breweries

---

## Spor F — Sammenstilling og konsistenssjekk

### Oppgaver

1. Samle alle skjermseksjoner i ett dokument.
2. Normaliser begrepsbruk mot `CONTEXT.md` og relevante ADR-er.
3. Sørg for at alle skjermene følger samme layoutnøkler:
   - sideoverskrift
   - handlinger
   - søk
   - liste/tabell
   - feilmelding
   - domenenotater
4. Kryssjekk at ingen wireframe motsier backend-reglene.
5. Legg inn en kort navigasjonsoversikt først i dokumentet.

### Resultat

Et konsistent wireframe-dokument som kan brukes både som designskisse og som implementasjonskontrakt.

---

## Foreslått rekkefølge

1. Spor A
2. Spor B, C, D og E parallelt
3. Spor F til slutt

---

## Ferdigdefinisjon

Arbeidet er ferdig når:

1. Alle avtalte adminskjermer er skisset i ett dokument.
2. Hver skjerm viser primære handlinger, søk, lister/former og feilflater.
3. Hver skjerm annoterer relevante domene- og statusregler.
4. Wireframene kan brukes direkte av parallelle utviklingsspor uten nye avklaringer om skjermstruktur.
