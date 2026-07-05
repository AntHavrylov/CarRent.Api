<!--
Template for FIX_PLAN.md. Replace bracketed placeholders. Every checkbox should cite a finding ID
from REVIEW_REPORT.md (e.g. "(REVIEW #3)") or be tagged [FEATURE]. Keep checkboxes commit-sized.
-->

# Fix Plan — CarRent.Api

**Based on:** `REVIEW_REPORT.md` (2026-07-05) · **Status:** Done
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
- **Do not run `git commit` between phases without the user reviewing first** (added after Phase
  1: the user wants to review the diff before each commit, not just before the branch is
  pushed). Do the work, do the self-review/`/verify` checkpoint, leave changes staged/unstaged in
  the working tree, and stop for the user's review before committing — do not batch multiple
  phases' work into one commit either; each phase is still its own commit once approved.

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

- [x] All Critical and High findings from `REVIEW_REPORT.md` (#1-#6) are fixed. (#3, #4, #5, #6
      fixed in Phases 0-2; #1, #2 fixed in Phase 1. A 16th finding, discovered mid-plan during
      Phase 5 — not part of the original sweep — was also fixed and logged.)
- [x] All Medium/Low findings (#7-#15) are fixed — none deferred.
- [x] No new features are introduced (none were requested; Phase 4 skipped).
- [x] Full test suite passes: `dotnet test` — **60/60 passing**, 0 failed, 0 skipped (verified
      fresh via `dotnet clean` + `dotnet build` + `dotnet test` at the end of Phase 6, not just
      incrementally per-phase).
- [x] `dotnet build` succeeds for the whole solution including `Identity.Api` — clean from a cold
      `dotnet clean`, only 6 pre-existing warnings (2× `CS8618`, 2× `CA2017`, 2× `xUnit1048`, none
      introduced by this work), 0 errors.
- [x] Whole-diff self-review at the end of Phase 6 (`git diff master..fix/review-findings
      --stat`, 46 files) surfaces no new Critical/High issues beyond what's already tracked as
      fixed in `REVIEW_REPORT.md`.
- [x] No secret values remain committed: `git grep` for both the old JWT key literal and the old
      DB password string returns nothing anywhere in the tree (checked at the end of Phase 6, not
      just Phase 1). `dotnet list package --vulnerable --include-transitive` also reports zero
      vulnerable packages across all six projects.

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
- [x] Docs updated to match the change: `README.md` now has a "Getting Started" step plus a new
      "Secrets Configuration" section covering `dotnet user-secrets` setup (#1/#2). `CLAUDE.md`
      could not be updated — it is untracked in git and is not present in the working tree in this
      environment (same transient-file issue observed earlier with `REVIEW_REPORT.md`); this is a
      pre-existing environment quirk unrelated to this fix pass, not something introduced by it.
      Neither doc needed a net8.0-specific update — neither mentioned the `net7.0` TFM by name.

## Plan

### Phase 0 — Setup
- [x] Create working branch: `fix/review-findings` (branched off `code-review-with-claude` @
      `d90205a`, which already carried the doc-only commits adding `REVIEW_REPORT.md`/
      `FIX_PLAN.md` on top of `master` @ `d4b4636` — not off `master` directly, since those docs
      were already committed there before this execution pass started)
- [x] Confirm `dotnet build` succeeds — it does (12 warnings, 0 errors), and the warnings already
      independently confirm REVIEW #6: `NU1903` (Npgsql 7.0.4, high-severity CVE) and `NU1902`
      (System.IdentityModel.Tokens.Jwt 6.26.1, moderate CVE), plus the SDK's own
      `NETSDK1138` EOL-framework warning on every `net7.0` project.
- [x] **Deviation, stated per the Execution Contract:** `dotnet test` cannot run at all in this
      environment — only the .NET 8 runtime is installed (no .NET 7 runtime), so every test host
      fails immediately with "You must install or update .NET to run this application." There is
      no baseline pass/fail count to record on `net7.0` in this sandbox. Since every later phase's
      checkpoint depends on being able to run tests, **REVIEW #6 (retarget to `net8.0`) is moved
      up from Phase 2 into this phase**, ahead of Phase 1, so a real baseline exists before any
      other fix lands. Severity order resumes normally (Phase 1 critical fixes next) once this is
      done.
- [x] Retarget all six `.csproj` files (`CarRent.Api`, `CarRent.Application`, `CarRent.Contracts`,
      `CarRent.Api.Tests.Unit`, `CarRent.Application.Tests.Unit`, `Helpers/Identity.Api`) from
      `net7.0` to `net8.0`. Bumped: `Npgsql` 7.0.4 → 10.0.3, `Microsoft.Extensions.DependencyInjection.Abstractions`
      7.0.0 → 10.0.9 (both multi-target down to net8.0 fine), `Microsoft.AspNetCore.Authentication.JwtBearer`
      7.0.10 → 8.0.28, `Microsoft.AspNetCore.OpenApi` 7.0.10/7.0.0 → 8.0.28 (both pinned to the
      8.0.x line specifically — these two packages are version-locked to their runtime major
      version, so the "latest" 10.0.9 resolved by a bare `dotnet add package` is NOT net8-compatible;
      had to pin explicitly), `System.IdentityModel.Tokens.Jwt` 6.26.1 → 8.19.1. Also discovered
      and fixed a second vulnerable-transitive-package issue while verifying: `xunit` 2.5.0 pulled
      in an ancient `NETStandard.Library 1.6.1` → `System.Net.Http`/`System.Text.RegularExpressions`
      4.3.0 chain (2 more High-severity CVEs, not in the original review) — bumped `xunit` 2.5.0 →
      2.9.3, `xunit.runner.visualstudio` 2.5.0 → 2.8.2, `Microsoft.NET.Test.Sdk` 17.7.2 → 17.14.1
      in both test projects, which drops the old chain entirely. (REVIEW #6 — moved up from Phase 2,
      see deviation note above)
- [x] Confirmed `dotnet build` and `dotnet test` succeed post-upgrade: build is clean (6 warnings,
      all pre-existing nullable/logging-template nits unrelated to this change, 0 errors);
      `dotnet list package --vulnerable --include-transitive` reports zero vulnerable packages
      across all six projects (down from 4 distinct CVEs: Npgsql, System.IdentityModel.Tokens.Jwt,
      System.Net.Http, System.Text.RegularExpressions). **Real baseline recorded: 22/22 tests
      passing** (5 in `CarRent.Application.Tests.Unit`, 17 in `CarRent.Api.Tests.Unit`), 0 failed,
      0 skipped.

### Phase 1 — Critical fixes
- [x] Removed the hardcoded JWT signing key from `CarRent.Api/appsettings.Development.json` and
      the `TokenSecret` const in `Helpers/Identity.Api/Controllers/IdentityController.cs` (now
      injects `IConfiguration` and reads `Jwt:Key`/`Jwt:Issuer`/`Jwt:Audience`, matching
      `CarRent.Api`'s existing pattern instead of duplicating the Issuer/Audience URLs a second
      time as separate hardcoded literals). Generated a new high-entropy key via `openssl rand
      -base64 64` (not the leaked value) and set it as `Jwt:Key` in both projects' `dotnet
      user-secrets` stores (`CarRent.Api` UserSecretsId `1dae518f-...`, `Identity.Api`
      `cd27ef5c-...`) — confirmed both resolve to the *same* value. Documented the setup in a new
      "Secrets Configuration" section in `README.md`. (REVIEW #1)
- [x] Removed the committed Postgres connection string from `CarRent.Api/appsettings.Development.json`
      (deleted entirely) and blanked it to `""` in `CarRent.Api/appsettings.json` (non-functional
      placeholder, loaded in every environment). Set the real value as `Database:ConnectionString`
      in `CarRent.Api`'s user-secrets store for local dev. Not rotating the password — confirmed
      it's a `localhost`-only value, not reachable beyond this machine. Documented in the same
      `README.md` section as above. (REVIEW #2)
- [x] Self-review + `/verify` on Phase 1 changes: `dotnet clean` + `dotnet build` confirms the
      same 6 pre-existing warnings as the Phase 0 baseline (2× CS8618, 2× CA2017, 2× xUnit1048,
      all unrelated to this phase's changes), 0 errors — no regression; `dotnet test` still
      22/22 passing. Ran `CarRent.Api` directly (`dotnet run --project CarRent.Api.csproj`) — it
      resolved `Database:ConnectionString` from user-secrets correctly and failed only at the
      expected point (`Npgsql...Connection refused` to `127.0.0.1:5432`, since no Postgres runs in
      this sandbox — not a config-resolution failure). Ran `Identity.Api` directly and called
      `POST /token` — it returned a well-formed JWT signed with the shared user-secrets key, with
      the correct `Issuer`/`Audience` claims sourced from config. `git grep` for both the old JWT
      key literal and the old DB password string returns nothing in the working tree. **Regression
      note:** none found — no other behavior changed.

### Phase 2 — High-severity fixes
- [x] Fixed the order-update IDOR: `OrdersService.UpdateAsync` now fetches the existing order via
      `GetByIdAsync` and returns `null` if it doesn't exist or `existingOrder.UserId !=
      order.UserId`, mirroring `CancelAsync`'s existing correct ownership check (previously it
      only checked `ExistsByIdAsync`, with no ownership check at all). Added
      `CarRent.Application.Tests.Unit/Services/OrdersServiceTests.cs` covering: rejection when the
      order belongs to another user, rejection when it doesn't exist, and the happy path when the
      caller owns it. (REVIEW #3)
- [x] Fixed `CarsRepository.GetByIdAsync` and `UsersRepository.GetByIdAsync` to use
      `QuerySingleOrDefaultAsync` instead of `QuerySingleAsync`, matching
      `OrdersRepository.GetByIdAsync`'s existing correct pattern — a lookup by a nonexistent id now
      returns `null` instead of throwing `InvalidOperationException`. (REVIEW #4)
- [x] Added a catch-all exception-handling middleware in `Program.cs` via
      `app.UseExceptionHandler(...)`, placed right after `MapHealthChecks` (wrapping
      `UseHttpsRedirection`/`UseAuthentication`/`UseAuthorization`/`ValidationMappingMiddleware`/
      `MapControllers`) so `ValidationMappingMiddleware` still gets first crack at
      `FluentValidation.ValidationException` and only genuinely unhandled exceptions fall through
      to the generic 500 JSON response. (REVIEW #4)
- [x] Whitelisted `GetAllCarsOptions.SortField` in `CarsRepository.GetAllAsync`: extracted a
      `public static string? ResolveSortColumn(string? sortField)` method backed by a fixed
      `Dictionary<string,string>` of known columns (`id`, `yearofproduction`, `brand`, `model`,
      `slug`, `enginetype`, `bodytype`, `rating`), returning `null` (→ no `order by` clause) for
      anything not on the list, instead of interpolating the raw value into SQL text. Made it a
      testable pure function (no DB dependency) since this repository has no existing
      integration-test infrastructure to exercise the real query. Added
      `CarRent.Application.Tests.Unit/Repositories/CarsRepositoryTests.cs` covering known fields
      (case-insensitive), `null`, and unrecognized/malicious input (including a `"; drop table
      cars;--"`-style payload) all resolving to `null`. (REVIEW #5)
- [x] ~~Retarget to net8.0~~ — done in Phase 0 (moved up; see deviation note there). (REVIEW #6)
- [x] Self-review + `/verify` on Phase 2 changes: `dotnet build` clean (5 pre-existing warnings,
      same set as baseline, 0 errors); `dotnet test` — **34/34 passing** (22 baseline + 12 new: 3
      `OrdersServiceTests` + 9 `CarsRepositoryTests`), 0 failed, 0 skipped. Read through the full
      diff: `OrdersService.UpdateAsync` change is a 3-line ownership check, no other logic
      touched; the two `QuerySingleAsync`→`QuerySingleOrDefaultAsync` swaps are one-line
      each and don't change the method signatures (`Car?`/`User?` return types were already
      nullable); the exception-handler is additive middleware with no changes to existing
      middleware ordering relative to each other; `ResolveSortColumn` is a pure addition that
      only replaces where `options.SortField` is read from (`sortColumn` instead of the raw
      value) — no other behavior in `GetAllAsync` changed. **Regression note:** none found. This
      work is NOT yet committed — holding for user review per the updated Execution Contract
      (review-before-commit, not just review-before-push).

### Phase 3 — Medium/Low fixes
- [x] Wrapped the three sequential deletes in `CarsRepository.DeleteByIdAsync` and
      `UsersRepository.DeleteByIdAsync` in a single `IDbTransaction` (`connection.BeginTransaction()`,
      passed to each `CommandDefinition`, explicit `transaction.Commit()` at the end — an
      exception before commit rolls back implicitly via the `using` disposal). (REVIEW #7)
- [x] Added `Page` (`>= 1`) and `PageSize` (inclusive `1-100`) validation rules to
      `GetAllCarsRequestValidator`. Added `GetAllCarsRequestValidatorTests.cs` covering valid
      input, `Page` `0`/`-1`, `PageSize` `0`/`-1`/`101`/`1000000`, and the `1`/`100` boundary
      values not throwing. (REVIEW #8)
- [x] `CarsRepository.GetByIdAsync` now runs the same `left join ratings ... group by id` query
      `GetAllAsync` uses (scoped to `where c.id = @id`) instead of a plain `select *`, so
      single-car and list responses compute `Rating` identically. (REVIEW #9)
- [x] Removed the unused `CancellationToken token = default` constructor parameter from
      `CreateOrUpdateOrderRequestValidator`, `CreateRatingValidator`, and `CreateUserValidator`.
      Verified no test constructs any of the three passing a token argument, so no test changes
      needed. (REVIEW #10)
- [x] Renamed `IRatingsRepository`/`RatingsRepository`'s `ExistsCarRatingForUser` →
      `HasOrderForCarAsync` (4 call sites: interface, implementation, `CreateRatingValidator`, and
      `CreateRatingValidatorTests`). (REVIEW #11)
- [x] Renamed `CreateOrUpdateCatRequestValidator` → `CreateOrUpdateCarRequestValidator` (file +
      class, via `git mv` to preserve history; updated the one test reference) and
      `CreateOrUpdateUserRequestValodator` → `CreateOrUpdateUserRequestValidator` (file + class;
      had zero other references anywhere). (REVIEW #12)
- [x] Added a car-slug uniqueness rule. Since `Car.Slug` is a computed property on the *domain*
      model (derived from Brand/Model/YearOfProduction) and isn't present on the
      `CreateOrUpdateCarRequest` DTO at all, this couldn't live in the existing request-shape
      validator — instead added a new **`CreateOrUpdateCarValidator : AbstractValidator<Car>`**
      in the Application layer, exactly mirroring `CreateUserValidator`'s
      email-uniqueness pattern (new `ICarsRepository.ExistsBySlugAndIdAsync(id, slug)` method,
      `where slug = @slug and id != @id`), and wired `IValidator<Car>` into `CarsService`
      (`CreateAsync`/`UpdateAsync` now call `ValidateAndThrowAsync` first, matching how
      `UsersService` already does it — `CarsService` previously called no validator at all).
      Added `CreateOrUpdateCarValidatorTests.cs` (2 tests). This makes the `Conflict()` branch in
      `CarsController.Create` reachable for real duplicate cars. (REVIEW #13)
- [x] Added indexes in `DbInitializer` for `cars(slug)`, `cars(yearofproduction)`,
      `orders(user_id)`, `users(email)`, and `ratings(car_id)`, each as its own
      `create index if not exists` statement (matching the existing `create table if not exists`
      idempotency style). (REVIEW #14)
- [x] Switched `CreateOrUpdateOrderRequestValidator`'s `DateFrom` comparison from `DateTime.Now`
      to `DateTime.UtcNow` (the smaller of the plan's two options — a full `DateTimeOffset`
      migration would touch `Contracts`/`Models`/the DB column type, out of proportion for this
      fix). Updated `CreateOrUpdateOrderRequestValidatorTests.cs`'s five `DateTime.Now` usages to
      `DateTime.UtcNow` too — the pre-existing tests were themselves timezone-fragile against the
      corrected check (a local time behind UTC would have failed the "valid request" test
      nondeterministically depending on the machine's timezone). (REVIEW #15)
- [x] Self-review + `/verify` on Phase 3 changes: `dotnet build` clean (6 pre-existing warnings,
      same set as baseline, 0 errors); `dotnet test` — **45/45 passing** (34 from Phase 2 + 11
      new: 9 `GetAllCarsRequestValidatorTests` + 2 `CreateOrUpdateCarValidatorTests`), 0 failed, 0
      skipped. Ran `CarRent.Api` directly again — DI still resolves cleanly with the new
      `IValidator<Car>` dependency on `CarsService`, fails at the same expected Postgres-connection
      point as before (no regression in startup wiring). **Regression note:** none found. This
      work is NOT yet committed — holding for user review.

### Phase 4 — Requested features
*(none — no new features were requested for this pass; skipped per the Non-Goals section above)*

### Phase 5 — Tests & verification
- [x] **New finding discovered and fixed during this phase (REVIEW #16, added to
      `REVIEW_REPORT.md`):** while writing `OrdersController` tests, found that `Update` builds
      its Mapster tuple as `(request, id, userId)` with `userId: Guid?`, but
      `MapsterConfiguration` only registers the tuple shape with a non-nullable `Guid` (what
      `Create` uses) — a distinct, unregistered closed generic type. Confirmed empirically with a
      throwaway probe test (written, run, and deleted — not part of the permanent suite) that
      this silently zeroes `CarId`/`DateFrom`/`DateTo` on every order update via Mapster's default
      fallback convention, while `Id`/`UserId` still map correctly by name. This was outside the
      original 15 findings; **checked with the user before fixing** — approved to fix now rather
      than only log it. Fixed by making `Update` build the same tuple shape as `Create`
      (`userId!.Value` instead of `userId`) in `OrdersController.cs`. Covered by a dedicated
      regression test (`Update_ShouldMapCarIdAndDatesCorrectly_WhenCallerOwnsTheOrder`) asserting
      every field on the captured `Order` matches the request.
- [x] Added controller-level tests for `CarsController` (`CarsControllerTests.cs`: `GetById`
      not-found (#4) and found, `GetAll` happy path touching sort/pagination options (#5, #8),
      `Update` not-found), `OrdersController` (`OrdersControllerTests.cs`: ownership-rejection
      not-found (#3), the #16 mapping regression test above, `Create`, `CancelOrder`
      not-found), and `CarRatingsController` (`CarRatingsControllerTests.cs`: `RateCar`
      ok/not-found, `DeleteRating` not-found) — all three had zero coverage per
      `REVIEW_REPORT.md`'s test-coverage snapshot.
- [x] Added service-level tests for `CarsService` (`CarsServiceTests.cs`) and `UsersService`
      (`UsersServiceTests.cs`) covering `UpdateAsync`'s not-found and happy paths — the same gap
      class as the `OrdersService` ownership bug in #3, now closed for all three services.
- [x] Full suite passes end to end: `dotnet test` — **60/60 passing** (45 from Phase 3 + 15 new:
      4 service tests — 2 `CarsServiceTests` + 2 `UsersServiceTests` — plus 11 controller tests —
      4 `CarsControllerTests` + 4 `OrdersControllerTests` + 3 `CarRatingsControllerTests`), 0
      failed, 0 skipped.
- [x] Manual golden-path exercise: **could not be run against a live HTTP server** — this sandbox
      has no Postgres instance, and `DbInitializer.InitializeAsync()` runs eagerly at startup
      before any endpoint is reachable, so `dotnet run` cannot get past that point (confirmed in
      every phase's self-review: it fails with `Npgsql...Connection refused`, not a code defect).
      Saying this explicitly rather than claiming a live-server exercise that didn't happen. The
      specific scenarios the plan asked for are instead covered by the unit tests added this
      phase: cross-user order update rejection
      (`Update_ShouldReturnNotFound_WhenOrderBelongsToAnotherUser`), nonexistent car/user id → 404
      (`CarsControllerTests.GetById_ShouldReturnNotFound_WhenCarDoesNotExist`, existing
      `UsersControllerTests.GetById_ReturnNotFound_WhenUserDoesntExists`), and pagination bounds
      (`GetAllCarsRequestValidatorTests`, Phase 3) — each exercises the real validator/service/
      controller code path, just not over a live socket.

### Phase 6 — Wrap-up
- [x] `/wrap-up` is not among the skills available in this session (the template's reference to
      it is generic, not a live skill here) — did the substance manually instead: `dotnet clean` +
      cold `dotnet build` + `dotnet test` (60/60 passing, 0 vulnerable packages), a whole-diff
      self-review (`git diff master..fix/review-findings --stat`, 46 files), a final secret-leak
      `git grep`, and checked every Success Criteria box above against actual verified evidence
      rather than assumption.
