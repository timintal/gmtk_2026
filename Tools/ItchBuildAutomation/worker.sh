#!/usr/bin/env bash

set -u
set -o pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd -P)"
# shellcheck source=common.sh
source "$SCRIPT_DIR/common.sh"

# A worker launched by a Git hook inherits variables such as GIT_INDEX_FILE.
# Clear them before any command targets the detached linked worktree.
clear_git_local_environment

LOCK_OWNED="0"

launcher_log() {
    ensure_state_dirs
    printf '%s %s\n' "$(date '+%Y-%m-%dT%H:%M:%S%z')" "$*" >>"$LOG_DIR/launcher.log"
}

release_lock() {
    if [[ "$LOCK_OWNED" == "1" ]]; then
        rm -f "$PID_FILE" "$ACTIVE_FILE"
        rmdir "$LOCK_DIR" 2>/dev/null || true
        LOCK_OWNED="0"
    fi
}

cleanup_on_exit() {
    release_lock
}

acquire_lock() {
    local existing_pid=""

    ensure_state_dirs
    if mkdir "$LOCK_DIR" 2>/dev/null; then
        LOCK_OWNED="1"
        atomic_write_value "$PID_FILE" "$$"
        return 0
    fi

    if [[ -f "$PID_FILE" ]]; then
        existing_pid="$(sed -n '1p' "$PID_FILE")"
    fi
    if [[ -n "$existing_pid" ]] && worker_pid_is_ours "$existing_pid"; then
        launcher_log "Worker $existing_pid is already active; this launcher is exiting."
        return 2
    fi

    launcher_log "Recovering stale worker lock (recorded PID: ${existing_pid:-none})."
    [[ "$LOCK_DIR" == "$STATE_DIR/worker.lock" ]] || {
        launcher_log "Refusing to remove unexpected lock path: $LOCK_DIR"
        return 1
    }
    rm -f "$PID_FILE"
    rmdir "$LOCK_DIR" 2>/dev/null || {
        launcher_log "Stale lock directory is not empty; manual inspection required: $LOCK_DIR"
        return 1
    }
    mkdir "$LOCK_DIR" 2>/dev/null || return 2
    LOCK_OWNED="1"
    atomic_write_value "$PID_FILE" "$$"
}

dequeue_request() {
    local claimed="$STATE_DIR/active-request.$$"

    [[ -f "$PENDING_FILE" ]] || return 1
    mv "$PENDING_FILE" "$claimed" 2>/dev/null || return 1
    IFS=$'\t' read -r DEQUEUED_SHA DEQUEUED_BUILD_ONLY <"$claimed"
    rm -f "$claimed"

    if [[ ! "$DEQUEUED_SHA" =~ ^[0-9a-fA-F]{40}$ ]] || \
       [[ "$DEQUEUED_BUILD_ONLY" != "0" && "$DEQUEUED_BUILD_ONLY" != "1" ]]; then
        launcher_log "Discarded malformed pending request."
        return 2
    fi
    atomic_write_value "$ACTIVE_FILE" "$DEQUEUED_SHA"
    return 0
}

unity_process_uses_project() {
    local project_path="$1"

    ps ax -o pid=,command= | awk -v project="$project_path" '
        index($0, "Unity.app/Contents/MacOS/Unity") && index($0, project) { found = 1 }
        END { exit(found ? 0 : 1) }
    '
}

prepare_worktree_for_commit() {
    local commit_sha="$1"
    local expected
    local actual

    ensure_worktree || return 1
    expected="$(canonical_path "$WORKTREE_PATH")" || return 1
    actual="$(cd "$WORKTREE_PATH" && pwd -P)" || return 1
    [[ "$expected" == "$actual" ]] || {
        printf 'error: refusing worktree cleanup because resolved path differs.\n' >&2
        return 1
    }
    validate_worktree || return 1

    git -C "$REPO_ROOT" cat-file -e "${commit_sha}^{commit}" 2>/dev/null || {
        printf 'error: queued commit no longer exists: %s\n' "$commit_sha" >&2
        return 1
    }

    git -C "$WORKTREE_PATH" reset --hard || return 1
    git -C "$WORKTREE_PATH" clean -fd || return 1
    git -C "$WORKTREE_PATH" checkout --detach --force "$commit_sha" || return 1

    if [[ -f "$WORKTREE_PATH/.gitattributes" ]] && grep -q 'filter=lfs' "$WORKTREE_PATH/.gitattributes"; then
        command -v git-lfs >/dev/null 2>&1 || {
            printf 'error: Git LFS is required by this commit but git-lfs is unavailable.\n' >&2
            return 1
        }
        git -C "$WORKTREE_PATH" lfs pull || return 1
    fi

    if [[ -f "$WORKTREE_PATH/.gitmodules" ]]; then
        git -C "$WORKTREE_PATH" submodule sync --recursive || return 1
        git -C "$WORKTREE_PATH" submodule update --init --recursive || return 1
    fi
}

