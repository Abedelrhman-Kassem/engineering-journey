# 🎯 Roadmap — From Mid-Level to Senior (.NET 10)

> A deliberate-practice apprenticeship. Every line of code in this repo was written by hand — no AI-generated code.
> Rules of the program: see [.claude/PLAN.md](.claude/PLAN.md).
> Every task is taught with **why it exists + when NOT to use it** — every interview includes an over-engineering question.
> 🧩 = design pattern learned in context on that task (must be named, justified, and located in the code at review).
>
> ✅ = passed review + interview questions &nbsp;|&nbsp; 🔄 = in progress &nbsp;|&nbsp; ⬜ = locked/not started

---

## Phase 0 — Git Foundations

- [x] **0.1** ✅ Repo birth: `git init`, .NET `.gitignore`, template cleanup, first commits, GitHub remote, push *(bonus learned in the field: reflog recovery, reset --hard, amend, rebase reword, force-with-lease)*
- [x] **0.2** ✅ Feature-branch workflow + first GitHub PR (fixed NU1903 vuln via direct package override; learned the one-branch-one-PR-one-merge rule the hard way)
- [x] **0.3** ✅ Merge vs rebase — did both on `exp/merge-demo` (merge commit, then reset + rebase to linearize); golden rule, reflog recovery, fast-forward vs `--no-ff`, squash ≠ `--no-ff`, revertability as the reason to keep merge commits
- [x] **0.4** ✅ Damage control essentials: `revert` + `stash` — non-tip revert, merge revert with correct mainline (`-m 1`), revert-the-revert proven by identical tree hashes, why re-merging after a revert doesn't restore the work; `stash -u`, `pop` vs `apply` (reflog/reset already learned in 0.1 — the hard way 😄)
- [x] **0.5** ✅ Minimal CI: GitHub Actions workflow building the solution on push to `main` + PRs — job-scoped `permissions`, `restore`/`build --no-restore` split, Release config, branch protection with `build` as a required check; `TreatWarningsAsErrors` in `Directory.Build.props` (binds locally *and* in CI, not just a CI flag); README documents what CI deliberately does **not** do yet and why

## Phase 1 — Architecture Fundamentals

- [x] **1.1** ✅ `ARCHITECTURE.md` — six projects documented (what lives here / never / may reference), dependency rule pointing inward at `Domain` with `Domain.Shared` as a shared kernel outside the ordering, audit table of every current reference, concrete example type per project. Reasoning earned: the deletion test justifying `Domain.Shared` (Contract would have to see `Domain`, DTOs one `using` from leaking entities), interfaces split by who they face (inbound → `Contract`, outbound like `IEmailSender` → `Application`), Application *consumes* the domain model but never shapes it, `Host → Infrastructure` as the deliberate composition-root exception
- [x] **1.2** ✅ Enforce the dependency rule — `Application → Domain` added, redundant `Host → Application.Contract` removed, audit rewritten to past tense, and an "Enforcing this rule" decision recorded in `ARCHITECTURE.md`. Reasoning earned: most violations of the rule are impossible because they form a cycle and MSBuild refuses to build one, so the compiler is already the enforcement mechanism; the single violation that compiles silently is `Application.Contract → Domain`, which would be added for the plausible-sounding reason "Domain is the core, so everything should reference it"; nothing in this repo catches it (green build, green CI, no second reviewer), and `revert` is no defence against a mistake nobody notices. Decision: no architecture test until a test project exists in Phase 3 — a named trigger, not a postponement. Branch protection gap from 1.1 closed and verified by a refused direct push (`GH006`)
- [x] **1.3** ✅ Composition root — `Host` is the only project that binds an abstraction to an implementation. First real types in the solution: `IEmailSender` as an outbound port in `Application`, `internal sealed` implementation in `Infrastructure` behind a single `AddInfrastructure` entry point. The guarantee is structural, not conventional, and was proved rather than asserted: naming the implementation from `Host` fails with `CS0246`. Reasoning earned: a layer *earns* a registration method by having something to register (the empty `AddApplication` was written, argued about, and deleted); referencing a DI package is not a dependency-rule violation because the rule orders layers, not packages — what is given up is that an inner layer now assumes a container exists; a project declares the packages it uses rather than borrowing them transitively; Singleton justified by statelessness *and* by depending on nothing scoped; `async` with no `await` builds a state machine that never moves, and returning a completed `Task` is not the same as awaiting one when an exception is thrown. Judgment: registration stays manual until adding a feature forces an edit to `DependencyInjection.cs` every time — with the developer's own boundary that scanning suits registrations carrying no decision, and that a keyed strategy can never be one of them

## Phase 2 — ASP.NET Core Pipeline

