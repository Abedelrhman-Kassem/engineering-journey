# 📋 Task 0.5 — Minimal CI: build the solution on every push & PR

**Status:** 🔄 In progress
**Branch:** `feature/ci-build` off **`main`** (not off a lab branch — this ships)

## Goal
A GitHub Actions workflow that builds `Q&A.slnx` on every push to `main` and on every pull request targeting `main`, and that **blocks the merge button when it fails**.

This closes Phase 0: git stops being something you do carefully by hand and becomes something a machine verifies for you.

## Why this exists / when NOT to use it
- **Why:** every rule you've learned so far (clean history, PR workflow, revert instead of reset) is a *convention* — a human can ignore it. CI is the first rule the repo enforces on its own. It exists to catch "it built on my machine": your machine has a warm NuGet cache, your local SDK, your uncommitted files. A clean Linux runner has none of that. The most common thing CI catches on day one is a file you forgot to `git add`.
- **When NOT to:** a CI pipeline is a cost — every push burns minutes and every red build interrupts someone. Don't build a matrix across three OSes, four SDK versions, code coverage upload, artifact publishing and a Docker push for a repo with six projects and zero tests. Start with the single check that would actually have caught a real failure, and add stages only when you can name the failure each one prevents.

## Requirements

**Functional**
1. Workflow lives in `.github/workflows/` (the directory exists and is empty — that's the only place GitHub looks).
2. Triggers: push to `main`, and pull requests targeting `main`. Lab branches (`exp/*`) must **not** trigger it.
3. Steps: check out the repo → install the .NET SDK the projects target → restore → build in **Release**.
4. The build must fail the job on any compiler error. Decide deliberately whether warnings should fail it too, and be ready to defend the answer.
5. No test step yet — there are no test projects. Phase 3 adds them; you'll extend this workflow then.

**Non-functional**
- Pin the actions you use to a major version tag, not a floating branch.
- Give the job the minimum permissions it needs on the repo.
- The workflow must be readable by someone who has never seen it: named job, named steps.

**The real deliverable**
6. Open a PR from `feature/ci-build` → `main`, watch the check run **on the PR itself**, and merge only once it's green.
7. Turn on branch protection for `main` requiring this check to pass. A CI that doesn't block a merge is decoration.

## Traps this repo has waiting for you
Don't ask me for these answers — hit them and read the log.
- The solution file is `Q&A.slnx`. Two separate problems live in that name, and neither one shows up on Windows.
- `.slnx` is new. Not every SDK version on a runner understands it.
- Linux runners are case-sensitive about paths. Your `.csproj` references and folder names are not case-checked on Windows.

## Acceptance Criteria
- [ ] Workflow file committed under `.github/workflows/`, Conventional Commit message
- [ ] A **red** run exists in the Actions history before the green one, and you can say what it caught (if your first run is green, you got lucky — don't fake one, just note it)
- [ ] PR shows the check running and passing; merged via PR, not a local push to `main`
- [ ] `main` is branch-protected: the check is required
- [ ] Short note in `README.md`: what the workflow does and what it deliberately does **not** do yet

## Things to Research
- GitHub Actions vocabulary: workflow / job / step / runner / action — and which of those is billed
- `on: push` vs `on: pull_request` — for a PR from a branch in the *same* repo, why does a naive config run the whole thing twice?
- `actions/checkout`, `actions/setup-dotnet` — what does each actually do to the runner?
- `dotnet restore` then `dotnet build --no-restore` — why split them instead of one command?
- Debug vs Release in CI — which one, and why
- Branch protection rules / required status checks
- NuGet caching on CI — read what it does, then decide whether *this* repo needs it yet

## Common Mistakes
- Workflow in the wrong path (`.github/workflow/`, or nested a level too deep) — GitHub silently ignores it and you debug a workflow that was never registered
- Committing the workflow on a branch and wondering why nothing ran on `main`
- Unquoted paths with special characters in YAML
- Editing the YAML by pushing 14 "fix ci" commits — you'll do some of this, but squash before merging
- Adding test/coverage/publish steps "while I'm here" for a solution with no tests

## Housekeeping before you start
You have uncommitted work: `LAB-0.4.md` is modified on `exp/merge-demo`. Commit it there before branching — 0.4's review already lost time to a deliverable sitting uncommitted. Then branch from `main`, not from where you're standing.

---
*Say **review** when the PR is green and merged. Interview questions will include an over-engineering one: given this repo today, name three CI stages you could add and argue why each is not worth it yet.*
