# 0047. Active arrangements allow participant self-join

**Status:** Accepted

## Context
ADR-0008 reserves membership mutations for administrators, but the participant product flow requires authenticated users to discover committed arrangements and join without admin intervention. Reusing the admin endpoint would allow clients to choose another user's identity and would blur the admin and participant authorization boundaries.

## Decision
Any authenticated user (`Admin` or `User`) may discover arrangements in `Active` status and self-join them through participant-specific API endpoints. The backend derives the user identity from the authentication token, accepts self-join only during `Active`, and preserves the membership uniqueness invariant. Admin-managed membership during `Created` remains unchanged.

This decision supersedes ADR-0008 only for self-join during `Active`.

## Consequences
- Participant clients cannot nominate another user when joining.
- Duplicate joins and joins outside `Active` return conflict errors from backend rules.
- Discovery responses expose only data needed to choose an arrangement and do not expose beer or participant membership details.
- Admin-managed membership and self-join remain distinct workflows with separate endpoints and rules.
