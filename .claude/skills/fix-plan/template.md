<!--
Template for FIX_PLAN.md. Replace bracketed placeholders. Every checkbox should cite a finding ID
from REVIEW_REPORT.md (e.g. "(REVIEW #3)") or be tagged [FEATURE]. Keep checkboxes commit-sized.
-->

# Fix Plan — [Project Name]

**Based on:** `REVIEW_REPORT.md` ([date]) · **Status:** [Draft / Confirmed / In progress / Done]
**Time budget:** [stated/implied deadline, or "none given"] — if time runs out, stop after the
highest phase you can fully complete; do not leave a phase half-done to start the next one.

## Execution Contract

*Read this before doing any work below, in this conversation or a future one.*

- Check boxes off (`- [x]`) in this file as each task completes — this file is the source of
  truth, not the in-session task tracker.
- Work phases in order. At the end of each phase, run `/verify` and do a direct self-review of the
  phase's diff (read it, reason through correctness, confirm the build/tests still pass), then
  explicitly confirm: every task in the phase was actually implemented as intended (not just
  checked off), no other existing behavior broke as a result (**critical** — a regression found
  here blocks moving on, it does not get deferred to a later phase), and every sub-task box in the
  phase is honestly marked before you check off that phase's own checkpoint box. Only start the
  next phase once all three hold. **Don't invoke `/code-review` as part of this** — its
  multi-agent-swarm process is resource-heavy and not meant for a recurring per-phase (or even
  whole-cycle) checkpoint; a direct self-review is the checkpoint here.
- If a task turns out to be bigger than expected, split it into new checkboxes rather than
  quietly expanding scope inside one commit.
- If you deviate from this plan (skip something, descope something, reorder something), say so
  in chat and update this file to match — don't let the file silently go stale.

## Project Summary

[2-3 sentences: what this application does, what stack it's built on, and what state it's in
today per the review.]

## Goals

- [Restore correct behavior for the Critical/High findings from the review]
- [Deliver requested feature(s), if any — name them]
- [Leave the codebase measurably easier to reason about than it was found, without rewriting
  what already works]

## Success Criteria

- [ ] All Critical and High findings from `REVIEW_REPORT.md` are fixed or explicitly deferred with
      a stated reason.
- [ ] All requested features are implemented and covered by tests.
- [ ] Full test suite passes (`[test command(s)]`).
- [ ] Whole-diff self-review (during `/wrap-up`) surfaces no new Critical/High issues — a direct
      read-through of the full diff, not a `/code-review` invocation.
- [ ] [Any task-specific acceptance criteria stated by the interviewer/task description.]

## Non-Goals / Out of Scope

- [Anything explicitly not being touched — e.g. "not migrating the test framework even though
  NSubstitute is preferred, because the repo already has consistent Moq usage"]
- [Any Low-severity findings being deliberately deferred, and why]
- [Explicitly: no speculative refactors, no new abstractions beyond what's needed for the stated
  goals]

## Code Standards & Guidelines

**DRY:** [concrete duplication being addressed, if any — cite finding IDs]

**KISS:** [where restraint is being deliberately applied — what NOT to abstract]

**SOLID (S, O, L, I — D has its own field below):** [finding → principle mapping, e.g. "Finding #4
(REVIEW #4): Single Responsibility — split X out of Y"]

**Dependency Inversion:** [concrete-typed dependency found in the review, and the interface being
introduced/used in its place so it can be swapped or mocked — cite finding IDs]

**Function size & clarity:** [oversized or mixed-abstraction-level functions found in the review,
and how each is being split — one level of abstraction per function, ~30-35 lines max]

**Self-documenting names over comments:** [comment-heavy code being replaced with extracted,
clearly-named functions/variables, if any — cite finding IDs]

## Stack Conventions & Best Practices Checklist

State the convention once, then check it off as it's actually applied — don't restate the same
default twice in different words.

- [ ] Backend data access: [repository pattern, or "match existing: ___"] — applied to new/changed
      data access
- [ ] Backend validation: [FluentValidation, or "match existing: ___"] — applied to new/changed
      endpoints
- [ ] Backend tests: [xUnit + FluentAssertions + NSubstitute, or "match existing: ___"] — new/changed
      logic covered
- [ ] Frontend tests: [repo's existing runner — Jasmine/Karma or Jest] — new/changed
      components/services covered
- [ ] Frontend components: [standalone + OnPush for anything newly written, or "match existing: ___"]
- [ ] No secrets or environment-specific values committed as part of this work
- [ ] Docs updated to match the change (README / CLAUDE.md / API docs) where the change makes
      existing documentation inaccurate — don't leave docs describing the old behavior

## Plan

### Phase 0 — Setup
- [ ] If this isn't already a git repo (some take-home tasks arrive as a plain folder/zip), run
      `git init` and commit the as-given state first, so everything from here on is tracked
      incrementally
- [ ] Create working branch: `[branch-name]`
- [ ] Confirm build succeeds and record baseline test pass/fail counts on the starting commit

### Phase 1 — Critical fixes
- [ ] [Task] (REVIEW #[n])
- [ ] [Task] (REVIEW #[n])
- [ ] Self-review + `/verify` on Phase 1 changes

### Phase 2 — High-severity fixes
- [ ] [Task] (REVIEW #[n])
- [ ] Self-review + `/verify` on Phase 2 changes

### Phase 3 — Medium/Low fixes
- [ ] [Task] (REVIEW #[n])
- [ ] Self-review + `/verify` on Phase 3 changes

### Phase 4 — Requested features
- [ ] [Feature name] — backend [FEATURE]
- [ ] [Feature name] — frontend [FEATURE]
- [ ] Self-review + `/verify` on Phase 4 changes

### Phase 5 — Tests & verification
- [ ] Close testing gaps noted in `REVIEW_REPORT.md`'s coverage snapshot
- [ ] Full suite passes end to end
- [ ] Manually exercise the golden path (and key edge cases) per the `/verify` skill

### Phase 6 — Wrap-up
- [ ] Run `/wrap-up`
