# Knowledge Base Manual

This folder contains editable knowledge assets that the application uses to store matching behavior, destination-schema memory, and changeable workflow rules.

These files are meant to be readable by both the application and a human operator. They are part of the product's memory layer, not hidden internal implementation details.

## What Is In This Folder

### `semantic_dictionary.json`

Purpose:
Stores learned header-matching behavior, shared concepts, vendor-specific rules, default rules, and normalization guidance.

How to read it:
- `schema_version`: version of the file structure
- `description`: high-level purpose of the asset
- `governance`: global rules about review order, statuses, and confidence interpretation
- `cross_vendor_seed_behavior`: how the system reuses known patterns before escalating to AI
- `shared_concepts`: reusable canonical concepts like `Item` or `UPC`
- `vendors`: vendor-specific rule groups and learned mappings

Important editable sections:
- `shared_concepts`
- `vendors.{vendor}.rule_sets`
- `mapping_rules`
- `default_rules`
- `normalization_rules`

Editing guidance:
- Prefer changing data, not structure, unless the schema itself is intentionally evolving.
- Preserve `status`, `confidence`, and `provenance` when editing an existing rule.
- If a rule becomes obsolete, mark it `deprecated`, `retired`, or `replaced` instead of deleting history.
- If a mapping is uncertain, do not invent a value. Ask for clarification first.

### `propello_template_memory.json`

Purpose:
Stores lightweight knowledge about Propello destination templates, shared destination concepts, and how templates relate to each other.

How to read it:
- `governance`: execution assumptions such as single-template default behavior and the canonical link field
- `shared_concepts`: destination concepts like `Item`, `ProductPricing`, or `PurchaseOrderLine`
- `templates`: per-template descriptions and canonical fields
- `relationships`: lightweight links that help the system understand how Propello works as a whole

Important editable sections:
- `shared_concepts`
- `templates`
- `relationships`

Editing guidance:
- Use this file to teach the system destination-side knowledge, not to force multi-template runtime behavior by default.
- Keep the current user-confirmed rules intact unless the user explicitly changes them.
- Avoid inventing broader template links without clarification.

### `baseline_schema_rules.json`

Purpose:
Stores changeable baseline-readiness rules such as workflow-specific required fields and dynamic time-series detection patterns.

How to read it:
- `dynamic_time_series.supported_period_formats`: allowed date-like header formats that should be treated as rolling fields
- `dynamic_time_series.quarter_pattern`: regex for quarter-style dynamic columns
- `workflow_profiles`: required baseline fields per workflow

Important editable sections:
- `dynamic_time_series`
- `workflow_profiles`

Editing guidance:
- Only change workflow requirements when the business rule for readiness actually changes.
- Treat format patterns carefully because broad patterns can accidentally classify stable columns as dynamic.

### `manual_links.json`

Purpose:
Stores user-approved manual cross-references when automated matching is not reliable enough and the operator explicitly decides how two records should be linked.

How to read it:
- `schema_version`: version of the file structure
- `description`: high-level purpose of the registry
- `governance`: review and lifecycle rules for manual links
- `links`: the actual approved link records

Important editable sections:
- `governance`
- `links`

Editing guidance:
- Only store links that were explicitly approved by the user.
- Treat this file as an exception registry, not a place to bulk-load guessed matches.
- Prefer lifecycle changes such as `deprecated`, `retired`, or `replaced` over deleting old decisions.
- Keep the source-side and target-side identifiers specific enough that a later user can understand why the link exists.

## General Editing Rules

- Keep files valid JSON.
- Keep names and field spellings consistent.
- Prefer additive, traceable edits over destructive cleanup.
- Preserve provenance and status information when possible.
- If you are unsure what a field means, stop and get clarification.

## Documentation Maintenance Rule

When any of these knowledge assets change materially, this manual must be updated as part of the same change.

Examples of material change:
- a new top-level section is added
- a new editable rule type is introduced
- the meaning of an existing field changes
- a new knowledge asset file is added to this folder
- the recommended editing process changes

The goal is that a user can open this folder, read this manual, and understand what each file is for, how to read it, and how to edit it safely.

## When Adding A New Knowledge Asset

If a new editable memory, parameter, matching, or audit file is added to this folder, update this manual in the same change and cover at least:

- the file name and purpose
- whether it is runtime-generated, seeded, or fully user-maintained
- the important top-level sections
- what fields are safe to edit directly
- what fields should be changed cautiously
- how the application uses the file
- any no-assumption or review-before-AI rules that apply to it

If the new file replaces or supersedes an older asset, document that relationship here instead of silently removing the older asset from the manual.