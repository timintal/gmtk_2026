# Deterministic AI-agent runbook: local Unity builds and itch.io uploads

Use this document as a task specification for an AI coding agent. It describes a reusable **local macOS CI pipeline** for a Git-hosted Unity project. The finished pipeline reacts to a literal, case-sensitive `[build]` marker in a commit message, builds that exact commit in a persistent detached Git worktree, and optionally uploads a verified build to itch.io with Butler.

This is local developer automation, not hosted production CI. Installation and credentials are machine-local and must be configured separately on every Mac that should build.

## 1. Required outcome

Implement this event flow:

```text
post-commit hook
  -> read complete HEAD commit message
  -> if it contains [build], atomically enqueue the exact HEAD SHA
  -> return immediately
  -> one detached worker acquires a lock
  -> persistent sibling worktree checks out the queued SHA detached
  -> exact Unity version builds through a public static Editor method
  -> verify Unity exit code, BuildReport result, and output existence
  -> if explicitly enabled, Butler uploads the verified output
  -> record status, release lock, and process the newest pending SHA
```

The main working tree must never be reset, cleaned, switched, or used as the background Unity project.

## 2. Non-negotiable safety rules

The agent must enforce all of these:

1. Never run `git reset`, `git clean`, `git checkout`, or `git switch` in the developer's main worktree.
2. Never use `git clean -fdx`; it deletes ignored Unity caches.
3. Never share or symlink `Library` between worktrees.
4. Never build a branch name or moving tip. Resolve and queue a full commit SHA.
5. Never start Butler after a Unity failure or missing expected output.
6. Never enable uploads during initial validation.
7. Never store Butler credentials, API keys, logs, builds, PIDs, or locks in tracked files.
8. Never replace existing Git hook behavior without composing with it.
9. Never silently substitute a different Unity Editor version.
10. Never claim upload success without a zero Butler exit code and a read-only status check.
11. Preserve unrelated dirty working-tree changes.
12. Stop and report instead of deleting an unexpected or unverified worktree/lock path.

## 3. Phase A — inspect before editing

Run the following from the repository supplied by the user. Record every result for the final report.

### A1. Capture Git state

```bash
git status --short --branch
git rev-parse --show-toplevel
git rev-parse --path-format=absolute --git-common-dir
git config --show-origin --get core.hooksPath || true
git worktree list --porcelain
```

Do not alter any pre-existing changes.

Inspect executable legacy hooks:

```bash
find .git/hooks -maxdepth 1 -type f -perm +111 -print
```

If `core.hooksPath` already points somewhere other than `.githooks`, do not overwrite it. Either implement a reviewed dispatcher in that directory or stop and explain the conflict.

### A2. Locate the Unity project

Find every version file while excluding generated caches:

```bash
find . \
  -path './.git' -prune -o \
  -path '*/Library' -prune -o \
  -path '*/Temp' -prune -o \
  -path '*/Logs' -prune -o \
  -name ProjectVersion.txt -path '*/ProjectSettings/ProjectVersion.txt' -print
```

A valid Unity root contains all three:

```text
Assets/
Packages/
ProjectSettings/ProjectVersion.txt
```

Set `UNITY_PROJECT_RELATIVE_PATH="."` for a root project or its repository-relative subdirectory otherwise. If multiple valid projects exist, stop and ask which one to automate.

### A3. Require the exact Unity version

Parse:

```bash
sed -n 's/^m_EditorVersion:[[:space:]]*//p' \
  <unity-root>/ProjectSettings/ProjectVersion.txt
```

The normal executable must be:

```text
/Applications/Unity/Hub/Editor/<exact-version>/Unity.app/Contents/MacOS/Unity
```

Verify it is executable. Do not use another installed version as fallback. Also verify that the intended platform module exists under that Editor's `PlaybackEngines` directory.

### A4. Find the intended build target and entry point

Search source and documentation:

```bash
rg -n -S \
  'BuildPipeline\.BuildPlayer|BuildPlayerOptions|-executeMethod|MenuItem\(|butler|itch\.io|GameCI|fastlane|jenkins' \
  Assets Packages README.md docs .github 2>/dev/null
```

Selection order is deterministic:

1. Use an existing documented command-line build method if it validates `BuildReport` and returns a reliable process exit code.
2. Otherwise extend an existing Editor build script instead of creating a competing pipeline.
3. If no usable entry point exists, add `Assets/Editor/ItchBuildAutomation.cs` under the selected Unity root.
4. Infer the target from existing build scripts and project documentation.
5. If it still cannot be inferred, use `StandaloneOSX` on macOS and state that default explicitly.

