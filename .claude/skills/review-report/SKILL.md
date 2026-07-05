---
name: review-report
description: Perform a structured senior-level review of the current project (C# backend and/or Angular frontend) and produce a severity-ranked REVIEW_REPORT.md. Read-only — does not fix anything.
---

# review-report

Produce a thorough, honest, senior-level review of the codebase in the current working
directory. This is a read-only pass: no code changes, no fixes, no refactors. The output is a
single file, `REVIEW_REPORT.md`, written at the repo root.

## Process

1. **Orient first.** Read any README, task description, or CLAUDE.md in the repo before touching
   code — an interview/take-home task often states explicit requirements or constraints
   ("implement X", "don't touch Y"). The review should judge the code against what it's supposed
   to do, not just against abstract best practice.
2. **Map the repo.** Identify: backend language/framework and its layout (controllers, services,
   repositories, DTOs, validators), frontend framework and structure (components, services,
   state management), test projects (and whether they contain real tests or are empty scaffolds),
   CI config, and any committed config/secrets files.
3. **Read the actual code**, not just file names. For a repo this size, read directly — Program.cs
   / Startup, DI registration, auth setup, the core request-handling path end to end (controller →
   service → repository), any concurrency/caching logic, and the equivalent Angular services /
   guards / interceptors. Spawn a research subagent only if the repo is large enough that direct
   reading would blow the context budget — otherwise read it yourself so findings are grounded in
   real line numbers, not guesses.
4. **Apply the checklist below** to whichever stack(s) are present.
5. **Assign a severity** to each finding using the bands below. Don't inflate — a finding with no
   real failure scenario doesn't belong in the report.
6. **Write `REVIEW_REPORT.md`** following the structure in `template.md` in this same skill
   folder. Every finding needs a concrete failure scenario (what input/state causes what wrong
   behavior), not just a description of what the code does.
   **Never reproduce an actual secret value in the report.** If a finding is a committed
   credential, describe it by file:line and type ("live MongoDB URI with embedded credentials",
   "hardcoded JWT signing key") — writing the raw value into `REVIEW_REPORT.md` just commits the
   same secret a second time, in a file explicitly about the fact that it leaked.
7. **Do not fix anything.** If something is trivially fixable and clearly not in scope for
   discussion, still just report it — fixing happens in the next phase (`/fix-plan`).
8. End your turn with a short chat summary (top 3-5 findings by severity) and point to
   `REVIEW_REPORT.md` for the full list. Do not paste the entire report into chat.

## Checklist

Apply whichever sections are relevant to the stack(s) present.

### Security (check first — these tend to be the highest-severity findings)
- Secrets committed to git (connection strings, API keys, signing keys) — check `git ls-files`
  and `appsettings*.json` / `environment*.ts`, not just `.gitignore` intent. Also check history,
  not just the current tree: `git log -p -- <file>` (or `git log --all -p -- <file>`) for a
  secret that was later "removed" in a newer commit but never rotated or scrubbed — a removal
  commit alone does not un-leak it.
- Known-vulnerable dependencies: `dotnet list package --vulnerable --include-transitive` for the
  backend, `npm audit` for the Angular side.
- Authentication/authorization: which endpoints are anonymous vs authorized, and is that
  intentional? Any endpoint that mutates state or exposes another user's data should be
  authenticated unless there's a clear reason not to be.
- Privilege escalation: can a client-supplied field (role, user ID, isAdmin flag) change what
  the request is allowed to do? Is it validated/whitelisted server-side?
  Client-side "hiding" of a role field is not a fix.
- Password handling: hashing algorithm and iteration/work-factor (compare against current OWASP
  guidance, not the value the codebase happens to use), constant-time comparison for secrets.
- Input validation at trust boundaries (controllers, not just deep in a service).
- CORS, JWT validation parameters (issuer/audience/lifetime), token signing key handling.
- Injection surfaces — grep for string concatenation/interpolation feeding a query, don't just
  read a few call sites and assume it's fine:
  - SQL: raw ADO.NET/Dapper commands built with `$"...{input}..."` or `+` instead of parameters
    (`@param`, `SqlParameter`); EF Core `FromSqlRaw`/`ExecuteSqlRaw` with interpolated input instead
    of `FromSqlInterpolated`/parameterized `ExecuteSqlInterpolated`.
  - MongoDB: raw `BsonDocument`/JSON filter strings built by concatenating user input, `$where`
    clauses containing untrusted JS, or `MongoDB.Driver` filters built via string interpolation
    instead of the typed `Builders<T>.Filter` API.
  - Angular: unescaped output via `[innerHTML]` / `bypassSecurityTrust*`.

### Correctness / bugs
- Trace at least one full request path end-to-end (happy path + one failure path) rather than
  reading files in isolation.
- Dead/commented-out code that looks like an unfinished feature or a silently broken path.
- Null/empty handling: `.First()`/`.Single()` calls that assume a non-empty collection,
  nullable-reference-type mismatches between declared return type and actual behavior.
- Off-by-one, pagination without a stable sort, race conditions in concurrent/cached code paths.
- Error handling that maps the wrong exception type to the wrong HTTP status, or leaks internal
  exception messages to API responses.

### Architecture & design (SOLID / DRY / KISS / clean code)
- Single Responsibility violations: controllers/services doing validation + orchestration +
  persistence + external-API calls all in one method.
- Repeated logic that should be a shared helper/base class, vs. premature abstraction where three
  similar lines have been turned into an unnecessary interface.
- Dependency Inversion — check at two levels, layering and construction:
  - **Layering:** does the domain/service layer depend on infrastructure details it shouldn't
    (e.g., HTTP-specific types leaking into a service layer)?
  - **Construction:** do constructors/DI registrations depend on an abstraction (interface)
    rather than a concrete class — or is there a `new ConcreteService()`/`new SomeClient()` inside
    a class instead of an injected dependency? Depending on concrete types couples the consumer to
    implementation details it doesn't need to know about, and blocks unit testing (nothing to
    substitute with NSubstitute/a fake in tests). Note any concrete-typed constructor parameter or
    in-method `new` of something that should be behind an interface.
