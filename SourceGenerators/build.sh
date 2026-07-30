#!/usr/bin/env bash
# Builds the CommonGenerators Roslyn source generators and copies the DLL into the Unity project.
# Requires the .NET SDK (dotnet). Run from anywhere.
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT="$SCRIPT_DIR/CommonGenerators/CommonGenerators.csproj"
OUT_DIR="$SCRIPT_DIR/../Assets/Plugins/SourceGenerators"

dotnet build "$PROJECT" -c Release

mkdir -p "$OUT_DIR"
cp "$SCRIPT_DIR/CommonGenerators/bin/Release/netstandard2.0/CommonGenerators.dll" "$OUT_DIR/"

echo "Copied CommonGenerators.dll -> $OUT_DIR"
echo "In Unity, make sure the DLL asset has the 'RoslynAnalyzer' label (already set via .meta)."
