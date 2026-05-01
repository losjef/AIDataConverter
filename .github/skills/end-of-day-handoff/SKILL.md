---
name: "end-of-day-handoff"
description: "Create the repo end-of-day handoff, set tomorrow's resume point, and sync the branch with git commit, pull --rebase, and push. Use when ending the day, wrapping up work, switching computers, or wanting tomorrow's restart to be low-token and deterministic."
domain: "workflow"
confidence: "high"
source: "project rule"
---

## Purpose

Use this skill when the user is done for the day and wants the repository left in a resumable, cross-machine-safe state.

## Required outcomes

1. Update the authoritative repo-local resume state in `.squad/identity/now.md`.
2. Update the detailed next-step handoff in `.squad/identity/resume-next.md`.
3. Reconcile any documentation that changed during the session.
4. Capture the best next implementation step with exact files to open first.
5. Sync the repository so another machine can resume from the same state:
   - inspect `git status`
   - stage the intended changes
   - create a commit
   - run `git pull --rebase`
   - run `git push`
6. Report what validation actually ran and what still needs validation.

## Handoff checklist

### 1. Verify repo state
- Read `.squad/identity/now.md` and `.squad/identity/resume-next.md` if they exist.
- Capture `git status --short`.
- Capture the latest commit with `git log -1 --pretty=format:"%H%n%ad%n%s" --date=iso-strict`.

### 2. Reconcile docs
- Update any repo docs affected by the session.
- If editable knowledge assets changed materially, update `AgentAssets/KnowledgeBase/README.md` in the same change.

### 3. Write the resume point
- `.squad/identity/now.md` should contain the concise current focus.
- `.squad/identity/resume-next.md` should contain:
  - summary of today's completed work
  - exact files changed
  - last successful validation
  - open risks or deferred work
  - the exact first step for the next session
  - the exact files to open first

### 4. Sync for cross-machine use
- Do not stop at a local note.
- If the user wants end-of-day state preserved across machines, the branch must be committed and pushed.
- Preferred order:
  1. `git add ...`
  2. `git commit -m "..."`
  3. `git pull --rebase`
  4. `git push`
- If pull or push fails, report the blocker explicitly in the handoff note.

### 5. Final report
- State the branch.
- State whether the repo is clean after sync.
- State the latest validation that actually succeeded.
- State the first instruction for tomorrow in one sentence.

## Guardrails

- Do not invent a vague resume point like "continue implementation."
- Do not claim the repo is cross-machine ready unless commit, pull/rebase, and push were attempted.
- Do not hide dirty worktree state.
- Keep the next step narrow and code-adjacent.