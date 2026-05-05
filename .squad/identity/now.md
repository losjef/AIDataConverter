---
updated_at: 2026-05-05T15:05:00-04:00
focus_area: Continue Phase 2 by adding the first Propello XLSX metadata reader beside the landed CSV header reader
active_issues: []
resume_file: .squad/identity/resume-next.md
---

# What We're Focused On

The first Propello template-reader slice is now in place: `ProductImport.csv` header extraction is implemented as a reusable service, wired into the MAUI shell, and covered by a focused test.

The next concrete implementation step is to add the paired Propello XLSX metadata reader for `ProductImport.xlsx`, using the new CSV reader as the local anchor before moving on to schema validation.

Read `.squad/identity/resume-next.md` before resuming deeper work.
