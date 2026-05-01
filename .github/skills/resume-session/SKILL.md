---
name: "resume-session"
description: "Resume work from the repo-local handoff with minimal searching by pulling latest changes first, then reading the squad identity resume files and only the exact project docs needed. Use when starting a new day, resuming on another computer, or asking where to pick back up."
domain: "workflow"
confidence: "high"
source: "project rule"
---

## Purpose

Use this skill at the start of a new session so the agent can resume work without broad repo scanning.

## Required order

1. Sync the repository first.
2. Read the repo-local resume state.
3. Open only the files named in the resume note.
4. Continue from the exact next step unless the user redirects.

## Resume workflow

### 1. Sync before reasoning
- Run `git status --short`.
- If the worktree is clean, run `git pull`.
- If the worktree is not clean, report that immediately before doing more work.
- Treat successful pull/sync as a prerequisite for cross-machine continuity.

### 2. Read the authoritative resume files
- Read `.squad/identity/now.md`.
- Read `.squad/identity/resume-next.md`.

### 3. Read only the named project docs
- Open the exact docs and code files listed in `.squad/identity/resume-next.md`.
- Avoid broad repo exploration unless the resume note is stale or contradicted by current files.

### 4. Re-anchor validation
- Reuse the most recent verified command from the resume note when practical.
- If code changed after the note was written, rerun the narrowest relevant validation.

## Guardrails

- Do not start with a whole-repo search when the resume note already names the next file and next step.
- Do not assume local state is current across machines until `git pull` succeeds or the user confirms an intentional divergence.
- If `.squad/identity/resume-next.md` is missing or stale, create or refresh it before doing broad work.