
sql-server-start:
	docker run -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=yourStrong(!)Password' -p 1433:1433 -d --name local-sql-server mcr.microsoft.com/mssql/server:2022-latest

sql-server-stop:
	docker stop local-sql-server

sql-server-drop:
	docker rm -f local-sql-server

postgres-start:
	docker run -e 'POSTGRES_PASSWORD=yourStrong(!)Password' -p 5432:5432 -d --name local-postgres postgres:17

postgres-stop:
	docker stop local-postgres

postgres-drop:
	docker rm -f local-postgres

run-sql-migrator:
	cd src/UUIDTester.Migrator && dotnet run

run-postgres-migrator:
	cd src/UUIDTester.Migrator && dotnet run -- --database=postgres

watch-api:
	cd src/UUIDTester.Api && dotnet watch

run-api:
	cd src/UUIDTester.Api && dotnet run

sql-server-up:	sql-server-start run-sql-migrator

postgres-up: postgres-start run-postgres-migrator

up: sql-server-up postgres-up run-api

watch:	sql-server-up postgres-up watch-api

#.PHONY: sql-server-start sql-server-stop sql-server-drop run-migrator run-api watch-api up watch