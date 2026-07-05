# Review Report — CarRent.Api

**Reviewed:** 2026-07-05 · **Scope:** Backend (.NET 7 Web API + Identity.Api helper) · **Reviewer:** Claude Code

## Summary

The architecture is clean and consistently applied: thin controllers, a real service/repository split, centralized Mapster configuration, and FluentValidation used at both the request-shape and domain layers exactly as `CLAUDE.md` describes. The main problems are not architectural — they're a committed shared JWT signing secret that allows full auth bypass, a broken-object-level-authorization gap on order updates, a `QuerySingleAsync` bug that turns every "not found" lookup on two controllers into an unhandled 500, and a dormant but real SQL-injection landmine in the cars sort clause. None of these require a redesign; they're all local, fixable bugs in an otherwise sound codebase.

## Findings

### 🔴 Critical

**1. Shared, hardcoded JWT signing key committed to source control** — `CarRent.Api/appsettings.Development.json:8`, `Helpers/Identity.Api/Controllers/IdentityController.cs:13`
The symmetric signing key is a literal string checked into git and reused verbatim between `CarRent.Api` (the resource server) and `Identity.Api` (the token issuer). `Identity.Api/IdentityController.GenerateToken` also accepts a client-supplied `CustomClaims` dictionary and stamps every claim onto the token with no restriction on claim name or value.
**Failure scenario:** Anyone with read access to the repo (or who finds this key deployed unchanged, since nothing forces rotation outside of source control) can mint their own JWT offline using the well-known key, issuer, and audience, setting `userid` to any GUID and adding `admin=true` / `trusted_member=true` custom claims. That token passes `TokenValidationParameters` validation in `Program.cs:25-37` and satisfies both the `Admin` and `TrustedMember` policies — full authentication and authorization bypass for every endpoint in the API.
**Fix direction:** Move the signing key out of source control (user secrets locally, a real secrets manager in any shared/deployed environment) and generate a unique, high-entropy key. If `Identity.Api` is only ever a local dev helper, make that explicit (e.g. gate `CustomClaims` behind a dev-only flag) so it can never be mistaken for something safe to point at a shared environment.

**2. Database credentials committed to source control** — `CarRent.Api/appsettings.json:3`, `CarRent.Api/appsettings.Development.json:4`
The Postgres connection string, including a plaintext username/password, is committed directly in both config files (values differ only by environment file, not by secret management).
**Failure scenario:** Anyone with repo access has full read/write credentials to whatever database `products` on `localhost:5432` resolves to for a given deployment; if these values are ever reused for a real environment rather than rotated, the whole dataset (users, orders, ratings) is exposed.
**Fix direction:** Same as above — pull the connection string from environment variables/secrets manager, and never let a real credential sit in a file that gets committed, even as a "just for demo" placeholder.

### 🟠 High

