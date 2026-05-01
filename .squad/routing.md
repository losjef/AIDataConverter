# Work Routing

How to decide who handles what.

## Routing Table

| Work Type | Route To | Examples |
|-----------|----------|----------|
| Architecture & project structure | Morpheus | .sln/.csproj setup, MAUI/Blazor app boundaries, pattern decisions |
| Vendor file parsing & normalization | Oracle | Reading XLSX/CSV from VendorFiles/, alias mapping |
| Inventory cross-reference | Oracle | New vs. existing items, UPC conflict detection |
| Semantic dictionary updates | Oracle | Adding/updating learned column aliases |
| C# and MAUI/Blazor implementation | Neo | Conversion services, app wiring, Razor components, file import flows |
| UI workflow and interaction design | Mouse | Import/review/export flows, navigation, screen states |
| Validation presentation and UX copy | Mouse | Error summaries, ambiguity prompts, review affordances |
| UI preparation for service contracts | Mouse | View-model needs, file upload affordances, review grids |
| Propello schema analysis | Switch | Template metadata, mandatory fields, UOM tables |
| Field mapping documentation | Switch | Source-to-target column mapping, constraint docs |
| Test authorship & validation | Trinity | Unit tests, integration tests, edge cases |
| Code review | Morpheus | Review PRs, check quality, suggest improvements |
| Scope & priorities | Morpheus | What to build next, trade-offs, decisions |
| Session logging | Scribe | Automatic — never needs routing |

## Issue Routing

| Label | Action | Who |
|-------|--------|-----|
| `squad` | Triage: analyze issue, assign `squad:{member}` label | Lead |
| `squad:{name}` | Pick up issue and complete the work | Named member |

### How Issue Assignment Works

1. When a GitHub issue gets the `squad` label, the **Lead** triages it — analyzing content, assigning the right `squad:{member}` label, and commenting with triage notes.
2. When a `squad:{member}` label is applied, that member picks up the issue in their next session.
3. Members can reassign by removing their label and adding another member's label.
4. The `squad` label is the "inbox" — untriaged issues waiting for Lead review.

## Rules

1. **Eager by default** — spawn all agents who could usefully start work, including anticipatory downstream work.
2. **Scribe always runs** after substantial work, always as `mode: "background"`. Never blocks.
3. **Quick facts → coordinator answers directly.** Don't spawn an agent for "what port does the server run on?"
4. **When two agents could handle it**, pick the one whose domain is the primary concern.
5. **UI workflow work pairs Mouse with Neo.** Mouse defines the interaction model and review states; Neo implements the app surface and contracts.
6. **"Team, ..." → fan-out.** Spawn all relevant agents in parallel as `mode: "background"`.
7. **Anticipate downstream work.** If a feature is being built, spawn the tester to write test cases from requirements simultaneously.
8. **Issue-labeled work** — when a `squad:{member}` label is applied to an issue, route to that member. The Lead handles all `squad` (base label) triage.
