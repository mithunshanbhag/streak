
pushd %~dp0
dotnet ef dbcontext scaffold "Data Source=c:\Users\mithu\Downloads\temp-delete-me-later2.db" Microsoft.EntityFrameworkCore.Sqlite -p Streak.DbScaffold.csproj -f --use-database-names --no-onconfiguring -c StreakDbContext --context-dir ..\Streak.Ui\Repositories\DbContexts --context-namespace Streak.Ui.Repositories.DbContexts -n Streak.Ui.Models.Storage -o ..\Streak.Ui\Models\Storage
popd
