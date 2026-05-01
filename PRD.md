# Product Requirements Document (PRD): Epicor Propello Data Conversion Agent

## 1. Project Overview
The objective of this project is to build an AI-powered data conversion engine that transforms disparate vendor data (CSV/XLSX) into one of two primary outcomes: validated, import-ready formats for the **Epicor Propello** POS system, or comparison-ready views against the existing **Superior Hardware** inventory. The system must intelligently handle semantic variations in vendor terminology and cross-reference data against the existing **Superior Hardware** inventory. The repository already includes a .NET 10 MAUI Blazor scaffold in `AIDataConvertor/`, wired through `AIDataConvertor.slnx`, which will host this workflow as the engine is implemented.

## 2. Key Stakeholders & Context
- **Primary User**: Operations/Data Manager at Superior Hardware.
- **Target System**: Epicor Propello POS (Cloud-based).
- **Baseline Data**: Existing inventory snapshots from Superior Hardware. These exports should be treated as variable input layouts rather than guaranteed fixed schemas because different users may export different column sets or orders.

## 3. Functional Requirements

### 3.1 Template Management
- **Metadata Extraction**: The system must parse `.xlsx` files in `PropelloTemplates/` to extract field-level constraints (Data Type, Mandatory status, Allowed Values, Max Length).
- **Structural Reference**: The system must use `.csv` files in `PropelloTemplates/` to determine the exact header row and file encoding for the output.
- **Templates As Goal Memory**: Propello templates should participate in the application's learning and memory structure because they define the destination fields the engine is trying to reach.
- **Cross-Template Field Reuse**: The memory model should support overlapping fields and relationships across templates so shared concepts such as product identity, pricing, and purchasing data do not have to be relearned separately for every import type.
- **System-Education Value**: Template relationships should also be treated as a way to teach the engine how Epicor Propello works as a system, even when the operator is only preparing one export template at a time.
- **Template Relationship Awareness**: The engine should be able to recognize that some templates are related views of the same business entities. For example, pricing-oriented templates may overlap product templates, and purchasing templates may overlap product and vendor concepts even when the target workflow differs.
- **Current Relationship Scope For Common Export Templates**: For `ProductImport`, `PriceChangeImport`, and `POLinesImport`, the main shared linkable field is `Item`, representing the Superior Hardware product code. Within the application's working assumptions, `Item` should be unique and can be treated as the canonical index for those template relationships unless the user later defines a more specific exception.
- **Single-Template Execution Bias**: Even when templates share destination concepts, the application should generally assume one Propello export target is being prepared at a time rather than trying to coordinate multiple export templates in the same workflow.
- **Execution Scope Constraint**: Learning cross-template relationships is useful for system understanding, but that does not imply the runtime should assume multiple export templates are being produced together unless the user explicitly asks for that.

### 3.1.1 Supported Workflow Outcomes
- **Template Output Mode**: The system must be able to convert a vendor or input file into one of the supported Propello templates.
- **Comparison Mode**: The system must be able to display vendor or input data in a comparison workflow against existing Superior Hardware products.
- **Mode-Specific Behavior**: The application should not assume every workflow ends in direct file export. Some workflows will end in operator review, matching, and comparison decisions instead.

### 3.2 Source Data Processing (Agent-First Testing)
- **Format Support**: Support for both `.csv` and `.xlsx` vendor files.
- **Upload-First Vendor Intake**: The final product should not load vendor data files by default. The user should explicitly upload or select the vendor file set for the current workflow.
- **Vendor Intake Questions**: During file intake, the application should be able to ask structured clarification questions such as whether the file belongs to a new vendor, whether more than one source file is involved, and how multiple files relate to each other.
- **Multi-File Relationship Capture**: When a vendor workflow uses more than one file, the system should capture the intended relationship between those files before processing, such as primary product file, pricing supplement, promotional overlay, or comparison-only companion data.
- **Hybrid Interpretation Pipeline**: The engine should prefer deterministic C# parsing, normalization, and persisted learning before escalating unresolved semantics to an AI SDK.
- **AI-Driven Semantic Interpretation**: AI remains responsible for interpreting diverse or previously unseen terminology (e.g., `SKU`, `Product ID`, `Item#`) when local rules cannot confidently classify the source structure.
- **Adaptive Learning**: The engine must record and refine its understanding of vendor formats based on user corrections and AI-assisted resolutions, then replay that learning locally in future runs.
- **Format Drift Support**: Learned patterns must support deprecation, retirement, and replacement so the system can adapt when a long-running vendor layout changes over time.
- **Data Cleaning**: Normalize values (e.g., removing leading/trailing spaces, handling diverse date formats).
- **Token Budgeting**: The system should minimize token spend by sending compact problem slices to AI rather than entire source files whenever deterministic preprocessing can narrow the unknowns.
- **User-First Review Path**: When the engine can explain a likely match or unresolved ambiguity in human terms, it should give the user a chance to approve, reject, or edit the match before AI is invoked.

