<!--
Template for FIX_PLAN.md. Replace bracketed placeholders. Every checkbox should cite a finding ID
from REVIEW_REPORT.md (e.g. "(REVIEW #3)") or be tagged [FEATURE]. Keep checkboxes commit-sized.
-->

# Fix Plan — CarRent.Api

**Based on:** `REVIEW_REPORT.md` (2026-07-05) · **Status:** Draft
**Time budget:** none given — work through all phases in full severity order.

**Scope assumptions (clarifying questions were declined, so these are stated explicitly rather
than guessed silently):**
- No new features are in scope — this plan only fixes the 15 findings in `REVIEW_REPORT.md`
  (findings #14 and #15 were added on a later review pass — missing indexes and a
  local-time-vs-UTC date validation bug — and are folded into Phase 3 below).
- Finding #6 (.NET 7 is EOL) **is included** — retargeting to `net8.0` is the recommended default
  for a High finding with no stated hosting constraint blocking it. If there turns out to be an
  external constraint (e.g. a hosting environment pinned to .NET 7), skip Phase 2's upgrade task
  and note it as deferred instead.
- Finding #5's SQL-injection fix is scoped to closing the vulnerability (whitelisting
  `SortField`), not to finishing the half-built sort feature end-to-end (wiring the request's
  `SortBy` through to `GetAllCarsOptions.SortField`) — that would be new functionality, not a fix,
  per the no-new-features assumption above. See Non-Goals.

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

CarRent.Api is a layered ASP.NET Core 7 Web API (Contracts / Application / Api, plus a standalone
`Identity.Api` JWT-minting helper) backed by Postgres via Dapper, with FluentValidation and
Mapster used consistently across the request pipeline. The review found the architecture sound
but surfaced a committed shared JWT signing secret (full auth-bypass risk), a broken
object-level-authorization gap on order updates, an unhandled-exception bug on two "get by id"
lookups, a dormant SQL-injection vector, and an EOL target framework, plus several
medium/low-severity correctness and cleanup items.

## Goals

- Close both Critical findings (committed JWT key and DB credentials) so the API can no longer be
  trivially impersonated or have its database accessed by anyone with repo read access.
- Fix all four High findings: the order-update IDOR, the `QuerySingleAsync` crash-instead-of-404
  bug (plus adding a catch-all exception handler), the dormant SQL injection in car sorting, and
  the EOL `net7.0` target.
- Clean up the Medium/Low findings without introducing new abstractions beyond what each fix
  needs.
- Leave the codebase measurably easier to reason about than it was found, without rewriting what
  already works (see "What's already good" in `REVIEW_REPORT.md`).

## Success Criteria

