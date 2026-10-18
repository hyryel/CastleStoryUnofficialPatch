#!/usr/bin/env bash
# Restaure Assembly-CSharp-firstpass.dll d'origine et retire le correctif BuildGoalFix.
# Linux et Windows (Git Bash) : voir env.sh (CS_GAME pour forcer le dossier du jeu).
set -euo pipefail
. "$(dirname "$0")/env.sh"
[ -f "$DLL.orig" ] || { echo "Pas de sauvegarde $DLL.orig" >&2; exit 1; }
cp -p "$DLL.orig" "$DLL"
rm -f "$M/CastleStory.BuildGoalFix.dll" "$M/BuildGoalFix.patched.sha256"
echo "DLL d'origine restaurée (sauvegarde conservée : $DLL.orig)"
