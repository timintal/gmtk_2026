#!/usr/bin/env bash

set -u
set -o pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd -P)"
# shellcheck source=common.sh
source "$SCRIPT_DIR/common.sh"

commitish="HEAD"
build_only="0"

usage() {
    cat <<'EOF'
Usage: request-build.sh [--commit COMMIT] [--build-only]

Enqueues an exact commit and wakes the detached single build worker. A newer
request replaces an older request that has not started yet.
EOF
}

while [[ $# -gt 0 ]]; do
    case "$1" in
        --commit)
            [[ $# -ge 2 ]] || {
                printf 'error: --commit requires a revision.\n' >&2
                exit 2
            }
            commitish="$2"
            shift 2
            ;;
        --build-only)
            build_only="1"
            shift
            ;;
        -h|--help)
            usage
            exit 0
            ;;
        *)
            printf 'error: unknown option: %s\n' "$1" >&2
            usage >&2
            exit 2
            ;;
    esac
done

validate_build_config || exit 1
commit_sha="$(enqueue_request "$commitish" "$build_only")" || exit 1
launch_worker_detached || {
    printf 'error: commit %s was queued, but the worker could not be launched. See %s/launcher.log\n' \
        "$commit_sha" "$LOG_DIR" >&2
    exit 1
}

printf 'Queued %s%s. Worker output: %s/launcher.log\n' \
    "$commit_sha" "$([[ "$build_only" == "1" ]] && printf ' (build only)')" "$LOG_DIR"
