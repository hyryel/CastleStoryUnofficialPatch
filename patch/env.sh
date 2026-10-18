# Environnement commun à build.sh / install.sh / restore.sh (sourcé). Linux et Windows (Git Bash).
# Dossier du jeu : $CS_GAME s'il est défini, sinon parent de re/ (chemin logique), sinon emplacements connus.
HERE="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
[ -d "$HOME/.dotnet" ] && export DOTNET_ROOT="$HOME/.dotnet" PATH="$HOME/.dotnet:$PATH"
export DOTNET_CLI_TELEMETRY_OPTOUT=1

find_game() {
  local c
  for c in "${CS_GAME:-}" "$HERE/../.." \
           "/games/steamapps/common/Castle Story" \
           "/e/SteamLibrary/steamapps/common/Castle Story"; do
    [ -n "$c" ] && [ -d "$c/Castle Story_Data/Managed" ] && { (cd "$c" && pwd); return; }
  done
  echo "Dossier du jeu introuvable : définir CS_GAME" >&2; return 1
}
GAME="$(find_game)"
M="$GAME/Castle Story_Data/Managed"
DLL="$M/Assembly-CSharp-firstpass.dll"

# Chemin passé à dotnet : natif Windows sous Git Bash, inchangé ailleurs.
native() { if command -v cygpath >/dev/null 2>&1; then cygpath -w "$1"; else printf '%s' "$1"; fi; }
sha() { sha256sum "$1" | cut -d' ' -f1; }