- [x] **2.1** ✅ Request-logging middleware — written inline first, then refactored to a `sealed` class behind a single `UseRequestLogging()` call. One structured log event per request (method, path, status, duration) using a message template, wrapped in `try`/`finally` so a request that throws still records its status and timing. Reasoning earned: the status code can only be read *after* `next()`, because before it the value is simply the default 200 — the same fact that made the short-circuit experiment return an empty 200 body; method and path are captured *before* `next()` because downstream middleware can rewrite the path; three log lines per request cannot be correlated under concurrency and cannot be queried across fields, so one request means one event. Health checks decided independently and better than the mentor's suggestion: a passing check logs nothing at all, `>= 400` logs critical. Ordering proved by experiment against the plain-HTTP port — placed before `UseHttpsRedirection` the middleware logs the 307 *and* the follow-up 200; placed after, the redirected request never reaches it. 🧩 Chain of Responsibility named and located at `await next(context)`, with the honest divergence: control returns after `next()`, so middleware is a nesting/Decorator shape as much as a chain. First ADR written: `docs/adr/0001-hand-rolled-request-logging.md`, recording why this is hand-rolled rather than `UseHttpLogging`, with header redaction and safe body handling named as the real losses and "when we need to log headers or bodies" as the trigger to switch back
  🧩 *Chain of Responsibility — the middleware pipeline itself*
- [ ] **2.2** Global exception-handling middleware returning RFC 9457 ProblemDetails
- [ ] **2.3** Action filter (execution timing/audit) — filter pipeline vs middleware, when to use which
- [ ] **2.4** Model binding + validation deep dive; automatic 400 behavior
- [ ] **2.5** Filter ordering experiment (resource/action/result/exception; global vs controller vs action scope)
- [ ] **2.6** Options pattern: `IOptions`/`IOptionsSnapshot`/`IOptionsMonitor`, validate-on-start
- [ ] **2.7** DI lifetimes lab: prove Singleton/Scoped/Transient; create then fix a captive dependency
  🧩 *Singleton — and why DI-managed beats the hand-rolled GoF version*

🔧 **Refactoring Sprint #1**

## Phase 3 — Domain & Data (unit testing starts here)

- [ ] **3.1** Domain entities + enums/constants (anemic for now — DDD refactor comes in Phase 8)
- [ ] **3.2** EF Core + Npgsql: DbContext, Fluent API configurations, connection management
- [ ] **3.3** Migrations workflow + PostgreSQL setup
- [ ] **3.4** Seeding + querying: projections, `AsNoTracking`, split query, demonstrate and fix an N+1
- [ ] **3.5** Repository + Unit of Work: build it, then critique — is it worth it over DbContext?
  🧩 *Repository, Unit of Work, Facade*
- [ ] **3.6** First unit tests: domain logic + test project setup (xUnit)

🔧 **Refactoring Sprint #2**

## Phase 4 — REST API Craft

- [ ] **4.1** Resource design: URI conventions, status codes, PUT idempotency, PATCH vs PUT, DELETE semantics
- [ ] **4.2** CRUD endpoints with proper semantics + ProblemDetails everywhere
- [ ] **4.3** Pagination, filtering, sorting — done right (and safely)
- [ ] **4.4** Validation: manual/DataAnnotations first → refactor to FluentValidation; compare
  🧩 *Strategy (interchangeable validators), Composite (composing rules)*
- [ ] **4.5** Mapping: manual first → compare against Mapster; write up the trade-offs
  🧩 *Adapter/Mapper*
- [ ] **4.6** API versioning + OpenAPI polish

🔧 **Refactoring Sprint #3**

## Phase 5 — Concurrency (database & in-process)

- [ ] **5.1** Demonstrate a real race condition (lost update) with two concurrent requests
- [ ] **5.2** Optimistic concurrency with PostgreSQL `xmin`; handle `DbUpdateConcurrencyException` properly
- [ ] **5.3** Transactions + isolation levels lab; `SELECT FOR UPDATE` (pessimistic locking)
- [ ] **5.4** Mini reservation-style feature that survives concurrent access
- [ ] **5.5** In-process concurrency lab: reproduce an in-memory race; fix with `lock` / `Interlocked` / thread-safe collections; threads vs tasks — and when locking is over-engineering
- [ ] **5.6** Async internals: the state machine, `ConfigureAwait`, sync-context deadlocks, `Task.Run` misuse — and when async adds complexity without benefit

## Phase 6 — Security

- [ ] **6.1** Authentication: JWT issuance, validation, refresh tokens — end to end
- [ ] **6.2** Authorization: policies, requirements, handlers
- [ ] **6.3** RBAC + permission-based authorization (permissions in DB, not hardcoded)
- [ ] **6.4** Resource-based authorization ("only the owner can edit")
- [ ] **6.5** Hardening: security headers, CORS done right, built-in rate limiting

🔧 **Refactoring Sprint #4**

## Phase 7 — Performance (evidence first — no premature optimization)

