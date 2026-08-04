- revert when i undo the commit by creating new commit so the history didnot ruined, reset means move the head to a diffrent commit or undo the commit soft when i only undo the commit but changes is staged, mixed i unstage the changes and undo the commit, hard when i delete the changes entirely revert when i work with team and reset when private alone

- -m 1 means revert the other parent brought in

- when there is a bug in production and not time to fix then they revert the commit that has the bug and fix it so the production is stable

- pop applies stash and remove it if success, apply applying the stash without removing it

git stash → Save only changes to files Git already knows about.
git stash -u → Also save newly created (untracked) files.
