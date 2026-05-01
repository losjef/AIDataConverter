# AIDataConverter

AIDataConverter is a .NET 10 MAUI Blazor Hybrid application and conversion engine for transforming vendor CSV/XLSX data into either validated, import-ready files for Epicor Propello POS or comparison-ready views against existing Superior Hardware products.

The product goal is to make vendor onboarding and catalog maintenance repeatable, explainable, and eventually sellable without requiring full-file AI interpretation on every run. The preferred architecture is a memory-first C# pipeline: local rules, learned patterns, and user-reviewed matches resolve as much as possible before any optional AI escalation is considered.

That memory model should include both sides of the mapping problem: vendor/source patterns and the Propello destination schemas themselves. The Propello templates are often the goal vocabulary the engine is trying to satisfy, and related templates can share destination concepts that should be reusable instead of modeled in isolation.

## Product Direction

This repository is being shaped around a commercially viable workflow:

- Deterministic C# services handle parsing, normalization, validation, and replay of learned match rules.
- Human-readable knowledge assets let an experienced operator review and correct matching behavior without editing application code.
- User review should happen before AI when a likely match can be explained clearly.
- AI is an optional fallback for unresolved or low-confidence cases, not the default processing engine.

The software has two primary outcomes:

- Convert a vendor or input file into one of the Propello import templates.
- Display the vendor/input data in a comparison workflow against existing Superior Hardware products.

The final product should be upload-first rather than auto-loading vendor files by default. The user should supply the vendor file or file set for the current workflow, and the app should be prepared to ask clarifying intake questions such as whether the vendor is new, whether multiple files belong to the same workflow, and how those files relate to each other.

Superior Hardware baseline loading should remain available in two modes: manual upload when needed, and optional auto-load when the user has explicitly enabled that behavior as a saved preference.

The comparison workflow is not expected to be one universal screen. Comparison UI will likely need to be developed case by case depending on vendor structure, matching strategy, and the kind of decision the user needs to make.

If this approach works, the product can provide real value even in environments where an AI SDK is unavailable, undesirable, or too expensive to use broadly.

## Current State

The repository currently contains:

- A checked-in .NET 10 MAUI Blazor Hybrid app scaffold in `AIDataConvertor/`
- A solution entry point in `AIDataConvertor.slnx`
- Propello template files in `PropelloTemplates/`
- Vendor source files in `VendorFiles/`
- Superior Hardware baseline inventory exports in `SuperiorHardware/`
- Adaptive semantic learning assets in `AgentAssets/KnowledgeBase/`

The app shell builds, but the core conversion services, template parsers, validation engine, and operator-facing rule management UI are still ahead of the codebase.

## Core Workflow

1. The user selects a target Propello template.
2. The engine reads Propello template metadata from the paired XLSX/CSV reference files.
3. The engine asks any required intake questions and reads the user-supplied vendor source file or file set.
4. The engine applies local memory first: shared concepts, vendor rule sets, defaults, normalization rules, and approved corrections.
5. The engine loads the Superior Hardware baseline either from a user upload or from the user's saved auto-load preference, then checks it for schema readiness, enrichment, linking, overlap risks, and UPC conflicts.
6. The engine presents unresolved or low-confidence matches to the user when practical.
7. Depending on the workflow, the engine either prepares Propello-template output or prepares a comparison-ready view against Superior Hardware products.
8. Only after deterministic rules and user review are exhausted should optional AI assistance be considered.
9. The engine writes validated output or persists comparison decisions and newly approved learning.

## Knowledge Assets

The primary learned-mapping asset is `AgentAssets/KnowledgeBase/semantic_dictionary.json`.

That file now follows an adaptive v2 schema intended to support:

- Human-readable and user-editable rule definitions
- Shared concepts reusable across vendors
- Vendor-specific rule sets
- Confidence and provenance metadata
- Lifecycle tracking for rules that become stale over time
- Cross-vendor seeding for new vendor interpretation

The intent is not just to store aliases, but to create a durable memory layer that can evolve as vendor formats drift.

That memory layer now also has an initial companion asset at `AgentAssets/KnowledgeBase/propello_template_memory.json` to capture Propello template field knowledge and lightweight cross-template relationships so shared destination concepts like product, pricing, and purchasing data can be understood once and reused across related import workflows.

## Adaptive Matching Strategy

Vendor formats will change over time. A long-running feed may rename columns, split fields, or repurpose old headers. Because of that, learned patterns must be adaptable rather than permanent.

The same is true for Superior Hardware baseline exports. Different users may export different column layouts, so the application must verify that required baseline fields are present for the requested workflow before claiming match readiness or output completeness.

The knowledge model should support rule lifecycle states such as:

- `active`
- `deprecated`
- `retired`
- `replaced`

Older rules should not be deleted when they go stale. They should remain traceable so the system can explain prior behavior and avoid relearning the same history.

When a new vendor is introduced, the engine should not start from zero. It should review:

1. Shared concepts
2. Existing vendor patterns
3. User review and correction paths
4. Optional AI assistance only after the earlier steps fail

This allows the product to learn across vendors without immediately paying AI token cost for every new file layout.

## Why This Can Work Without An AI SDK

The strongest product path is to make AI optional rather than foundational.

Many vendor files are repetitive once the business learns their structure. If the system can persist and expose those learned match patterns clearly, then much of the interpretation work becomes a C# problem instead of a token-spend problem.

That creates a better commercial story:

- Lower operating cost
- Better explainability
- Safer user-controlled corrections
- More predictable offline or low-connectivity behavior
- Optional premium AI assistance instead of mandatory AI dependency

## Repository Guide

- `GEMINI.md`: project mission, workflow, and operating model
- `PRD.md`: detailed requirements, constraints, and architecture
- `IMPLEMENTATION_TRACKER.md`: working implementation checklist and phase status
- `DECISION_LOG.md`: architectural decisions and safety constraints
- `AGENTS.md`: repo-specific guidance for coding agents
- `AgentAssets/KnowledgeBase/semantic_dictionary.json`: adaptive semantic memory and matching rules
- `AgentAssets/KnowledgeBase/propello_template_memory.json`: Propello goal-schema memory and lightweight template relationship knowledge
- `AgentAssets/KnowledgeBase/baseline_schema_rules.json`: data-driven baseline workflow requirements and dynamic time-series detection rules
- `AgentAssets/KnowledgeBase/manual_links.json`: user-approved manual cross-reference registry for exceptions that should not be guessed automatically
- `AgentAssets/KnowledgeBase/README.md`: user manual for the editable knowledge-base assets and how to maintain them

## Build

The checked-in Windows build task uses:

```powershell
dotnet build AIDataConvertor/AIDataConvertor.csproj -f net10.0-windows10.0.19041.0 -c Debug
```

## Near-Term Priorities

1. Implement Propello template parsing and validation services in C#.
2. Create the runtime models and repository layer for the adaptive semantic memory schema.
3. Expose match provenance, confidence, and edit/approval flows in the UI.
4. Add selective AI escalation only after local rules and user review paths are in place.

## README Maintenance

This README should be updated whenever any of the following change materially:

- Product positioning or sellable product direction
- Core workflow order
- AI vs non-AI execution strategy
- Knowledge asset ownership or schema expectations
- Knowledge-base manual coverage for editable memory files
- Build/run instructions
- Major implementation milestones

Treat this file as the repo's public-facing technical entry point. It should stay aligned with `GEMINI.md`, `PRD.md`, `DECISION_LOG.md`, and the current knowledge asset structure.