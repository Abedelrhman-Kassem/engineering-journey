# 📌 Reference — Conventional Commits

> Mentor-provided reference. Keep open while committing until it's muscle memory.
> Full spec: https://www.conventionalcommits.org

## Anatomy

```
<type>: <short summary, imperative, lowercase, no period, ~50 chars>
                                        ← blank line (required if body exists)
<body — optional. The WHY and details. Wrap ~72 chars.>
```

- **Imperative** = command form: "add X", "remove Y", "fix Z" — *not* "added", "adding", "adds"
- Optional scope: `type(scope): summary` — e.g. `feat(auth): ...` (adopt later when the project grows)
- The message must **describe the diff** — never actions from another commit or another life

## Types

| Type | Use when the commit... |
|------|------------------------|
| `feat` | adds a capability users/API consumers can see |
| `fix` | repairs broken behavior |
| `chore` | maintenance touching neither src behavior nor docs (scaffolding, .gitignore, cleanup) |
| `docs` | only changes documentation |
| `refactor` | restructures code, behavior unchanged |
| `build` | build system, csproj, dependencies |
| `ci` | pipelines, GitHub Actions |
| `test` | only adds/changes tests |
| `perf` | improves performance, behavior unchanged |
| `style` | formatting only (whitespace, missing semicolons) — NOT css |

## Worked examples (imaginary bookstore project)

```
chore: scaffold solution with empty class libraries

Set up Bookstore.sln with Api, Core and Data projects plus
.gitignore so build artifacts never enter history.
```

```
docs: add contributing guide and architecture notes

CONTRIBUTING.md explains the branch workflow. ARCHITECTURE.md
records why Core has no external dependencies.
```

```
feat: add ISBN lookup endpoint

GET /api/books/{isbn} returns book details or 404.
Validation rejects malformed ISBN-10/13 before hitting the db.
```

```
fix: return 404 instead of 500 for unknown ISBN

The repository threw InvalidOperationException on missing rows;
now returns null and the controller maps it to NotFound.
```

## Self-check before every commit

1. Does the type match **this diff** (not what I did earlier today)?
2. Would the subject make sense in `git log --oneline` scrolled by a stranger?
3. If I wrote "and" in the subject — should this be two commits?
4. Spelling checked? (History is forever.)

## Related conventions (used throughout this program)

- **Branch names**: `feature/task-name` — kebab-case after the slash (e.g. `feature/request-logging`)
- **Push new branch with tracking**: `-u` flag on first push
- **Force push**: always `--force-with-lease`, never bare `--force`