Inspect enabled scenes through the chosen Editor method using `EditorBuildSettings.scenes`. A serialized `EditorBuildSettings.asset` count is acceptable only as a diagnostic fallback when Unity is not running.

### A5. Inspect tooling

```bash
command -v git-lfs || true
git lfs version 2>/dev/null || true
command -v butler || true
butler -V 2>/dev/null || true
command -v shellcheck || true
sed -n '1,260p' .gitignore
test -f .gitattributes && sed -n '1,200p' .gitattributes
test -f .gitmodules && sed -n '1,200p' .gitmodules
```

Missing Butler is not a build-only blocker. Missing the exact Unity executable or required platform module is a blocker.

## 4. Phase B — establish the tracked file layout

Create or port this layout:

```text
.githooks/
  post-commit
  post-checkout       # when needed to preserve Git LFS/legacy behavior
  post-merge          # when needed to preserve Git LFS/legacy behavior
  pre-push            # when needed to preserve Git LFS/legacy behavior
  run-legacy-hook

Tools/ItchBuildAutomation/
  AI_AGENT_SETUP_GUIDE.md
  README.md
  common.sh
  config.example.env
  doctor.sh
  install.sh
  request-build.sh
  uninstall.sh
  worker.sh

<unity-root>/Assets/Editor/ItchBuildAutomation.cs  # only if no usable method exists
```

Every shell script and hook must begin with `#!/usr/bin/env bash`, support macOS Bash 3.2, quote paths, and be executable in both the filesystem and Git index (`100755`).

Add precisely this ignored local configuration path:

```gitignore
Tools/ItchBuildAutomation/config.local.env
```

Do not ignore runtime state because it belongs under the Git common directory, not the repository worktree.

## 5. Phase C — define configuration

Commit `config.example.env`; create `config.local.env` only during installation and never track it.

Use these keys:

```bash
ITCH_TARGET="username/game-slug"
ITCH_CHANNEL="development-channel"
ITCH_UPLOAD_ENABLED="0"

UNITY_EXECUTABLE=""
UNITY_PROJECT_RELATIVE_PATH="."
BUILD_TARGET="StandaloneOSX"
BUILD_PRODUCT_NAME="GameName"
UNITY_BUILD_METHOD="ItchBuildAutomation.BuildFromCommandLine"

WORKTREE_PATH=""
```

Rules:

- Empty `UNITY_EXECUTABLE` means derive the exact standard path from the queued commit's `ProjectVersion.txt`.
- Empty `WORKTREE_PATH` means `<parent>/<repository-name>-itch-build-worktree`.
- Reject absolute or `..`-containing project-relative paths.
- Reject a placeholder itch target when upload is enabled.
- `--build-only` must override uploads for one request without editing local configuration.
- Do not put `BUTLER_API_KEY` in either config file. Prefer `butler login`.

## 6. Phase D — implement runtime state and atomic queueing

Resolve state from Git's common directory:

```bash
GIT_COMMON_DIR="$(git rev-parse --path-format=absolute --git-common-dir)"
STATE_DIR="$GIT_COMMON_DIR/itch-build-automation"
```

Use:

```text
pending-commit
active-commit
last-successful-commit
last-failed-commit
worker.pid
worker.lock/
logs/
builds/
```

Enqueue format is one tab-separated line:

```text
<40-character-sha><TAB><0-or-1-build-only-flag>
```

Write to a same-directory temporary file and atomically `mv` it to `pending-commit`. Each request replaces the unstarted pending request, producing a coalescing single-item queue.

The worker must claim a request with an atomic `mv` from `pending-commit` to a process-specific active request. It must never separately read and then delete `pending-commit`, because that loses requests in a race.

## 7. Phase E — handle Git hook environment correctly

This is a mandatory regression guard.

Git hooks export repository-local variables such as:

```text
GIT_DIR
GIT_INDEX_FILE
GIT_COMMON_DIR
GIT_PREFIX
```

If a detached worker inherits `GIT_INDEX_FILE=.git/index`, a linked-worktree command fails with:

```text
fatal: .git/index: index file open failed: Not a directory
```

Implement a shared cleanup function and call it both immediately before `nohup` and again at worker startup:

```bash
clear_git_local_environment() {
    local variable_name
    local automation_git_common_dir="$GIT_COMMON_DIR"

    while IFS= read -r variable_name; do
        if [[ -n "$variable_name" ]]; then
            unset "$variable_name"
        fi
    done < <(git -C "$REPO_ROOT" rev-parse --local-env-vars)

    # Restore the automation's canonical internal value as a non-exported
    # shell variable. Without this, `set -u` later fails with
    # "GIT_COMMON_DIR: unbound variable".
    GIT_COMMON_DIR="$automation_git_common_dir"
}
```

Do not omit the restore. The internal automation variable and Git's exported variable have the same name.

Launch with:

```bash
clear_git_local_environment
nohup "$AUTOMATION_DIR/worker.sh" \
  </dev/null >>"$LOG_DIR/launcher.log" 2>&1 &
```

## 8. Phase F — implement the tracked hooks

`post-commit` must:

1. Invoke any pre-existing `.git/hooks/post-commit` behavior through a non-recursive dispatcher.
2. Read the complete message with `git log -1 --pretty=%B`.
3. Match the literal, case-sensitive marker `[build]` anywhere in subject or body.
4. Resolve `git rev-parse HEAD` to a full SHA.
5. Call `request-build.sh --commit <sha>`.
6. Return immediately without waiting for Unity.
7. Log enqueue/launch failure without undoing or altering the existing commit.
8. Preserve the exit status of an unrelated legacy hook, not the automation launch.

When `.gitattributes` uses Git LFS, tracked `post-checkout`, `post-merge`, `post-commit`, and `pre-push` wrappers must call the corresponding existing `.git/hooks/<name>` directly or fall back to `git lfs <name>`. Setting `core.hooksPath=.githooks` otherwise silently bypasses Git LFS hooks.

## 9. Phase G — implement lock and lifecycle behavior

Use `mkdir "$LOCK_DIR"` as the lock; do not depend on GNU `flock`.

The worker algorithm is:

1. Clear Git-local environment.
2. Attempt `mkdir worker.lock`.
3. If it succeeds, atomically write the current PID.
4. If it fails, read the PID and use both `kill -0` and process command inspection to verify it belongs to this `worker.sh`.
5. If verified alive, exit because another worker owns the queue.
6. If stale, remove only the expected empty lock directory and retry once.
7. Install traps for normal exit and common signals.
8. Loop over atomically claimed pending requests.
9. Record success or failure without deleting logs or builds.
10. Release PID and lock.
11. Check once more for a pending request after release; `exec` a new worker if one raced with shutdown.
12. Exit. Do not poll permanently.

The post-release recheck is required. Without it, a launcher can see the old worker, exit, and leave a request stranded during shutdown.

## 10. Phase H — create and guard the persistent worktree

Default location:

```text
<parent-of-repository>/<repository-name>-itch-build-worktree
```

Create once:

```bash
git -C "$REPO_ROOT" worktree add --detach "$WORKTREE_PATH" HEAD
```

Before every destructive worktree command, verify all of these:

1. The configured expected path and `pwd -P` are identical.
2. `git -C "$WORKTREE_PATH" rev-parse --is-inside-work-tree` returns `true`.
3. The exact canonical path appears in `git worktree list --porcelain`.
4. The worktree's `--git-common-dir` equals the main repository's common directory.

Only then run:

```bash
git -C "$WORKTREE_PATH" reset --hard
git -C "$WORKTREE_PATH" clean -fd
git -C "$WORKTREE_PATH" checkout --detach --force "$COMMIT_SHA"
```

Then, when applicable:

```bash
git -C "$WORKTREE_PATH" lfs pull
git -C "$WORKTREE_PATH" submodule sync --recursive
git -C "$WORKTREE_PATH" submodule update --init --recursive
```

Never clean with `-x`. Confirm through a test marker that `Library` survives reset/clean/checkout.

## 11. Phase I — define or validate the Unity entry point

If the project already has a reliable method, use it. Otherwise implement an Editor-only public static method with this contract:

1. Parse `-buildOutput` and optionally `-buildTargetName` from `Environment.GetCommandLineArgs()`.
2. Require an absolute or safely resolved output path.
3. Read enabled scenes from `EditorBuildSettings.scenes`.
4. Fail if there are no enabled scenes.
5. Map the configured target to `BuildTarget`.
6. Call `BuildPipeline.BuildPlayer`.
7. Inspect `BuildReport.summary.result`.
8. Log result, duration, total size, warnings, and errors.
9. Call `EditorApplication.Exit(0)` only after a successful report.
10. Call `EditorApplication.Exit(nonzero)` on any exception or failed report.
11. Restore any temporary Editor setting in a `finally` block.
12. Do not change checked-in Player Settings merely to build.

