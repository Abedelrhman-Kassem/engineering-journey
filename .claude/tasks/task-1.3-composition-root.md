# 📋 Task 1.3 — The composition root

**Status:** 🔄 In progress
**Branch:** `feat/composition-root` off **`main`**

## Goal
Make `Host` the only place in the solution that knows how the object graph is built — and make that structural, not a convention people are asked to respect. Every other layer exposes **one** registration entry point and hides everything behind it.

Five of your six projects are currently empty. This task creates the first real types in the solution, chosen so that the wiring question has something real to wire.

## Why this exists / when NOT to use it
- **Why:** Right now `Program.cs` could register anything from anywhere, and nothing would stop it. A composition root is the single point where abstractions are bound to implementations. When there is exactly one, you can answer "what is this application actually made of?" by reading one file. When registration is scattered, that question has no answer and startup behaviour becomes something you discover at runtime.
- **When NOT to:** a single-project application does not need this. The layer-per-registration-method pattern buys you the ability to hide implementation types from the host. If everything is already in one assembly, there is nothing to hide, and you have added ceremony for nothing.

## Requirements

**Functional**

1. **Create one real outbound dependency to wire.** Your `ARCHITECTURE.md` already names the example: `IEmailSender`.
   - The **interface** belongs to `Application` — it is an outbound port, and `Application` is the layer that would call it.
   - The **implementation** belongs to `Infrastructure`.
   - Keep the method simple. This task is about wiring, not about sending mail. A method that logs to the console is fine as the implementation body.

2. **The implementation type must be `internal`.** `Host` must not be able to name it.

3. **One registration entry point per layer that has something to register.** `Program.cs` calls those entry points and does no per-service registration of its own for those layers. Host's own concerns — `AddControllers`, `AddOpenApi` — stay in `Program.cs`, because they are Host's, not another layer's.

4. **Do not create empty registration methods** for `Domain`, `Domain.Shared`, or `Application.Contract`. They hold no services. Symmetry is not a reason.

5. **Choose a lifetime** for the email sender and write one sentence saying why. Singleton, Scoped, or Transient — you must be able to defend the choice, not just pick the middle one.

6. **Decide where configuration enters.** The implementation would realistically need settings (an SMTP host, an API key). Does `Infrastructure` read `IConfiguration` itself, or does `Host` hand it what it needs? Decide the *direction* and justify it. You do not need the full options pattern — that is Task 2.6.

**The judgment deliverable (graded, same as 1.2's requirement 5)**

7. A section in `ARCHITECTURE.md` titled **"The composition root"**, answering two questions in your own words.

   **(a) Should `Application` be allowed to reference a DI package at all?**
   For `Application` to expose its own registration method, it must reference `Microsoft.Extensions.DependencyInjection.Abstractions`. There are two defensible positions:
   - *Strict*: only `Host` references anything DI-related. `Host` registers every service explicitly, naming each implementation type. The cost: nothing can be `internal`, because `Host` has to see it.
   - *Practical*: each layer references the abstractions package only, exposes one method, and keeps its types `internal`. The cost: an inner layer now knows a container exists.

   Pick one. Say what you gave up.

   **(b) Should registration be automatic?**
   Libraries such as `Scrutor` scan an assembly and register everything matching a convention, so you never write a registration line again. You currently have one service. Address: what problem does scanning solve, does that problem exist here, what does it cost when someone reads the code and cannot find where a type was registered — and your decision **with a named trigger**, in the same form as your Phase 3 trigger in the previous task.

**Non-functional**
- Conventional Commit messages. **Fix the PR title before merging** — GitHub's squash merge overwrites your commit subject with it.
- Build green locally and in CI. `TreatWarningsAsErrors` is on.
- Merged via PR. Direct push to `main` is now refused, as you proved.

## Traps
- **The symmetry trap.** Four layers, so four `Add...` methods feels right. Two of them would be empty. Empty methods are code that must be read, understood, and maintained, and they return nothing for it.
- **The `public` reflex.** Making the implementation `public` "in case something needs it later" silently destroys requirement 2. Nothing needs it. That is the point.
- **The folder-is-architecture trap.** Putting the extension methods in a folder called `Extensions` does not make this a composition root. What makes it one is that `Host` *cannot* register `Infrastructure`'s types even if it wanted to.
- **Configuration leaking backwards.** If `Infrastructure` reads `IConfiguration` directly, ask yourself which project owns `appsettings.json`, and what that means for testing `Infrastructure` in isolation.

## Things to Research
- **Composition Root** — Mark Seemann's definition, and why he insists there is exactly one per application
- The `IServiceCollection` extension-method convention (`Add{Something}`) — read how the ASP.NET Core packages themselves do it
- `internal` access, and `InternalsVisibleTo` for when tests need in
- Service lifetimes: Singleton, Scoped, Transient — what each one costs
- `TryAddScoped` vs `AddScoped` — why library authors reach for the `Try` versions and application authors usually do not
- What is actually inside `Microsoft.Extensions.DependencyInjection.Abstractions` versus the full `Microsoft.Extensions.DependencyInjection` package. The difference matters for requirement 7(a).
- `Scrutor` and assembly scanning — read what it does before deciding against it

## Common Mistakes
- Writing requirement 7 as a description of what Scrutor does, instead of a decision about this repo. This is the mistake the previous task's brief warned about too.
- Choosing Scoped by default because it is what tutorials use
- Registering the interface and implementation in `Program.cs` "just for now"

## Acceptance Criteria
- [ ] `IEmailSender` owned by `Application`; implementation owned by `Infrastructure` and declared `internal`
- [ ] Exactly two registration entry points exist, both called from `Program.cs`; no per-service registration for those layers in `Program.cs`
- [ ] No empty registration methods for layers that hold no services
- [ ] **Prove requirement 2:** write a line in `Program.cs` that names the implementation type, confirm it does not compile, paste the compiler error, then delete the line. A boundary you have not seen refuse you is decoration.
- [ ] Lifetime chosen, with a one-sentence justification
- [ ] Configuration direction decided and justified
- [ ] `ARCHITECTURE.md` has "The composition root" section answering 7(a) and 7(b), with a named trigger
- [ ] Build green locally and in CI
- [ ] Merged via PR, Conventional Commits, PR title corrected before merge

---
*Say **review** when the PR is merged.*