- [ ] All Critical and High findings from `REVIEW_REPORT.md` (#1-#6) are fixed or explicitly
      deferred with a stated reason.
- [ ] All Medium/Low findings (#7-#15) are fixed or explicitly deferred with a stated reason.
- [ ] No new features are introduced (none were requested).
- [ ] Full test suite passes (`dotnet test` from the solution root, and each project
      individually: `dotnet test CarRent.Application.Tests.Unit`, `dotnet test
      CarRent.Api.Tests.Unit`).
- [ ] `dotnet build` succeeds for the whole solution, including `Identity.Api`.
- [ ] Whole-diff self-review (during `/wrap-up`) surfaces no new Critical/High issues — a direct
      read-through of the full diff, not a `/code-review` invocation.
- [ ] No secret values (JWT key, DB credentials) remain committed anywhere in the tree, including
      `Identity.Api`.

## Non-Goals / Out of Scope

- Wiring `GetAllCarsRequest.SortBy` through to `GetAllCarsOptions.SortField` end-to-end (finding
  #5's fix whitelists the column so it *can't* be exploited if this is wired up later, but
  actually shipping client-driven sorting is new functionality, not part of this plan).
- Any new features beyond the 13 findings — none were requested.
- Replacing Dapper/repository pattern, FluentValidation, or the xUnit/FluentAssertions/NSubstitute
  test stack — all already work and aren't implicated in any finding.
- A generic secrets-manager integration (e.g. Azure Key Vault, HashiCorp Vault client code) — out
  of scope for a project with no existing secrets-manager dependency; user-secrets (dev) +
  environment variables (anywhere else) is enough to close findings #1/#2 without adding new
  infrastructure.
- Rewriting the `Identity.Api` helper into something production-grade — it stays a local
  dev-only token minting tool per `CLAUDE.md`; the fix is only to stop hardcoding its secret and
  keep it in sync with `CarRent.Api`'s.

## Code Standards & Guidelines

**DRY:** Finding #9's fix duplicates the car-rating-average join between `GetAllAsync` and
`GetByIdAsync` today; extract the shared `left join ratings ... group by id`-style query fragment
(or just add the same join to `GetByIdAsync`) so both code paths compute `Rating` identically
instead of drifting again later.

**KISS:** Don't introduce a generic base-repository class for the four repositories while fixing
the transaction/query bugs (#4, #7) — each repository stays a plain Dapper class, only the
specific buggy methods change. Don't build a generic secrets-provider abstraction for #1/#2 —
`dotnet user-secrets` + environment variables is enough.

**SOLID (S, O, L, I):** No Single Responsibility / Open-Closed / Liskov / Interface Segregation
violations were flagged in the review — controllers, services, and repositories are already
narrowly scoped one-per-aggregate per `CLAUDE.md`'s documented convention, and that pattern is
preserved as-is while fixing findings #3 and #4 inside the existing service/repository methods.

**Dependency Inversion:** No concrete-typed dependency or in-method `new` of a service was found
in the review — every repository/service is already consumed via its interface
(`ICarsRepository`, `IOrdersService`, etc.) and DI registrations already point at interfaces. No
changes needed here.

**Function size & clarity:** No oversized or mixed-abstraction-level functions were flagged. The
fixes in this plan (ownership check in `OrdersService.UpdateAsync`, whitelist lookup in
`CarsRepository.GetAllAsync`, transaction wrapping in the two `DeleteByIdAsync` methods) are all
small, targeted additions to existing short methods — none should push any method past ~30 lines;
split further if a fix grows larger than expected.

**Self-documenting names over comments:** Finding #11 is exactly this class of issue —
`RatingsRepository.ExistsCarRatingForUser` actually checks for an existing *order*, not a rating.
Renaming it to `HasOrderForCarAsync` removes the need for any clarifying comment. Finding #12's
typo fixes (`CreateOrUpdateCatRequestValidator` → `CreateOrUpdateCarRequestValidator`,
`CreateOrUpdateUserRequestValodator` → `CreateOrUpdateUserRequestValidator`) are the same
category of naming cleanup.

## Stack Conventions & Best Practices Checklist

- [ ] Backend data access: match existing — Dapper + repository-per-aggregate pattern, applied to
      the transaction (#7) and query (#4, #5, #9) fixes
- [ ] Backend validation: match existing — FluentValidation, applied to the pagination bounds
      (#8) and car-slug-uniqueness (#13) additions
- [ ] Backend tests: match existing — xUnit + FluentAssertions + NSubstitute, applied to every new
      test added in Phase 5
- [ ] No frontend in this repo — n/a
- [ ] No secrets or environment-specific values committed as part of this work (closes #1, #2)
- [ ] Docs updated to match the change: `CLAUDE.md`'s "Running the API requires..." section and
      `README.md`'s "Getting Started" should mention `dotnet user-secrets` setup once #1/#2 land,
      and both should reflect `net8.0` once #6 lands

## Plan

### Phase 0 — Setup
- [ ] Create working branch: `fix/review-findings`
- [ ] Confirm `dotnet build` succeeds and record baseline `dotnet test` pass/fail counts on the
      starting commit (master @ `d4b4636`)

### Phase 1 — Critical fixes
- [ ] Remove the hardcoded JWT signing key from `CarRent.Api/appsettings.Development.json` and
      `Helpers/Identity.Api/Controllers/IdentityController.cs`; move it to `dotnet user-secrets`
      for local dev (both projects must share the same key value in their respective user-secrets
      stores) and document the setup step in `README.md`. Generate a new, high-entropy key value
      (do not reuse the leaked one). (REVIEW #1)
- [ ] Remove the committed Postgres connection string/credentials from
      `CarRent.Api/appsettings.json` and `appsettings.Development.json`; move to `dotnet
      user-secrets` for local dev, leaving only a non-functional placeholder shape in the
      committed `appsettings.json` (e.g. empty `ConnectionString`) plus a `README.md` note on how
      to supply it. Rotate the password if this local Postgres instance is reachable from
      anywhere beyond localhost. (REVIEW #2)
- [ ] Self-review + `/verify` on Phase 1 changes — specifically confirm the app still starts
      locally with secrets supplied via user-secrets, and that `git grep` for the old key/password
      strings returns nothing in the working tree

### Phase 2 — High-severity fixes
- [ ] Fix the order-update IDOR: in `OrdersService.UpdateAsync`
      (`CarRent.Application/Services/OrdersService.cs:46-55`), fetch the existing order via
      `GetByIdAsync` first and return `null` if `existingOrder.UserId != order.UserId`, before
      calling `UpdateAsync` — mirroring the ownership check `CancelAsync` already does correctly.
      Add a unit test on `OrdersService` covering both the "not your order" rejection and the
      happy-path update. (REVIEW #3)
- [ ] Fix `CarsRepository.GetByIdAsync` and `UsersRepository.GetByIdAsync` to use
      `QuerySingleOrDefaultAsync` instead of `QuerySingleAsync`, matching
      `OrdersRepository.GetByIdAsync`'s existing correct pattern. (REVIEW #4)
- [ ] Add a catch-all exception-handling middleware (e.g. `app.UseExceptionHandler(...)` mapping
      unhandled exceptions to a generic `500` `ProblemDetails` response) registered in
      `Program.cs` alongside the existing `ValidationMappingMiddleware`, so any future unhandled
      exception degrades gracefully instead of leaking internals. (REVIEW #4)
- [ ] Whitelist `GetAllCarsOptions.SortField` in `CarsRepository.GetAllAsync` against a fixed set
      of known column names (e.g. a `switch` mapping an allowed set of strings to the literal
      column name, defaulting to no `order by` clause or throwing a validation error for anything
      unrecognized) instead of interpolating the raw value into the SQL string. Add a repository-
      or service-level test asserting an unrecognized/malicious `SortField` value does not reach
      the SQL string unescaped. (REVIEW #5)
- [ ] Retarget all six `.csproj` files (`CarRent.Api`, `CarRent.Application`, `CarRent.Contracts`,
      `CarRent.Api.Tests.Unit`, `CarRent.Application.Tests.Unit`, `Helpers/Identity.Api`) from
      `net7.0` to `net8.0`, and bump `Microsoft.AspNetCore.*`, `Microsoft.Extensions.*`, and any
      other EOL-line packages to their `net8.0`-compatible versions. Confirm `dotnet build` and
      `dotnet test` still pass after the bump. (REVIEW #6)
- [ ] Self-review + `/verify` on Phase 2 changes

### Phase 3 — Medium/Low fixes
- [ ] Wrap the three sequential deletes in `CarsRepository.DeleteByIdAsync` and
      `UsersRepository.DeleteByIdAsync` in a single `IDbTransaction` (begin before the first
      delete, commit after the last, rollback on exception) so a partial failure can't orphan
      child rows. (REVIEW #7)
- [ ] Add `Page` (`>= 1`) and `PageSize` (e.g. inclusive `1-100`) validation rules to
      `GetAllCarsRequestValidator`, and add validator tests for the boundary values (`0`,
      negative, and an oversized `PageSize`). (REVIEW #8)
- [ ] Make `CarsRepository.GetByIdAsync` compute `Rating` the same way `GetAllAsync` does (reuse
      the same joined query, or extract a shared fragment) so single-car and list responses never
      disagree on a car's average rating. (REVIEW #9)
- [ ] Remove the unused `CancellationToken token = default` constructor parameter from
      `CreateOrUpdateOrderRequestValidator`, `CreateRatingValidator`, and `CreateUserValidator` —
      the real token already flows through each `MustAsync` lambda's own parameter. (REVIEW #10)
- [ ] Rename `IRatingsRepository.ExistsCarRatingForUser` / `RatingsRepository.ExistsCarRatingForUser`
      to `HasOrderForCarAsync` (and update the one call site in `CreateRatingValidator`) to match
      what the query actually checks. (REVIEW #11)
- [ ] Fix the typo'd validator file/class names: `CreateOrUpdateCatRequestValidator` →
      `CreateOrUpdateCarRequestValidator`, `CreateOrUpdateUserRequestValodator` →
      `CreateOrUpdateUserRequestValidator` (rename files to match), and update any references.
      (REVIEW #12)
- [ ] Add a car-slug uniqueness rule to the car validator (mirroring
      `CreateUserValidator`'s email-uniqueness pattern), using `ICarsRepository`, so the
      `Conflict()` branch in `CarsController.Create` becomes reachable for actual duplicate cars
      instead of being dead code. (REVIEW #13)
- [ ] Add indexes in `DbInitializer` for `cars(slug)`, `cars(yearofproduction)`,
      `orders(user_id)`, `users(email)`, and `ratings(car_id)` so the query patterns each
      repository actually uses (list/search, "my orders", email uniqueness, rating average) get
      an index seek instead of a full table scan. (REVIEW #14)
- [ ] Standardize order date handling on UTC: switch `CreateOrUpdateOrderRequestValidator`'s
      `DateFrom`/`DateTo` comparison from `DateTime.Now` to `DateTime.UtcNow` (or migrate the
      request/model to `DateTimeOffset`), and confirm inbound timestamps are normalized to UTC
      before validation/persistence so the comparison isn't sensitive to the API server's local
      timezone. Add a validator test pinning this behavior. (REVIEW #15)
- [ ] Self-review + `/verify` on Phase 3 changes

### Phase 4 — Requested features
*(none — no new features were requested for this pass; skipped per the Non-Goals section above)*

### Phase 5 — Tests & verification
- [ ] Add controller-level tests for `CarsController`, `OrdersController`, and
      `CarRatingsController` (currently zero coverage per `REVIEW_REPORT.md`'s test-coverage
      snapshot), at minimum covering the paths touched in this plan: order update
      ownership-rejection (#3), car/user `GetById` not-found (#4), and car list sort/pagination
      bounds (#5, #8)
- [ ] Add service-level tests for `CarsService` and `UsersService` `UpdateAsync` not-found paths
      (currently untested, same gap class as the `OrdersService` bug in #3)
- [ ] Full suite passes end to end: `dotnet test` from the solution root
- [ ] Manually exercise the golden path (and key edge cases) per the `/verify` skill: create a
      car/user, create an order as one user, attempt to update another user's order and confirm
      it's rejected, look up a nonexistent car/user id and confirm a clean 404, hit the cars list
      with `page=0` and an oversized `pageSize` and confirm validation errors instead of a 500

### Phase 6 — Wrap-up
- [ ] Run `/wrap-up`
