# 🛤️ Engineering Journey

> A deliberate-practice apprenticeship: rebuilding senior-level .NET engineering skills — **every line of code written by hand, no AI-generated code.**

## Why this repo exists

Like many developers, I spent the last years shipping projects with heavy AI assistance. Projects shipped — but I felt my hands-on engineering instincts getting weaker. This repo is my answer: a structured, mentored program where I write **100% of the code myself**, from ASP.NET Core pipeline fundamentals all the way to senior-level architecture.

AI participates in exactly one role here: as a **strict senior reviewer**. It assigns tasks with acceptance criteria, reviews my pull requests with scored feedback, and grills me with interview questions — but it is contractually forbidden from writing a single line of implementation code ([the contract](.claude/PLAN.md) is committed to this repo, and enforced by [CLAUDE.md](CLAUDE.md)).

**Every commit here is me, my editor, and the documentation.**

## The program

A 13-phase curriculum, small daily tasks growing into feature-sized work — tracked task-by-task in **[ROADMAP.md](ROADMAP.md)**:

| Phases | Focus                                                                               |
| ------ | ----------------------------------------------------------------------------------- |
| 0–2    | Git discipline, Clean Architecture, ASP.NET Core pipeline (middleware, filters, DI) |
| 3–4    | EF Core + PostgreSQL, REST API craft                                                |
| 5–7    | Concurrency, security (JWT, RBAC), performance & caching                            |
| 8–10   | DDD, CQRS, multi-tenancy, feature flags, idempotency, background processing         |
| 11–12  | Testing depth, Docker, observability, CI/CD                                         |

Every task = feature branch → Conventional Commits → PR-style review (scored ★ across architecture, readability, security, performance, testability...) → 3–5 senior interview questions. Tasks don't pass until both the code _and_ the understanding do.

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

## CI

Every push to `main` and every pull request targeting `main` runs [`.github/workflows/ci.yaml`](.github/workflows/ci.yaml) on a clean Ubuntu runner: checkout → install the .NET 10 SDK → `dotnet restore` → `dotnet build --configuration Release`. `main` is branch-protected and this build is a required check, so a red run blocks the merge button rather than merely embarrassing me.

Two deliberate details: the build runs in **Release**, the configuration that actually ships, not the one that's convenient locally. And restore is its own step, with `--no-restore` on the build — so a dependency problem reads as a dependency problem in the log instead of hiding inside a build failure.

Warnings fail the build. TreatWarningsAsErrors lives in Directory.Build.props rather than as a CI flag, so it binds locally as well — a warning breaks my build before it ever reaches a runner. Adopted now, at zero warnings, because the price of this policy only ever goes up.

What it deliberately does **not** do yet:

| Not here                  | Why not                                                                                                                                                  |
| ------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Test step                 | There are no test projects yet. Phase 3 adds them, and this workflow grows then.                                                                         |
| Coverage reporting        | Nothing to cover. A coverage badge over zero tests is a lie told in green.                                                                               |
| NuGet caching             | A cold restore costs ~30 seconds. Caching buys back seconds nobody is waiting on, in exchange for a cache key to maintain and stale-cache bugs to debug. |
| OS / SDK matrix           | One target SDK, deployed to Linux. A three-OS matrix would triple the bill to test platforms this project never runs on.                                 |
| Publish / Docker / deploy | Nothing to deploy yet. That arrives with Phase 11.                                                                                                       |

Each of those is a real cost, not an oversight. They get added when I can name the failure each one prevents.

This workflow has never gone red — not because it was written carefully, but because it never names the solution file. `dotnet restore` with no argument discovers `Q&A.slnx` on its own, so the `&` never reaches a shell that would read it as "run in background." Path casing across the six projects _was_ tested by the Linux runner, and passed. The quoting trap is still there, waiting for the first step that takes a path.

_This README describes the journey. The product-focused README with architecture diagrams and ADRs arrives at the end of Phase 12 — written by hand, like everything else._
