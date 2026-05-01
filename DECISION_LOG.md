# Architectural Decision Log (ADL)

This file records the rationale behind architectural choices, directory structures, and procedural decisions to ensure consistency across AI sessions.

## 2026-04-30: Initial Project Structuring & Documentation
- **Decision**: Creation of `GEMINI.md` and `PRD.md`.
- **Rationale**: `GEMINI.md` provides high-level mandates for the AI agent, while `PRD.md` captures detailed functional requirements and the implementation task tracker.
- **Decision**: Selection of **C# / .NET 10** as the primary tech stack.
- **Rationale**: User preference and expertise; provides a robust ecosystem for eventual Blazor/MAUI UI development.

## 2026-05-01: Documentation Normalization To Current Repo State
- **Decision**: Treat the checked-in `.NET 10` `AIDataConvertor/` MAUI Blazor Hybrid project and `AIDataConvertor.slnx` as established repository state, not planned work.
- **Rationale**: The scaffold already exists and builds. Project documentation should point the next milestone at template parsing and validation behind that shell instead of describing UI/bootstrap work as still pending.
- **Decision**: Keep template metadata source-of-truth guidance tied to the paired Propello `.xlsx` files, while describing `AgentAssets/KnowledgeBase/semantic_dictionary.json` as the current checked-in source of truth for learned mappings.
- **Rationale**: The `.xlsx` templates are present in `PropelloTemplates/`, and `semantic_dictionary.json` is the concrete checked-in knowledge asset that currently stores learned vendor aliases and defaults. The docs should point to the real maintained artifact rather than implying the mappings primarily live in prose.
- **Decision**: Keep the PRD detailed, but normalize its tracker structure and numbering so current milestones, architecture decisions, and knowledge-base ownership read as present repository state instead of mixed historical scaffolding notes.
- **Rationale**: The repo already has substantial documentation and app scaffolding. Correcting stale phase wording and duplicate numbering improves handoff quality without collapsing project detail.

## 2026-05-01: Hybrid AI Cost-Control Strategy
- **Decision**: Use AI as a selective semantic fallback, not as the default interpreter for entire vendor files.
- **Rationale**: Full-file AI interpretation would scale token cost with file size. Most repeat work in vendor imports is deterministic once headers, normalization patterns, and vendor quirks are learned.
- **Decision**: Build learning and memory in C#-managed local assets first, then escalate only unresolved or low-confidence slices to the AI SDK.
- **Rationale**: Structured local memory keeps learning versioned, testable, and reusable across runs while reducing token usage over time.
- **Decision**: Persist user-approved corrections and AI-assisted resolutions as checked-in knowledge assets rather than ephemeral prompt context.
- **Rationale**: This turns each clarification into durable automation and prevents paying for the same ambiguity more than once.
- **Decision**: Preserve detailed initial documentation instead of collapsing it into a short summary during normalization.
- **Rationale**: The initial project documentation carries important implementation nuance, vendor rules, and safety constraints that should remain explicit for future handoffs.

## 2026-05-01: User-Directed Matching Before AI Escalation
- **Decision**: Keep the matching knowledge asset human-readable and suitable for controlled user edits.
- **Rationale**: The user already has strong domain expertise in creating match patterns. Letting them inspect and correct rules directly can reduce both ambiguity and recurring AI cost.
- **Decision**: The UI should expose match provenance and allow the user to add, edit, disable, and approve match rules.
- **Rationale**: If the user can see why a match happened, they can correct the system before bad automation spreads through future runs.
- **Decision**: Prefer an ask-the-user checkpoint before AI escalation when the unresolved match can be clearly presented.
- **Rationale**: A direct user correction is often cheaper, safer, and more durable than invoking AI for a case the user can resolve immediately.

## 2026-05-01: Adaptive Semantic Dictionary v2
- **Decision**: Evolve `AgentAssets/KnowledgeBase/semantic_dictionary.json` from a flat alias map into a versioned, human-readable schema with shared concepts, vendor rule sets, provenance, confidence, and lifecycle metadata.
- **Rationale**: No runtime code consumes the file yet, so the schema can safely evolve now without a compatibility burden. The richer structure preserves current mappings while making future learning auditable and editable.
- **Decision**: Represent stale or changed vendor patterns with explicit lifecycle states such as `deprecated`, `retired`, and `replaced` rather than deleting them outright.
- **Rationale**: Vendors change long-running layouts over time. Preserving rule history makes it possible to review drift safely and understand why an old pattern stopped applying.
- **Decision**: Review shared concepts and reusable patterns from existing vendors before AI when onboarding a new vendor or changed layout.
- **Rationale**: Cross-vendor reuse and user review are cheaper and safer than treating every new layout as an AI-first problem.

