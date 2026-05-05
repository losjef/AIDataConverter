# Resume Next

## Session Anchor

- Date: 2026-05-05
- Branch: `master`
- Last committed anchor before this handoff update: `d74b3ac50bce3f354f420806bd37347226e28a6e`
- Last successful validation before end-of-day sync:
	- `dotnet test AIDataConvertor.Tests/AIDataConvertor.Tests.csproj -f net10.0-windows10.0.19041.0`
	- `dotnet build AIDataConvertor/AIDataConvertor.csproj -f net10.0-windows10.0.19041.0 -c Debug`

## What Was Completed

- Integrated manual-link fallback into `MilwaukeeComparisonPreviewService` so active Milwaukee-to-IMAP manual links are used only after deterministic matching fails to resolve safely.
- Updated the Milwaukee comparison text in `Home.razor` so the fallback behavior is visible in the UI surface.
- Added a focused `AIDataConvertor.Tests` project with xUnit coverage for deterministic-match precedence, manual-link fallback success, and manual-link conflict handling.
- Added the new test project to `AIDataConvertor.slnx` and generalized `.gitignore` so all .NET `bin` and `obj` folders are ignored.
- Fixed the VS Code Windows launch configuration by replacing the hardcoded broken path with a project-based auto-launch entry plus a current-machine ARM64 fallback label.

## Exact Files Changed Today

- `.gitignore`
- `.vscode/launch.json`
- `AIDataConvertor.slnx`
- `AIDataConvertor/Components/Pages/Home.razor`
- `AIDataConvertor/Services/KnowledgeBase/MilwaukeeComparisonPreviewService.cs`
- `AIDataConvertor.Tests/AIDataConvertor.Tests.csproj`
- `AIDataConvertor.Tests/Models/KnowledgeBase/RuleProvenance.cs`
- `AIDataConvertor.Tests/Services/KnowledgeBase/MilwaukeeComparisonPreviewServiceTests.cs`

## Files To Open First Tomorrow

- `AIDataConvertor/Services/KnowledgeBase/PropelloTemplateMemoryRepository.cs`
- `AIDataConvertor/Models/KnowledgeBase/PropelloTemplateMemoryDocument.cs`
- `PropelloTemplates/ProductImport.csv`
- `PropelloTemplates/ProductImport.xlsx`
- `IMPLEMENTATION_TRACKER.md`
- `PRD.md`

## Exact Next Step

Implement the first Propello template-reader service behind the existing MAUI shell, starting with exact CSV header extraction for a selected template and using `ProductImport` as the first proof path.

## Why This Is Next

- Phase 2 template parsing and validation is still the highest-priority unfinished milestone in the tracker.
- The template files are present in `PropelloTemplates/`, and CSV header extraction is the narrowest concrete parsing slice before the richer XLSX metadata pass.
- The existing Propello template memory repository is already in place, so the new reader can be added next to a familiar, local owning abstraction instead of starting from a blank area of the codebase.

## Suggested First Edit Slice

1. Add a focused service interface and implementation that reads the header row from a Propello template CSV.
2. Start with `ProductImport.csv` as the first proof path and return the exact ordered headers.
3. Keep the service independent of Razor pages so it can be validated from tests first.
4. Add a narrow test that proves the extracted header count and first/last header positions for `ProductImport.csv`.

## Open Risks And Deferred Work

- There is no Propello template parsing or validation service yet; Phase 2 has not started beyond memory scaffolding.
- The current Windows launch setup is stable through the project-based auto entry, but explicit RID-specific x64 launch tasks were not viable in this SDK/feed combination.
- The Milwaukee manual-link tests use an isolated source-linked harness instead of a direct MAUI app-project reference because the MAUI resource pipeline introduced duplicate resizetizer outputs in the test project.
- No runtime UI exercise beyond successful build/launch-path verification was captured for the Milwaukee fallback behavior.

## Cross-Machine Resume Rule

Before starting new work on another machine:

1. Run `git status --short`.
2. If clean, run `git pull`.
3. Read `.squad/identity/now.md`.
4. Read this file.
5. Open the files listed above and begin with the exact next step.