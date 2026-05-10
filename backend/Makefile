.PHONY: build run clean test restore migrate migrate-add migrate-update migrate-remove

# Default target
all: build

# Restore dependencies
restore:
	dotnet restore

# Build the API project
build: restore
	dotnet build src/AbujaSocialMetaverse.API/AbujaSocialMetaverse.API.csproj --no-restore

# Run the API
run:
	dotnet run --project src/AbujaSocialMetaverse.API/AbujaSocialMetaverse.API.csproj --no-build

# Clean solution
clean:
	dotnet clean

# Run tests
test:
	dotnet test

# Database Migrations
# Add a new migration (usage: make migrate-add name=MigrationName)
INFRA_PROJ := src/Infrastructure/AbujaSocialMetaverse.Infrastructure.csproj
API_PROJ := src/AbujaSocialMetaverse.API/AbujaSocialMetaverse.API.csproj

migrate-add:
	@echo "Adding migration: $(name)"
	@set -a; source .env; set +a; \
	dotnet ef migrations add $(name) --project $(INFRA_PROJ) --startup-project $(API_PROJ) --output-dir Data/Migrations

# Update database to latest migration
migrate-update:
	@echo "Updating database..."
	@set -a; source .env; set +a; \
	dotnet ef database update --project $(INFRA_PROJ) --startup-project $(API_PROJ)

# Remove last migration (if not applied to database)
migrate-remove:
	@echo "Removing last migration..."
	@set -a; source .env; set +a; \
	dotnet ef migrations remove --project $(INFRA_PROJ) --startup-project $(API_PROJ)

# Add and update in one command (usage: make migrate name=MigrationName)
migrate: migrate-add migrate-update