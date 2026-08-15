# 🌳 Git — the working set

Everything Phase 0 covered, organised by **the situation you're in**, not by command name.
This is deliberately *not* every git command — it's the ones that earn their place, plus the traps you personally hit.

---

## 0. The model everything else rests on

- A **commit** is a full snapshot of the tree plus a pointer to its parent(s). Not a diff — git computes diffs on demand.
- A **branch** is a movable label pointing at one commit. Deleting a branch deletes a label, not commits.
- **HEAD** is where you are. Usually it points at a branch; when it points straight at a commit you're "detached".
- Three places a change can live: **working tree** (your files) → **index/staging** (`git add`) → **repository** (`git commit`).

Most confusing git behaviour becomes obvious once you ask: *which of those three is this command touching?*

---

## 1. "I need to undo something"

The one question that picks the command: **has this commit left your machine?**

| Situation | Command | What it does |
|---|---|---|
| Not pushed, want to fix the last commit | `git commit --amend` | Replaces the last commit. New hash — so it's a rewrite. |
| Not pushed, want commits gone but keep the changes staged | `git reset --soft <commit>` | Moves the branch label back. Changes stay staged. |
| Not pushed, want commits gone, keep changes unstaged | `git reset <commit>` (`--mixed`, the default) | Moves label back, unstages. Files untouched. |
| Not pushed, want it all gone | `git reset --hard <commit>` | **Destroys uncommitted work.** Recoverable only via reflog, and only if committed. |
| **Already pushed / shared** | `git revert <commit>` | Adds a *new* commit that undoes it. History is append-only. Safe. |
| Discard changes to one file | `git restore <file>` | Working tree back to last commit. |
| Unstage one file, keep the edit | `git restore --staged <file>` | Index only. |

**The rule:** `reset` rewrites, `revert` appends. Anything other people can see, you `revert`.

### Revert specifics you already hit

- **Reverting a non-tip commit** works fine. Git applies the inverse of that one commit's diff. It conflicts if later commits touched the same lines.
- **Reverting a merge commit** needs `-m` because the commit has two parents and git can't guess which line of history you want to keep:
  ```
  git revert -m 1 <merge-sha>
  ```
  `-m 1` = keep the **first parent** = the branch you were standing on when you merged. That's almost always what you want.
  `-m 2` keeps the branch you merged *in*. If the merge was a fast-forward-shaped one where parent 2's tree already equals the merge's tree, this produces **an empty commit — nothing at all**, not an empty file.
- **Revert the revert** to bring work back: `git revert <the-revert-sha>`. Prove it worked by comparing tree hashes, not by eyeballing files.
- **Re-merging a reverted branch does NOT restore the work.** The merge base already contains those commits, so git sees nothing new to bring. Only reverting the revert restores it.

### `revert` vs fix-forward
`revert` when the change is wrong and you want it gone *now*, cleanly, with an audit trail. **Fix-forward** (a new commit that corrects it) when the change was mostly right — reverting and re-applying three-quarters of it is more churn than fixing it.

---

## 2. "I need to combine two branches"

| Command | Result | Use when |
|---|---|---|
| `git merge <branch>` | Fast-forwards if possible, otherwise makes a merge commit | Default. Integrating a finished feature. |
| `git merge --no-ff <branch>` | **Keeps every commit AND adds a merge commit**, even if a fast-forward was possible | You want the branch visible as a unit in history |
| Squash merge | **Collapses the whole branch into one new commit.** No merge commit, no parent link to the branch | The branch has 14 "fix ci" commits nobody needs |
| `git rebase <base>` | Replays your commits on top of base. New hashes. Linear history | Cleaning up **your own unpushed** work before a PR |

`--no-ff` ≠ squash. `--no-ff` *preserves* commits and adds one. Squash *destroys* them and adds one. They look similar in a diagram and do opposite things to history.

**Fast-forward** happens when your branch's tip is a direct ancestor of the target — git just slides the label forward. No merge commit exists because no merging was needed.

### Why keep merge commits at all
Two reasons, both practical: **revertability** (`git revert -m 1 <merge>` undoes the whole feature in one move) and **traceability** (you can see which commits shipped together as one unit).

