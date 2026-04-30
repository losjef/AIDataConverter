# Product Requirements Document (PRD): Epicor Propello Data Conversion Agent

## 1. Project Overview
The objective of this project is to build an AI-powered data conversion engine that transforms disparate vendor data (CSV/XLSX) into validated, import-ready formats for the **Epicor Propello** POS system. The system must intelligently handle semantic variations in vendor terminology and cross-reference data against the existing **Superior Hardware** inventory.

## 2. Key Stakeholders & Context
- **Primary User**: Operations/Data Manager at Superior Hardware.
- **Target System**: Epicor Propello POS (Cloud-based).
- **Baseline Data**: Existing inventory snapshots from Superior Hardware.

## 3. Functional Requirements

### 3.1 Template Management
- **Metadata Extraction**: The system must parse `.xlsx` files in `PropelloTemplates/` to extract field-level constraints (Data Type, Mandatory status, Allowed Values, Max Length).
- **Structural Reference**: The system must use `.csv` files in `PropelloTemplates/` to determine the exact header row and file encoding for the output.

### 3.2 Source Data Processing (Agent-First Testing)
- **Format Support**: Support for both `.csv` and `.xlsx` vendor files.
- **AI-Driven Semantic Interpretation**: The initial phase of development focuses on the AI's ability to interpret diverse terminology (e.g., `SKU`, `Product ID`, `Item#`) without pre-defined code rules.
- **Adaptive Learning**: The agent must record and refine its understanding of vendor formats based on user corrections.
- **Data Cleaning**: Normalize values (e.g., removing leading/trailing spaces, handling diverse date formats).

### 3.3 Inventory Cross-Referencing & Linking
The agent must use a **Flexible linking strategy** to compare vendor items against the Superior Hardware baseline. Linking logic is not static and may require different fields based on the vendor.

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

### 3.4 Transformation & Mapping
- **UOM Mapping**: Translate vendor Units of Measure (e.g., "BOX", "PK") to Propello-compatible codes (e.g., "BX", "EA").
- **Boolean Formatting**: Ensure all boolean fields are exported as `True` or `False`.
- **Store Defaults**: Apply default values like `Store Number = 1` unless otherwise specified.

## 4. Architectural Decisions
- **Decision 1: XLSX as Metadata Source**: We decided that `.xlsx` files in the template folder are the "Source of Truth" for metadata because they contain rich field descriptions and constraints that the CSVs lack.
- **Decision 2: User-Led Selection**: The user must explicitly select the target template type (e.g., Product vs. Customer) to ensure the agent uses the correct constraints.
- **Decision 3: Continuous Baselining**: The Superior Hardware inventory file is dynamic; the agent must always look for the most recent file in the directory before starting a conversion.
- **Decision 4: Decoupled Backend (Future UI Ready)**: The core conversion logic must be built as a reusable C# Class Library. This ensures the engine can eventually be consumed by a modern C# UI framework (such as **Blazor** or **.NET MAUI**) without heavy refactoring.
- **Decision 5: Adaptive Learning & Human-in-the-Loop**: The "Semantic Dictionary" and mapping logic must be dynamic. When the system encounters an unknown vendor column or fails validation, it must pause and prompt the user for a correction. This user feedback must be persisted (e.g., to a JSON configuration or local database) so the system gets "smarter" over time.

## 5. Tech Stack
- **Framework**: .NET 10
- **Language**: C#
- **Libraries**: 
  - `ClosedXML` or `ExcelDataReader` (for XLSX parsing)
  - `CsvHelper` (for CSV generation/parsing)
- **Future UI Options**: Blazor (Web) or .NET MAUI (Desktop/Cross-platform).

## 6. Implementation Task Tracker

### Phase 1: Environment & Context Setup
- [x] Analyze repository structure.
- [x] Create/Update `GEMINI.md` with project mission and core logic.
- [x] Create verbose `PRD.md` (this document).
- [x] Standardize on C#/.NET as the primary stack.
- [x] Establish `AgentAssets/` directory for portable AI knowledge.
- [x] Create initial `semantic_dictionary.json`.
- [x] Initialize Git repository and push to GitHub.

### Phase 2: Template Parsing Engine (C#) -> **NEXT PRIORITY**
- [ ] Initialize .NET 10 Class Library project.
- [ ] Develop C# logic to parse Propello XLSX metadata rows.
- [ ] Develop C# logic to read Propello CSV headers.
- [ ] Create a C# validation utility to check if a generated file matches the target schema.

### Phase 3: Semantic & Adaptive Mapping Logic (Agent-First Focus)
- [x] Test the Agent's ability to interpret files in `VendorFiles/` against `PropelloTemplates/`.
- [x] Build the initial "Semantic Dictionary" for common vendor aliases (Recorded below).
- [ ] Implement the **Human-in-the-Loop** feedback prompt for unknown mappings.
- [ ] Implement logic to save user corrections back to the Semantic Dictionary.
- [ ] Implement UOM conversion table.
- [ ] Implement Boolean normalization logic.

## 7. Adaptive Knowledge Base
To ensure the agent gets "smarter" over time, all learned semantic mappings, unit of measure (UOM) conversions, and manual item links are stored in the `AgentAssets/KnowledgeBase/` directory.

### 7.1 Semantic Aliases
- **Source of Truth**: `AgentAssets/KnowledgeBase/semantic_dictionary.json`
- **Purpose**: Maps vendor-specific headers (e.g., `SKU`, `Box Cost`) to standard Propello fields (`Item`, `Purchase Cost`).
- **Global Aliases**: Shared across all vendors (e.g., `EAN` -> `UPC`).

### 7.2 Manual Linking Registry
- **Source of Truth**: `AgentAssets/KnowledgeBase/manual_links.json` (Pending Creation)
- **Purpose**: Persists manual cross-references made by the user when automated linking fails.

### Phase 4: Inventory Integration
- [ ] Develop "Latest File" detection for `SuperiorHardware/` directory.
- [ ] Implement UPC/Part# matching logic between vendor and existing inventory.
- [ ] Create "Enrichment" logic to merge vendor updates with existing data.

### Phase 5: Execution & Output
- [ ] Create the main "Conversion Loop" (Extract -> Map -> Transform -> Validate).
- [ ] Implement CSV export with strict Propello formatting.
- [ ] User Acceptance Testing (UAT) with sample vendor files.

## 6. Future Considerations
- Automatic detection of vendor template type based on column headers.
- Support for complex multi-store mapping.
- Error reporting for "Ambiguous Matches" during the cross-reference phase.
