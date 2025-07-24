Инструкция для переноса базы данных с одного устройства на другое, необходимо ввести следующую команду в командную строку:
& "D:\Programmes\PostgreSQL\bin\pg_dump.exe" -U postgres -d QuestPlannerDB -f dump.sql
& "D:\GAME\PostgreSQL\bin\psql.exe" -U postgres -d QuestPlannerDB -f dump.sql