### 3.3 Inventory Cross-Referencing & Linking
The agent must use a **Flexible linking strategy** to compare vendor items against the Superior Hardware baseline. Linking logic is not static and may require different fields based on the vendor.

- **Baseline Loading Preference**:
  - Superior Hardware data must also be uploadable on demand.
  - The final product should provide a user preference that controls whether the latest Superior Hardware baseline is auto-loaded at workflow start.
  - That preference should be stored per user so the application can remember whether baseline auto-load is desired.
  - When auto-load is disabled or no usable baseline is found, the user must still be able to upload the Superior Hardware file manually.

- **Baseline Export Variability**:
  - Superior Hardware exports may change by user, report choice, or export configuration.
  - Column order must not matter.
  - Column names may vary and should be interpreted through the same local learning/memory approach used for vendor files where practical.
  - The engine must determine whether the loaded Superior Hardware file is sufficient for the requested operation before attempting cross-reference or enrichment.

- **Required-Column Awareness**:
  - Different operations may require different baseline fields.
  - For example, if the matching strategy requires `UPC`, the engine must detect whether the loaded Superior Hardware export actually contains a usable UPC-equivalent column before claiming match readiness.
  - If a required field is missing, the system must explain what is missing, why it matters, and what operation is blocked.
  - Missing-column detection must apply both to cross-referencing logic and to template-filling scenarios such as generating output for `ProductImport.xlsx`.

- **Link Key Hierarchy (Prioritized)**:
  1.  **UPC Match**: Primary and most reliable link, but subject to **UPC Volatility** (Superior Hardware codes may be outdated).
  2.  **Mfg Part Number Match**: Secondary link; often used for items without UPCs.
  3.  **Item Number Match**: Tertiary link; internal Superior Hardware ID comparison.
  4.  **Compound Key**: A combination (e.g., `Manufacturer` + `Mfg Part #`) may be required for certain vendors to avoid collisions.

- **Vendor-Specific Formatting Rules**:
  - **Midwest Fastener**: All item numbers must be standardized to `M` + 5 digits (e.g., `M00250`). Discard prefixes like `MF` or raw numeric strings during transformation.
  - **Midwest "Custom Part Number"**: This field refers to the **Orgill equivalent**. Orgill should be treated as an **Alternate Vendor** for these parts.

- **Data Safety & Overlap Protection**:
  - **ID Overlap**: If a vendor part ID matches an existing item but the UPCs do not match, the agent **must not** assume a link. This is critical for vendors like Milwaukee that have overlapping numeric ranges with others.
  - **The "No Assumption" Mandate**: The agent is strictly prohibited from guessing. If a field mapping is uncertain or a link is ambiguous, the agent **must** pause and ask the user for clarification.

- **UPC Conflict Resolution**: If a vendor UPC differs from the Superior Hardware baseline but the `Mfg Part #` or `Item #` matches exactly, the agent should flag this as a "Potential UPC Update" for the user to confirm.

### 3.3.1 Baseline Schema Readiness and Missing-Column Validation
Before using a Superior Hardware export for matching, enrichment, or output population, the system must evaluate whether the file contains the minimum viable data required for the requested workflow.

- **Schema Readiness Checks**:
  - Determine which baseline fields are required for the current operation.
  - Resolve likely column matches using local rules, shared concepts, vendor/baseline memory, and user review before any AI assistance.
  - Mark each required field as resolved, unresolved, or missing.
- **Operational Examples**:
  - If UPC-based matching is selected or preferred, the baseline file must expose a usable UPC-equivalent field.
  - If `ProductImport.xlsx` generation requires values that should come from Superior Hardware enrichment, the engine must detect whether those inputs are available before claiming the output can be completed.
  - If a baseline export is missing a critical column, the workflow should degrade gracefully by either using a secondary strategy or stopping with an actionable explanation.
- **User Feedback Requirements**:
  - The UI should identify missing baseline columns clearly.
  - The UI should show which operation is blocked and what alternate strategy, if any, remains available.
  - The user should be able to confirm or correct a suspected baseline column match before the system marks it usable.
  - The UI should make it clear whether the baseline was auto-loaded from a saved preference or supplied manually by the user for the current session.

