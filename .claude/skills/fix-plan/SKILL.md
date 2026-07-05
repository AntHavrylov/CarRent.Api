---
name: fix-plan
description: Turn REVIEW_REPORT.md (plus any requested new features) into a checkbox-driven execution plan — goals, success criteria, non-goals, coding standards, and a phased task list. Produces FIX_PLAN.md. Does not implement anything itself.
---

# fix-plan

Turn a completed `REVIEW_REPORT.md` into `FIX_PLAN.md`: a concrete, checkbox-driven spec the
user can execute step by step in Claude Code, and that a reviewer/interviewer can read as
evidence of structured thinking.

## Process

1. **Require `REVIEW_REPORT.md` to exist — with one exception.** If it's missing, tell the user to
   run `/review-report` first and stop. The only exception is a task that is explicitly pure
   feature-add with nothing to fix (the user says so directly) — in that case skip straight to
   drafting a plan with an empty/omitted findings-derived phase, but still say out loud that
   you're skipping the review so it's a visible decision, not a silent gap.
2. **Clarify scope before drafting**, if not already stated:
   - Are there new features requested beyond fixing the review's findings? What are they,
     specifically?
   - **Time budget.** Is there a stated or implied deadline (take-home tasks usually have one)?
     If so, the phase order (Critical → High → Medium/Low → Features) doubles as the triage line —
     make it explicit in the plan where that line falls if time runs out, rather than leaving it
     implicit.
   - Any other hard constraints: libraries that must not change, parts of the code that are
     off-limits, whether backward compatibility with existing data/API contracts matters.
   - Use the AskUserQuestion tool for this if it's genuinely ambiguous — don't guess at scope for
     an evaluated task.
3. **Survey existing conventions before prescribing new ones.** Check what test framework,
   validation library, and data-access pattern the repo already uses. The "default preferred
   stack" below is a fallback for gaps, not a mandate to replace things that already work — ripping
   out an existing, working pattern to replace it with a "better" one is scope creep and a DRY/KISS
   violation in its own right, unless the review specifically flagged the existing approach as a
   finding.
4. **Draft `FIX_PLAN.md`** following `template.md` in this skill folder, filling in every section:
   - Project Summary, Goals, Success Criteria, Non-Goals — see template for what belongs in each.
   - Code Standards & Guidelines — state every standard listed in the "Code Standards section"
     below as a concrete rule for *this* codebase (not an abstract definition), plus the
     stack-specific conventions decided in step 3. Don't skip any of them just because a given
     review turned up no finding for it — say so explicitly instead ("no Dependency Inversion
     issues found").
   - Phased checklist (see phase structure below). Every checkbox must cite the finding ID from
     `REVIEW_REPORT.md` it addresses, or be tagged `[FEATURE]` if it's new-feature work.
5. **Keep checkboxes commit-sized.** If a checkbox would touch more than one concern, split it.
6. **Fill in the "Execution Contract" section at the top of the template** — don't skip it. It's
   the part of this file that a *later, separate* conversation will actually be reading while
   executing the plan (there is no dedicated "execute" skill), so it needs to carry its own
   instructions rather than relying on this skill's process being remembered. Mirror the checklist
   into TaskCreate/TaskUpdate for in-session progress tracking too, but `FIX_PLAN.md` is the
   durable source of truth — boxes get checked (`- [x]`) in the file itself as work completes.
7. **Wire in checkpoints, don't reinvent them.** At the end of each phase, the plan should tell
   the executor to run `/verify` and do a direct self-review of the phase's diff (read it, reason
   through correctness, confirm the build/tests still pass) before checking off that phase's
   summary box. **Do not wire in `/code-review` as a per-phase or whole-diff checkpoint at all** —
   its multi-agent-swarm process is resource-heavy, and running it repeatedly (or even once over a
   whole cycle) has drawn explicit user pushback as excessive for what this kind of iterative
   checkpoint needs. A direct self-review by whoever is executing the plan is the checkpoint here,
   not a separate skill invocation. The checkpoint itself is more than just running `/verify`: it
   also means confirming every task in the phase actually landed as intended (not just got checked
   off), that nothing else broke as a result — treat any regression found here as blocking, not
   something to defer to a later phase — and that every sub-task box in the phase is honestly
   marked before moving on.
8. **This skill only produces/updates the plan.** Do not start implementing fixes as part of
   running it. Show the user the drafted plan and confirm before execution begins.

## Phase structure (adapt counts to what the review actually found)

Default order is severity-first, features last — but if the requested features are a stated hard
requirement of the task (not a stretch goal) and the time budget is tight, consider moving
Phase 4 ahead of Phase 3: landing a required feature usually matters more to an evaluator than
polishing Low-severity cleanup. Make that reordering decision explicitly in the drafted plan
(state it in the Project Summary or a note under this heading) rather than silently defaulting.

- **Phase 0 — Setup:** `git init` first if the task wasn't handed over as a git repo already,
  then branch, confirm build/test run cleanly before any changes, note baseline test pass/fail
  counts.
- **Phase 1 — Critical fixes:** security/data-loss findings, one checkbox per finding.
- **Phase 2 — High-severity fixes.**
- **Phase 3 — Medium/Low fixes:** group small related cleanups into single checkboxes where they
  genuinely belong together; don't over-split trivial renames.
- **Phase 4 — Requested features:** one checkbox per feature, broken into sub-checkboxes if a
  feature spans backend + frontend.
- **Phase 5 — Tests & verification:** close testing gaps identified in the review, then a final
  full-suite run.
- **Phase 6 — Wrap-up:** run `/wrap-up`.

## Code Standards section — what to actually write (not just name-drop the acronyms)

- **DRY:** name the specific duplication found in the review (if any) and state the shared
  abstraction to introduce — don't state DRY as a slogan with no concrete referent.
- **KISS:** explicitly note where *not* to add abstraction — e.g. "don't introduce a generic
  repository-of-repositories base class for two collections."
- **SOLID (S, O, L, I — D has its own bullet below):** call out which finding(s) map to which
  principle (e.g. "Finding #4 is a Single Responsibility violation — split X out of Y").
- **Dependency Inversion (interfaces over concrete types):** consumers should depend on an
  abstraction, not a concrete implementation — so the implementation can be swapped or mocked
  without changing the consumer. Name the specific concrete-typed dependency found in the review
  (a constructor param typed as a class instead of an interface, or an in-method `new` of a
  dependency) and the interface being introduced/used instead.
- **Function size & clarity:** functions/methods stay short (roughly 30-35 lines max) and operate
  at a single level of abstraction — don't interleave high-level orchestration with low-level
  detail in the same function. Name the specific oversized/mixed-abstraction functions found in
  the review and how each is being split.
- **Self-documenting names over comments:** a comment explaining *what* a block of code does is a
  signal to extract that block into a well-named function or variable instead — the name should
  make the comment unnecessary. Comments are still fine for *why* (a non-obvious constraint, a
  workaround), just not as a substitute for a clear name.

## Default preferred stack (fallback only — see step 3)

- **Backend (.NET):** Repository pattern for data access, FluentValidation for request
  validation, xUnit + FluentAssertions + NSubstitute for tests.
- **Frontend (Angular):** whatever test runner is already configured (Jasmine/Karma or Jest);
  standalone components and `OnPush` for anything newly written.
