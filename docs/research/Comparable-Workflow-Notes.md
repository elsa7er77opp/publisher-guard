# Comparable Workflow Notes — Publisher Guard

## What to borrow, not copy

Publisher Guard must remain a local delivery-preparation tool. It will not copy visual layouts, branding, cloud actions, marketplace publishing, or account flows from the references below. The useful lessons concern **sequence, wording, and decision clarity**.

| Reference | Observable workflow lesson | Publisher Guard interpretation |
|---|---|---|
| Unity Asset Store publishing workflow | The public flow is stated in order: create draft, validate assets, then upload/submit. Validation is a distinct step before irreversible handoff. | Use four visible local stages: **Choose package → Check it → Resolve items → Create delivery**. The final stage produces a local folder, never an upload or submission. |
| GitHub Desktop review flow | A user sees a change list, chooses what to include, then performs one meaningful action. Technical data exists but selection precedes the commit action. | Show files as **Included by default** and make exclusions clear. Keep hashes/rule IDs in an optional detail area. Do not demand technical interpretation before the user can act. |

## Consequence for the redesign

The current four-column workstation exposes implementation concepts too early: “canonical containment,” “metadata review,” “evidence inspector,” queue internals, rule IDs, and several competing buttons. These remain valuable audit capabilities, but they should appear only when a user opens **Why?** or **Details**.

The primary experience should state one next action at a time. A plain readiness card should answer: *Is this package ready to deliver? If not, what exactly needs my attention?* A finding should present a short explanation and the relevant action, such as **Keep this file out of delivery**, rather than leading with a rule identifier.

## Sources

1. [Unity Manual — Asset package Asset Store publishing workflow](https://docs.unity3d.com/2022.3/Documentation/Manual/asset-store-workflow.html)
2. [GitHub Docs — Committing and reviewing changes in GitHub Desktop](https://docs.github.com/en/desktop/making-changes-in-a-branch/committing-and-reviewing-changes-to-your-project-in-github-desktop)
