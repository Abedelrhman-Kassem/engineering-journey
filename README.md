# 🛤️ Engineering Journey

> A deliberate-practice apprenticeship: rebuilding senior-level .NET engineering skills — **every line of code written by hand, no AI-generated code.**

## Why this repo exists

Like many developers, I spent the last years shipping projects with heavy AI assistance. Projects shipped — but I felt my hands-on engineering instincts getting weaker. This repo is my answer: a structured, mentored program where I write **100% of the code myself**, from ASP.NET Core pipeline fundamentals all the way to senior-level architecture.

AI participates in exactly one role here: as a **strict senior reviewer**. It assigns tasks with acceptance criteria, reviews my pull requests with scored feedback, and grills me with interview questions — but it is contractually forbidden from writing a single line of implementation code ([the contract](.claude/PLAN.md) is committed to this repo, and enforced by [CLAUDE.md](CLAUDE.md)).

**Every commit here is me, my editor, and the documentation.**

## The program

A 13-phase curriculum, small daily tasks growing into feature-sized work — tracked task-by-task in **[ROADMAP.md](ROADMAP.md)**:

| Phases | Focus |
|--------|-------|
| 0–2 | Git discipline, Clean Architecture, ASP.NET Core pipeline (middleware, filters, DI) |
| 3–4 | EF Core + PostgreSQL, REST API craft |
| 5–7 | Concurrency, security (JWT, RBAC), performance & caching |
| 8–10 | DDD, CQRS, multi-tenancy, feature flags, idempotency, background processing |
| 11–12 | Testing depth, Docker, observability, CI/CD |

Every task = feature branch → Conventional Commits → PR-style review (scored ★ across architecture, readability, security, performance, testability...) → 3–5 senior interview questions. Tasks don't pass until both the code *and* the understanding do.

### Guiding principle: built-in first, library second

Every cross-cutting concern is first implemented with raw BCL/ASP.NET Core primitives, then refactored to the industry-standard library — so I understand what the abstraction actually does:

- `BackgroundService` + `Channels` → Wolverine queues
- Hand-rolled CQRS dispatcher → Wolverine handlers
- Hand-rolled outbox → Wolverine durable outbox
- Manual validation → FluentValidation
- `IMemoryCache` → Redis
- `ILogger` → Serilog

## Stack

.NET 10 · ASP.NET Core · PostgreSQL (Npgsql) · EF Core · Wolverine · xUnit · Docker · GitHub Actions

## Following along

- 📋 [ROADMAP.md](ROADMAP.md) — live progress, checked off task by task
- 📜 [.claude/PLAN.md](.claude/PLAN.md) — the full mentorship contract
- 🌳 The commit history itself — it's part of the curriculum (Conventional Commits, feature branches, clean history from commit #1)

---

*This README describes the journey. The product-focused README with architecture diagrams and ADRs arrives at the end of Phase 12 — written by hand, like everything else.*
