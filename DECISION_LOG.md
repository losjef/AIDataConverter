# Architectural Decision Log (ADL)

This file records the rationale behind architectural choices, directory structures, and procedural decisions to ensure consistency across AI sessions.

## 2026-04-30: Initial Project Structuring & Documentation
- **Decision**: Creation of `GEMINI.md` and `PRD.md`.
- **Rationale**: `GEMINI.md` provides high-level mandates for the AI agent, while `PRD.md` captures detailed functional requirements and the implementation task tracker.
- **Decision**: Selection of **C# / .NET 10** as the primary tech stack.
- **Rationale**: User preference and expertise; provides a robust ecosystem for eventual Blazor/MAUI UI development.

## 2026-04-30: Directory Rationale
- **`PropelloTemplates/`**: Contains "Gold Standard" pairs. XLSX for metadata (rows = fields), CSV for structural reference.
- **`VendorFiles/`**: Renamed from `VendorTemplates` to reflect that these contain actual raw data files provided by vendors for processing.
- **`SuperiorHardware/`**: Established as the folder for existing inventory baselines. Contains high-column-count XLSX exports from the live POS.

## 2026-04-30: Data Mapping Strategy
- **Decision**: XLSX templates are the "Source of Truth" for metadata.
- **Rationale**: Propello CSVs lack data type and constraint information; XLSX provides the mandatory rules for a successful import.
- **Decision**: Semantic Alias Dictionary in `PRD.md`.
- **Rationale**: Vendors use inconsistent terminology (SKU vs Item #). Recording these mappings allows the agent to get "smarter" and ensures deterministic transformations.

## 2026-04-30: AI Asset Centralization
- **Decision**: Creation of the `AgentAssets/` root directory with subfolders for `Prompts`, `Instructions`, `KnowledgeBase`, and `Skills`.
- **Rationale**: To ensure all AI-specific configurations, learned behaviors, and instructional context are stored within the project repository. This facilitates portability across AI sessions and different AI models/tools.
- **Decision**: Migration of semantic mappings to `AgentAssets/KnowledgeBase/semantic_dictionary.json`.
- **Rationale**: Moving from a Markdown table in `PRD.md` to a structured JSON format allows the data to be consumed programmatically by the future C# implementation while remaining human-readable for maintenance.

## 2026-04-30: Midwest Formatting & Vendor Overlap Safety
- **Decision**: Standardize all Midwest Item Numbers to the `M#####` format.
- **Rationale**: Midwest uses inconsistent prefixes (None, 'M', 'MF'). Forcing a consistent `M` prefix for 5-digit codes ensures deterministic matching within Superior Hardware inventory.
- **Decision**: Implement strict **UPC Cross-Validation** for ID matches.
- **Rationale**: Vendor numbers (e.g., Milwaukee) can overlap with other suppliers. If IDs match but UPCs differ, the item must **not** be linked automatically.
- **Decision**: **"No Assumption" Mandate**.
- **Rationale**: To prevent data corruption, the agent is strictly forbidden from making assumptions about data mapping or linking. User clarification is mandatory for any ambiguity.
