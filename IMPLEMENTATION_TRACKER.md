# AIDataConverter Implementation Tracker

This file holds the working implementation checklist for AIDataConverter.

The product requirements, architecture, constraints, and knowledge-base rules remain in `PRD.md`, `GEMINI.md`, and `DECISION_LOG.md`. This file exists so the todo list can evolve independently without turning the PRD into a mixed requirements-and-status document.

## Phase 1: Environment & Context Setup

- [x] Analyze repository structure.
- [x] Create/Update `GEMINI.md` with project mission and core logic.
- [x] Create verbose `PRD.md`.
- [x] Standardize on C#/.NET as the primary stack.
- [x] Confirm the checked-in .NET 10 MAUI Blazor app scaffold in `AIDataConvertor/` and its solution wiring in `AIDataConvertor.slnx`.
- [x] Establish `AgentAssets/` directory for portable AI knowledge.
- [x] Create initial `semantic_dictionary.json` as the checked-in source of truth for learned header aliases and defaults.
- [x] Initialize Git repository and push to GitHub.

## Phase 2: Template Parsing & Validation Services (C#) -> NEXT PRIORITY

- [ ] Develop C# logic to parse Propello XLSX metadata rows.
- [x] Develop C# logic to read Propello CSV headers.
- [ ] Create a C# validation utility to check if a generated file matches the target schema.
- [ ] Integrate those services behind the existing MAUI Blazor shell without coupling parsing logic to UI pages.
- [x] Design and scaffold a local memory model for Propello template fields, shared concepts, and cross-template relationships.

## Phase 3: Semantic & Adaptive Mapping Logic (Agent-First Focus)

- [x] Test the Agent's ability to interpret files in `VendorFiles/` against `PropelloTemplates/`.
- [x] Build the initial semantic dictionary for common vendor aliases in `AgentAssets/KnowledgeBase/semantic_dictionary.json`.
- [x] Scaffold the runtime semantic memory layer: typed schema models, file-backed repository, packaged seed asset, and lifecycle-aware local query service.
- [x] Scaffold the Propello template memory layer: checked-in JSON asset, typed schema model, file-backed repository, and packaged seed asset wiring.
- [x] Move first-pass baseline workflow requirements and dynamic period-detection patterns into a checked-in data asset with typed repository support.
- [x] Scaffold the manual linking registry as a checked-in JSON asset with typed repository support and operator documentation.
- [x] Create a first user-facing manual for editable knowledge assets and require it to stay aligned with those files.
- [ ] Implement a C# learning store abstraction for vendor aliases, defaults, normalization rules, and future manual links.
- [ ] Extend the learning store to include Propello goal-schema memory and reusable field relationships between templates.
- [ ] Implement vendor fingerprinting so known layouts can be classified locally before AI escalation.
- [ ] Implement confidence scoring for header and row-level mapping outcomes.
- [x] Design the JSON schema so match rules remain human-readable and safely editable.
- [ ] Expose match provenance and confidence in the UI.
- [x] Expose a minimal semantic-memory status and local match-resolution probe in the UI.
- [ ] Allow the user to add, edit, disable, and approve match rules from the UI.
- [ ] Add a user-review checkpoint before AI escalation for unresolved but explainable matches.
- [ ] Implement selective AI escalation that sends only unresolved headers, compact samples, and validation failures.
- [ ] Implement the human-in-the-loop feedback prompt for unknown mappings.
- [ ] Implement logic to save user corrections back to the semantic dictionary and related knowledge assets.
- [ ] Implement UOM conversion table.
- [ ] Implement Boolean normalization logic.

## Phase 4: Inventory Integration

- [ ] Develop latest-file detection for `SuperiorHardware/`.
- [x] Implement a first-pass Superior Hardware baseline schema analyzer for differing user-exported column layouts.
- [x] Read header rows from the latest real SuperiorHardware `.xlsx` export instead of relying only on sample baseline headers.
- [x] Detect dynamic rolling-period columns such as `Apr-26`, `Apr 2026`, or `Q1-26` so they do not get treated as stable required fields.
- [x] Add first-pass workflow-specific required-column readiness checks for baseline-dependent scenarios such as UPC matching and ProductImport enrichment.
- [x] Surface a minimal baseline-readiness probe in the UI, including missing required fields, dynamic-column classification, and workflow summaries.
- [ ] Implement UPC/part-number matching logic between vendor and existing inventory.
- [ ] Create enrichment logic to merge vendor updates with existing data.

## Phase 5: Execution & Output

- [ ] Create the main conversion loop: Extract -> Map -> Transform -> Validate.
- [ ] Implement CSV export with strict Propello formatting.
- [ ] Implement mode selection between Propello-template output and comparison-oriented review workflows.
- [ ] Build an upload-first intake flow for vendor files instead of auto-loading vendor data by default.
- [ ] Add intake questions for new vendors, multi-file workflows, and file relationship selection.
- [ ] Add a saved user preference that controls whether the Superior Hardware baseline auto-loads at workflow start.
- [ ] Keep manual Superior Hardware upload available even when the auto-load preference exists.
- [x] Build a first case-specific comparison preview for Milwaukee IMAP headers against SuperiorHardware baseline readiness.
- [ ] Expand comparison UI surfaces case by case for vendor/product review scenarios rather than assuming one generic comparison screen.
- [ ] Perform user acceptance testing with sample vendor files.