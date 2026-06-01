.PHONY: migration-add migration-update migration-remove migration-list seed-demo-votes

# Создать новую миграцию
# Usage: make migration-add name=MigrationName
migration-add:
	dotnet ef migrations add $(name) -s ./src/EduVote.API -p ./src/EduVote.DAL.Postgresql/

# Применить миграции к БД
migration-update:
	dotnet ef database update -s ./src/EduVote.API -p ./src/EduVote.DAL.Postgresql/

# Удалить последнюю миграцию
migration-remove:
	dotnet ef migrations remove -s ./src/EduVote.API -p ./src/EduVote.DAL.Postgresql/

migration-list:
	dotnet ef migrations list -s  ./src/EduVote.API -p ./src/EduVote.DAL.Postgresql/

# Демо-голоса
# Перед запуском: скопируйте актуальную строку подключения Postgres из Aspire Dashboard
# в src/EduVote.API/appsettings.json -> ConnectionStrings -> eduvote-db
#
# В pgAdmin для голосования: статус Active, EndTime в будущем, без строк в VotingResults
# и BlockchainRecords. После seed нажмите «Завершить» на фронте.
#
# Usage:
#   make seed-demo-votes
#   make seed-demo-votes title="Принцесса" count=100
seed-demo-votes-title ?= Принцесса
seed-demo-votes-count ?= 100

seed-demo-votes:
	ASPNETCORE_ENVIRONMENT=Development dotnet run --project src/EduVote.API -- --seed-demo-votes "$(seed-demo-votes-title)" $(seed-demo-votes-count)