## 2026-05-01: Separate Implementation Tracker
- **Decision**: Move the working implementation checklist out of `PRD.md` into `IMPLEMENTATION_TRACKER.md`.
- **Rationale**: The PRD should stay focused on requirements and architecture, while the delivery checklist needs to change more frequently. Splitting them keeps both documents clearer and easier to maintain.

## 2026-05-01: Variable Superior Hardware Export Handling
- **Decision**: Treat Superior Hardware baseline files as variable user-generated export layouts rather than a fixed schema.
- **Rationale**: Different users may export different column sets, names, or orders. The app must interpret the baseline file and verify readiness instead of assuming one canonical export shape.
- **Decision**: Evaluate required-column readiness before relying on the baseline for matching or output population.
- **Rationale**: Operations such as UPC matching or `ProductImport.xlsx` enrichment can fail silently or produce weak results if the necessary baseline columns are missing. The app should detect that condition and explain it early.

## 2026-05-01: Dual Workflow Product Goal
- **Decision**: Treat the product as having two primary outcomes: Propello-template conversion and comparison-oriented review against Superior Hardware products.
- **Rationale**: Not every useful workflow ends in immediate file export. Some workflows need a review surface first so users can compare, resolve, and decide before any downstream output is produced.
- **Decision**: Allow comparison UI to evolve case by case instead of forcing one generic comparison screen.
- **Rationale**: Vendor structure, match ambiguity, and review decisions vary enough that a rigid one-size-fits-all comparison surface would likely reduce clarity and operator efficiency.

## 2026-05-01: Upload-First File Intake And Baseline Preference
- **Decision**: The final product should not load vendor files by default. Vendor workflows should start from explicit user upload or selection of the current vendor file set.
- **Rationale**: Vendor runs are operator-driven and may involve different vendors, changed layouts, or multiple related source files. Silent default loading would blur operator intent and make ambiguity harder to resolve safely.
- **Decision**: The intake flow should ask clarifying questions when file relationships are uncertain, including whether the vendor is new, whether more than one file is involved, and how those files relate.
- **Rationale**: Multi-file vendor workflows can combine product, pricing, promotion, or overlay data. The app should capture that relationship explicitly instead of inferring it.
- **Decision**: Superior Hardware baseline loading should support a saved user preference for auto-load, while always retaining a manual upload path.
- **Rationale**: Some users will want the latest baseline brought in automatically, while others will want explicit session control. Supporting both keeps the workflow flexible without forcing one default behavior on everyone.

## 2026-05-01: Propello Templates As Memory And Goal Schema
- **Decision**: Treat `PropelloTemplates/` as part of the learning and memory model, not only as static output references.
- **Rationale**: Propello fields are the destination concepts the engine is trying to produce. Modeling those destination schemas explicitly should improve local reasoning, reuse, and explainability.
- **Decision**: Capture overlap and relationships across Propello templates where fields or business concepts recur.
- **Rationale**: Templates such as product, price-change, and purchasing imports share meaningful concepts. Representing those overlaps should reduce duplicated mapping logic and improve reuse across workflows.
- **Decision**: For `ProductImport`, `PriceChangeImport`, and `POLinesImport`, use `Item` as the current shared key and treat it as the linkable index for template-memory purposes.
- **Rationale**: The user clarified that `Item` is the main common field for Superior Hardware products, it should be unique, and additional linking structure is not currently needed.
- **Decision**: Bias the workflow toward one export template at a time even when template-memory captures shared concepts.
- **Rationale**: These templates are export goals for Epicor Propello, not concurrent working sets. Overbuilding cross-template coordination would add complexity without matching the current operating model.
- **Decision**: Preserve cross-template relationships as educational memory about how Propello works as a system.
- **Rationale**: Even when exports are typically prepared one at a time, those relationships still help the engine reason about destination concepts, field reuse, and template intent more accurately.

