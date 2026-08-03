# 0015. Beer requires Brewery reference; Brewery uses soft delete only

**Status:** Accepted

## Context
Relasjonen mellom beer og brewery, samt livssyklus for brewery, måtte avklares for å sikre dataintegritet mot arrangement/rating.

## Decision
- Hver `Beer` må være knyttet til et `Brewery` (obligatorisk relasjon).
- `Brewery` kan ikke hard-slettes.
- `Brewery` håndteres med soft delete via status `Inactive`.

## Consequences
- Beer-oppretting må validere gyldig og aktiv `BreweryId`.
- Krever foreign key mellom `Beer` og `Brewery` i PostgreSQL, opprettet via FluentMigration.
- Historiske arrangement/rating-data forblir konsistente siden brewery ikke fjernes fysisk.
- Krever eksplisitte regler for om `Inactive` brewery fortsatt kan brukes for nye beers.
- Skal implementeres i `IRequestHandler`-laget og verifiseres med unit tests for handlerlogikken og integrasjonstester for endepunktene.
