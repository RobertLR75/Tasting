# 0008. Admin controls arrangement and membership mutations

**Status:** Proposed

## Context
Tilgangsmodell for muterende endepunkter måtte presiseres.

## Decision
Kun administrator kan:
- opprette arrangement
- legge til beers i arrangement
- legge til participants i arrangement

## Consequences
- Endepunkter må håndheve rollekrav (`Admin`) eksplisitt.
- Klient må håndtere `403 Forbidden` for ikke-admins.
- Trenger eksplisitt avklaring for `Start`, `Rating` og `Results` lesing/skriving.