## 2026-05-01: Data-Driven Relationship And Parameter Storage
- **Decision**: Prefer storing hard data links, relationship rules, and workflow parameters in editable data assets rather than hard-coding them in C# when the values may change over time.
- **Rationale**: Relationship rules and operational parameters are part of the system's learned business knowledge. Keeping them in data files makes them easier to review, update, version, and reuse without recompiling code.

## 2026-05-01: Knowledge Assets Need User Manuals
- **Decision**: Editable knowledge assets that store matching information, parameters, learned behavior, or similar memory must have supporting user documentation that explains what they are, how to read them, and how to edit them safely.
- **Rationale**: If these files are part of the product's operator-facing memory layer, users need practical guidance for maintaining them.
- **Decision**: Those manuals must be kept up to date whenever the corresponding knowledge assets change materially.
- **Rationale**: A stale manual is almost as risky as opaque code because it can cause users to misunderstand or corrupt the product's learned rules.

## 2026-05-01: Manual Linking Registry Starts As A Conservative Exception Store
- **Decision**: `manual_links.json` should begin as a user-approved exception registry for explicit cross-references rather than a broad inferred-link engine.
- **Rationale**: This preserves the no-assumption rule while still giving the C# runtime a local place to remember operator-approved links.

## 2026-05-01: End-Of-Day Handoff Must Be Cross-Machine Safe
- **Decision**: Repo-local end-of-day state must include a concrete resume file and a short current-focus file under `.squad/identity/`.
- **Rationale**: The next session should be able to resume from a narrow, authoritative handoff instead of re-scanning the repository and reconstructing context.
- **Decision**: End-of-day handoff for this project must include git sync steps so switching computers does not strand local-only state.
- **Rationale**: A handoff is incomplete if the branch was not committed, pulled/rebased, and pushed, because the next machine may otherwise start from stale code.

## 2026-04-30: Directory Rationale
- **`PropelloTemplates/`**: Contains "Gold Standard" pairs. XLSX for metadata (rows = fields), CSV for structural reference.
- **`VendorFiles/`**: Renamed from `VendorTemplates` to reflect that these contain actual raw data files provided by vendors for processing.
- **`SuperiorHardware/`**: Established as the folder for existing inventory baselines. Contains high-column-count XLSX exports from the live POS.

## 2026-04-30: Data Mapping Strategy
- **Decision**: XLSX templates are the "Source of Truth" for metadata.
- **Rationale**: Propello CSVs lack data type and constraint information; XLSX provides the mandatory rules for a successful import.
- **Decision**: Semantic alias mappings are maintained in `AgentAssets/KnowledgeBase/semantic_dictionary.json`, with the PRD documenting behavior and ownership rather than acting as the editable mapping registry.
- **Rationale**: Vendors use inconsistent terminology (SKU vs Item #). Keeping the mappings in structured JSON allows the future implementation to consume them programmatically while the documentation explains how and when those mappings are used.

## 2026-04-30: AI Asset Centralization
- **Decision**: Creation of the `AgentAssets/` root directory, with `KnowledgeBase/` currently checked in for reusable semantic mappings and learned data.
- **Rationale**: AI-specific configurations and learned behaviors should live in-repo, but the documentation should only describe subfolders that are actually present today.
- **Decision**: Migration of semantic mappings to `AgentAssets/KnowledgeBase/semantic_dictionary.json`.
- **Rationale**: Moving from a Markdown table in `PRD.md` to a structured JSON format allows the data to be consumed programmatically by the future C# implementation while remaining human-readable for maintenance.

## 2026-04-30: Midwest Formatting & Vendor Overlap Safety
- **Decision**: Standardize all Midwest Item Numbers to the `M#####` format.
- **Rationale**: Midwest uses inconsistent prefixes (None, 'M', 'MF'). Forcing a consistent `M` prefix for 5-digit codes ensures deterministic matching within Superior Hardware inventory.
- **Decision**: Implement strict **UPC Cross-Validation** for ID matches.
- **Rationale**: Vendor numbers (e.g., Milwaukee) can overlap with other suppliers. If IDs match but UPCs differ, the item must **not** be linked automatically.
- **Decision**: **"No Assumption" Mandate**.
- **Rationale**: To prevent data corruption, the agent is strictly forbidden from making assumptions about data mapping or linking. User clarification is mandatory for any ambiguity.
