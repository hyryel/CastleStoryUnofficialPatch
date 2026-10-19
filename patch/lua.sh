#!/usr/bin/env bash
# Correctif Lua (v1.9.3) : le compteur « Goals » du panneau d'un ordre ne se rafraîchissait qu'au changement de sélection.
# GameMenu.lua : ajoute _m.onSetStatus (émis à chaque ajout/retrait de Blueprint, hk_OnSetStatus) au bloc task.goals.build
# et décommente la même ligne, désactivée d'origine, dans task.goals.all.
# Réécrit depuis GameMenu.lua.orig (sauvegarde ; renouvelée si une mise à jour Steam a remplacé le fichier).
# Usage : lua.sh install | restore
set -euo pipefail
. "$(dirname "$0")/env.sh"
LUA="$GAME/Info/Lua/LUI/Menus/GameMenu.lua"
ORIG="$LUA.orig"
STAMP="$LUA.patched.sha256"
case "${1:-install}" in
  install)
    if [ ! -f "$ORIG" ]; then
      cp -p "$LUA" "$ORIG"; echo "Sauvegarde : $ORIG"
    elif [ -f "$STAMP" ] && [ "$(sha "$LUA")" != "$(cat "$STAMP")" ] && [ "$(sha "$LUA")" != "$(sha "$ORIG")" ]; then
      cp -p "$LUA" "$ORIG"; echo "GameMenu.lua mis à jour : nouvelle sauvegarde $ORIG"
    fi
    awk '
      { sub(/\r$/, "") }
      /^--_m\.onSetStatus:AddListener\(\|\|h\.ev_refresh:Invoke\(\)\)$/ { print "_m.onSetStatus:AddListener(||h.ev_refresh:Invoke())\r"; all++; next }
      /^---task\.goals\.build$/ { inb = 1 }
      { print $0 "\r" }
      inb && /^_m\.onSetSelectedProject:AddListener\(\|\|h\.ev_refresh:Invoke\(\)\)$/ { print "_m.onSetStatus:AddListener(||h.ev_refresh:Invoke())\r"; build++; inb = 0 }
      END { if (all != 1 || build != 1) { print "GameMenu.lua : lignes attendues introuvables (" all + 0 "/" build + 0 ")" > "/dev/stderr"; exit 1 } }
    ' "$ORIG" > "$LUA.tmp"
    mv "$LUA.tmp" "$LUA"
    sha "$LUA" > "$STAMP"
    echo "GameMenu.lua : compteur Goals rafraîchi à chaque pose"
    ;;
  restore)
    if [ -f "$ORIG" ]; then cp -p "$ORIG" "$LUA"; rm -f "$STAMP"; echo "GameMenu.lua d'origine restauré"; fi
    ;;
  *) echo "usage : lua.sh install|restore" >&2; exit 2 ;;
esac