When the method explicitly exits, do not also pass `-quit` unless the exact Unity version and method have been tested together.

## 12. Phase J — invoke and verify Unity

After checking that no Unity process is using the **build worktree project path**, invoke:

```bash
"$UNITY_EXECUTABLE" \
  -batchmode \
  -nographics \
  -projectPath "$UNITY_PROJECT_PATH_IN_WORKTREE" \
  -buildTarget "$BUILD_TARGET" \
  -executeMethod "$UNITY_BUILD_METHOD" \
  -buildOutput "$BUILD_OUTPUT" \
  -buildTargetName "$BUILD_TARGET" \
  -logFile "$UNITY_LOG_FILE"
```

Capture the exit code. A zero exit code is necessary but not sufficient: also require the expected output to exist and, for a directory, contain at least one file.

Recommended output conventions:

| Target | Build output | Butler upload path |
| --- | --- | --- |
| `WebGL` | `builds/<short-sha>/<product>/` | that directory |
| `StandaloneOSX` | `builds/<short-sha>/<product>.app` | containing commit build directory |
| `StandaloneWindows64` | `builds/<short-sha>/<product>.exe` | containing commit build directory |
| `StandaloneLinux64` | `builds/<short-sha>/<product>` | containing commit build directory |

Write a provenance marker containing the full SHA only after verified build success. Require the marker to match before upload so stale output can never be pushed.

## 13. Phase K — gate Butler upload

Upload only when all conditions are true:

1. The request is not `--build-only`.
2. `ITCH_UPLOAD_ENABLED="1"`.
3. Unity exited zero.
4. The expected output exists and is non-empty.
5. The provenance marker matches the active full SHA.
6. `ITCH_TARGET` matches `username/game-slug` and is not the placeholder.
7. `ITCH_CHANNEL` is non-empty and valid.
8. Butler exists.

Use:

```bash
butler push "$UPLOAD_PATH" \
  "$ITCH_TARGET:$ITCH_CHANNEL" \
  --userversion "$SHORT_SHA"
```

Capture and log Butler's exit code. Preserve the successful local build if upload fails. Never print environment variables or credentials.

After a successful push, verify read-only state:

```bash
butler status "$ITCH_TARGET:$ITCH_CHANNEL" --context-timeout=15
```

Before the first push, `butler status <target>` may report no channel. That is normal; the first successful push creates the channel.

## 14. Phase L — installer, doctor, and uninstaller behavior

### Installer

`install.sh` must:

1. Locate repository and selected Unity root.
2. Copy example config only if local config is absent.
3. Refuse to overwrite an unrelated existing `core.hooksPath`.
4. `chmod +x` scripts and hooks.
5. Set `git config core.hooksPath .githooks`.
6. Create runtime directories under the Git common directory.
7. Create or validate the persistent detached worktree.
8. Run Doctor.
9. Print remaining Butler and upload steps.

Remember: `core.hooksPath` is local Git configuration. Every clone/build machine must run `install.sh`; committing `.githooks` alone does not activate them.

### Doctor

`doctor.sh` must report and return nonzero for required build blockers:

- repository/common directory;
- project root and exact version;
- exact Unity executable and platform module;
- build method and enabled scenes;
- executable script modes;
- active hooks path;
- valid registered worktree;
- worker/lock state;
- available disk space;
- Git LFS when required;
- Butler version/authentication indication;
- local config validity;
- upload enablement and target/channel validity;
- ignored local config.

Missing upload configuration is a warning while build-only operation remains valid.

### Uninstaller

Default uninstall must stop only a verified worker and disable only this hook integration. Preserve worktree, config, logs, and builds unless explicit flags request their removal. Never force-remove a dirty worktree.

## 15. Phase M — deterministic installation and validation order

Follow this order exactly. Do not skip ahead.

### Gate 1: static implementation

```bash
chmod +x .githooks/* Tools/ItchBuildAutomation/*.sh
bash -n .githooks/* Tools/ItchBuildAutomation/*.sh
git diff --check
command -v shellcheck >/dev/null && \
  shellcheck .githooks/* Tools/ItchBuildAutomation/*.sh
```

