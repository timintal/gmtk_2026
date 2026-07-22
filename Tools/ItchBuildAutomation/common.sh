#!/usr/bin/env bash

# Shared, macOS Bash 3.2-compatible helpers for the local itch.io build worker.

set -u
set -o pipefail

AUTOMATION_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd -P)"
REPO_ROOT="$(git -C "$AUTOMATION_DIR" rev-parse --show-toplevel 2>/dev/null)" || {
    printf 'error: Itch build automation must live inside a Git repository.\n' >&2
    exit 1
}
REPO_ROOT="$(cd "$REPO_ROOT" && pwd -P)"

git_common_dir_raw="$(git -C "$REPO_ROOT" rev-parse --git-common-dir)"
if [[ "$git_common_dir_raw" = /* ]]; then
    GIT_COMMON_DIR="$(cd "$git_common_dir_raw" && pwd -P)"
else
    GIT_COMMON_DIR="$(cd "$REPO_ROOT/$git_common_dir_raw" && pwd -P)"
fi

STATE_DIR="$GIT_COMMON_DIR/itch-build-automation"
LOG_DIR="$STATE_DIR/logs"
BUILD_ROOT="$STATE_DIR/builds"
PENDING_FILE="$STATE_DIR/pending-commit"
ACTIVE_FILE="$STATE_DIR/active-commit"
PID_FILE="$STATE_DIR/worker.pid"
LOCK_DIR="$STATE_DIR/worker.lock"
CONFIG_FILE="$AUTOMATION_DIR/config.local.env"

# Project defaults. A local, ignored config file may override these values.
ITCH_TARGET=""
ITCH_CHANNEL="html5-development"
ITCH_UPLOAD_ENABLED="0"
UNITY_EXECUTABLE=""
UNITY_PROJECT_RELATIVE_PATH="."
BUILD_TARGET="WebGL"
BUILD_PRODUCT_NAME="Proto"
UNITY_BUILD_METHOD="Game.Editor.WebBuild.PerformBuild"
WORKTREE_PATH=""

if [[ -f "$CONFIG_FILE" ]]; then
    # This is a user-owned local shell configuration file.
    # shellcheck disable=SC1090
    source "$CONFIG_FILE"
fi

if [[ -z "$WORKTREE_PATH" ]]; then
    repo_parent="$(dirname "$REPO_ROOT")"
    repo_name="$(basename "$REPO_ROOT")"
    WORKTREE_PATH="$repo_parent/${repo_name}-itch-build-worktree"
fi

ensure_state_dirs() {
    umask 077
    mkdir -p "$STATE_DIR" "$LOG_DIR" "$BUILD_ROOT"
}

clear_git_local_environment() {
    local variable_name
    local automation_git_common_dir="$GIT_COMMON_DIR"

    # Git exports repository-local variables to hooks (notably
    # GIT_INDEX_FILE=.git/index). They must not leak into commands targeting the
    # detached linked worktree, whose .git entry is a file rather than a folder.
    while IFS= read -r variable_name; do
        if [[ -n "$variable_name" ]]; then
            unset "$variable_name"
        fi
    done < <(git -C "$REPO_ROOT" rev-parse --local-env-vars)

    # GIT_COMMON_DIR is also the name of our already-canonicalized internal
    # path. Restore it as a non-exported shell variable after removing Git's
    # inherited environment value.
    GIT_COMMON_DIR="$automation_git_common_dir"
}

atomic_write_value() {
    local destination="$1"
    local value="$2"
    local temporary

    temporary="${destination}.tmp.$$.$RANDOM"
    umask 077
    printf '%s\n' "$value" >"$temporary"
    mv -f "$temporary" "$destination"
}

canonical_path() {
    local input="$1"
    local parent
    local name

    if [[ -d "$input" ]]; then
        (cd "$input" && pwd -P)
        return
    fi

    parent="$(dirname "$input")"
    name="$(basename "$input")"
    (cd "$parent" 2>/dev/null && printf '%s/%s\n' "$(pwd -P)" "$name")
}

project_path_from_root() {
    local root="$1"

    if [[ "$UNITY_PROJECT_RELATIVE_PATH" == "." ]]; then
        printf '%s\n' "$root"
    else
        printf '%s/%s\n' "$root" "$UNITY_PROJECT_RELATIVE_PATH"
    fi
}

validate_relative_project_path() {
    case "$UNITY_PROJECT_RELATIVE_PATH" in
        ""|/*|..|../*|*/..|*/../*)
            printf 'error: UNITY_PROJECT_RELATIVE_PATH must be a repository-relative path without .. components.\n' >&2
            return 1
            ;;
    esac
}

validate_build_config() {
    local failed=0

    validate_relative_project_path || failed=1

    if [[ "$WORKTREE_PATH" != /* ]]; then
        printf 'error: WORKTREE_PATH must be absolute when configured.\n' >&2
        failed=1
    fi
    if [[ -z "$BUILD_TARGET" ]]; then
        printf 'error: BUILD_TARGET cannot be empty.\n' >&2
        failed=1
    fi
    if [[ -z "$BUILD_PRODUCT_NAME" || "$BUILD_PRODUCT_NAME" == */* ]]; then
        printf 'error: BUILD_PRODUCT_NAME must be a non-empty file name without slashes.\n' >&2
        failed=1
    fi
    if [[ -z "$UNITY_BUILD_METHOD" ]]; then
        printf 'error: UNITY_BUILD_METHOD cannot be empty.\n' >&2
        failed=1
    fi
    if [[ "$ITCH_UPLOAD_ENABLED" != "0" && "$ITCH_UPLOAD_ENABLED" != "1" ]]; then
        printf 'error: ITCH_UPLOAD_ENABLED must be 0 or 1.\n' >&2
        failed=1
    fi

    return "$failed"
}

