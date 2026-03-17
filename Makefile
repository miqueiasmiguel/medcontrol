.PHONY: dev docker-up migrate api stop reset help

# Sobe docker, aplica migrations e inicia a API
dev: docker-up migrate api

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

# Para e remove os containers (dados persistem nos volumes)
stop:
	docker compose down

# Para e remove containers E volumes (reset total do banco)
reset:
	docker compose down -v

help:
	@echo ""
	@echo "  make dev        Sobe docker + migrations + API"
	@echo "  make docker-up  Só sobe postgres e redis"
	@echo "  make migrate    Só aplica migrations"
	@echo "  make api        Só inicia a API"
	@echo "  make stop       Para os containers"
	@echo "  make reset      Para containers e apaga volumes (reset do banco)"
	@echo ""
