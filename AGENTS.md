# AI agent instructions

## Required Git workflow for AI agents

This standing rule applies to every AI agent adding or modifying code in this repository.

1. Before editing, inspect the working tree, branch, and remote, then run `git pull --ff-only` from the branch's upstream. For a new branch, pull the intended base branch before creating it. If the branch has no upstream, explicitly pull the appropriate remote/base branch before editing.
2. Preserve existing work. If pulling is blocked by local changes, divergence, conflicts, or authentication, resolve it without discarding anyone's changes; otherwise report the blocker before making code changes.
3. Make the requested changes and run checks appropriate to the change.
4. Before finishing, stage only the changes belonging to the task, create a descriptive Git commit, and push the working branch to GitHub (`git push`, or `git push -u origin <branch>` for a new branch). Committing and pushing are part of the task and do not require another confirmation.
5. Verify that the push succeeded and report the branch, commit hash, and validation results. Do not claim completion while task changes remain uncommitted or unpushed. If a push is blocked, preserve the local commit and clearly report the blocker.
6. Respect branch protections and use a feature branch/pull request when required. Never force-push, discard unrelated changes, or commit secrets to satisfy this rule. Read-only tasks and tasks that produce no changes do not require an empty commit.

When creating another repository for Frailey-Woodworks or asher-netizen, include this rule in its root `AGENTS.md` and its AI tool instruction files.