### 3.3.2 Comparison UI Strategy
Comparison workflows should be treated as domain-specific operator surfaces rather than a single generic screen that works equally well for every vendor and every decision type.

- **Case-by-Case UI Development**:
  - Comparison UI may need to be developed per vendor, per feed type, or per matching scenario.
  - The system should allow for specialized comparison layouts when a vendor's data shape or the user's decision process demands it.
- **Comparison UI Responsibilities**:
  - Show source data beside or against existing Superior Hardware product data.
  - Highlight likely matches, conflicts, missing keys, and required user decisions.
  - Support review paths that do not immediately produce a Propello output file.
- **Design Constraint**:
  - Shared components and patterns are good, but the product should not force all comparison workflows into one rigid UI if that reduces clarity or usefulness.

### 3.4 Transformation & Mapping
- **UOM Mapping**: Translate vendor Units of Measure (e.g., "BOX", "PK") to Propello-compatible codes (e.g., "BX", "EA").
- **Boolean Formatting**: Ensure all boolean fields are exported as `True` or `False`.
- **Store Defaults**: Apply default values like `Store Number = 1` unless otherwise specified.

### 3.5 Learning, Memory, and Token-Control Architecture
The system should be designed so that **learning compounds in local C# services**, reducing dependence on repeated AI interpretation. AI should help classify ambiguity, not re-process known vendor conventions on every run.

- **Learning Storage Goals**:
  - Persist learned vendor header aliases and defaults in `AgentAssets/KnowledgeBase/semantic_dictionary.json`.
  - Add structured knowledge for Propello template fields, shared concepts, and cross-template relationships so destination schemas become part of local memory rather than remaining isolated static files.
  - Add structured stores for manual item links, normalization rules, UOM mappings, and future confidence/audit metadata as implementation advances.
  - Prefer storing hard data links, workflow parameters, and changeable relationship rules in data files or other editable assets rather than hard-coding them in C# where practical.
  - Provide supporting user documentation or manuals for editable knowledge assets so operators can understand what each file is for, how to read it, and how to edit it safely.
  - Keep those manuals updated whenever the related knowledge assets change materially.
  - Keep these assets readable by both humans and the C# engine.
  - Favor formats that an experienced user can review and safely edit when correcting a match rule.
  - Capture schema version, lifecycle status, provenance, confidence, shared concepts, vendor rule sets, and cross-vendor seed behavior in the semantic dictionary.
- **C#-Managed Memory Responsibilities**:
  - Maintain vendor fingerprints derived from normalized header sets, worksheet names, and stable column groupings.
  - Maintain Superior Hardware baseline fingerprints and known export variants so repeated baseline layouts can be recognized locally.
  - Maintain Propello template fingerprints, canonical destination fields, and reusable field-group relationships across related import templates.
  - Cache known mappings from source headers to Propello fields.
  - Interpret data-driven links and parameters from checked-in knowledge assets instead of treating those rules as fixed code behavior wherever practical.
  - Record approved user corrections so identical or near-identical layouts can be resolved locally.
  - Track confidence and reason codes for every resolved field or row.
- **Selective AI Escalation Rules**:
  - Review shared concepts and reusable patterns from known vendors before treating a new vendor layout as fully unknown.
  - Review known Superior Hardware export variants and shared baseline concepts before treating a baseline column as unknown.
  - Offer a user-review step before AI when a proposed match pattern can be presented clearly and the user is available to decide.
  - Escalate only unresolved headers, validation failures, or low-confidence record clusters.
  - Send schema context, normalized header candidates, and a compact representative sample instead of full spreadsheets when possible.
  - Never send rows that are already fully resolved by deterministic rules.
  - Respect the "No Assumption" mandate: AI output is advisory when ambiguity remains.
- **Expected Cost-Control Outcome**:
  - First-time vendor onboarding may require more AI assistance.
  - Repeat vendor runs should trend toward mostly local processing.
  - Token spend should correlate with uncertainty and novelty, not with total row count.
- **Review and Audit Requirements**:
  - The engine should log when AI was invoked, what subset was escalated, and what learning was persisted afterward.
  - User-confirmed corrections must be attributable so risky auto-learning does not silently corrupt future mappings.

### 3.6 Match Rule Visibility and User Editing
Because the user already has substantial experience creating correct mappings, the product should expose match behavior in a way that is understandable and editable rather than burying it behind prompts or opaque code paths.

- **Visible Match Patterns**:
  - The UI should show how a source header or field was matched.
  - It should identify whether the match came from a global alias, vendor-specific alias, normalization rule, manual correction, or AI suggestion.
  - It should show confidence and any notable transformation assumptions.