### The golden rule
> **Never rebase commits that exist outside your repository.**

Rebasing rewrites hashes. If someone else has your old commits, their history and yours have now diverged — and the "fix" is a force-push that silently deletes work they based on the old commits.

---

## 3. "I need to put this aside for a minute"

```
git stash            # tracked changes only
git stash -u         # include untracked files  ← usually what you actually want
git stash list
git stash pop        # apply + remove from stash list
git stash apply      # apply, KEEP in stash list
```

**`pop` vs `apply`:** pop removes the entry after applying. Use `apply` when you want the same changes on two branches.

**The trap you hit:** if `pop` hits a **conflict**, it does **not** drop the stash entry — it stays in the list. That's deliberate (you might need it again), but the bite comes days later when you `pop` it a second time and re-apply changes you already resolved. Check `git stash list` after any conflicted pop.

Stash is for *minutes*, not days. A stash is invisible, unnamed, and not pushed anywhere. For anything you'd be annoyed to lose, make a commit on a branch instead — commits are cheap and you can always `reset --soft` them away later.

---

## 4. "I think I destroyed something"

```
git reflog
```

Every time HEAD moves — commit, checkout, reset, rebase, merge — git logs it. Reflog is a local, ~90-day record of where you've been, including commits that no branch points at any more.

```
git reflog                       # find the sha you were at
git reset --hard <sha>           # go back there
git branch rescue <sha>          # or: save it as a branch first (safer)
```

**This is the reason `reset --hard` is survivable.** What reflog cannot save: changes that were never committed. Uncommitted work destroyed by `reset --hard` or `checkout` is genuinely gone.

---

## 5. Working with the remote

```
git push
git push --force-with-lease      # after a rebase — NEVER plain --force
git pull --rebase                # ← use this by default
git fetch                        # download without touching your branches
```

**`--force-with-lease` vs `--force`:** plain `--force` overwrites the remote unconditionally, including commits a teammate pushed while you were rebasing. `--force-with-lease` refuses if the remote moved since your last fetch. There is no situation in this repo where plain `--force` is the right call.

**`pull` vs `pull --rebase`:** plain `git pull` is fetch + **merge**, which creates a merge commit — including the pointless "Merge branch 'x' of github.com/... into x" self-merge you get from pulling your own branch. `git pull --rebase` replays your local commits on top instead, and the history stays clean.

*(You hit this in 0.5 — commit `0c75d79`.)*

**One branch → one PR → one merge.** Reusing a feature branch for a second unrelated change is how 0.2's history got tangled.

---

## 6. Commit messages

```
type: what changed, lowercase, no period
```

| Type | When |
|---|---|
| `feat` | new behaviour a user could notice |
| `fix` | a bug is fixed |
| `docs` | only documentation changed |
| `chore` | tooling, config, CI, deps, gitignore |
| `refactor` | code changed, behaviour didn't |

Two habits cover the rest: **subject under ~70 characters**, and **if the subject needs the word "and", it's two commits.** Detail goes in the body after a blank line.

Full spec: [conventional-commits.md](conventional-commits.md)

---

## 7. Your actual scars

Things that already cost you time in this repo — worth rereading before you think you're safe:

- **`reset --hard` without committing first** (0.1) — reflog saved the commits; it cannot save uncommitted files.
- **Reusing one branch for multiple PRs** (0.2) — produced a four-merge tangle on `main` that is permanent.
- **Assuming `-m 2` would give an empty file** (0.4) — it gives *nothing*. Parent 2's tree already equalled the merge's tree. Two long detours came from misreading an empty diff as a bad command.
- **Re-popping a conflicted stash** (0.4) — the entry survives a conflicted `pop`.
- **Deliverable left uncommitted at review time** (0.4, 0.5) — twice. CI cannot test a file that only exists on your disk.
- **`git pull` instead of `git pull --rebase`** (0.5) — one junk self-merge commit, permanent.
- **A commit named `test`** (0.5) — `af0d059`, on `main` forever, adding a stray `y` to `.gitignore`.

The pattern in most of these isn't a wrong command. It's acting before reading what git actually said.