Stop on syntax errors. If ShellCheck is unavailable, report that honestly.

### Gate 2: commit the implementation without uploading

Keep `ITCH_UPLOAD_ENABLED="0"`. Commit the automation and any new Unity build entry point **without** `[build]`. The exact commit must contain every file needed by the build worktree.

### Gate 3: install locally

```bash
Tools/ItchBuildAutomation/install.sh
Tools/ItchBuildAutomation/doctor.sh
```

Require Doctor to show zero build-blocking errors.

### Gate 4: hook parser test without project history pollution

Use a temporary Git repository and a fake request script to prove:

- a normal commit creates no request;
- `[build]` in the subject queues the exact SHA;
- `[build]` in the body queues the exact SHA;
- the hook returns quickly.

Do not create throwaway commits in the real project solely for parser testing.

### Gate 5: hook-environment regression test

First demonstrate the original linked-worktree failure is reproducible:

```bash
bash -c '
  source Tools/ItchBuildAutomation/common.sh
  GIT_INDEX_FILE=.git/index \
    git -C "$WORKTREE_PATH" status --short
'
```

It should fail with `.git/index ... Not a directory`.

Then require the sanitized path to pass:

```bash
GIT_INDEX_FILE=.git/index GIT_PREFIX=simulated-hook/ bash -c '
  set -e
  source Tools/ItchBuildAutomation/common.sh
  clear_git_local_environment
  git -C "$WORKTREE_PATH" status --short --branch
  validate_worktree
'
```

Also run the worker with an empty queue under that simulated environment and verify it removes its PID and lock before exit.

### Gate 6: queue and stale-lock tests

Verify:

- two enqueues leave only the newest SHA/flag in `pending-commit`;
- an empty stale lock with a nonexistent PID is safely recovered;
- a verified live worker PID is not removed;
- two launch attempts cannot produce two active workers;
- shutdown does not strand a request.

### Gate 7: build-only test

```bash
Tools/ItchBuildAutomation/request-build.sh --commit HEAD --build-only
```

Wait for terminal status. Require:

- worktree HEAD equals the requested full SHA and is detached;
- main worktree branch and HEAD are unchanged;
- Unity exit code is zero;
- expected output exists and is non-empty;
- Butler was skipped;
- final status is `SUCCESS`;
- PID/lock/active files are absent afterward;
- `Library` still exists in the build worktree.

The first build may take many minutes. An AI execution environment may kill a detached `nohup` descendant when its command cell returns; if so, test the worker in a persistent foreground terminal session. Do not misdiagnose that harness behavior as a production `nohup` failure.

### Gate 8: configure and validate Butler

The user must create the itch.io project page. Then:

```bash
butler login
```

Set target and channel in the ignored local config, keep uploads disabled, and run Doctor again.

### Gate 9: explicit upload test

Obtain explicit permission before external upload. Then set `ITCH_UPLOAD_ENABLED="1"` and run:

```bash
Tools/ItchBuildAutomation/request-build.sh --commit HEAD
```

Require Unity zero, Butler zero, local final `SUCCESS`, and a matching `butler status` version.

### Gate 10: automatic trigger test

Use the next meaningful commit:

```bash
git commit -m "Meaningful change [build]"
```

Confirm the launcher log records that exact commit within seconds, then monitor through Unity and Butler. A successful manual request does not prove the hook path; this gate specifically validates inherited hook environment and detachment.

## 16. Monitoring commands

```bash
STATE="$(git rev-parse --path-format=absolute --git-common-dir)/itch-build-automation"

tail -f "$STATE/logs/launcher.log"
tail -f "$STATE/logs/latest.log"
ls -t "$STATE"/logs/*-unity.log | head -n 1

for name in pending-commit active-commit worker.pid \
            last-successful-commit last-failed-commit; do
  test -f "$STATE/$name" && printf '%s: %s\n' \
    "$name" "$(sed -n '1p' "$STATE/$name")"
done

git worktree list --porcelain
bash -c '
  source Tools/ItchBuildAutomation/common.sh
  ps ax -o pid=,ppid=,etime=,command= | \
    awk -v worktree="$WORKTREE_PATH" '\''
      index($0,"ItchBuildAutomation/worker.sh") ||
      index($0,worktree) ||
      index($0,"butler push")
    '\''
'
```

## 17. Failure diagnosis table