build_output_paths() {
    local commit_sha="$1"
    local short_sha="$2"

    JOB_BUILD_DIR="$BUILD_ROOT/$short_sha"
    case "$BUILD_TARGET" in
        StandaloneOSX)
            BUILD_OUTPUT="$JOB_BUILD_DIR/$BUILD_PRODUCT_NAME.app"
            UPLOAD_PATH="$JOB_BUILD_DIR"
            ;;
        StandaloneWindows|StandaloneWindows64)
            BUILD_OUTPUT="$JOB_BUILD_DIR/$BUILD_PRODUCT_NAME.exe"
            UPLOAD_PATH="$JOB_BUILD_DIR"
            ;;
        StandaloneLinux64)
            BUILD_OUTPUT="$JOB_BUILD_DIR/$BUILD_PRODUCT_NAME"
            UPLOAD_PATH="$JOB_BUILD_DIR"
            ;;
        *)
            BUILD_OUTPUT="$JOB_BUILD_DIR/$BUILD_PRODUCT_NAME"
            UPLOAD_PATH="$BUILD_OUTPUT"
            ;;
    esac
    BUILD_MARKER="$BUILD_ROOT/.commit-$short_sha"

    case "$JOB_BUILD_DIR" in
        "$BUILD_ROOT"/*) ;;
        *)
            printf 'error: refusing to use build directory outside state root: %s\n' "$JOB_BUILD_DIR" >&2
            return 1
            ;;
    esac

    # Every commit gets a fresh output directory. Other commits' builds remain available.
    rm -rf "$JOB_BUILD_DIR"
    rm -f "$BUILD_MARKER"
    mkdir -p "$JOB_BUILD_DIR"
    : "$commit_sha"
}

perform_job() {
    local commit_sha="$1"
    local build_only="$2"
    local short_sha="$3"
    local project_path
    local unity_version
    local unity_executable
    local unity_exit_code
    local butler_exit_code
    local butler_version

    prepare_worktree_for_commit "$commit_sha" || {
        printf 'error: failed to prepare detached build worktree.\n'
        return 1
    }

    project_path="$(project_path_from_root "$WORKTREE_PATH")"
    [[ -d "$project_path/Assets" && -d "$project_path/Packages" && -f "$project_path/ProjectSettings/ProjectVersion.txt" ]] || {
        printf 'error: Unity project is not valid at %s\n' "$project_path"
        return 1
    }

    unity_version="$(unity_version_from_project "$project_path")" || {
        printf 'error: could not read the queued commit Unity version.\n'
        return 1
    }
    unity_executable="$(unity_executable_for_project "$project_path")" || return 1
    verify_unity_executable_version "$unity_executable" "$unity_version" || return 1
    verify_unity_platform_module "$unity_executable" "$BUILD_TARGET" || return 1

    if unity_process_uses_project "$project_path"; then
        printf 'error: another Unity process is already using build worktree project %s\n' "$project_path"
        return 1
    fi

    build_output_paths "$commit_sha" "$short_sha" || return 1

    printf 'Unity version: %s\n' "$unity_version"
    printf 'Unity executable: %s\n' "$unity_executable"
    printf 'Build target: %s\n' "$BUILD_TARGET"
    printf 'Build method: %s\n' "$UNITY_BUILD_METHOD"
    printf 'Build output: %s\n' "$BUILD_OUTPUT"
    printf 'Starting Unity at %s\n' "$(date '+%Y-%m-%dT%H:%M:%S%z')"

    "$unity_executable" \
        -batchmode \
        -nographics \
        -projectPath "$project_path" \
        -buildTarget "$BUILD_TARGET" \
        -executeMethod "$UNITY_BUILD_METHOD" \
        -buildOutput "$BUILD_OUTPUT" \
        -buildTargetName "$BUILD_TARGET" \
        -logFile "$UNITY_LOG_FILE"
    unity_exit_code=$?
    printf 'Unity exit code: %s\n' "$unity_exit_code"

    if [[ "$unity_exit_code" -ne 0 ]]; then
        printf 'error: Unity build failed; Butler will not run.\n'
        return 1
    fi
    if [[ ! -e "$BUILD_OUTPUT" ]]; then
        printf 'error: Unity exited successfully but expected output is missing: %s\n' "$BUILD_OUTPUT"
        return 1
    fi
    if [[ -d "$BUILD_OUTPUT" ]] && [[ -z "$(find "$BUILD_OUTPUT" -mindepth 1 -print -quit 2>/dev/null)" ]]; then
        printf 'error: Unity exited successfully but expected output directory is empty.\n'
        return 1
    fi

    atomic_write_value "$BUILD_MARKER" "$commit_sha"

    if [[ "$build_only" == "1" ]]; then
        printf 'Upload skipped: this request was build-only.\n'
        return 0
    fi
    if [[ "$ITCH_UPLOAD_ENABLED" != "1" ]]; then
        printf 'Upload skipped: ITCH_UPLOAD_ENABLED is not 1.\n'
        return 0
    fi

    validate_upload_config || return 1
    command -v butler >/dev/null 2>&1 || {
        printf 'error: upload is enabled but Butler is not installed.\n'
        return 1
    }
    [[ -e "$UPLOAD_PATH" ]] || {
        printf 'error: refusing upload because expected upload path is missing: %s\n' "$UPLOAD_PATH"
        return 1
    }
    [[ "$(sed -n '1p' "$BUILD_MARKER" 2>/dev/null)" == "$commit_sha" ]] || {
        printf 'error: refusing upload because build provenance does not match this commit.\n'
        return 1
    }

    butler_version="$(butler -V 2>&1 || true)"
    printf 'Butler version: %s\n' "$butler_version"
    printf 'Uploading %s to %s:%s with version %s\n' \
        "$UPLOAD_PATH" "$ITCH_TARGET" "$ITCH_CHANNEL" "$short_sha"
    butler push "$UPLOAD_PATH" "$ITCH_TARGET:$ITCH_CHANNEL" --userversion "$short_sha"
    butler_exit_code=$?
    printf 'Butler exit code: %s\n' "$butler_exit_code"
    [[ "$butler_exit_code" -eq 0 ]] || return 1

    printf 'Verifying uploaded channel state with Butler.\n'
    butler status "$ITCH_TARGET:$ITCH_CHANNEL" --context-timeout=15
    butler_exit_code=$?
    printf 'Butler status exit code: %s\n' "$butler_exit_code"
    [[ "$butler_exit_code" -eq 0 ]] || return 1
}

run_job() {
    local commit_sha="$1"
    local build_only="$2"
    local short_sha
    local timestamp
    local subject
    local result

    short_sha="$(git -C "$REPO_ROOT" rev-parse --short=12 "$commit_sha")"
    timestamp="$(date '+%Y-%m-%d_%H%M%S')"
    subject="$(git -C "$REPO_ROOT" show -s --format=%s "$commit_sha" 2>/dev/null || printf '(unavailable)')"
    WORKER_LOG_FILE="$LOG_DIR/$timestamp-$short_sha-worker.log"
    UNITY_LOG_FILE="$LOG_DIR/$timestamp-$short_sha-unity.log"
    ln -sfn "$(basename "$WORKER_LOG_FILE")" "$LOG_DIR/latest.log"

    (
        printf 'Commit: %s\n' "$commit_sha"
        printf 'Short commit: %s\n' "$short_sha"
        printf 'Subject: %s\n' "$subject"
        printf 'Started: %s\n' "$(date '+%Y-%m-%dT%H:%M:%S%z')"
        printf 'Repository: %s\n' "$REPO_ROOT"
        printf 'Build worktree: %s\n' "$WORKTREE_PATH"
        printf 'Build-only request: %s\n' "$build_only"
        printf 'Unity log: %s\n' "$UNITY_LOG_FILE"
        perform_job "$commit_sha" "$build_only" "$short_sha"
        result=$?
        printf 'Finished: %s\n' "$(date '+%Y-%m-%dT%H:%M:%S%z')"
        if [[ "$result" -eq 0 ]]; then
            printf 'Final status: SUCCESS\n'
        else
            printf 'Final status: FAILED\n'
        fi
        exit "$result"
    ) >>"$WORKER_LOG_FILE" 2>&1
    result=$?

    if [[ "$result" -eq 0 ]]; then
        atomic_write_value "$STATE_DIR/last-successful-commit" "$commit_sha"
    else
        atomic_write_value "$STATE_DIR/last-failed-commit" "$commit_sha"
    fi
    launcher_log "Job $commit_sha finished with status $result. Log: $WORKER_LOG_FILE"
    return "$result"
}

validate_build_config || exit 1
acquire_lock
lock_result=$?
if [[ "$lock_result" -eq 2 ]]; then
    exit 0
elif [[ "$lock_result" -ne 0 ]]; then
    exit "$lock_result"
fi

trap cleanup_on_exit EXIT HUP INT TERM
launcher_log "Worker $$ acquired the build lock."

while true; do
    dequeue_request
    dequeue_result=$?
    if [[ "$dequeue_result" -eq 1 ]]; then
        break
    elif [[ "$dequeue_result" -ne 0 ]]; then
        continue
    fi

    run_job "$DEQUEUED_SHA" "$DEQUEUED_BUILD_ONLY" || true
    rm -f "$ACTIVE_FILE"
done

# Release before the final pending check. If a request raced with shutdown, this
# worker restarts itself; if it arrived later, its own launcher can acquire lock.
release_lock
trap - EXIT HUP INT TERM
if [[ -f "$PENDING_FILE" ]]; then
    exec "$SCRIPT_DIR/worker.sh"
fi
launcher_log "Worker $$ exited with no pending request."
