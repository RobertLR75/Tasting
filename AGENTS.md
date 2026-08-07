# Project Instructions

- For every new Codex conversation/task in this project, create or switch to a dedicated branch before making changes.
- Use the `codex/` prefix for these branches unless the user explicitly asks for a different branch name.

## Agent skills

### Issue tracker

Issues live in GitHub Issues for `RobertLR75/Tasting`. See `docs/agents/issue-tracker.md`.

### Triage labels

Two axes: the standard five triage roles (`needs-triage`, `needs-info`, `ready-for-agent`, `ready-for-human`, `wontfix`) with `ready-for-agent-analyse` added as the gate for analysis, plus this repo's workflow stages (`ready for development`, `in progress`, `verification`). See `docs/agents/triage-labels.md`.

### Domain docs

Single-context layout — one `CONTEXT.md` and `docs/adr/` at the repo root. See `docs/agents/domain.md`.