- Consistency: does the codebase already have an established pattern (repository, validator,
  DI registration style) that some other part of the code ignores?
- Function size & abstraction level: methods that run well past ~30-35 lines, or mix high-level
  orchestration ("validate, save, notify") with low-level detail (raw loops, string parsing) in
  the same function instead of extracting the low-level part to its own well-named method.
- Comments used to explain *what* code does rather than *why* — usually a sign the code should be
  extracted into a clearly-named function/variable instead of narrated with a comment.

### Error handling & resilience
- Timeouts/retries/circuit breakers on outbound calls (HTTP clients, DB, third-party APIs) —
  present, absent, or inconsistent between similar calls?
- Are failures logged with enough context to debug, without leaking sensitive data?

### Testing
- What's actually tested vs. what test *projects* merely exist (empty scaffolds count as zero
  coverage, note it explicitly).
- Are there tests for the auth/permission boundary, not just happy-path service logic?
- Frontend: are components/services under test, or only trivial "should create" specs?

### Performance & scalability
- Missing database indexes on frequently-filtered fields (foreign-key-like fields, email/username
  lookups).
- N+1 query patterns: a `foreach`/`for` loop (or a `.Select()` over a collection) that does an
  `await` on a DB call or HTTP request per iteration, instead of one batched query
  (`Find(Builders<T>.Filter.In(...))`, a single `$in`/join, or a bulk fetch). This is one of the
  most common interview-task mistakes — look inside every loop that touches a repository/service
  call, not just at the top-level controller method.
- Unbounded result sets, large binary blobs stored inline in documents/rows instead of blob
  storage.
