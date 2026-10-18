#!/usr/bin/env bash
# Compile les trois projets dans l'ordre (le Patcher référence la DLL compilée du Loader), puis installe.
set -euo pipefail
export DOTNET_ROOT="$HOME/.dotnet" PATH="$HOME/.dotnet:$PATH" DOTNET_CLI_TELEMETRY_OPTOUT=1
HERE="$(cd "$(dirname "$0")" && pwd)"
for p in BuildGoalFix.Loader BuildGoalFix.Patcher BuildGoalFix; do
  dotnet build "$HERE/$p" -c Release -nologo -v q --no-incremental
done
# Le build du Patcher juste après celui du Loader laisse parfois un dossier de sortie sans runtimeconfig/dépendances
# (libhostpolicy.so introuvable à l'exécution) : on le rebuild seul dans ce cas.
if [ ! -f "$HERE/BuildGoalFix.Patcher/bin/Release/net8.0/BuildGoalFix.Patcher.runtimeconfig.json" ]; then
  echo "Sortie du Patcher incomplète, rebuild"
  dotnet build "$HERE/BuildGoalFix.Patcher" -c Release -nologo -v q --no-incremental
fi
[ "${1:-}" = "--no-install" ] || "$HERE/install.sh"
