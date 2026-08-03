# 0022. Snapshot beer metadata at Arrangement Started

**Status:** Accepted

## Context
Katalogmetadata kan endres av admin, men historiske resultater må være sporbare og stabile.

## Decision
Ved overgang til `Started` tas snapshot av relevante beer-metadata (f.eks. navn/stil/type) i arrangementkontekst/resultatgrunnlag.
Resultatvisning for arrangementet bruker snapshot-data, ikke live-katalog.

## Consequences
- Historikk påvirkes ikke av senere katalogendringer.
- Krever snapshot-felt i arrangement/resultatstruktur.
- Øker dataduplisering noe, men gir sterkere revisjonssporbarhet.
- Skal implementeres i `IRequestHandler`-laget og verifiseres med unit tests for handlerlogikken og integrasjonstester for endepunktene.