- [ ] **7.1** Indexes + execution plans (`EXPLAIN ANALYZE`); find and fix a slow query
- [ ] **7.2** BenchmarkDotNet: benchmark something real, draw conclusions
- [ ] **7.3** Caching: HTTP caching + `IMemoryCache` (cache-aside) → Redis distributed cache; invalidation
  🧩 *Decorator + Proxy — cache-aside as a decorator over the data service*
- [ ] **7.4** Async pitfalls audit + connection pooling + response compression
- [ ] **7.5** Memory & GC: allocations, generations, LOH; measure with BenchmarkDotNet memory diagnoser; `Span<T>` overview — and when memory micro-optimization is premature
- [ ] **7.6** PostgreSQL full-text search: real question-search feature (`tsvector`, GIN index, ranking, EF Core integration) — vs `LIKE`, and when FTS beats reaching for Elasticsearch

## Phase 8 — DDD (refactor the anemic model)

- [ ] **8.1** Rich entities + aggregates + value objects; invariants enforced inside the domain
  🧩 *Factory (static creation methods), State (question lifecycle: open → answered → closed)*
- [ ] **8.2** Factories, specifications, domain services
  🧩 *Specification, Factory, Domain Service — name them explicitly and justify each*
- [ ] **8.3** Domain events + eventual consistency
  🧩 *Observer / Publish–Subscribe*
- [ ] **8.4** Hand-rolled Outbox: outbox table + polling `BackgroundService` publisher

## Phase 9 — CQRS

- [ ] **9.1** Hand-rolled CQRS: commands, queries, dispatcher, pipeline behaviors (validation, logging, transactions)
  🧩 *Command, Mediator, Decorator (pipeline behaviors)*
- [ ] **9.2** Refactor to Wolverine; articulate what the library replaced and why Wolverine over MediatR

🔧 **Refactoring Sprint #5**

## Phase 10 — Advanced Architecture

- [ ] **10.1** Multi-tenancy: tenant resolution, global query filters, schema-per-tenant vs db-per-tenant analysis
- [ ] **10.2** Feature flags: config-based toggles first → Microsoft.FeatureManagement, feature filters, safe rollout
  🧩 *Strategy, Null Object*
- [ ] **10.3** Idempotency keys + retry-safe APIs
- [ ] **10.4** Background processing: `BackgroundService` + `System.Threading.Channels` queue, graceful shutdown, scoped services
  🧩 *Producer–Consumer*
- [ ] **10.5** Refactor background processing to Wolverine local queues/durable messaging; compare
- [ ] **10.6** Resilience: small outbound-HTTP feature; `HttpClientFactory` + standard resilience handler (retry/timeout/circuit breaker) first → Polly comparison — and when retries make things worse (non-idempotent calls, retry storms)
- [ ] **10.7** Real-time: live "new answer" notifications; WebSockets vs SSE vs SignalR decision, connection lifetime, scale-out/backplane question (feeds Phase 12) — and when polling is honestly good enough

## Phase 11 — Testing (deepening)

- [ ] **11.1** Integration tests with `WebApplicationFactory`
- [ ] **11.2** Test data builders + fake data strategy
  🧩 *Builder*
- [ ] **11.3** TestContainers with PostgreSQL

## Phase 12 — System Design (repo-grounded, no code — design docs in `docs/design/`)

- [ ] **12.1** Capacity & requirements: back-of-envelope for this Q&A platform at 1M+ users (read/write ratio, storage, QPS); define SLOs
- [ ] **12.2** Architecture styles: modular monolith vs microservices vs vertical slices — argue what THIS system should be, and whether splitting now would be over-engineering
- [ ] **12.3** Scaling the read path: caching tiers, read replicas, CDN — grounded in the real Redis work from Phase 7
- [ ] **12.4** Scaling writes & data: partitioning/sharding, the hot-partition problem (a viral question), replication, CAP/PACELC trade-offs
- [ ] **12.5** Async at scale: queues, outbox durability, fan-out (notify followers — grounded in the real outbox 8.4, SignalR 10.7, Wolverine 10.5), delivery guarantees
- [ ] **12.6** 🎤 Mock system-design interview: "Design StackOverflow at scale" end-to-end, defended live; deliverable `docs/design/architecture-at-scale.md` + diagram

## Phase 13 — Production Readiness

- [ ] **13.1** Docker + Docker Compose (app + PostgreSQL + Redis)
- [ ] **13.2** Configuration, secrets, environment variables done right
- [ ] **13.3** Health checks + Kestrel + reverse proxy + HTTPS
- [ ] **13.4** Observability: Serilog refactor (sinks, enrichers, request logging), OpenTelemetry, metrics, tracing
- [ ] **13.5** CI/CD: full GitHub Actions pipeline
- [ ] **13.6** 🏁 Final: professional README, architecture diagrams, ADRs (required) — architecture defense interview

---

*Tracked and reviewed task-by-task. Each ✅ means: passed a scored senior-level code review and answered 3–5 interview questions about the concept.*
