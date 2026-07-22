#!/usr/bin/env bash

set -u
set -o pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd -P)"
# shellcheck source=common.sh
source "$SCRIPT_DIR/common.sh"

errors=0
warnings=0

ok() {
    printf 'OK: %s\n' "$*"
}

warn() {
    printf 'WARNING: %s\n' "$*" >&2
    warnings=$((warnings + 1))
}

fail() {
    printf 'ERROR: %s\n' "$*" >&2
    errors=$((errors + 1))
}

printf 'Repository: %s\n' "$REPO_ROOT"
printf 'Git common directory: %s\n' "$GIT_COMMON_DIR"
printf 'State directory: %s\n' "$STATE_DIR"

if validate_build_config; then
    ok 'local build configuration syntax is valid'
else
    fail 'local build configuration is invalid'
fi

project_path="$(project_path_from_root "$REPO_ROOT")"
printf 'Unity project: %s\n' "$project_path"
if [[ -d "$project_path/Assets" && -d "$project_path/Packages" && -f "$project_path/ProjectSettings/ProjectVersion.txt" ]]; then
    ok 'Unity project contains Assets, Packages, and ProjectSettings'
else
    fail 'Unity project root is missing required directories or ProjectVersion.txt'
fi

unity_version="$(unity_version_from_project "$project_path" 2>/dev/null || true)"
if [[ -n "$unity_version" ]]; then
    printf 'Unity version: %s\n' "$unity_version"
else
    fail 'could not parse m_EditorVersion'
fi

unity_executable="$(unity_executable_for_project "$project_path" 2>/dev/null || true)"
printf 'Unity executable: %s\n' "${unity_executable:-unresolved}"
if [[ -n "$unity_executable" && -n "$unity_version" ]] && \
   verify_unity_executable_version "$unity_executable" "$unity_version"; then
    ok 'exact required Unity executable is available'
else
    fail 'exact required Unity executable is unavailable or version could not be verified'
fi

printf 'Configured target: %s\n' "$BUILD_TARGET"
if [[ -n "$unity_executable" ]] && verify_unity_platform_module "$unity_executable" "$BUILD_TARGET"; then
    ok "Unity platform module for $BUILD_TARGET is installed"
else
    fail "Unity platform module for $BUILD_TARGET is unavailable"
fi
printf 'Configured execute method: %s\n' "$UNITY_BUILD_METHOD"

method_class="${UNITY_BUILD_METHOD%.*}"
method_name="${UNITY_BUILD_METHOD##*.}"
method_class="${method_class##*.}"
if grep -R -q --include='*.cs' "class[[:space:]]\+$method_class" "$project_path/Assets" 2>/dev/null && \
   grep -R -q --include='*.cs' "$method_name[[:space:]]*(" "$project_path/Assets" 2>/dev/null; then
    ok 'configured Unity build entry point is present in Editor code'
else
    fail 'configured Unity build entry point was not found in Assets'
fi

# The configured build method performs the authoritative runtime scene check.
# This serialized count is only a fast diagnostic fallback that avoids opening Unity.
enabled_scenes="$(awk '
    /^  m_Scenes:/ { in_scenes = 1; next }
    in_scenes && /^  m_configObjects:/ { in_scenes = 0 }
    in_scenes && /^[[:space:]]*- enabled: 1$/ { count++ }
    END { print count + 0 }
' "$project_path/ProjectSettings/EditorBuildSettings.asset" 2>/dev/null || printf '0')"
if [[ "$enabled_scenes" -gt 0 ]]; then
    ok "$enabled_scenes enabled build scene(s) detected; the build method rechecks through EditorBuildSettings"
else
    fail 'no enabled build scenes detected'
fi

if [[ -f "$CONFIG_FILE" ]]; then
    ok "local config exists: $CONFIG_FILE"
else
    warn "local config is absent; run install.sh to create $CONFIG_FILE"
fi

if git -C "$REPO_ROOT" check-ignore -q "Tools/ItchBuildAutomation/config.local.env"; then
    ok 'config.local.env is ignored by Git'
else
    fail 'config.local.env is not ignored by Git'
fi

hooks_path="$(git -C "$REPO_ROOT" config --get core.hooksPath || true)"
printf 'core.hooksPath: %s\n' "${hooks_path:-not set}"
if [[ "$hooks_path" == ".githooks" && -x "$REPO_ROOT/.githooks/post-commit" ]]; then
    ok 'tracked post-commit hook is active'
else
    fail 'tracked post-commit hook is not active; run install.sh'
fi

script_failure=0
for script in "$REPO_ROOT"/.githooks/* "$SCRIPT_DIR"/*.sh; do
    if [[ ! -x "$script" ]]; then
        fail "script is not executable: $script"
        script_failure=1
    fi
done
if [[ "$script_failure" -eq 0 ]]; then
    ok 'all automation scripts and hooks are executable'
fi

printf 'Build worktree: %s\n' "$WORKTREE_PATH"
if validate_worktree >/dev/null 2>&1; then
    ok 'persistent detached build worktree is registered and belongs to this repository'
else
    fail 'build worktree is absent or invalid; run install.sh'
fi

if [[ -d "$LOCK_DIR" ]]; then
    worker_pid="$(sed -n '1p' "$PID_FILE" 2>/dev/null || true)"
    if [[ -n "$worker_pid" ]] && worker_pid_is_ours "$worker_pid"; then
        ok "worker is active with PID $worker_pid"
    else
        warn "worker lock appears stale: $LOCK_DIR"
    fi
else
    ok 'no worker is currently active'
fi

available_kb="$(df -Pk "$GIT_COMMON_DIR" | awk 'NR == 2 { print $4 }')"
if [[ "$available_kb" =~ ^[0-9]+$ ]]; then
    available_gb=$((available_kb / 1024 / 1024))
    printf 'Available disk space: %s GiB\n' "$available_gb"
    if [[ "$available_kb" -lt 10485760 ]]; then
        warn 'less than 10 GiB is available for the Unity Library cache and builds'
    else
        ok 'disk space is above the 10 GiB warning threshold'
    fi
else
    warn 'could not determine available disk space'
fi

butler_executable="$(find_butler_executable 2>/dev/null || true)"
if [[ -n "$butler_executable" ]]; then
    butler_version="$("$butler_executable" -V 2>&1 || true)"
    ok "Butler is installed: $butler_version ($butler_executable)"
    butler_credentials="$HOME/Library/Application Support/itch/butler_creds"
    if [[ -s "$butler_credentials" || -n "${BUTLER_API_KEY:-}" ]]; then
        ok 'Butler authentication material appears to be available (not displayed)'
    else
        warn 'Butler authentication was not found; run `butler login` before enabling upload'
    fi
else
    if [[ "$ITCH_UPLOAD_ENABLED" == "1" ]]; then
        fail 'Butler is required because upload is enabled'
    else
        warn 'Butler is not installed; build-only operation remains available'
    fi
fi

if [[ "$ITCH_UPLOAD_ENABLED" == "1" ]]; then
    if validate_upload_config; then
        ok "uploads are enabled for $ITCH_TARGET:$ITCH_CHANNEL"
    else
        fail 'uploads are enabled but target/channel configuration is invalid'
    fi
else
    ok 'uploads are disabled; successful builds will remain local'
    if ! validate_upload_config >/dev/null 2>&1; then
        warn 'itch.io target is still a placeholder or invalid; this does not block build-only operation'
    fi
fi

printf '\nDoctor summary: %s error(s), %s warning(s).\n' "$errors" "$warnings"
[[ "$errors" -eq 0 ]]
