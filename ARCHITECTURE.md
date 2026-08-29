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

## The composition root

`Host` is the only project that binds an abstraction to an implementation. Every layer that has something to register exposes exactly one public entry point — `AddInfrastructure` today — and `Program.cs` calls it. Nothing else in the solution touches the container.

**A layer earns a registration method by having something to register.** `Infrastructure` has earned one, and the benefit is provable rather than stylistic: `EmailSender` is `internal sealed`, so naming it from `Host` fails to compile — `CS0246: the type or namespace name 'EmailSender' could not be found`. From `Host`'s position the type does not merely resist use, it does not exist. Without `AddInfrastructure` the implementation would have to be `public` so that `Host` could name it, and every layer would gain reach it has no business having. `Application` has not earned one. It owns the `IEmailSender` port but implements nothing, so it registers nothing, and an `Add` method that returns the collection unchanged is a method the next reader must open before discovering it does nothing.

**Referencing a DI package is not a violation of the dependency rule.** The rule orders *layers*; `Microsoft.Extensions.DependencyInjection.Abstractions` is a package, and no arrow turns outward because of it. What is given up is smaller, and worth naming rather than waving away: a layer that exposes a registration method now assumes a DI container exists. That assumption would have to be unwound if the layer were ever hosted without one. The trade is accepted because it buys `internal` implementations — a compile-time guarantee instead of a convention.

**Packages are declared by the project that uses them.** `Infrastructure` declares the DI and configuration abstractions because `Infrastructure` is what uses them. It had been reaching them transitively through `Application`, which compiled perfectly well, but left `Application`'s package list load-bearing for a project that is not `Application` — a change there would have broken a file nobody had touched.

**Lifetime.** `IEmailSender` is a singleton. The implementation holds no state between calls and depends on nothing scoped, so a per-request instance would be allocated and discarded for nothing. The second half of that test is the half that matters later: a singleton which captures a scoped dependency keeps the first one it is handed, for the life of the process.

**Configuration enters at the composition root.** `Host` owns `appsettings.json` and passes `IConfiguration` to each layer's registration method, so a layer reads its own settings once, while it is being wired. Services themselves must not take `IConfiguration` as a constructor dependency: a misspelled key would then surface at the first call rather than at startup, and nothing about the class would declare what settings it needs. Strongly-typed options with validate-on-start replace this in Task 2.6. The direction is decided but not yet exercised — nothing in `Infrastructure` reads a setting today.

### Should registration be automatic?

`Scrutor` can scan an assembly and register every type matching a convention. Against one registered service it would trade an explicit line for a rule the reader has to know.

Registration stays manual, and the trigger to revisit that is observable: **when adding a feature forces an edit to `DependencyInjection.cs` every single time.** Phases 3 and 9 bring repositories and handlers; that is when the ratio changes, and it can be checked rather than felt.

One boundary survives that trigger. Scanning suits registrations that carry no decision — the fiftieth `AddScoped<IThingRepository, ThingRepository>()` is noise, and a convention states it better than a line does. Registrations that carry a decision are a different case. A keyed strategy — one interface, three implementations, each resolved by its key — cannot be expressed by a scanner at all, because a scanner reads types and has no way to know which key was meant. The workarounds are to attribute each implementation, which teaches the implementation about the container and undoes exactly what `internal` bought, or to split registration between a scan and a hand-written list, leaving the reader two places to look. Those registrations stay explicit whatever else is scanned.

## What each project holds, concretely

| Project                | Example                 |
| ---------------------- | ----------------------- |
| `Host`                 | `QuestionsController`   |
| `Application`          | `CreateQuestionHandler` |
| `Application.Contract` | `CreateQuestionDto`     |
| `Infrastructure`       | `QuestionRepository`    |
| `Domain`               | `Question`              |
| `Domain.Shared`        | `QuestionStatus`        |
