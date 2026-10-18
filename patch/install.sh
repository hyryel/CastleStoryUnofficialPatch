#!/usr/bin/env bash
# Installe le correctif BuildGoalFix en remplaçant Assembly-CSharp-firstpass.dll (original sauvegardé en .orig).
# Relançable : après une mise à jour Steam, la nouvelle DLL d'origine devient la sauvegarde.
set -euo pipefail
export DOTNET_ROOT="$HOME/.dotnet" PATH="$HOME/.dotnet:$PATH" DOTNET_CLI_TELEMETRY_OPTOUT=1
HERE="$(cd "$(dirname "$0")" && pwd)"
M="/games/steamapps/common/Castle Story/Castle Story_Data/Managed"
DLL="$M/Assembly-CSharp-firstpass.dll"
ORIG="$DLL.orig"
STAMP="$M/BuildGoalFix.patched.sha256"
sha() { sha256sum "$1" | cut -d' ' -f1; }

if [ ! -f "$ORIG" ]; then
  cp -p "$DLL" "$ORIG"; echo "Sauvegarde : $ORIG"
elif [ -f "$STAMP" ] && [ "$(sha "$DLL")" != "$(cat "$STAMP")" ] && [ "$(sha "$DLL")" != "$(sha "$ORIG")" ]; then
  cp -p "$DLL" "$ORIG"; echo "DLL du jeu mise à jour : nouvelle sauvegarde $ORIG"
fi

dotnet "$HERE/BuildGoalFix.Patcher/bin/Release/net8.0/BuildGoalFix.Patcher.dll" "$ORIG" "$DLL.tmp" "$M"
mv "$DLL.tmp" "$DLL"
sha "$DLL" > "$STAMP"
cp "$HERE/BuildGoalFix/bin/Release/net35/CastleStory.BuildGoalFix.dll" "$M/CastleStory.BuildGoalFix.dll.tmp" && mv "$M/CastleStory.BuildGoalFix.dll.tmp" "$M/CastleStory.BuildGoalFix.dll"  # remplacement atomique (jeu éventuellement en cours)
echo "Correctif installé. Log : $M/BuildGoalFix.log"
