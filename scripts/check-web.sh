#!/usr/bin/env bash
# Simula o CI web (web.yml) localmente antes de abrir PR.
# Uso: bash scripts/check-web.sh

set -euo pipefail

echo "==> lint"
pnpm nx lint web

echo "==> type-check"
pnpm nx run web:type-check

echo "==> test"
pnpm nx test web --coverage

echo "==> build (production)"
pnpm nx build web --configuration=production

echo ""
echo "✓ todos os checks passaram — seguro para abrir PR"
