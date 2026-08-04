- revert when i undo the commit by creating new commit so the history didnot ruined, reset means move the head to a diffrent commit or undo the commit soft when i only undo the commit but changes is staged, mixed i unstage the changes and undo the commit, hard when i delete the changes entirely revert when i work with team and reset when private alone

- -m 1 means revert the other parent brought in the number decide the branch 1 for the branch i merge on it to add the edits 2 for the branch that contains the new edits

- when there is a bug in production and not time to fix then they revert the commit that has the bug and fix it so the production is stable it named reapply revert it restores what the revert removed

- pop applies stash and remove it if success, apply applying the stash without removing it
  Pop-conflict bite: git stash pop can produce merge conflicts, leaving you to resolve them before continuing (the stash is typically kept if the pop doesn't complete cleanly).
  "Lose track of it" case: If git stash pop succeeds, it deletes the stash, so you can't easily reapply it later if you realize you still needed it.
  Safer habit: Use git stash apply first when the stash is important, then git stash drop after you've confirmed everything is correct.

git stash → Save only changes to files Git already knows about.
git stash -u → Also save newly created (untracked) files.
