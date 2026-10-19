#!/usr/bin/env bash
# Construit l'installeur autonome CastleStoryPatch (exécutable unique, runtime .NET inclus) pour Windows et Linux.
# Sortie : patch/dist/CastleStoryPatch-<version>/{win-x64,linux-x64}/ + LISEZMOI.txt (+ .zip si `zip` est présent).
# Usage personnel : ne pas redistribuer.
set -euo pipefail
. "$(dirname "$0")/env.sh"
"$HERE/build.sh" --no-install
VERSION=$(sed -n 's/.*const string Version = "\([^"]*\)".*/\1/p' "$HERE/CastleStoryPatch/Program.cs")
OUT="$HERE/dist/CastleStoryPatch-$VERSION"
rm -rf "$OUT"
for rid in win-x64 linux-x64; do
  dotnet publish "$(native "$HERE/CastleStoryPatch")" -c Release -r "$rid" --self-contained true -nologo -v q \
    -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true \
    -p:DebugType=none -o "$(native "$OUT/$rid")"
done
cp "$HERE/CastleStoryPatch/LISEZMOI.txt" "$OUT/"
if command -v zip >/dev/null 2>&1; then (cd "$HERE/dist" && rm -f "CastleStoryPatch-$VERSION.zip" && zip -qr "CastleStoryPatch-$VERSION.zip" "CastleStoryPatch-$VERSION"); fi
find "$OUT" -name "*.pdb" -delete
ls -la "$OUT"/*