validate_upload_config() {
    if [[ "$ITCH_TARGET" == "username/game-slug" ]]; then
        printf 'error: ITCH_TARGET is still the example placeholder.\n' >&2
        return 1
    fi
    if [[ ! "$ITCH_TARGET" =~ ^[^/[:space:]]+/[^/[:space:]]+$ ]]; then
        printf 'error: ITCH_TARGET must look like username/game-slug.\n' >&2
        return 1
    fi
    if [[ ! "$ITCH_CHANNEL" =~ ^[A-Za-z0-9][A-Za-z0-9._-]*$ ]]; then
        printf 'error: ITCH_CHANNEL must be a non-empty Butler channel name.\n' >&2
        return 1
    fi
}

find_butler_executable() {
    local discovered=""
    local candidate

    discovered="$(command -v butler 2>/dev/null || true)"
    if [[ -n "$discovered" && -x "$discovered" ]]; then
        printf '%s\n' "$discovered"
        return 0
    fi

    # GUI applications and Git hooks commonly start with a minimal PATH that
    # excludes user-local and Homebrew binaries. Resolve standard macOS install
    # locations explicitly instead of depending on interactive shell startup.
    for candidate in \
        "$HOME/.local/bin/butler" \
        /opt/homebrew/bin/butler \
        /usr/local/bin/butler; do
        if [[ -x "$candidate" ]]; then
            printf '%s\n' "$candidate"
            return 0
        fi
    done

    return 1
}

unity_version_from_project() {
    local project_path="$1"
    local version_file="$project_path/ProjectSettings/ProjectVersion.txt"

    [[ -f "$version_file" ]] || return 1
    sed -n 's/^m_EditorVersion:[[:space:]]*//p' "$version_file" | head -n 1
}

unity_executable_for_project() {
    local project_path="$1"
    local version

    version="$(unity_version_from_project "$project_path")" || return 1
    if [[ -n "$UNITY_EXECUTABLE" ]]; then
        printf '%s\n' "$UNITY_EXECUTABLE"
    else
        printf '/Applications/Unity/Hub/Editor/%s/Unity.app/Contents/MacOS/Unity\n' "$version"
    fi
}

verify_unity_executable_version() {
    local executable="$1"
    local expected_version="$2"
    local info_plist
    local detected=""

    [[ -x "$executable" ]] || {
        printf 'error: Unity executable is missing or not executable: %s\n' "$executable" >&2
        return 1
    }

    case "$executable" in
        *"/$expected_version/Unity.app/Contents/MacOS/Unity")
            return 0
            ;;
    esac

    info_plist="$(dirname "$executable")/../Info.plist"
    if [[ -f "$info_plist" && -x /usr/libexec/PlistBuddy ]]; then
        detected="$(/usr/libexec/PlistBuddy -c 'Print :CFBundleShortVersionString' "$info_plist" 2>/dev/null || true)"
        if [[ -z "$detected" ]]; then
            detected="$(/usr/libexec/PlistBuddy -c 'Print :CFBundleVersion' "$info_plist" 2>/dev/null || true)"
        fi
    fi

    if [[ "$detected" != "$expected_version" ]]; then
        printf 'error: cannot verify that configured Unity executable is version %s (detected: %s).\n' \
            "$expected_version" "${detected:-unknown}" >&2
        return 1
    fi
}

