# 🎯 Roadmap — From Mid-Level to Senior (.NET 10)

> A deliberate-practice apprenticeship. Every line of code in this repo was written by hand — no AI-generated code.
> Rules of the program: see [.claude/PLAN.md](.claude/PLAN.md).
>
> ✅ = passed review + interview questions &nbsp;|&nbsp; 🔄 = in progress &nbsp;|&nbsp; ⬜ = locked/not started

---

## Phase 0 — Git Foundations

- [ ] **0.1** Repo birth: `git init`, .NET `.gitignore`, template cleanup, first commits, GitHub remote, push
- [ ] **0.2** Feature-branch workflow + Conventional Commits (practiced from here on, every task)
- [ ] **0.3** Merge vs rebase — do both on throwaway branches, explain the difference from the DAG
- [ ] **0.4** Damage control: `revert`, `cherry-pick`, `bisect` drills
- [ ] **0.5** Minimal CI: GitHub Actions workflow that builds the solution on every push/PR

## Phase 1 — Architecture Fundamentals

- [ ] **1.1** Explain the current solution layout: what belongs in Domain / Domain.Shared / Application / Application.Contract / Infrastructure / Host — written as a short ARCHITECTURE.md
- [ ] **1.2** Enforce the dependency rule: fix project references so dependencies point inward only; justify each reference
- [ ] **1.3** Composition root: understand Host as the only place that wires everything; DI registration strategy

## Phase 2 — ASP.NET Core Pipeline

- [ ] **2.1** Request-logging middleware (inline → class-based; understand `next()`, ordering, short-circuiting)
- [ ] **2.2** Global exception-handling middleware returning RFC 9457 ProblemDetails
- [ ] **2.3** Action filter (execution timing/audit) — filter pipeline vs middleware, when to use which
- [ ] **2.4** Model binding + validation deep dive; automatic 400 behavior
- [ ] **2.5** Filter ordering experiment (resource/action/result/exception filters, global vs controller vs action scope)
- [ ] **2.6** Options pattern: strongly-typed config, `IOptions`/`IOptionsSnapshot`/`IOptionsMonitor`, validate-on-start
- [ ] **2.7** DI lifetimes lab: prove Singleton/Scoped/Transient differences; create and then fix a captive dependency

🔧 **Refactoring Sprint #1**

## Phase 3 — Domain & Data (unit testing starts here)

- [ ] **3.1** Domain entities + enums/constants (anemic for now — DDD refactor comes in Phase 8)
- [ ] **3.2** EF Core + Npgsql: DbContext, Fluent API configurations, connection management
- [ ] **3.3** Migrations workflow + PostgreSQL setup
- [ ] **3.4** Seeding + querying: projections, `AsNoTracking`, split query, demonstrate and fix an N+1
- [ ] **3.5** Repository + Unit of Work: build it, then write a critique — is it worth it over DbContext?
- [ ] **3.6** First unit tests: domain logic + test project setup (xUnit)

🔧 **Refactoring Sprint #2**

## Phase 4 — REST API Craft

- [ ] **4.1** Resource design: URI conventions, status codes, PUT idempotency, PATCH vs PUT, DELETE semantics
- [ ] **4.2** CRUD endpoints with proper semantics + ProblemDetails everywhere
- [ ] **4.3** Pagination, filtering, sorting — done right (and safely)
- [ ] **4.4** Validation: manual/DataAnnotations first → refactor to FluentValidation; compare
- [ ] **4.5** Mapping: manual first → compare against Mapster; write up the trade-offs
- [ ] **4.6** API versioning + OpenAPI polish

🔧 **Refactoring Sprint #3**

## Phase 5 — Concurrency

- [ ] **5.1** Demonstrate a real race condition (lost update) with two concurrent requests
- [ ] **5.2** Optimistic concurrency with PostgreSQL `xmin`; handle `DbUpdateConcurrencyException` properly
- [ ] **5.3** Transactions + isolation levels lab; `SELECT FOR UPDATE` (pessimistic locking)
- [ ] **5.4** Mini reservation-style feature that survives concurrent access

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
- [ ] **7.3** Caching: HTTP caching + `IMemoryCache` (cache-aside) → Redis distributed cache; invalidation strategy
- [ ] **7.4** Async pitfalls audit + connection pooling + response compression

## Phase 8 — DDD (refactor the anemic model)

- [ ] **8.1** Rich entities + aggregates + value objects; invariants enforced inside the domain
- [ ] **8.2** Factories, specifications, domain services
- [ ] **8.3** Domain events + eventual consistency
- [ ] **8.4** Hand-rolled Outbox: outbox table + polling `BackgroundService` publisher

## Phase 9 — CQRS

- [ ] **9.1** Hand-rolled CQRS: commands, queries, dispatcher, pipeline behaviors (validation, logging, transactions)
- [ ] **9.2** Refactor to Wolverine; articulate exactly what the library replaced and what it cost; why Wolverine over MediatR

🔧 **Refactoring Sprint #5**

## Phase 10 — Advanced Architecture

- [ ] **10.1** Multi-tenancy: tenant resolution, global query filters, schema-per-tenant vs db-per-tenant analysis
- [ ] **10.2** Feature flags: config-based toggles first → Microsoft.FeatureManagement, feature filters, safe rollout
- [ ] **10.3** Idempotency keys + retry-safe APIs
- [ ] **10.4** Background processing: `BackgroundService` + `System.Threading.Channels` queue, graceful shutdown, scoped services
- [ ] **10.5** Refactor background processing to Wolverine local queues/durable messaging; compare

## Phase 11 — Testing (deepening)

- [ ] **11.1** Integration tests with `WebApplicationFactory`
- [ ] **11.2** Test data builders + fake data strategy
- [ ] **11.3** TestContainers with PostgreSQL

## Phase 12 — Production Readiness

- [ ] **12.1** Docker + Docker Compose (app + PostgreSQL + Redis)
- [ ] **12.2** Configuration, secrets, environment variables done right
- [ ] **12.3** Health checks + Kestrel + reverse proxy + HTTPS
- [ ] **12.4** Observability: Serilog refactor (sinks, enrichers, request logging), OpenTelemetry, metrics, tracing
- [ ] **12.5** CI/CD: full GitHub Actions pipeline
- [ ] **12.6** 🏁 Final: professional README, architecture diagrams, ADRs — architecture defense interview

---

*Tracked and reviewed task-by-task. Each ✅ means: passed a scored senior-level code review and answered 3–5 interview questions about the concept.*