| Symptom | Cause | Required correction |
| --- | --- | --- |
| No launcher entry after `[build]` commit | Hook path inactive, hook not executable, or marker case mismatch | Run Doctor; verify `core.hooksPath=.githooks`, mode `100755`, and literal lowercase `[build]` |
| Hook logs job immediately but Unity never starts; `.git/index` is “Not a directory” | Git hook's `GIT_INDEX_FILE` leaked into linked worktree commands | Clear every name from `git rev-parse --local-env-vars` before launch and at worker start |
| `GIT_COMMON_DIR: unbound variable` after environment cleanup | Cleanup also removed the automation's internal variable | Save and restore canonical internal `GIT_COMMON_DIR` as shown in Phase E |
| Worktree points at wrong SHA | Branch/tip was queued instead of resolved commit | Resolve `${commitish}^{commit}` and store the resulting full SHA |
| Main Editor/project changes during build | Worker used main project path | Fail immediately; only use the registered sibling worktree path |
| `Library` disappears every build | `git clean -fdx` or worktree recreation | Use persistent worktree and only `git clean -fd` |
| LFS pointer warning | Repository contains content that should have been an LFS pointer, or LFS hook/object is unavailable | Verify `git-lfs`, run `git lfs pull`, and repair malformed history separately; do not hide the warning |
| Unity exits zero but output is absent | Entry point returned before a verified `BuildReport`, or wrong output convention | Require report result and worker-side output/provenance checks |
| Unity fails but Butler starts | Upload gating is incorrectly ordered | Return immediately on Unity/report/output failure before any Butler command |
| Butler says no channel before first upload | Target exists but has never received that channel | Normal; first successful `butler push` creates it |
| Old failed commit never retries | Queue is event-driven, not polling | Manually request that SHA or create a new meaningful `[build]` commit |
| Uncommitted changes missing from build | Pipeline intentionally builds exact committed SHA | Commit the desired changes; never make worker copy the dirty main tree |

## 18. Acceptance checklist

The agent may declare the pipeline complete only when all applicable items pass:

- [ ] Unity root and exact editor version detected and reported.
- [ ] Existing build method reused or a verified Editor-only method added.
- [ ] Target and output conventions are explicit configuration.
- [ ] Automation files are tracked; local config is ignored.
- [ ] Existing hooks, especially Git LFS, are preserved.
- [ ] Hook-local Git environment is cleared with internal `GIT_COMMON_DIR` restored.
- [ ] `[build]` queues the exact SHA asynchronously.
- [ ] Normal commits do not queue.
- [ ] Queue coalesces to the newest unstarted request.
- [ ] Only one worker can own the lock.
- [ ] Persistent registered detached worktree is used.
- [ ] Main working tree is not reset, cleaned, switched, or opened by background Unity.
- [ ] Worktree `Library` survives.
- [ ] Real build-only Unity run succeeded.
- [ ] Failed build cannot invoke Butler.
- [ ] Upload remains opt-in and credentials remain external.
- [ ] If tested, Butler push and subsequent channel status both succeeded.
- [ ] Logs identify exact SHA, paths, versions, target, exit codes, and final status.
- [ ] PID/lock/active state cleans up after success and failure.
- [ ] Installer, Doctor, manual request, troubleshooting, and uninstall are documented.

## 19. Required final report from the agent

Return these sections:

1. **Detected setup:** repository root, Unity root, exact version/executable, target, build method, enabled scenes, Butler/LFS status.
2. **Files changed:** one concise explanation per file or file group.
3. **Local installation:** hooks path, state root, worktree path, upload enablement.
4. **Validation:** every command/test run, exact SHA tested, Unity/Butler exit codes, output path/size, and any skipped test.
5. **Manual steps:** target/channel, `butler login`, enabling upload, committing automation, per-clone installation.
6. **Usage:** Doctor, build-only request, upload request, `[build]` commit, log/status commands.
7. **Honest limitations:** do not call build operational without a real Unity success; do not call upload operational without a real Butler success and status confirmation.

## 20. Primary references

- [Unity command-line arguments](https://docs.unity3d.com/Manual/EditorCommandLineArguments.html)
- [Git worktree](https://git-scm.com/docs/git-worktree)
- [Git hooks](https://git-scm.com/docs/githooks)
- [Butler authentication](https://itch.io/docs/butler/login.html)
- [Butler push](https://itch.io/docs/butler/pushing.html)
