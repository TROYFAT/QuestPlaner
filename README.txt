Инструкция для переноса базы данных с одного устройства на другое, необходимо ввести следующую команду в командную строку:
& "D:\Programmes\PostgreSQL\bin\pg_dump.exe" -U postgres -d QuestPlannerDB -f dump.sql
& "D:\GAME\PostgreSQL\bin\psql.exe" -U postgres -d QuestPlannerDB -f dump.sql
Также для удобства можно переносить и сам проект полностью целиком, если вы работаете в Visual Studio. Для этого необходимо перейти в каталог вашего проекта, и ввести сдедующие команды в командную строку:
git status
git add --all
git commit -m "Любой текст на ваш выбор"
git push origin master/main