- **User Editing Capabilities**:
  - The user should be able to add a new match rule.
  - The user should be able to edit an existing match rule.
  - The user should be able to disable, deprecate, retire, or replace a bad rule without editing source code.
  - The user should be able to confirm whether a rule is vendor-specific or global.
- **Persistence Behavior**:
  - Approved changes should be written back to human-readable knowledge assets.
  - Changes should be attributable to user review versus AI suggestion versus seeded defaults.
  - The engine should prefer approved user-authored rules over AI-derived suggestions on future runs.
- **AI Invocation Order**:
  - Local rules run first.
  - User review/edit runs next when practical.
  - AI is invoked only after local matching and user-directed correction paths are exhausted or intentionally skipped.

## 4. Architectural Decisions
- **Decision 1: XLSX as Metadata Source**: We decided that `.xlsx` files in the template folder are the "Source of Truth" for metadata because they contain rich field descriptions and constraints that the CSVs lack.
- **Decision 2: User-Led Selection**: The user must explicitly select the target template type (e.g., Product vs. Customer) to ensure the agent uses the correct constraints.
- **Decision 3: Continuous Baselining**: The Superior Hardware inventory file is dynamic; the agent must always look for the most recent file in the directory before starting a conversion.
- **Decision 4: Decoupled Backend Inside the Existing App Shell**: The core conversion logic must be built as reusable C# services and supporting components that remain cleanly separated from MAUI pages and Blazor UI concerns. This keeps the engine testable while fitting the repository's current `.NET 10` app structure.
- **Decision 5: Engine Behind Existing MAUI Shell**: The repository's current app shell is a .NET 10 MAUI Blazor Hybrid project wired through `AIDataConvertor.slnx`. The conversion engine should be implemented behind that shell in focused services and reusable components so the UI stays thin and the core logic remains testable.
- **Decision 6: Adaptive Learning & Human-in-the-Loop**: The semantic mapping layer must remain dynamic. When the system encounters an unknown vendor column or fails validation, it must pause and prompt the user for a correction. That user feedback must be persisted to checked-in knowledge assets, with `AgentAssets/KnowledgeBase/semantic_dictionary.json` serving as the current source of truth for learned header aliases, lifecycle state, and defaults.
- **Decision 7: C# Memory First, AI Escalation Second**: Repeated interpretation work should move into C#-managed learning artifacts and local heuristics so AI token usage is reserved for unresolved or low-confidence cases. The engine should evolve toward a classifier-and-fallback model rather than sending full vendor files to AI by default.
- **Decision 8: User-Editable Match Rules and Ask-Before-AI Flow**: Match rules should remain visible and user-editable, and the application should prefer a user-review checkpoint before AI escalation whenever a likely but unresolved match can be explained clearly.
- **Decision 9: Cross-Vendor Reuse Before AI**: When onboarding a new vendor or a changed vendor layout, the engine should review shared concepts and reusable patterns from existing vendors before escalating to AI.
- **Decision 10: Lifecycle-Based Rule Evolution**: Learned rules should support explicit lifecycle states such as active, deprecated, retired, and replaced so stale patterns can be preserved, reviewed, and superseded safely.
- **Decision 11: Superior Hardware Exports Are Variable Inputs**: Superior Hardware baseline files must be interpreted as potentially different user-generated export layouts rather than fixed schemas.
- **Decision 12: Required-Column Readiness Must Be Evaluated Per Workflow**: Before matching or template population proceeds, the engine must verify that required baseline fields are present or explain why the workflow cannot continue.
- **Decision 13: The Product Has Two Primary Outcomes**: The application must support both Propello-template conversion and comparison-oriented review against Superior Hardware products.
- **Decision 14: Comparison UI Is Case-Specific**: Comparison workflows should be allowed to evolve case by case rather than being forced into a single generic UI pattern.
- **Decision 15: Upload-First File Intake With Optional Baseline Auto-Load**: Vendor files should be user-supplied for each workflow rather than silently loaded by default, while Superior Hardware baseline loading may be auto-started only when the user explicitly enables that behavior as a saved preference.
- **Decision 16: Propello Templates Participate In Memory**: Propello template schemas and their shared field relationships should be modeled as part of the system's learning layer because they define the destination concepts the engine is trying to satisfy.
- **Decision 17: Common Propello Export Templates Currently Share `Item` As The Main Link**: For `ProductImport`, `PriceChangeImport`, and `POLinesImport`, the current relationship model should treat `Item` as the primary shared key and avoid inventing broader linkage unless the user provides more detail.
- **Decision 18: Template Relationships Can Educate The System About Propello**: Cross-template knowledge should help the engine understand the broader Propello domain model even when day-to-day execution remains focused on a single export template at a time.
- **Decision 19: Changeable Links And Parameters Should Be Data-Driven**: Hard data links, relationship rules, and workflow parameters should live in editable data assets where practical so they can evolve over time without forcing code changes.

