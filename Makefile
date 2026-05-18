.PHONY: migration-add migration-update migration-remove

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

