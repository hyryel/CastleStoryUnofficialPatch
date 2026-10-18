#!/usr/bin/env bash
# Decompile one type to /games/steamapps/common/Castle Story/re/decomp/<Type>.cs (cached).
# Usage: decomp.sh <Full.Type.Name> [dll basename, default Assembly-CSharp-firstpass]
set -euo pipefail
export DOTNET_ROOT="$HOME/.dotnet" PATH="$HOME/.dotnet:$HOME/.dotnet/tools:$PATH" DOTNET_CLI_TELEMETRY_OPTOUT=1
M="/games/steamapps/common/Castle Story/Castle Story_Data/Managed"
T="$1"; DLL="${2:-Assembly-CSharp-firstpass}"
OUT="/games/steamapps/common/Castle Story/re/decomp/${T//\//+}.cs"
mkdir -p "$(dirname "$OUT")"
[ -s "$OUT" ] || ilspycmd -t "$T" -r "$M" "$M/$DLL.dll" > "$OUT"
echo "$OUT"
