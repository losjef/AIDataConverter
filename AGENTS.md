# AIDataConverter Agent Guide

## Purpose
- This repository currently holds project requirements, source data, Propello template references, and AI knowledge assets for an Epicor Propello conversion engine.
- Treat the repository as a data-and-spec workspace first. A .NET MAUI Blazor scaffold exists, but the core conversion engine is still mostly ahead of the codebase.

## First Files To Read
- [GEMINI.md](./GEMINI.md): project mission, directory roles, and end-to-end conversion workflow.
- [PRD.md](./PRD.md): functional requirements, architectural constraints, and current priorities.
- [IMPLEMENTATION_TRACKER.md](./IMPLEMENTATION_TRACKER.md): working implementation checklist and current phase status.
- [DECISION_LOG.md](./DECISION_LOG.md): architectural rationale and safety constraints.
- [.squad/identity/now.md](./.squad/identity/now.md): current focus and short resume anchor for the next session.
- [.squad/identity/resume-next.md](./.squad/identity/resume-next.md): detailed end-of-day resume point, exact next step, and first files to open.
- [AgentAssets/KnowledgeBase/semantic_dictionary.json](./AgentAssets/KnowledgeBase/semantic_dictionary.json): adaptive v2 semantic rules, lifecycle metadata, shared concepts, and defaults.
- [AgentAssets/KnowledgeBase/propello_template_memory.json](./AgentAssets/KnowledgeBase/propello_template_memory.json): Propello goal-schema memory, shared destination concepts, and lightweight template relationships.
- [AgentAssets/KnowledgeBase/baseline_schema_rules.json](./AgentAssets/KnowledgeBase/baseline_schema_rules.json): baseline workflow requirements and dynamic-column detection rules stored as editable data.
- [AgentAssets/KnowledgeBase/manual_links.json](./AgentAssets/KnowledgeBase/manual_links.json): approved manual cross-reference registry for user-resolved linking exceptions.
- [AgentAssets/KnowledgeBase/README.md](./AgentAssets/KnowledgeBase/README.md): user-facing manual for reading and editing the knowledge-base assets.

## Working Rules
- Follow the no-assumption mandate. If a field mapping, item match, or vendor-specific transformation is ambiguous, stop and ask the user instead of guessing.
- When ending the day or handing off across machines, update `.squad/identity/now.md` and `.squad/identity/resume-next.md` with a concrete resume point before stopping.
- Cross-machine handoff is not complete until the intended changes are committed, `git pull --rebase` has been attempted, and `git push` has been attempted.
- Preserve Propello header integrity. Output columns must match the selected template exactly.
- Treat Propello XLSX templates as the metadata source of truth and the paired CSVs as structural examples.
- When linking vendor items to Superior Hardware inventory, do not auto-link overlapping IDs when UPCs disagree.
- For Midwest item numbers, standardize to `M` plus 5 digits when implementing or testing mapping logic.
- Review `shared_concepts` and existing vendor `rule_sets` before proposing a new vendor-specific rule or escalating to AI.
- When a learned pattern goes stale, mark it deprecated, retired, or replaced in the JSON instead of deleting the trail of prior learning.
- Prefer data-driven configuration for changeable links, parameters, and relationship rules instead of hard-coding them in C# when a checked-in knowledge asset can own them safely.

## Repository Layout
- `PropelloTemplates/`: target import schemas and sample structures.
- `VendorFiles/`: raw vendor source files grouped by vendor and feed type.
- `SuperiorHardware/`: current baseline inventory exports used for enrichment and item matching.
- `AgentAssets/KnowledgeBase/`: reusable semantic mappings and other learned data.

## Current Engineering State
- The repository now includes a .NET 10 MAUI Blazor app scaffold at `AIDataConvertor/`, plus the solution file `AIDataConvertor.slnx`.
- The current app surface is still template-level scaffolding. Core conversion services, template parsing, and automated tests are not implemented yet.
- Do not invent additional build, test, or run commands beyond the checked-in .NET solution state. Prefer reading the solution and project files first.
- The next practical milestone is still Phase 2 in [IMPLEMENTATION_TRACKER.md](./IMPLEMENTATION_TRACKER.md): implement template parsing and validation logic behind the app shell.

## Change Guidance
- Prefer small, traceable updates that keep documentation, code, and knowledge-base files aligned.
- If you create or update end-of-day handoff state, keep the named next step narrow and list the exact files to open first.
- If product direction, workflow order, build instructions, or knowledge asset ownership changes, update `README.md` along with the deeper project docs.
- If you change learned mappings or vendor normalization rules, update the relevant JSON asset and any linked documentation that names it as the source of truth.
- If you change an editable knowledge asset materially, update its supporting manual in `AgentAssets/KnowledgeBase/README.md` in the same change.
- Keep `semantic_dictionary.json`, `propello_template_memory.json`, `baseline_schema_rules.json`, and `manual_links.json` human-readable. Preserve lifecycle status, provenance, shared concepts, relationship context, and user-review traceability when editing them.
- Link to existing project docs instead of duplicating detailed requirements into new customization files.
