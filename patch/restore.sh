#!/usr/bin/env bash
# Restaure Assembly-CSharp-firstpass.dll d'origine et retire le correctif BuildGoalFix.
set -euo pipefail
M="/games/steamapps/common/Castle Story/Castle Story_Data/Managed"
DLL="$M/Assembly-CSharp-firstpass.dll"
[ -f "$DLL.orig" ] || { echo "Pas de sauvegarde $DLL.orig" >&2; exit 1; }
cp -p "$DLL.orig" "$DLL"
rm -f "$M/CastleStory.BuildGoalFix.dll" "$M/BuildGoalFix.patched.sha256"
echo "DLL d'origine restaurée (sauvegarde conservée : $DLL.orig)"