- Client/connection reuse: is a new `HttpClient`, `MongoClient`, or similar connection object
  constructed per-request/per-method-call instead of once and reused (via `IHttpClientFactory`,
  a singleton-registered client, or a shared connection factory)? `new HttpClient()` inside a
  method is a classic socket-exhaustion bug; `new MongoClient(...)` per request throws away
  connection pooling and is a common mistake to plant in a take-home task. Check DI registrations
  (`Program.cs`) to confirm these are registered once, not resolved/constructed fresh per call.
- Allocation efficiency on repeated/hot paths: a `new byte[]`/`new T[]` (or `string.Substring`/
  `Split`/`+` concatenation producing many small string copies) created per iteration of a loop,
  per chunk, or per request — where `ReadOnlySpan<T>`/`Memory<T>` could slice existing data without
  copying, or `ArrayPool<T>.Shared`/`MemoryPool<T>` could rent and return a reusable buffer instead
  of allocating a fresh one each time. Check any code that repeatedly handles byte buffers —
  streaming/chunked responses, file or network I/O, request/response body processing — since that's
  where per-iteration allocations turn into real GC pressure under load, not just a style nit.
- Angular: unnecessary `*ngFor` without `trackBy`, subscriptions not unsubscribed (memory leaks),
  missing `OnPush` where it would matter, oversized bundles from eager-loaded modules.

### Backend-specific (.NET)
- DI lifetimes (singleton holding scoped/transient dependencies), options validation on startup.
  See "Client/connection reuse" and "Allocation efficiency" under Performance above for the
  `HttpClient`/`MongoClient`/`Span`/`ArrayPool` specifics.

### Frontend-specific (Angular)
- `isPlatformBrowser` guards around `localStorage`/`window`/`navigator` if SSR is in play.
- RxJS subscription leaks, error handling in `catchError`/`subscribe`, unguarded template
  null-access.
- Hardcoded backend URLs / environment leakage between dev and prod config.

## Severity bands

- **Critical** — exploitable now, or actively broken in a way that reaches production/users.
  Secrets in git, auth bypass, privilege escalation, data loss, SQL/NoSQL injection reachable with
  attacker-controlled input.
- **High** — a real bug or gap that will bite in normal operation or under moderate load; not yet
  actively exploited/broken but close to it (weak crypto params, missing indexes causing
  correctness issues, unhandled failure paths on the main flow, N+1 query patterns or a
  per-request `HttpClient`/`MongoClient` construction that will exhaust sockets/connections under
  real load).
- **Medium** — a real issue but lower blast radius or harder to trigger (inconsistent resilience
  config, swallowed exceptions that only affect debuggability, missing tests for edge cases,
  concrete-class DI dependencies that block unit testing of an otherwise-covered service,
  unnecessary per-iteration allocations where `ReadOnlySpan<T>`/`ArrayPool<T>` would avoid the
  copy/GC pressure). Bump to **High** if the allocation sits on a genuinely hot path — e.g. per
  audio/streaming chunk, or per item in a large per-request loop — where it will show up as real
  throughput/latency degradation under load, not just extra GC churn.
- **Low** — cleanup, naming, dead code, minor consistency nits, function-length/abstraction-level
  issues, comments substituting for a clearer name. Still worth listing, but don't let these crowd
  out the top of the report.

## Output

Write the full report to `./REVIEW_REPORT.md` using the structure in `template.md`. Do not
create any other files and do not modify existing source files during this skill.

**If `REVIEW_REPORT.md` already exists**, check whether `FIX_PLAN.md` also exists and references
its finding numbers (`REVIEW #N`). If it does, don't renumber or drop existing findings when
regenerating the report — append new findings with new numbers and preserve the existing ones,
so `FIX_PLAN.md`'s references stay valid. If no `FIX_PLAN.md` exists yet, it's safe to
regenerate the report from scratch.
