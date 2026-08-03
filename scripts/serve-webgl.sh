#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
DOCS="$ROOT/docs"

if [[ ! -f "$DOCS/index.html" ]]; then
  echo "WebGL build not found at $DOCS/index.html"
  echo "Build first: Unity menu Build → WebGL for GitHub Pages"
  exit 1
fi

PORT="${1:-8080}"
echo "Local preview: http://127.0.0.1:${PORT}/"
echo "Stop with Ctrl+C"
cd "$DOCS"
exec python3 -m http.server "$PORT"
