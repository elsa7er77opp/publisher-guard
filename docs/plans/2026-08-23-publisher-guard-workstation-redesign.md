# Publisher Guard Workstation Redesign — Draft

## Objective

Replace the rejected panel-first Alpha screen with a **professional full Workstation** that makes local package delivery understandable to a normal creator. This is not a wizard. It keeps the strength of a multi-panel desktop tool but makes one decision visible at a time.

## What failed in the first screen

The prior version exposed too much engineering vocabulary before a user had even selected a folder. Its icon rail had destinations without useful content; its header offered competing commands; its center area began with rule IDs, zero counters, and a table taxonomy; its inspector and queue consumed permanent space while presenting placeholder prose. A user could not immediately answer the only questions that matter: **What package am I checking? Is it ready? What do I need to do next?**

## New composition

| Region | Role | Default language |
|---|---|---|
| Compact command strip | Globe-L signature, current package name, local-only state, one primary command | “Choose package” / “Check another package” |
| Package Library | The package root plus actual files grouped as All files, Needs attention, and Kept out | File names and simple status labels |
| Review Canvas | The central decision surface. It displays a readiness summary first, then the actionable findings list. | “Ready to deliver”, “Needs attention”, “Keep out of delivery” |
| Delivery Details | Contextual explanation for the selected finding or delivery plan. Technical evidence is collapsed by default. | “Why is this here?” / “What will be created?” |
| Delivery Activity | A low, compact operational strip. It stays quiet until a delivery is running or complete. | “Checking files”, “Creating delivery”, “Delivery ready” |

The left icon rail is removed. It does not help this single-purpose product. The main grid begins with the Package Library, so real package context appears immediately.

## Four visible stages, no four-page wizard

The canvas has a small stage rail that communicates progress without forcing navigation:

1. **Choose package**: Select source folder and delivery location.
2. **Check it**: Inventory and preflight run locally.
3. **Resolve items**: Findings are explained as short human sentences. A user can keep a problem file out of the delivery; technical evidence stays available under “Details”.
4. **Create delivery**: A single delivery button becomes available only when no open finding remains. The right panel tells the user exactly what local files will be generated.

## Interaction and data rules

The central screen must never invent a count, status, file, or evidence item. Its summary binds directly to the existing `PackageWorkflowSnapshot`, issue collection, inclusion decisions, and build queue. Excluding a finding re-runs preflight as it does today, then updates the stage and readiness summary. Building remains blocked by any open finding, uses the existing bounded local queue, and retains cancellation safety.

## New ViewModel projection tests before implementation

1. An empty workstation presents **Choose package** as the sole next action and no fabricated statistics.
2. An authorized clean package projects **Ready to deliver** and enables the build action.
3. A package with findings projects **Needs attention**, a truthful count, and a plain-language next action.
4. Selecting a finding fills contextual Delivery Details; hiding technical detail does not remove evidence data.
5. Excluding a finding refreshes the readiness state and keeps the decision visible in the package grouping and evidence ledger.
6. A running or cancelled build projects an Activity status without changing the underlying local safety gate.

## Visual direction

Use a narrow graphite command strip and a subtle midnight background. Amber belongs only to an attention state and delivery gating; cool blue marks evidence and selected context. Globe-L stays small and disciplined in the command strip; its larger 3D presence is restricted to Splash. The main canvas is a real data/work surface, not a marketing hero or a grid of generic cards.