## 5. Tech Stack
- **Framework**: .NET 10
- **Language**: C#
- **Current App Shell**: .NET MAUI Blazor Hybrid (`AIDataConvertor/`)
- **Solution Entry Point**: `AIDataConvertor.slnx`
- **Libraries**: 
  - `ClosedXML` or `ExcelDataReader` (for XLSX parsing)
  - `CsvHelper` (for CSV generation/parsing)
- **Planned Implementation Focus**: Template parsing, validation, and conversion services behind the existing app shell.

## 6. Implementation Tracking

The working implementation checklist now lives in `IMPLEMENTATION_TRACKER.md` so requirements and architecture can stay stable while delivery status evolves independently.

Current next milestone: Phase 2 template parsing and validation services behind the existing MAUI Blazor shell.

## 7. Adaptive Knowledge Base
To ensure the agent gets "smarter" over time, all learned semantic mappings, unit of measure (UOM) conversions, and manual item links are stored in the `AgentAssets/KnowledgeBase/` directory.

Each editable knowledge asset in that directory must also be represented in the folder manual at `AgentAssets/KnowledgeBase/README.md` so the operator has a current explanation of the file's purpose, structure, and safe editing expectations.

### 7.1 Semantic Aliases
- **Source of Truth**: `AgentAssets/KnowledgeBase/semantic_dictionary.json`
- **Purpose**: Maps vendor-specific headers (e.g., `SKU`, `Box Cost`) to standard Propello fields (`Item`, `Purchase Cost`).
- **Shared Concepts**: Reusable across vendors (e.g., `EAN` -> `UPC`) and used as seeds before inventing a new vendor-specific rule.
- **Documentation Rule**: Detailed requirements should reference this file as the learned-mapping authority instead of duplicating editable alias tables in Markdown.
- **Evolution Direction**: The file should remain human-readable while supporting schema versioning, provenance, confidence, lifecycle status, replacement/deprecation, vendor rule sets, and cross-vendor seeding behavior.

### 7.2 Manual Linking Registry
- **Source of Truth**: `AgentAssets/KnowledgeBase/manual_links.json`
- **Purpose**: Persists manual cross-references made by the user when automated linking fails.
- **Current Shape**: The initial registry stores governance rules plus an editable list of approved link records. It is intentionally conservative so user-confirmed links can be persisted without implying broader auto-linking behavior.
- **Editing Rule**: Only approved user decisions belong in this registry. Uncertain links should stop for clarification rather than being written as guesses.

### 7.3 Planned Learning Stores For C# Runtime Use
- **Vendor Fingerprints**: A planned structured asset that records recognizable vendor/layout signatures so the engine can short-circuit repeated AI interpretation.
- **Propello Goal-Schema Memory**: `AgentAssets/KnowledgeBase/propello_template_memory.json` is now the initial structured asset for Propello template concepts, shared destination fields, and lightweight cross-template relationships.
- **Baseline Schema Rules**: `AgentAssets/KnowledgeBase/baseline_schema_rules.json` is now the initial structured asset for workflow-specific baseline required fields and dynamic time-series detection patterns.
- **Knowledge-Base Manual**: `AgentAssets/KnowledgeBase/README.md` is the user-facing guide for understanding and safely editing the knowledge assets in this folder.
- **Manual Linking Registry**: `AgentAssets/KnowledgeBase/manual_links.json` is now the initial structured asset for preserving user-approved cross-references when deterministic linking is insufficient.
- **Normalization Rules**: A planned structured asset for canonicalizing raw source values before semantic matching.
- **Confidence and Audit Metadata**: A planned structured asset or runtime log that explains why a mapping was resolved locally, escalated to AI, or stopped for user review.
- **Design Intent**: These stores should be consumable directly from C# services so learning remains local, versioned, testable, and progressively less token-dependent.

## 8. Future Considerations
- Automatic detection of vendor template type based on column headers.
- Support for complex multi-store mapping.
- Error reporting for "Ambiguous Matches" during the cross-reference phase.
- Configurable escalation thresholds so AI is only invoked above a defined uncertainty level.
- Optional local embedding or vector-assisted lookup only if it proves materially cheaper and safer than simpler structured rule stores.
