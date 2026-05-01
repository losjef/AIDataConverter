# AIDataConverter Agent Guide

## Purpose
- This repository currently holds project requirements, source data, Propello template references, and AI knowledge assets for an Epicor Propello conversion engine.
- Treat the repository as a data-and-spec workspace first. A .NET MAUI Blazor scaffold exists, but the core conversion engine is still mostly ahead of the codebase.

## First Files To Read
- [GEMINI.md](./GEMINI.md): project mission, directory roles, and end-to-end conversion workflow.
- [PRD.md](./PRD.md): functional requirements, implementation phases, and current priorities.
- [DECISION_LOG.md](./DECISION_LOG.md): architectural rationale and safety constraints.
- [AgentAssets/KnowledgeBase/semantic_dictionary.json](./AgentAssets/KnowledgeBase/semantic_dictionary.json): current vendor header aliases and defaults.

## Working Rules
- Follow the no-assumption mandate. If a field mapping, item match, or vendor-specific transformation is ambiguous, stop and ask the user instead of guessing.
- Preserve Propello header integrity. Output columns must match the selected template exactly.
- Treat Propello XLSX templates as the metadata source of truth and the paired CSVs as structural examples.
- When linking vendor items to Superior Hardware inventory, do not auto-link overlapping IDs when UPCs disagree.
- For Midwest item numbers, standardize to `M` plus 5 digits when implementing or testing mapping logic.

## Repository Layout
- `PropelloTemplates/`: target import schemas and sample structures.
- `VendorFiles/`: raw vendor source files grouped by vendor and feed type.
- `SuperiorHardware/`: current baseline inventory exports used for enrichment and item matching.
- `AgentAssets/KnowledgeBase/`: reusable semantic mappings and other learned data.

## Current Engineering State
- The repository now includes a .NET 10 MAUI Blazor app scaffold at `AIDataConvertor/`, plus the solution file `AIDataConvertor.slnx`.
- The current app surface is still template-level scaffolding. Core conversion services, template parsing, and automated tests are not implemented yet.
- Do not invent additional build, test, or run commands beyond the checked-in .NET solution state. Prefer reading the solution and project files first.
- The next practical milestone is still Phase 2 in [PRD.md](./PRD.md): implement template parsing and validation logic behind the app shell.

## Change Guidance
- Prefer small, traceable updates that keep documentation, code, and knowledge-base files aligned.
- If you change learned mappings or vendor normalization rules, update the relevant JSON asset and any linked documentation that names it as the source of truth.
- Link to existing project docs instead of duplicating detailed requirements into new customization files.
