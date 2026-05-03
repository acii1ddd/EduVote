.PHONY: migration-add migration-update migration-remove

# Создать новую миграцию
# Usage: make migration-add name=MigrationName
migration-add:
	dotnet ef migrations add $(name) -s ./EduVote.API -p ./EduVote.DAL.Postgresql/

# Применить миграции к БД
migration-update:
	dotnet ef database update -s ./EduVote.API -p ./EduVote.DAL.Postgresql/

# Удалить последнюю миграцию
migration-remove:
	dotnet ef migrations remove -s ./EduVote.API -p ./EduVote.DAL.Postgresql/

migration-list:
	dotnet ef migrations list -s  ./EduVote.API -p ./EduVote.DAL.Postgresql/

