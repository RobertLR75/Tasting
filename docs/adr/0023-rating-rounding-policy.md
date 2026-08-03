# 0023. Rating rounding policy for deterministic scoring

**Status:** Accepted

## Context
Uten eksplisitt avrundingsregel kan totalscore og ranking avvike mellom miljøer og implementasjoner.

## Decision
- Bruk `decimal` for ratingberegninger.
- Rund `TotalRating` og resultatsnitt til 2 desimaler.
- Bruk `MidpointRounding.AwayFromZero`.
- Intern urundet verdi kan brukes for tie-break-beregning.

## Consequences
- Deterministiske scorer og stabil presentasjon.
- Mindre risiko for subtile ranking-avvik på grunn av flyttall/avrunding.
- Krever konsekvent implementasjon i både write- og read-path.
