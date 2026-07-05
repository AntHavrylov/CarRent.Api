<!--
Template for REVIEW_REPORT.md. Replace bracketed placeholders. Delete any severity section that
has zero findings rather than leaving it empty. Keep one entry per finding — don't bundle
unrelated issues together just to save space.

Never paste an actual secret value (key, password, connection string) into this file — describe
it by file:line and type instead. This file is written into the repo; reproducing the secret here
re-commits it.
-->

# Review Report — [Project Name]

**Reviewed:** [date] · **Scope:** [backend / frontend / both] · **Reviewer:** Claude Code

## Summary

[2-4 sentences: overall state of the codebase, the one or two things that matter most, and
whether the architecture is fundamentally sound with fixable gaps vs. structurally compromised.]

## Findings

Ordered most-severe first within each band.

### 🔴 Critical

**[N]. [One-line title]** — `[file:line]`
[What's wrong, in concrete terms.]
**Failure scenario:** [specific input/state → specific wrong outcome]
**Fix direction:** [one or two sentences, not a full implementation]

### 🟠 High

**[N]. [One-line title]** — `[file:line]`
[...]
**Failure scenario:** [...]
**Fix direction:** [...]

### 🟡 Medium

**[N]. [One-line title]** — `[file:line]`
[...]
**Failure scenario:** [...]
**Fix direction:** [...]

### 🟢 Low

**[N]. [One-line title]** — `[file:line]`
[...]

## What's already good

[Don't skip this — call out genuinely solid design decisions by name. It's part of an honest
review, and it stops the fix-plan phase from "fixing" things that aren't broken.]

## Test coverage snapshot

[What's actually covered vs. what test projects merely exist. Be explicit about empty scaffolds.]
