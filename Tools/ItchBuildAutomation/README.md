# Local Unity builds and itch.io uploads

This repository can enqueue an exact Git commit for a detached background Unity build whenever its commit message contains the literal marker `[build]`. The worker builds in a persistent sibling Git worktree, so the main working tree and its open Unity Editor are never switched, reset, cleaned, or locked. Successful uploads are opt-in.

For a deterministic AI-agent procedure that ports this architecture to another Unity repository, see [`AI_AGENT_SETUP_GUIDE.md`](AI_AGENT_SETUP_GUIDE.md).

> **Warning:** once uploads are enabled, any commit containing `[build]` can publish unfinished code to the configured itch.io development channel.

## Install and configure

Run:

```bash
Tools/ItchBuildAutomation/install.sh
```

The installer creates the ignored `Tools/ItchBuildAutomation/config.local.env`, activates `.githooks` through `core.hooksPath`, composes with the existing hooks in `.git/hooks`, creates the runtime state directory, and creates or validates the persistent detached worktree.

Edit `config.local.env` and replace `username/game-slug` with the existing itch.io project target. Keep the WebGL channel or choose another development channel:

```bash
ITCH_TARGET="username/game-slug"
ITCH_CHANNEL="html5-development"
ITCH_UPLOAD_ENABLED="0"
```

Authenticate outside the repository:

```bash
butler login
```

Do not place `BUTLER_API_KEY` in the config file. If a secure environment already provides it, the worker inherits it without printing the environment or token.

Run diagnostics at any time:

```bash
Tools/ItchBuildAutomation/doctor.sh
```

## Trigger and queue behavior

A normal commit does nothing. A case-sensitive `[build]` anywhere in the full subject or body enqueues that exact SHA and returns without waiting for Unity:

```bash
git commit -m "Fix character animation [build]"
```

Only one worker owns the `mkdir`-based lock. If A is building and B, C, then D are requested, each new request atomically replaces the pending request; A finishes and D builds next. The worker exits once the queue is empty, so there is no polling daemon.

Manual requests use the same queue and worker:

```bash
Tools/ItchBuildAutomation/request-build.sh
Tools/ItchBuildAutomation/request-build.sh --commit HEAD
Tools/ItchBuildAutomation/request-build.sh --commit HEAD --build-only
```

`--build-only` applies only to that queued request and does not rewrite persistent configuration. To retain all builds without uploading, leave `ITCH_UPLOAD_ENABLED="0"`.

## Project-specific build entry point

The worker invokes the repository's existing `Game.Editor.WebBuild.PerformBuild` method with target `WebGL`. That method reads enabled scenes through `EditorBuildSettings.scenes`, runs `BuildPipeline.BuildPlayer`, checks `BuildReport.summary.result`, logs its summary, and exits Unity nonzero on failure. The worker uses only the exact editor version recorded by the queued commit's `ProjectVersion.txt`; it does not substitute another installed version.

To change platforms later, update `BUILD_TARGET`, `BUILD_PRODUCT_NAME`, and `UNITY_BUILD_METHOD` together with an entry point that supports that platform. Install the matching platform module in the exact Unity Editor first.

## State, logs, and builds

Machine state is below Git's common directory:

```text
.git/itch-build-automation/
  pending-commit
  active-commit
  last-successful-commit
  last-failed-commit
  worker.pid
  worker.lock/
  logs/
  builds/
```

The default worktree is the persistent sibling `<repository>-itch-build-worktree`; override it with an absolute `WORKTREE_PATH` in local config. Its ignored `Library` is deliberately retained and never shared with the main worktree.

Inspect status and recent output with:

```bash
Tools/ItchBuildAutomation/doctor.sh
tail -f .git/itch-build-automation/logs/launcher.log
cat .git/itch-build-automation/logs/latest.log
```

Each job has separate `*-worker.log` and `*-unity.log` files. Successful outputs remain under `.git/itch-build-automation/builds/<short-sha>/`, including when an upload fails.

If `doctor.sh` reports a stale lock, first verify no worker process owns the recorded PID. Starting a new request automatically removes an empty stale lock after verifying the PID is not this automation. If the lock directory contains unexpected files, the worker refuses to remove it; inspect it and remove it manually only after confirming no build is active.

## Removal

Disable the hook and stop this automation's verified worker while preserving configuration, worktree, logs, and builds:

```bash
Tools/ItchBuildAutomation/uninstall.sh
```

Removal of retained data is always explicit:

```bash
Tools/ItchBuildAutomation/uninstall.sh --remove-worktree
Tools/ItchBuildAutomation/uninstall.sh --remove-state --remove-config
```

The worktree removal is non-forced and refuses a dirty worktree. Uninstall leaves unrelated hooks and hook configuration unchanged.

## Unity caveats

- The first import in the build worktree can be slow; later builds reuse its separate `Library` cache.
- Background Unity consumes CPU, memory, disk, and thermal headroom.
- The installed Unity license and project setup must permit the main Editor and background Editor to coexist.
- The configured platform module must be installed for the exact project editor version.
- The worker rejects another Unity process using the build worktree path, but does not treat the developer's main-worktree Editor as a conflict.
