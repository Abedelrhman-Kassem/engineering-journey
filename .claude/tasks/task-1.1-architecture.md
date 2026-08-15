# 📋 Task 1.1 — ARCHITECTURE.md: what belongs in each layer

**Status:** 🔄 In progress
**Branch:** `docs/architecture` off **`main`**

## Goal
A short `ARCHITECTURE.md` at the repo root that explains, for each of the six projects in this solution, **what belongs in it, what must never be in it, and what it is allowed to depend on** — plus an honest section on where the current wiring already disagrees with those rules.

No code this task. Not a single `.cs` file. This is the task where you decide what the rules *are*, so that 1.2 can enforce them.

## Why this exists / when NOT to use it
- **Why:** you inherited a six-project layout from a template and have been moving project references around by instinct — `Application.Contract` was pointed at `Domain.Shared` in commit `7931a37` with no stated reason. A layered solution where nobody wrote down the rule is just six folders. The written rule is what makes a wrong reference *reviewable* instead of a matter of taste. It also front-loads the argument: it's far cheaper to be wrong about layering in a markdown file than in 40 classes.
- **When NOT to:** most applications do not need six projects. A single project with folders is the correct architecture for a large number of real systems, and splitting early buys you assembly boundaries you pay for in ceremony on every single change. Be ready to defend why *this* project earns the split — "Clean Architecture says so" is not a defence.

## Requirements

**Functional**
1. `ARCHITECTURE.md` at the repo root.
2. One short section per project: `Domain.Shared`, `Domain`, `Application.Contract`, `Application`, `Infrastructure`, `Host`. Each says: **what lives here**, **what must never live here**, **what it may reference**.
3. A **dependency rule** stated in one sentence — the general principle, not a list. Then a diagram (ASCII or mermaid) of what the references *should* be.
4. A section listing the references that exist **today**, and for each one whether it obeys your rule. Do not fix anything — 1.2 does that. Just be honest about what you find.
5. For each project, name a concrete example of a type that would live there in *this* app (a Q&A site). Not "entities" — an actual name, like the thing you'd create in Phase 3.

**Non-functional**
- Under 150 lines. If it's longer, you're writing a textbook chapter instead of a repo document.
- Written for a new teammate on day one, not for a grader.
- No copy-pasted Clean Architecture diagrams from the internet. Yours, about your six projects.

## The current reference graph — read this before you write

```
Domain.Shared      → (nothing)
Domain             → Domain.Shared
Application.Contract → Domain.Shared
Application        → Application.Contract
Infrastructure     → Application, Domain
Host               → Application.Contract, Application, Infrastructure
```

Do not "fix" this. Study it and answer honestly in the document:

- `Application` does **not** reference `Domain`. Given what you think Application is for — is that a deliberate design, or an accident? What can Application currently not do because of it?
- `Host` references `Application.Contract` explicitly, even though `Application` already brings it in transitively. Is that redundant, or is it saying something?
- `Infrastructure` references both `Application` and `Domain`. Which of those two does it need, and why?
- What is `Domain.Shared` actually *for*, as distinct from `Domain`? You created the split. Justify it — or write down that you can't, which is also a legitimate finding.

## Things to Research
- The dependency rule / dependency inversion — what "dependencies point inward" means at the *project reference* level, not the class level
- Why the interface for a repository lives in one project and its implementation in another — what problem that solves, and what it costs
- Transitive project references in MSBuild — if A→B and B→C, what can A actually see?
- `InternalsVisibleTo`, `internal` vs `public` — a second way to enforce boundaries that doesn't need six projects
- Read one real .NET repo's architecture doc for shape (not content)

## Common Mistakes
- Describing the layers in the abstract instead of describing *your six projects*
- Writing the rule you wish were true and quietly not mentioning that the current references break it
- Fixing the references while writing the document — that's 1.2, and mixing them makes both un-reviewable
- Naming a project's contents as "business logic" without saying what distinguishes it from the next project's contents

## Acceptance Criteria
- [ ] `ARCHITECTURE.md` at repo root, under 150 lines, committed on `docs/architecture` with a Conventional Commit message
- [ ] All six projects covered with what-lives-here / what-never / may-reference
- [ ] Dependency rule stated in one sentence + a diagram of the intended graph
- [ ] Honest audit section: every current reference marked as obeying or violating the rule, with no fixes applied
- [ ] Each project has a concrete example type named for this Q&A app
- [ ] Merged via PR with the `build` check green — no local push to `main`

---
*Say **review** when the PR is merged. Interview will include the NuGet-cache question you owe me from 0.5, plus an over-engineering one: argue the case for collapsing these six projects into two.*