**3. Broken object-level authorization on order update (IDOR)** — `CarRent.Api/Controllers/OrdersController.cs:54-64`, `CarRent.Application/Services/OrdersService.cs:46-55`
`Update` takes the order id from the route and the caller's own id from the token, then calls `OrdersService.UpdateAsync`, which only checks `ExistsByIdAsync(order.Id)` — never whether the existing order actually belongs to the caller. Compare this to `CancelAsync`, which correctly scopes the delete with `CancelByUserIdAsync(userId, orderId, ...)`.
**Failure scenario:** User A, who is merely `[Authorize]` (no special role required), sends `PUT /api/orders/{B's order id}` with their own valid token. `ExistsByIdAsync` returns true (the order exists, just not A's), so `OrdersRepository.UpdateAsync` runs `update orders set user_id = @UserId, car_id = @CarId, date_from = @DateFrom, date_to = @DateTo where id = @Id` — silently reassigning B's order to A and overwriting the car/dates, with no ownership check anywhere in the path.
**Fix direction:** In `OrdersService.UpdateAsync`, fetch the existing order and verify `existingOrder.UserId == order.UserId` (or pass the caller's id down and check it explicitly) before updating; return `null`/404 otherwise, the same way `CancelAsync` already does it correctly.

**4. `GetByIdAsync` throws instead of returning null for missing Cars/Users, and there's no global exception handler** — `CarRent.Application/Repositories/CarsRepository.cs:132-143`, `CarRent.Application/Repositories/UsersRepository.cs:80-89`
Both methods use `connection.QuerySingleAsync<T>(...)`, which throws `InvalidOperationException` when the result set is empty. `OrdersRepository.GetByIdAsync` (line 108-116) correctly uses `QuerySingleOrDefaultAsync` for the same pattern, so this is an inconsistency, not an intentional choice. `Program.cs` registers `ValidationMappingMiddleware`, which only catches `FluentValidation.ValidationException` — there is no `app.UseExceptionHandler(...)` or catch-all middleware.
**Failure scenario:** `GET /api/cars/{random-nonexistent-guid}` or `GET /api/users/{random-nonexistent-guid}` throws an unhandled exception that propagates all the way out, producing a raw 500 (with a stack trace visible in Development via the default developer exception page) instead of the clean 404 that `CarsController.GetById`/`UsersController.GetById` are written to return.
**Fix direction:** Switch both calls to `QuerySingleOrDefaultAsync`, and add a catch-all exception-handling middleware/`UseExceptionHandler` so any future unhandled exception degrades to a generic 500 response rather than leaking internals.

**5. Latent SQL injection in the cars sort clause** — `CarRent.Application/Repositories/CarsRepository.cs:92-98`
`GetAllAsync` builds `order by {options.SortField} {...}` via raw string interpolation with zero validation or column whitelist. Today this is unreachable: `GetAllCarsRequest` (Contracts) exposes a `SortBy` property, but there is no Mapster mapping from `SortBy` to `GetAllCarsOptions.SortField` (`CarsController.cs:69` does a bare `request.Adapt<GetAllCarsOptions>()`, and Mapster's default convention only maps identically-named properties), so `SortField` is always `null` in practice.
**Failure scenario (once wired up):** The very next natural change here — mapping the already-existing `SortBy` request field to `SortField`, which is clearly what it was added for — turns this into `GET /api/cars?sortBy=id;drop table cars;--`-style raw SQL injection with no defense at all, since the value is concatenated straight into the query text rather than parameterized.
**Fix direction:** Whitelist `SortField` against a fixed set of known column names (e.g. a switch/dictionary mapping an enum or allowed string set to the actual column), never interpolate the raw client value into SQL text.

**6. Target framework is end-of-life** — all `.csproj` files (e.g. `CarRent.Api/CarRent.Api.csproj`), pinned to `net7.0` / `Microsoft.AspNetCore.*` 7.0.10
.NET 7 reached end of support in May 2024; as of this review (2026-07-05) it has received no security patches for over two years.
**Failure scenario:** Any vulnerability discovered in the ASP.NET Core/.NET 7 runtime or the pinned `Microsoft.*` packages since May 2024 has no official fix available on this branch — the app is running on an unsupported, unpatched runtime in production.
**Fix direction:** Upgrade to a currently-supported LTS (.NET 8 or later) and bump the `Microsoft.AspNetCore.*`/`Microsoft.Extensions.*` package versions to match. (`dotnet list package --vulnerable` could not be run in this environment — no `dotnet` CLI available — so specific transitive CVEs beyond the EOL status itself are not verified here; run it once tooling is available.)

### 🟡 Medium

**7. Cascading deletes are not transactional** — `CarRent.Application/Repositories/CarsRepository.cs:66-88` (`DeleteByIdAsync`), `CarRent.Application/Repositories/UsersRepository.cs:31-51` (`DeleteByIdAsync`)
Both methods issue three sequential `ExecuteAsync` calls on the same connection (delete orders/ratings, then the parent row) with no `IDbTransaction` wrapping them.
**Failure scenario:** If the connection drops or an error occurs between the second and third statement (e.g. deleting ratings succeeds but deleting the car row fails), the car's orders and ratings are gone but the car itself remains — a partially-applied delete with no rollback.
**Fix direction:** Wrap the three statements in a single `connection.BeginTransactionAsync()` / commit, matching the atomicity the delete is implicitly assumed to have.

**8. Pagination `Page`/`PageSize` are never validated** — `CarRent.Api/Validators/GetAllCarsRequestValidator.cs` (only validates `YearOfProduction`), consumed at `CarsRepository.cs:100-118`
`GetAllCarsOptions.Page`/`PageSize` flow straight from the query string into `offset = (options.Page - 1) * options.PageSize` and `limit @pageSize` with no lower/upper bound checks.
**Failure scenario:** `GET /api/cars?page=0` produces a negative `OFFSET` value, which Postgres rejects, causing an unhandled exception (compounds with finding 4 — no catch-all handler) instead of a 400; `GET /api/cars?pageSize=1000000` pulls the entire table in one request with no cap.
**Fix direction:** Add `RuleFor(x => x.Page).GreaterThanOrEqualTo(1)` and a sane inclusive range for `PageSize` (e.g. 1-100) to `GetAllCarsRequestValidator`.

**9. Single-car lookup never returns a rating, unlike the list endpoint** — `CarRent.Application/Repositories/CarsRepository.cs:132-143` vs. `90-130`
`GetAllAsync` computes `Rating` via `left join ratings ... group by id`, but `GetByIdAsync` does a plain `select * from cars where id = @id` with no join — the `cars` table itself has no `rating` column, so `Car.Rating` on a single-car fetch is always its default (`0`), never the real average.
**Failure scenario:** `GET /api/cars` shows a car with a 4.5 average rating; `GET /api/cars/{same id}` for the detail view shows `0`/`null` for the same car — inconsistent data for the same resource depending on which endpoint the client called.
**Fix direction:** Reuse the same joined query (or a shared query fragment) in `GetByIdAsync` so both endpoints compute `Rating` the same way.

## 🟢 Low

**10. Dead/misleading `CancellationToken` constructor parameter on async validators** — `CarRent.Api/Validators/CreateOrUpdateOrderRequestValidator.cs:15`, `CarRent.Application/Validators/CreateRatingValidator.cs:12`, `CarRent.Application/Validators/CreateUserValidator.cs:11`
Each validator constructor takes a `CancellationToken token = default` parameter that is never used — the actual cancellation token that matters is the one FluentValidation passes into each `MustAsync((x, token) => ...)` lambda, which shadows the constructor parameter. It reads as if cancellation is wired through DI, but it isn't; DI just falls back to the parameter's default value since nothing is registered for `CancellationToken`. Remove the misleading constructor parameter.

**11. `RatingsRepository.ExistsCarRatingForUser` queries the orders table, not ratings** — `CarRent.Application/Repositories/RatingsRepository.cs:35-43`
The method name implies it checks for an existing rating, but the query is `select count(1) from orders where user_id = @userId and car_id = @carId` — it's actually the "has this user ordered this car" check used to gate rating creation. The logic is correct for its caller (`CreateRatingValidator`), but the name will mislead the next reader. Rename to something like `HasOrderForCarAsync`.

**12. Typos in validator file/class names** — `CarRent.Api/Validators/CreateOrUpdateCatRequestValidator.cs` (class `CreateOrUpdateCatRequestValidator`, should be "Car"), `CarRent.Api/Validators/CreateOrUpdateUserRequestValodator.cs` (should be "Validator"). Cosmetic, but worth a rename during any nearby edit.

**13. `Conflict()` branch on Cars/Users `Create` is effectively unreachable** — `CarRent.Api/Controllers/CarsController.cs:35-48`, `CarRent.Api/Controllers/UsersController.cs:29-41`
`CreateAsync` returning `false` is treated as a 409 Conflict, but the only way the underlying `INSERT` fails is a PK collision on `id`, and `id` is always a freshly generated `Guid.NewGuid()` in the same request — so this branch essentially never fires. If the intent was to reject duplicate slugs/emails, that's already handled elsewhere (email uniqueness via `CreateUserValidator`); cars have no equivalent uniqueness rule (e.g. by `Slug`) despite `Slug` being derived from brand/model/year, so a car can be created as an exact duplicate today without either mechanism catching it.

## What's already good

- Layering matches `CLAUDE.md` exactly: `Application` has zero references to `Api`/`Contracts`, controllers stay thin (validate → map → call service → map response), and `Repositories`/`Services` are split one-per-aggregate consistently.
- Mapster configuration is centralized in one place (`MapsterConfiguration`) rather than scattered `.Adapt` customizations, and the tuple-based mapping pattern for combining a request DTO with a generated id/route value is applied consistently across all four controllers.
- `OrdersController.CancelOrder` / `CarRatingsController` correctly scope mutations to the calling user via `CancelByUserIdAsync`/`userId` parameters at the repository layer — the ownership bug in finding 3 is a real gap, not evidence of a systemic pattern; most of the codebase gets this right.
- FluentValidation is used for both request-shape rules (`Api/Validators`) and domain-level async rules that hit the database (`Application/Validators`), matching the documented split, and `ValidationMappingMiddleware` centralizes the 400 response shape instead of each controller catching `ValidationException` itself.
- Pagination consistently follows the documented `GetAllAsync` + `GetCountAsync` + tuple-adapt-to-`PagedResponse` pattern for the one paginated list endpoint that exists (`Cars.GetAll`).

## Test coverage snapshot

- **`CarRent.Application.Tests.Unit`**: `RatingsServiceTests` covers exactly one happy-path case for `RateCarAsync`; there is no coverage at all for `CarsService`, `OrdersService`, or `UsersService` business logic (including the ownership bug in finding 3, which unit tests against `OrdersService` would likely have caught). `CreateRatingValidatorTests` and `CreateUserValidatorTests` cover their respective async validators.
- **`CarRent.Api.Tests.Unit`**: Only `UsersController` has controller tests (4 tests, reasonably covering GetById/GetAll happy and not-found paths) — `CarsController`, `OrdersController`, and `CarRatingsController` have zero tests, meaning none of the update/cancel/rate flows (including the IDOR in finding 3) are exercised. `CreateOrUpdateCarRequestValidatorTests` and `CreateOrUpdateOrderRequestValidatorTests` give decent request-validator coverage; `MapsterTests` sanity-checks the tuple mappings.
- No test anywhere exercises the authorization policy boundary (`Admin`/`TrustedMember`/plain `Authorize`) — there's nothing that would catch a controller accidentally losing its `[Authorize]` attribute or a policy check being misapplied.
- No repository-level (integration) tests exist against a real Postgres instance, so bugs like the `QuerySingleAsync` issue (finding 4) or the non-transactional cascade deletes (finding 7) aren't caught by anything in the test suite.
