���������� ��� �������� ���� ������ � ������ ���������� �� ������, ���������� ������ ��������� ������� � ��������� ������:
& "D:\Programmes\PostgreSQL\bin\pg_dump.exe" -U postgres -Fc -f database.dump QuestPlannerDB
& "D:\GAME\PostgreSQL\bin\pg_restore.exe" -U postgres -d QuestPlannerDB database.dump
����� ��� �������� ����� ���������� � ��� ������ ��������� �������, ���� �� ��������� � Visual Studio. ��� ����� ���������� ������� � ������� ������ �������, � ������ ��������� ������� � ��������� ������:
git status
git add --all
git commit -m "����� ����� �� ��� �����"
git push origin master/main