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
| Host → Application.Contract          | ✅           | ✅ — but redundant  |
| Application → Application.Contract   | ✅           | ✅                  |
| Application → Domain                 | ❌           | ✅ — must be added  |
| Infrastructure → Application         | ✅           | ✅                  |
| Infrastructure → Domain              | ✅           | ✅                  |
| Domain → Domain.Shared               | ✅           | ✅                  |
| Application.Contract → Domain.Shared | ✅           | ✅                  |

**No reference in this solution violates the dependency rule** — every arrow already points inward. The audit turns up two defects of a different kind.

`Application → Domain` is **missing**. Handlers are supposed to orchestrate entities, and right now `Application` cannot see a single one.

`Host → Application.Contract` is **legal but redundant**. `Host` will always reference `Application`, and `Application` will always reference `Application.Contract`, so the contracts are reachable along a path this solution controls at both ends. An explicit reference here states something already guaranteed, so it is dropped.

Neither is corrected by this document — writing down what is true comes first. Task 1.2 makes the changes.

## What each project holds, concretely

| Project                | Example                 |
| ---------------------- | ----------------------- |
| `Host`                 | `QuestionsController`   |
| `Application`          | `CreateQuestionHandler` |
| `Application.Contract` | `CreateQuestionDto`     |
| `Infrastructure`       | `QuestionRepository`    |
| `Domain`               | `Question`              |
| `Domain.Shared`        | `QuestionStatus`        |
