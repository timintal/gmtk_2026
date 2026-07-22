#!/usr/bin/env bash

set -u
set -o pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd -P)"
# shellcheck source=common.sh
source "$SCRIPT_DIR/common.sh"

remove_worktree="0"
remove_state="0"
remove_config="0"

usage() {
    cat <<'EOF'
Usage: uninstall.sh [--remove-worktree] [--remove-state] [--remove-config]

By default, disables this automation and stops its worker while preserving the
persistent worktree, builds, logs, and local configuration.
EOF
}

while [[ $# -gt 0 ]]; do
    case "$1" in
        --remove-worktree) remove_worktree="1" ;;
        --remove-state) remove_state="1" ;;
        --remove-config) remove_config="1" ;;
        -h|--help) usage; exit 0 ;;
        *) printf 'error: unknown option: %s\n' "$1" >&2; usage >&2; exit 2 ;;
    esac
    shift
done

if [[ -f "$PID_FILE" ]]; then
    worker_pid="$(sed -n '1p' "$PID_FILE")"
    if worker_pid_is_ours "$worker_pid"; then
        printf 'Stopping worker %s...\n' "$worker_pid"
        kill -TERM "$worker_pid"
        wait_count=0
        while kill -0 "$worker_pid" 2>/dev/null && [[ "$wait_count" -lt 5 ]]; do
            sleep 1
            wait_count=$((wait_count + 1))
        done
        if kill -0 "$worker_pid" 2>/dev/null; then
            printf 'error: worker %s did not stop; retained all integration and runtime state.\n' "$worker_pid" >&2
            exit 1
        fi
    else
        printf 'warning: not stopping unverified PID from %s.\n' "$PID_FILE" >&2
    fi
fi

current_hooks_path="$(git -C "$REPO_ROOT" config --get core.hooksPath || true)"
if [[ "$current_hooks_path" == ".githooks" ]]; then
    git -C "$REPO_ROOT" config --unset core.hooksPath
    printf 'Restored default .git/hooks behavior.\n'
elif [[ -n "$current_hooks_path" ]]; then
    printf 'warning: core.hooksPath is %s; leaving unrelated configuration unchanged.\n' "$current_hooks_path" >&2
fi

if [[ "$remove_worktree" == "1" ]]; then
    validate_worktree || exit 1
    git -C "$REPO_ROOT" worktree remove "$WORKTREE_PATH"
    printf 'Removed build worktree: %s\n' "$WORKTREE_PATH"
fi

if [[ "$remove_config" == "1" && -f "$CONFIG_FILE" ]]; then
    rm -f "$CONFIG_FILE"
    printf 'Removed local configuration: %s\n' "$CONFIG_FILE"
fi

if [[ "$remove_state" == "1" && -d "$STATE_DIR" ]]; then
    [[ "$STATE_DIR" == "$GIT_COMMON_DIR/itch-build-automation" ]] || {
        printf 'error: refusing to remove unexpected state path: %s\n' "$STATE_DIR" >&2
        exit 1
    }
    rm -rf "$STATE_DIR"
    printf 'Removed runtime state, logs, and builds: %s\n' "$STATE_DIR"
fi

printf 'Itch build automation is disabled. Tracked files remain in the repository.\n'
