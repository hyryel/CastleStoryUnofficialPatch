#!/usr/bin/env bash
# Installe le correctif BuildGoalFix en remplaçant Assembly-CSharp-firstpass.dll (original sauvegardé en .orig).
# Relançable : après une mise à jour Steam, la nouvelle DLL d'origine devient la sauvegarde.
# Linux et Windows (Git Bash) : voir env.sh (CS_GAME pour forcer le dossier du jeu).
set -euo pipefail
. "$(dirname "$0")/env.sh"
ORIG="$DLL.orig"
STAMP="$M/BuildGoalFix.patched.sha256"

if [ ! -f "$ORIG" ]; then
  cp -p "$DLL" "$ORIG"; echo "Sauvegarde : $ORIG"
elif [ -f "$STAMP" ] && [ "$(sha "$DLL")" != "$(cat "$STAMP")" ] && [ "$(sha "$DLL")" != "$(sha "$ORIG")" ]; then
  cp -p "$DLL" "$ORIG"; echo "DLL du jeu mise à jour : nouvelle sauvegarde $ORIG"
fi

dotnet "$(native "$HERE/BuildGoalFix.Patcher/bin/Release/net8.0/BuildGoalFix.Patcher.dll")" "$(native "$ORIG")" "$(native "$DLL.tmp")" "$(native "$M")"
mv "$DLL.tmp" "$DLL"
sha "$DLL" > "$STAMP"
cp "$HERE/BuildGoalFix/bin/Release/net35/CastleStory.BuildGoalFix.dll" "$M/CastleStory.BuildGoalFix.dll.tmp" && mv "$M/CastleStory.BuildGoalFix.dll.tmp" "$M/CastleStory.BuildGoalFix.dll"  # remplacement atomique (jeu éventuellement en cours)
# v1.9.3 : compteur « Goals » du panneau de construction rafraîchi à chaque pose (Info/Lua, voir lua.sh).
"$HERE/lua.sh" install
echo "Correctif installé. Log : $M/BuildGoalFix.log"