unity_playback_engine_for_target() {
    case "$1" in
        WebGL) printf 'WebGLSupport\n' ;;
        StandaloneOSX) printf 'MacStandaloneSupport\n' ;;
        StandaloneWindows|StandaloneWindows64) printf 'WindowsStandaloneSupport\n' ;;
        StandaloneLinux64) printf 'LinuxStandaloneSupport\n' ;;
        *) return 1 ;;
    esac
}

verify_unity_platform_module() {
    local executable="$1"
    local target="$2"
    local editor_root
    local playback_engine

    playback_engine="$(unity_playback_engine_for_target "$target")" || {
        printf 'error: no Unity platform-module mapping is defined for target %s.\n' "$target" >&2
        return 1
    }
    editor_root="$(cd "$(dirname "$executable")/../../.." && pwd -P)"
    [[ -d "$editor_root/PlaybackEngines/$playback_engine" ]] || {
        printf 'error: Unity platform module is missing: %s/PlaybackEngines/%s\n' \
            "$editor_root" "$playback_engine" >&2
        return 1
    }
}

worktree_is_registered() {
    local expected

    expected="$(canonical_path "$WORKTREE_PATH")" || return 1
    git -C "$REPO_ROOT" worktree list --porcelain | awk -v expected="$expected" '
        /^worktree / {
            path = substr($0, 10)
            if (path == expected) {
                found = 1
            }
        }
        END { exit(found ? 0 : 1) }
    '
}

validate_worktree() {
    local expected
    local actual
    local worktree_common_raw
    local worktree_common

    [[ -d "$WORKTREE_PATH" ]] || {
        printf 'error: build worktree does not exist: %s\n' "$WORKTREE_PATH" >&2
        return 1
    }

    expected="$(canonical_path "$WORKTREE_PATH")" || return 1
    actual="$(cd "$WORKTREE_PATH" && pwd -P)" || return 1
    [[ "$expected" == "$actual" ]] || {
        printf 'error: worktree path mismatch: expected %s, resolved %s\n' "$expected" "$actual" >&2
        return 1
    }

    [[ "$(git -C "$WORKTREE_PATH" rev-parse --is-inside-work-tree 2>/dev/null)" == "true" ]] || {
        printf 'error: expected build path is not a Git worktree: %s\n' "$WORKTREE_PATH" >&2
        return 1
    }
    worktree_is_registered || {
        printf 'error: build worktree is not registered with this repository: %s\n' "$WORKTREE_PATH" >&2
        return 1
    }

    worktree_common_raw="$(git -C "$WORKTREE_PATH" rev-parse --git-common-dir)" || return 1
    if [[ "$worktree_common_raw" = /* ]]; then
        worktree_common="$(cd "$worktree_common_raw" && pwd -P)"
    else
        worktree_common="$(cd "$WORKTREE_PATH/$worktree_common_raw" && pwd -P)"
    fi
    [[ "$worktree_common" == "$GIT_COMMON_DIR" ]] || {
        printf 'error: build worktree belongs to a different Git common directory.\n' >&2
        return 1
    }
}

ensure_worktree() {
    local parent

    if [[ ! -e "$WORKTREE_PATH" ]]; then
        parent="$(dirname "$WORKTREE_PATH")"
        mkdir -p "$parent"
        git -C "$REPO_ROOT" worktree add --detach "$WORKTREE_PATH" HEAD || return 1
    fi
    validate_worktree
}

worker_pid_is_ours() {
    local pid="$1"
    local command_line

    [[ "$pid" =~ ^[0-9]+$ ]] || return 1
    kill -0 "$pid" 2>/dev/null || return 1
    command_line="$(ps -p "$pid" -o command= 2>/dev/null || true)"
    [[ "$command_line" == *"$AUTOMATION_DIR/worker.sh"* ]]
}

enqueue_request() {
    local commitish="$1"
    local build_only="$2"
    local commit_sha
    local temporary

    commit_sha="$(git -C "$REPO_ROOT" rev-parse --verify "${commitish}^{commit}" 2>/dev/null)" || {
        printf 'error: commit does not exist: %s\n' "$commitish" >&2
        return 1
    }
    ensure_state_dirs
    temporary="${PENDING_FILE}.tmp.$$.$RANDOM"
    umask 077
    printf '%s\t%s\n' "$commit_sha" "$build_only" >"$temporary"
    mv -f "$temporary" "$PENDING_FILE"
    printf '%s\n' "$commit_sha"
}

launch_worker_detached() {
    ensure_state_dirs
    clear_git_local_environment
    nohup "$AUTOMATION_DIR/worker.sh" </dev/null >>"$LOG_DIR/launcher.log" 2>&1 &
}
