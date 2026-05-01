# Resume Next

## Session Anchor

- Date: 2026-05-01
- Branch: `master`
- Last committed anchor before this session's uncommitted work: `ba840df6314187562e2ede90b07390737d5d9c55`
- Last successful validation before end-of-day sync: `dotnet build AIDataConvertor/AIDataConvertor.csproj -f net10.0-windows10.0.19041.0 -c Debug`

## What Was Completed

- Expanded the local knowledge-base architecture beyond `semantic_dictionary.json`.
- Added `propello_template_memory.json`, `baseline_schema_rules.json`, and `manual_links.json` as checked-in editable knowledge assets.
- Added typed C# document models and file-backed repositories for Propello template memory, baseline schema rules, and manual links.
- Wired those assets into MAUI packaging and dependency injection.
- Added the first operator manual for editable knowledge assets at `AgentAssets/KnowledgeBase/README.md`.
- Updated project docs so editable knowledge assets must keep their supporting manual in sync.

## Files To Open First Tomorrow

- `AIDataConvertor/Services/KnowledgeBase/MilwaukeeComparisonPreviewService.cs`
- `AIDataConvertor/Components/Pages/Home.razor`
- `AIDataConvertor/Services/KnowledgeBase/ManualLinksRepository.cs`
- `AgentAssets/KnowledgeBase/manual_links.json`
- `IMPLEMENTATION_TRACKER.md`

## Exact Next Step

Integrate `IManualLinksRepository` into the first real comparison or matching flow, starting with Milwaukee preview resolution, so user-approved exception links can influence local matching before any future AI escalation or broader UI work.

## Why This Is Next

- The manual links asset and repository are scaffolded but not yet consumed by runtime behavior.
- Milwaukee comparison logic already has deterministic item-number and UPC matching, so it is the narrowest real place to layer in approved exception links.
- `Home.razor` already exposes the diagnostic prototype surface and can show the first visible proof that manual links are being read.

## Suggested First Edit Slice

1. Add `IManualLinksRepository` to `MilwaukeeComparisonPreviewService`.
2. Load the manual links document during preview generation.
3. Apply only explicit, user-approved manual links when deterministic matching does not already resolve a row safely.
4. Surface the match basis clearly so manual-link usage is visible in the preview output.

## Deferred Work

- No UI editing workflow exists yet for manual links.
- Propello template parsing and validation services are still the broader Phase 2 milestone.
- The semantic/memory status page does not yet expose the new repositories directly.

## Cross-Machine Resume Rule

Before starting new work on another machine:

1. Run `git status --short`.
2. If clean, run `git pull`.
3. Read `.squad/identity/now.md`.
4. Read this file.
5. Open the files listed above and begin with the exact next step.