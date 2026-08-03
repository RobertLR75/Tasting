# 0020. Beer taxonomy uses concrete models and ID references

**Status:** Accepted

## Context
`BeerTypeModel` som interface er dårlig match for relasjonell persistens og stabil serialisering.

## Decision
- `BeerType` og `BeerStyle` modelleres som konkrete klasser/rader.
- `Beer` lagrer referanser (`BeerStyleId`, `BeerTypeId`) i stedet for å embedde komplette objekter.

## Consequences
- Mer robust serialisering og enklere foreign key-/indeks-/querymønstre i PostgreSQL.
- Krever oppslag/join ved lesing når navn/beskrivelse skal vises.
- Reduserer datadrift ved endring av taxonomy-navn/metadata.
- Skal implementeres i `IRequestHandler`-laget og verifiseres med unit tests for handlerlogikken og integrasjonstester for endepunktene.
