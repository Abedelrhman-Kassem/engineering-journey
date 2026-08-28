# Architecture

Dependencies point inward toward `Domain`, which holds the business model — no inner layer may reference an outer one.

`Domain.Shared` sits beneath the whole solution as a shared kernel. It is not a layer in the inward/outward ordering, which is why `Application.Contract` may reference it without crossing a layer boundary.

```
Host
├── Application
└── Infrastructure

Application
├── Application.Contract
└── Domain

Infrastructure
├── Application
└── Domain

Application.Contract
└── Domain.Shared

Domain
└── Domain.Shared

Domain.Shared
```

## Host

- The ASP.NET Core project — the process that actually runs. Controllers, middleware, the request pipeline, endpoint filters.
- No business logic and no entities. It stays thin.
- References `Application` and `Infrastructure` in order to register their DI; through those it transitively reaches every other layer.

`Host → Infrastructure` is the one deliberate exception to the inward rule, and it is what makes the rule hold everywhere else. Registering `AddScoped<IQuestionRepository, QuestionRepository>()` requires seeing the interface and its implementation at the same time, so exactly one place has to know both. That place is the composition root. Because it is `Host`, no other layer ever learns that Postgres exists — `Application` asks for `IQuestionRepository` and gets it. Any layer _other_ than `Host` referencing `Infrastructure` would be a real violation.

## Application

- Orchestration — handlers and use-case coordination.
- Knows nothing about infrastructure concerns such as the database or Redis.
- References `Application.Contract` (to implement its interfaces) and `Domain` (to use entities in its handlers). It _consumes_ the domain model — it never shapes it. Adding a property to an entity is a Domain decision, not an Application one.

## Application.Contract

- DTOs and the Application interfaces.
- No logic and no implementation of any kind. It exists only so that other modules can reference the contracts and inject Application interfaces without taking a dependency on Application itself.
- References `Domain.Shared` so it can use enums and localization — nothing else.

## Infrastructure

- Talks to the outside world: database, Redis, any external service. Holds service and repository implementations.
- Contains no entities of its own.
- References `Application` to implement infrastructure interfaces such as `IEmailSender`, and `Domain` to implement the repository interfaces and work with entities.

Interfaces are split by who they face. `Application.Contract` holds the interfaces that _callers_ use, because they are part of the public surface other modules consume. Infrastructure-facing interfaces such as `IEmailSender` stay in `Application` — a contract that exists so other modules can call in has no business describing outbound services.

## Domain

- Entities, repository interfaces, domain services.
- Depends on nothing but `Domain.Shared`. This is the core of the project in DDD terms.
- References `Domain.Shared` only. As the core, it is unaware of every layer above it.

## Domain.Shared

- Holds what `Domain` and `Application.Contract` must both know: enums, and the resource keys for translatable messages.
- No logic. It exists so `Application.Contract` can share those primitives with `Domain` **without referencing `Domain` itself**. Delete this project and `QuestionStatus` has to move into `Domain`; `Application.Contract` must then reference `Domain` to see it, and every DTO becomes one `using` away from exposing an entity. The separation is a compile-time guarantee, not a convention.
- `Domain` needs translation **keys** because it throws business exceptions — `Order.MustHaveAtLeastOneItem` is domain vocabulary, and the rule that raises it doesn't change with the reader's language. The key constants and the JSON resource files sit here together so they cannot drift apart; `Domain` only ever references the keys and never reads the files. Resolving a key into text is `Application`'s job.
- References nothing.

## Audit — the references that exist today

| Reference                            | Exists today | Allowed by the rule |
| ------------------------------------ | ------------ | ------------------- |
| Host → Application                   | ✅           | ✅                  |
| Host → Infrastructure                | ✅           | ✅ composition root |
| Application → Application.Contract   | ✅           | ✅                  |
| Application → Domain                 | ✅           | ✅                  |
| Infrastructure → Application         | ✅           | ✅                  |
| Infrastructure → Domain              | ✅           | ✅                  |
| Domain → Domain.Shared               | ✅           | ✅                  |
| Application.Contract → Domain.Shared | ✅           | ✅                  |

The graph now matches the rule. Two defects were found and corrected.

`Application → Domain` was **missing**. Handlers are meant to orchestrate entities, and `Application` could not see a single one. Added.

`Host → Application.Contract` was **legal but redundant** — it pointed inward, so it broke no rule, but it stated something already guaranteed. `Host` will always reference `Application`, and `Application` will always reference `Application.Contract`, so the contracts stay reachable along a path this solution controls at both ends. Removed.

That second decision is **not yet proven**. Dropping the reference compiled immediately only because `Host` has no controllers — nothing in it consumes a DTO today. The first controller that takes a `CreateQuestionDto` is the real test, and it arrives in Phase 4. If the transitive path ever fails, this is the line to revisit.

## Enforcing this rule

Libraries such as `NetArchTest` and `ArchUnitNET` can fail a build when a forbidden reference appears. The question is whether this repo needs one yet.

Most violations of the dependency rule cannot happen here at all: they would point back at a project that already references the offender, and MSBuild refuses to build a circular reference. For those, the compiler is already the enforcement mechanism, and a test would add nothing. Exactly one violation compiles silently — `Application.Contract → Domain`. It creates no cycle, so the build stays green, CI stays green, and nothing else is watching: this repo has one contributor and no second reviewer on its pull requests. It would also be added for a *plausible* reason — "`Domain` is the core, so every project should reference it", which is true for `Application` and `Infrastructure` and wrong for `Application.Contract`, the one project that exists so DTOs cannot see entities. `git reset` and `git revert` are no defence here, because they repair a mistake that has been noticed, and this mistake is never noticed.

**Decision: not yet — until the solution has a test project, which arrives in Phase 3.** The assertion itself is a few lines. Today it would mean standing up the first test project in the solution to hold a single check, ahead of the phase where testing is introduced. Once that project exists the marginal cost is close to zero and the test is worth adding; adding it now would be solving the right problem in the wrong order.

## What each project holds, concretely

| Project                | Example                 |
| ---------------------- | ----------------------- |
| `Host`                 | `QuestionsController`   |
| `Application`          | `CreateQuestionHandler` |
| `Application.Contract` | `CreateQuestionDto`     |
| `Infrastructure`       | `QuestionRepository`    |
| `Domain`               | `Question`              |
| `Domain.Shared`        | `QuestionStatus`        |
