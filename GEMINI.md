# Epicor Propello Data Conversion Agent

The primary mission of this project is to develop and utilize a custom AI agent specialized in converting data provided by various vendors (in `.xlsx` or `.csv` format) into either validated, ready-to-use import files for the **Epicor Propello** POS application or comparison-ready views against existing Superior Hardware products.

The conversion process is **user-directed**: the user provides a source file and either selects the specific Propello template (e.g., `ProductImport`) that the data should be mapped into, or chooses a comparison workflow against existing Superior Hardware products. The agent then acts as the transformation engine to produce either final import-ready output or comparison-ready review data.

The final product should be **upload-first** for vendor data. Vendor files should not be silently loaded by default; the user should upload the relevant file or file set for the current run, and the workflow should be allowed to ask clarifying intake questions when the relationship between files is not yet known.

## Directory Structure

- **PropelloTemplates/**: Contains the target "Gold Standard" import schemas for Epicor Propello. Files are provided in pairs:
  - **.xlsx Files**: The **Source of Truth** for schema metadata. 
    - **Structure**: Each **row** represents a specific data field. 
    - **Metadata Columns**: Columns contain definitions, data types, mandatory status, and allowed values.
    - **Headers**: The first row contains headers for these metadata columns.
  - **.csv Files**: Format examples containing **placeholder data** for structural reference.
- **VendorFiles/**: Repository of raw source data from various vendors (e.g., Do It Best, Midwest, Milwaukee). These represent the real-world "Source" data files (XLSX/CSV) that the agent must interpret and transform.
- **SuperiorHardware/**: Contains the **Existing Inventory Data** for Superior Hardware.
  - **Source File**: Typically an `.xlsx` file (e.g., `exported_data_*.xlsx`).
  - **Structure**: Exported inventory data where the first row contains headers and each subsequent row represents an existing product in Propello.
  - **Layout Variability**: Different users may export different column layouts, so the app must interpret the file rather than assume a single frozen schema.
  - **Dynamic Content**: This file is updated frequently to ensure data is current.
- **AIDataConvertor/**: Existing .NET 10 **MAUI Blazor Hybrid** app scaffold that will host the conversion workflow and supporting services.
  - **Solution Wiring**: The scaffold is already connected through `AIDataConvertor.slnx`.
  - **Current Role**: Provides the application shell while conversion services, parsing, and validation are implemented behind it.
- **AgentAssets/KnowledgeBase/**: Checked-in semantic mappings and other learned data used by the conversion engine.
  - **Current Source of Truth**: `semantic_dictionary.json` is the checked-in authority for learned vendor header aliases, lifecycle state, provenance, confidence, shared concepts, and vendor defaults.

## Agent Core Logic & Workflow

1.  **Template Selection**: The user specifies the target Propello template.
2.  **Schema Extraction**: The agent reads the `.xlsx` metadata from `PropelloTemplates/` to understand the target constraints.
3.  **Inventory Baselining**: The agent either loads a user-uploaded Superior Hardware file or, when enabled by a saved user preference, auto-loads the latest baseline file from `SuperiorHardware/`. Before relying on that baseline, it must determine whether required fields such as UPC or other enrichment columns are actually present in the exported layout.
4.  **Source Analysis & Semantic Mapping**: The engine first applies deterministic C# parsing, normalization, shared concepts, vendor rule sets, and other learned mappings before invoking AI. If the intake is unclear, the workflow should ask the user whether the vendor is new, whether multiple files belong together, and how those files should be related before semantic processing continues. The AI layer is reserved for low-confidence or previously unseen structures (for example, ambiguous header meaning, vendor-specific packing fields, or records that fail rule-based validation).
5.  **Comparison & Enrichment**: The agent cross-references vendor items against the Superior Hardware baseline to identify new vs. existing items.
6.  **Heuristic Mapping & Transformation**: The engine maps source columns to target template fields, applying necessary transformations (UOM mapping, boolean formatting). Learned rules should be applied in descending order of trust: hard-coded template rules, shared concepts, persisted vendor mappings, validated manual links, then AI-assisted suggestions.
7.  **Validation & Learning**: If a mapping is ambiguous, the agent prompts the user for correction and records the learned rule for future automation in the checked-in knowledge base. Learning should be stored in C#-managed artifacts so future runs can classify, transform, and validate more rows locally without paying repeated AI token cost. Vendor formats can drift over time, so learned rules must be deprecatable, replaceable, and attributable instead of treated as permanent truth.
8.  **Output Generation**: The agent generates a final, validated CSV file ready for Propello import.

## Data Standards

- **Boolean Values**: Propello typically requires `True`/`False` strings.
- **Store Numbers**: Handled as integers, usually starting with `1`.
- **Header Integrity**: Target file headers must match `PropelloTemplates` exactly; no columns should be added or removed.
- **Unit of Measure (UOM)**: Pay close attention to mapping vendor UOMs (e.g., "EA", "BX", "PK") to Propello's `Stocking UOM` and `Selling UOM`.

## Development & Usage

The implementation uses **C# / .NET 10**. The repository already contains a **MAUI Blazor Hybrid** scaffold in `AIDataConvertor/` and solution wiring in `AIDataConvertor.slnx`.

The next practical milestone is to implement template parsing and validation logic behind that app shell. Keep the UI scaffold thin while the conversion engine, template readers, and validation services take shape as reusable backend components. Learned header aliases and vendor defaults should continue to be maintained in `AgentAssets/KnowledgeBase/semantic_dictionary.json`, with documentation referencing that file rather than duplicating mapping tables.

## Hybrid AI Cost Strategy

The preferred operating model is a **hybrid C# memory-first pipeline** rather than an AI-only interpreter for every vendor file. AI remains valuable for ambiguous semantic interpretation, but repeated deterministic work should move into local code and persisted learning so token usage falls over time.

- **C# owns repeatable memory**: vendor header aliases, normalized column fingerprints, UOM conversions, boolean coercion rules, manual item links, and previously approved field mappings should be stored in JSON or other local persistence managed by the .NET engine.
- **Human-readable persistence is a feature**: the learning store should remain understandable enough that an experienced user can inspect or correct match rules without needing to reverse-engineer opaque application state.
- **Cross-vendor reuse happens before AI**: when onboarding a new vendor, the engine should review shared concepts and reusable patterns from existing vendors before escalating unresolved semantics.
- **AI is a fallback, not the default**: the engine should only send a reduced problem slice to the AI layer when deterministic matching fails or confidence falls below a configurable threshold.
- **User review should come before AI when practical**: if the engine can explain the unresolved match in clear terms and the user is available, the UI should offer the user a chance to approve, reject, or edit the proposed match pattern before spending tokens.
- **Token minimization happens before inference**: the C# pipeline should collapse repeated columns, normalize whitespace/casing, pre-classify known vendor layouts, and only send unresolved headers or representative row samples instead of entire files.
- **Learning must compound**: every approved clarification should update the local knowledge assets so the same ambiguity is handled by code on the next run.
- **Rule history should stay visible**: when a vendor changes formats, stale patterns should be deprecated or replaced in the knowledge asset rather than silently overwritten.
- **Auditing stays local**: the engine should record why a column or row was resolved locally versus escalated to AI so future tuning can reduce escalations further.

Concretely, this means the conversion engine should evolve toward these stages:

1. **File fingerprinting**: identify the vendor or layout signature from headers, sheet names, and stable column clusters.
2. **Local mapping pass**: resolve known headers and defaults using `semantic_dictionary.json`, UOM tables, manual links, and vendor-specific normalization rules.
3. **Confidence scoring**: mark each unresolved header, field, or row with a confidence level based on rule matches and validation results.
4. **User intervention checkpoint**: when the engine has a likely match but not enough confidence to auto-apply it, present the candidate pattern to the user first so they can approve or edit it.
5. **Selective AI escalation**: send only low-confidence headers, a compact schema summary, and a small set of representative examples to the AI SDK after local rules and user-review opportunities are exhausted.
6. **Human confirmation for ambiguity**: when AI output is still ambiguous or safety-sensitive, require user approval rather than auto-accepting guesses.
7. **Persistence of approved learning**: write newly approved rules back to the local knowledge base so the next run remains mostly deterministic.

This approach keeps the system aligned with the project's "No Assumption" mandate while making AI spend proportional to uncertainty rather than file size.

The same principle applies to Superior Hardware baseline exports: they should be interpreted as variable inputs, checked for required-column readiness, and matched through local memory before any optional AI assistance is considered.

Comparison UI should be allowed to evolve case by case. Shared infrastructure is useful, but vendor/product comparison surfaces should not be forced into a single generic design if the review task needs something more specialized.
