# 0020. Beer taxonomy uses concrete models and ID references

**Status:** Accepted

## Context
`BeerTypeModel` som interface er dårlig match for MongoDB-dokumentlagring og stabil serialisering.

## Decision
- `BeerType` og `BeerStyle` modelleres som konkrete klasser/dokumenter.
- `Beer` lagrer referanser (`BeerStyleId`, `BeerTypeId`) i stedet for å embedde komplette objekter.

## Consequences
- Mer robust serialisering og enklere indeks-/querymønstre i MongoDB.
- Krever oppslag/join-lignende berikelse ved lesing når navn/beskrivelse skal vises.
- Reduserer datadrift ved endring av taxonomy-navn/metadata.
