# Triage Labels

Issues in this repo carry labels on two independent axes. A single issue normally has one label from each.

- **Triage role** — how well understood the issue is, and who should pick it up. Owned by `/triage`.
- **Workflow stage** — where the work has reached in the pipeline. Owned by `/analyse-ticket` and `/implement-ticket`.

## Triage roles

The skills speak in terms of five canonical triage roles. This repo uses the canonical strings verbatim, so no translation is needed.

| Label in mattpocock/skills | Label in our tracker | Meaning                                  |
| -------------------------- | -------------------- | ---------------------------------------- |
| `needs-triage`             | `needs-triage`       | Maintainer needs to evaluate this issue  |
| `needs-info`               | `needs-info`         | Waiting on reporter for more information |
| `ready-for-agent`          | `ready-for-agent`    | Fully specified, ready for an AFK agent  |
| `ready-for-human`          | `ready-for-human`    | Requires human implementation            |
| `wontfix`                  | `wontfix`            | Will not be actioned                     |

When a skill mentions a role (e.g. "apply the AFK-ready triage label"), use the corresponding label string from this table.

This repo splits AFK-readiness in two, because two different agents pick work up at two different points:

| Label                     | Meaning                                        |
| ------------------------- | ---------------------------------------------- |
| `ready-for-agent-analyse` | Ready for `/analyse-ticket` to turn into a spec |
| `ready-for-agent`         | A ticket, ready for `/implement-ticket` to build |

Both are gates, and each skill refuses an issue that lacks its own. `/analyse-ticket` takes `ready-for-agent-analyse` off the parent once the spec and tickets are published, so an analysed issue is never analysed twice.

`ready-for-agent` belongs to tickets alone. It is the only thing marking an issue as directly buildable, so the parent issue and the spec issue must never carry it — they get `ready for development` instead.

## Workflow stages

Beyond triage, this repo moves an issue through a sequence of its own:

| Label                   | Meaning                                      |
| ----------------------- | -------------------------------------------- |
| `ready for development` | Specified and ready to be picked up          |
| `in progress`           | Actively being implemented                   |
| `verification`          | Implemented, awaiting review or verification |

Note the exact spelling of these: lowercase, with spaces.

## Which labels each skill applies

- `/triage` moves an issue between the five triage roles.
- `/analyse-ticket` picks up issues labelled `ready-for-agent-analyse`. It swaps that label on the parent for `ready for development`, gives the spec issue `ready for development`, and every published ticket `ready-for-agent`.
- `/implement-ticket` picks up tickets labelled `ready-for-agent`. It moves the ticket from `ready-for-agent` to `verification`. The parent moves to `in progress` while sibling tickets remain unbuilt, and reaches `verification` only when the last one is done.

## Labels outside both axes

`bug`, `documentation`, `duplicate`, `enhancement`, `good first issue`, `help wanted`, `invalid` and `question` are GitHub's defaults. They categorise; they do not move an issue through triage or the pipeline.

`question` predates `needs-info` and means much the same thing. Prefer `needs-info` — it is the one the skills read.

## Retired labels

`ai` ("can be picked up by AI") said the same thing as the `ready-for-agent` triage role, so the skills now apply `ready-for-agent` alone. Older issues may still carry `ai`; treat it as no longer meaningful and do not add it to new issues.

`development` marked a ticket as buildable, which is exactly what `ready-for-agent` now says. One gate label per skill is enough, so `development` is gone.
