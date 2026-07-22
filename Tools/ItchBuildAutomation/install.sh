#!/usr/bin/env bash

set -u
set -o pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd -P)"
# shellcheck source=common.sh
source "$SCRIPT_DIR/common.sh"

if [[ ! -f "$CONFIG_FILE" ]]; then
    cp "$SCRIPT_DIR/config.example.env" "$CONFIG_FILE"
    printf 'Created local configuration: %s\n' "$CONFIG_FILE"
fi

# Reload values in case the config was just created or edited before this run.
# shellcheck disable=SC1090
source "$CONFIG_FILE"
if [[ -z "${WORKTREE_PATH:-}" ]]; then
    WORKTREE_PATH="$(dirname "$REPO_ROOT")/$(basename "$REPO_ROOT")-itch-build-worktree"
fi

main_project_path="$(project_path_from_root "$REPO_ROOT")"
if [[ ! -d "$main_project_path/Assets" || ! -d "$main_project_path/Packages" || \
      ! -f "$main_project_path/ProjectSettings/ProjectVersion.txt" ]]; then
    detected_version_file=""
    detected_count=0
    while IFS= read -r candidate; do
        candidate_root="${candidate%/ProjectSettings/ProjectVersion.txt}"
        if [[ -d "$candidate_root/Assets" && -d "$candidate_root/Packages" ]]; then
            detected_version_file="$candidate"
            detected_count=$((detected_count + 1))
        fi
    done < <(find "$REPO_ROOT" \
        -path "$REPO_ROOT/.git" -prune -o \
        -path '*/Library' -prune -o \
        -path '*/Temp' -prune -o \
        -path '*/Logs' -prune -o \
        -name ProjectVersion.txt -path '*/ProjectSettings/ProjectVersion.txt' -print)

    if [[ "$detected_count" -eq 1 ]]; then
        detected_root="${detected_version_file%/ProjectSettings/ProjectVersion.txt}"
        UNITY_PROJECT_RELATIVE_PATH="${detected_root#"$REPO_ROOT"/}"
        [[ "$detected_root" == "$REPO_ROOT" ]] && UNITY_PROJECT_RELATIVE_PATH="."
        main_project_path="$detected_root"
        printf 'Detected Unity project at repository-relative path: %s\n' "$UNITY_PROJECT_RELATIVE_PATH"
        printf '\n# Auto-detected by install.sh after the configured path was invalid.\nUNITY_PROJECT_RELATIVE_PATH=%q\n' \
            "$UNITY_PROJECT_RELATIVE_PATH" >>"$CONFIG_FILE"
    else
        printf 'error: configured Unity project root is invalid: %s\n' "$main_project_path" >&2
        printf 'Found %s valid Unity project roots; set UNITY_PROJECT_RELATIVE_PATH in %s.\n' \
            "$detected_count" "$CONFIG_FILE" >&2
        exit 1
    fi
fi
validate_build_config || exit 1

existing_hooks_path="$(git -C "$REPO_ROOT" config --get core.hooksPath || true)"
if [[ -n "$existing_hooks_path" && "$existing_hooks_path" != ".githooks" ]]; then
    printf 'error: core.hooksPath is already set to %s. It was not overwritten.\n' "$existing_hooks_path" >&2
    printf 'Integrate .githooks/post-commit into that hook directory, or remove the custom setting, then rerun.\n' >&2
    exit 1
fi

chmod +x "$REPO_ROOT"/.githooks/* "$SCRIPT_DIR"/*.sh
git -C "$REPO_ROOT" config core.hooksPath .githooks
ensure_state_dirs

printf 'Creating or validating persistent detached worktree: %s\n' "$WORKTREE_PATH"
ensure_worktree || exit 1

printf '\nRunning diagnostics...\n'
doctor_status=0
"$SCRIPT_DIR/doctor.sh" || doctor_status=$?

cat <<EOF

Installation is active. Remaining upload setup:
  1. Edit $CONFIG_FILE and set ITCH_TARGET and ITCH_CHANNEL.
  2. Run: butler login
  3. Keep ITCH_UPLOAD_ENABLED=0 for build-only operation; set it to 1 when ready.

Trigger with: git commit -m "Your change [build]"
Manual build: $SCRIPT_DIR/request-build.sh --build-only
Logs: $LOG_DIR
Builds: $BUILD_ROOT
EOF

if [[ "$doctor_status" -ne 0 ]]; then
    printf '\nDiagnostics reported missing required prerequisites (exit %s). Fix them before requesting a build.\n' "$doctor_status" >&2
    exit "$doctor_status"
fi
