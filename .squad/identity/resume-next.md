# Resume Next

## Session Anchor

- Date: 2026-05-05
- Branch: `master`
- Last committed anchor before this handoff update: `ed0f8a57205238b38a2162aeff1484b1cb4aa051`
- Last successful validation before end-of-day sync:
	- `dotnet test AIDataConvertor.Tests/AIDataConvertor.Tests.csproj -f net10.0-windows10.0.19041.0 --filter PropelloTemplateCsvHeaderReaderTests`
	- `build-windows` task (`dotnet build AIDataConvertor/AIDataConvertor.csproj -f net10.0-windows10.0.19041.0 -c Debug`)

## What Was Completed

- Added a focused `IPropelloTemplateCsvHeaderReader` service and `PropelloTemplateCsvHeaderReader` implementation to read exact ordered headers from checked-in Propello CSV templates.
- Added the `PropelloTemplateCsvHeaders` result model and wired the new reader into `MauiProgram` so the service is available behind the existing MAUI shell.
- Added a focused source-linked xUnit test that proves `ProductImport.csv` resolves successfully with 95 headers and the expected first and last column names.
- Updated `AIDataConvertor.Tests.csproj` to link the new service files into the isolated test harness.
- Updated `IMPLEMENTATION_TRACKER.md` to mark Propello CSV header reading complete and extended `.gitignore` to ignore generated `*.csproj.user` files.

## Exact Files Changed Today

- `.gitignore`

- `AIDataConvertor/MauiProgram.cs`
- `AIDataConvertor/Services/KnowledgeBase/IPropelloTemplateCsvHeaderReader.cs`
- `AIDataConvertor/Services/KnowledgeBase/PropelloTemplateCsvHeaderReader.cs`
- `AIDataConvertor/Services/KnowledgeBase/PropelloTemplateCsvHeaders.cs`
- `AIDataConvertor.Tests/AIDataConvertor.Tests.csproj`
- `AIDataConvertor.Tests/Services/KnowledgeBase/PropelloTemplateCsvHeaderReaderTests.cs`

- `IMPLEMENTATION_TRACKER.md`

## Files To Open First Tomorrow

- `AIDataConvertor/Services/KnowledgeBase/PropelloTemplateCsvHeaderReader.cs`
- `AIDataConvertor/Services/KnowledgeBase/PropelloTemplateCsvHeaders.cs`
- `PropelloTemplates/ProductImport.xlsx`
- `IMPLEMENTATION_TRACKER.md`
- `PRD.md`

## Exact Next Step

Implement the first Propello XLSX metadata reader beside the new CSV header reader, starting with `ProductImport.xlsx` field metadata extraction and a focused proof path in tests.

## Why This Is Next

- Phase 2 still requires XLSX metadata parsing and schema validation; the CSV slice is now complete and provides the nearest local anchor.
- `ProductImport.xlsx` is already the paired source-of-truth metadata template named in the requirements, so it is the smallest next step after the CSV proof path.
- The eventual validation utility depends on field-level metadata, so extracting that structure next keeps the implementation order coherent.

## Suggested First Edit Slice

1. Add a focused service interface and implementation that opens a Propello template workbook and extracts the first field-metadata rows from `ProductImport.xlsx`.
2. Keep the reader independent of Razor pages and model the metadata result as a small reusable record set.
3. Add a narrow test in `AIDataConvertor.Tests` that proves the workbook can be opened and that expected metadata structure is returned for `ProductImport`.
4. Reuse the new CSV header reader only as a neighboring anchor, not as a UI dependency.

## Open Risks And Deferred Work

- Propello XLSX metadata parsing is still unimplemented, so the source-of-truth constraint data in the workbook remains unused.
- The validation utility that compares generated files to the target template schema is still unstarted.
- The new CSV header reader is validated only by a focused test and a Windows app build; the full test suite was not rerun after this slice.
- No UI surface consumes the new Propello template reader yet; integration remains DI-only until the next phase.

## Cross-Machine Resume Rule

Before starting new work on another machine:

1. Run `git status --short`.
2. If clean, run `git pull`.
3. Read `.squad/identity/now.md`.
4. Read this file.
5. Open the files listed above and begin with the exact next step.