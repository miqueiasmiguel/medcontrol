-include .env
export

.PHONY: dev docker-up migrate migrate-prd api web mobile stop reset help

# Sobe docker, aplica migrations e inicia a API + web em paralelo
# Mobile roda separado (make mobile) para o QR code do Expo aparecer corretamente
dev: docker-up migrate
	$(MAKE) -j2 api web

# Inicia postgres e redis; aguarda health checks passarem
docker-up:
	docker compose up -d --wait

# Aplica migrations pendentes
migrate:
	cd apps/backend && dotnet ef database update \
	  --project src/MedControl.Infrastructure \
	  --startup-project src/MedControl.Api

# Aplica migrations no banco de produção (Neon) — requer DATABASE_URL no .env ou no ambiente
migrate-prd:
	@if [ -z "$$DATABASE_URL_PRD" ]; then echo "ERROR: DATABASE_URL_PRD nao definida (defina no .env ou no ambiente)"; exit 1; fi
	cd apps/backend && dotnet ef database update \
	  --project src/MedControl.Infrastructure \
	  --startup-project src/MedControl.Api \
	  --connection "$$DATABASE_URL_PRD"

# Inicia a API em modo Development (foreground)
# Bind em 0.0.0.0 para que dispositivos físicos na rede local consigam conectar
api:
	cd apps/backend && dotnet run --project src/MedControl.Api --urls "http://0.0.0.0:5113"

# Inicia o app Angular (foreground)
# --host 0.0.0.0 permite acesso de dispositivos físicos na mesma rede (ex: magic link no celular)
web:
	pnpm nx serve web -- --host 0.0.0.0

# Inicia o app Expo diretamente (sem Nx) para o QR code aparecer no terminal
# --dev-client garante que o QR abra no EAS dev build, não no Expo Go
mobile:
	cd apps/mobile && npx expo start --clear --dev-client

# Para e remove os containers (dados persistem nos volumes)
stop:
	docker compose down

# Para e remove containers E volumes (reset total do banco)
reset:
	docker compose down -v

help:
	@echo ""
	@echo "  make dev        Sobe docker + migrations + API + web (paralelo)"
	@echo "  make docker-up  Só sobe postgres e redis"
	@echo "  make migrate    Só aplica migrations (local)"
	@echo "  make migrate-prd Aplica migrations no banco de produção (Neon) — requer DATABASE_URL"
	@echo "  make api        Só inicia a API"
	@echo "  make web        Só inicia o Angular"
	@echo "  make mobile     Só inicia o Expo (React Native) — requer EAS dev build instalado"
	@echo "  make stop       Para os containers"
	@echo "  make reset      Para containers e apaga volumes (reset do banco)"
	@echo ""
