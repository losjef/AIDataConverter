# Epicor Propello Data Conversion Agent

The primary mission of this project is to develop and utilize a custom AI agent specialized in converting data provided by various vendors (in `.xlsx` or `.csv` format) into validated, ready-to-use import files for the **Epicor Propello** POS application.

The conversion process is **user-directed**: the user provides a source file and selects the specific Propello template (e.g., `ProductImport`) that the data should be mapped into. The agent then acts as the transformation engine to produce the final import-ready output.

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
  - **Structure**: Standard data sheet where the first row contains headers and each subsequent row represents an existing product in Propello.
  - **Dynamic Content**: This file is updated frequently to ensure data is current.

## Agent Core Logic & Workflow

1.  **Template Selection**: The user specifies the target Propello template.
2.  **Schema Extraction**: The agent reads the `.xlsx` metadata from `PropelloTemplates/` to understand the target constraints.
3.  **Inventory Baselining**: The agent reads the latest inventory file from `SuperiorHardware/` to understand existing products, prices, and attributes.
4.  **Source Analysis & Semantic Mapping**: The agent's core capability is tested here—it must independently interpret the vendor's column headers and data structures (e.g., `SKU` vs `Item Number`).
5.  **Comparison & Enrichment**: The agent cross-references vendor items against the Superior Hardware baseline to identify new vs. existing items.
6.  **Heuristic Mapping & Transformation**: The agent maps source columns to target template fields, applying necessary transformations (UOM mapping, boolean formatting).
7.  **Validation & Learning**: If a mapping is ambiguous, the agent prompts the user for correction and records the "learned" rule for future automation.
8.  **Output Generation**: The agent generates a final, validated CSV file ready for Propello import.

## Data Standards

- **Boolean Values**: Propello typically requires `True`/`False` strings.
- **Store Numbers**: Handled as integers, usually starting with `1`.
- **Header Integrity**: Target file headers must match `PropelloTemplates` exactly; no columns should be added or removed.
- **Unit of Measure (UOM)**: Pay close attention to mapping vendor UOMs (e.g., "EA", "BX", "PK") to Propello's `Stocking UOM` and `Selling UOM`.

## Development & Usage

The implementation of the conversion engine will use **C# / .NET 10**. The initial phase focuses on developing the "Agent" capability to interpret data, followed by building a decoupled C# Class Library that can eventually support a **Blazor** or **.NET MAUI** UI.
