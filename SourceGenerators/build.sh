#!/usr/bin/env bash
# Builds the MakeSerializable Roslyn source generator and copies the DLL into the Unity project.
# Requires the .NET SDK (dotnet). Run from anywhere.
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT="$SCRIPT_DIR/MakeSerializableGenerator/MakeSerializableGenerator.csproj"
OUT_DIR="$SCRIPT_DIR/../Assets/Plugins/SourceGenerators"

dotnet build "$PROJECT" -c Release

mkdir -p "$OUT_DIR"
cp "$SCRIPT_DIR/MakeSerializableGenerator/bin/Release/netstandard2.0/MakeSerializableGenerator.dll" "$OUT_DIR/"

echo "Copied MakeSerializableGenerator.dll -> $OUT_DIR"
echo "In Unity, make sure the DLL asset has the 'RoslynAnalyzer' label (already set via .meta)."
