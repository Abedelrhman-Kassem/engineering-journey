# 📋 Task 1.2 — Enforce the dependency rule

**Status:** 🔄 In progress
**Branch:** `refactor/dependency-rule` off **`main`**

## Goal
Make the project references match the rule you wrote in `ARCHITECTURE.md`, then update the audit so it describes reality again. Two edits to `.csproj` files — and a written decision about whether to automate the rule going forward.

## Why this exists / when NOT to use it
- **Why:** 1.1 produced a rule and an audit that already disagrees with it. A rule nobody enforces decays into a comment. This is the task where the documented architecture and the actual build graph become the same thing — and where you find out whether your `Host → Application.Contract` decision was right, because you're about to remove it and see what happens.
- **When NOT to:** do not chase a "perfect" reference graph in a solution with no features. Reference hygiene matters because it prevents a *specific* leak you can name. Where you can't name the leak, leave the reference alone.

## Requirements

**Functional**
1. Add the missing `Application → Domain` reference.
2. Remove the redundant `Host → Application.Contract` reference — the decision you already made and justified.
3. Solution builds clean, locally and in CI. Remember `TreatWarningsAsErrors` is on now.
4. Update the audit table in `ARCHITECTURE.md` so "Exists today" is true again. Keep the prose findings — rewrite them in the past tense as what *was* fixed, don't delete the reasoning.

**The over-engineering deliverable (this replaces the interview question)**
5. A short section in `ARCHITECTURE.md` — five or six lines, titled something like "Enforcing this rule" — answering: **should a test enforce the dependency rule automatically?**
   Tools exist for this (`NetArchTest`, `ArchUnitNET`) that fail a build when a forbidden reference appears. Address, in your own words:
   - What failure would such a test prevent, concretely, in *this* repo?
   - Can that failure currently happen? Who would have to do what, and would anyone notice without the test?
   - What does the test cost — to write, to keep true as the architecture changes, and in false alarms during refactoring?
   - Your decision, and the condition that would change it. "Not yet, until X" is a complete answer. So is "yes, because Y." What isn't acceptable is a decision with no named trigger.

   Write this yourself. It is the deliverable I'm actually grading.

**Non-functional**
- Commit hygiene: decide whether the two reference changes are one commit or two, and be ready to say why.
- Conventional Commit messages. This is now an automatic process failure if missed.
- Merged via PR with `build` green. No direct push to `main` — and fix the branch-protection gap from 0.5 first (see Housekeeping).

## Traps
- Removing `Host → Application.Contract` will compile trivially **because `Host` has no controllers yet**. Nothing in it uses a DTO. Your decision is therefore *untested* until Phase 4 — note that honestly in the doc rather than claiming it's proven.
- Adding `Application → Domain` may surface warnings that are now errors.
- MSBuild caches. If a reference change seems to have no effect, you know what to suspect.

## Things to Research
- `ProjectReference` and `PrivateAssets` — how you'd stop a reference flowing transitively, and why that changes the "reference what you use" argument
- What actually happens at compile time when you use a type from a transitively-referenced assembly
- Architecture-testing libraries — read what they check, then decide (requirement 5)
- Central Package Management (`Directory.Packages.props`) — the package-version equivalent of the `Directory.Build.props` you already added. You don't need it yet; know it exists.

## Common Mistakes
- Deleting the audit's reasoning instead of moving it to past tense — the *why* is the valuable part, not the table
- "Fixing" references that weren't in your findings because they look untidy
- Writing requirement 5 as a list of tool features instead of a decision about this repo

## Acceptance Criteria
- [ ] `Application → Domain` added; `Host → Application.Contract` removed
- [ ] Build green locally and in CI
- [ ] `ARCHITECTURE.md` audit table matches reality; findings preserved in past tense
- [ ] "Enforcing this rule" section written — your words, with a named trigger condition
- [ ] Trap noted: the Host/Contract decision is unproven until `Host` actually consumes a DTO
- [ ] Merged via PR, Conventional Commit messages, no direct push to `main`

## Housekeeping before you start
Close the branch-protection gap found in 1.1's review — commit `a213e97` reached `main` with no PR. On the `main` rule, tick **Require a pull request before merging** and **Do not allow bypassing the above settings**. Then verify it: try pushing a trivial commit straight to `main` and confirm it's rejected. A protection you haven't seen refuse you is still decoration.

---
*Say **review** when the PR is merged.*
