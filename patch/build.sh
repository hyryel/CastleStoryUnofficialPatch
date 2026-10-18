#!/usr/bin/env bash
# Compile les trois projets dans l'ordre (le Patcher référence la DLL compilée du Loader), puis installe.
set -euo pipefail
. "$(dirname "$0")/env.sh"
for p in BuildGoalFix.Loader BuildGoalFix.Patcher BuildGoalFix; do
  dotnet build "$(native "$HERE/$p")" -c Release -nologo -v q --no-incremental "-p:GameManaged=$(native "$M")"
done
# Le build du Patcher juste après celui du Loader laisse parfois un dossier de sortie sans runtimeconfig/dépendances
# (libhostpolicy introuvable à l'exécution) : on le rebuild seul dans ce cas.
if [ ! -f "$HERE/BuildGoalFix.Patcher/bin/Release/net8.0/BuildGoalFix.Patcher.runtimeconfig.json" ]; then
  echo "Sortie du Patcher incomplète, rebuild"
  dotnet build "$(native "$HERE/BuildGoalFix.Patcher")" -c Release -nologo -v q --no-incremental "-p:GameManaged=$(native "$M")"
fi
[ "${1:-}" = "--no-install" ] || "$HERE/install.sh"
