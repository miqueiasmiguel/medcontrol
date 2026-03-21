.PHONY: dev docker-up migrate api web mobile stop reset help

# Sobe docker, aplica migrations e inicia a API + web + mobile em paralelo
dev: docker-up migrate
	$(MAKE) -j3 api web mobile

# Inicia postgres e redis; aguarda health checks passarem
docker-up:
	docker compose up -d --wait

# Aplica migrations pendentes
migrate:
	cd apps/backend && dotnet ef database update \
	  --project src/MedControl.Infrastructure \
	  --startup-project src/MedControl.Api

# Inicia a API em modo Development (foreground)
api:
	cd apps/backend && dotnet run --project src/MedControl.Api

# Inicia o app Angular (foreground)
web:
	pnpm nx serve web

# Inicia o app Expo (foreground) — abra no Expo Go ou emulador
mobile:
	pnpm nx serve mobile

# Para e remove os containers (dados persistem nos volumes)
stop:
	docker compose down

# Para e remove containers E volumes (reset total do banco)
reset:
	docker compose down -v

help:
	@echo ""
	@echo "  make dev        Sobe docker + migrations + API + web + mobile (paralelo)"
	@echo "  make docker-up  Só sobe postgres e redis"
	@echo "  make migrate    Só aplica migrations"
	@echo "  make api        Só inicia a API"
	@echo "  make web        Só inicia o Angular"
	@echo "  make mobile     Só inicia o Expo (React Native)"
	@echo "  make stop       Para os containers"
	@echo "  make reset      Para containers e apaga volumes (reset do banco)"
	@echo ""
