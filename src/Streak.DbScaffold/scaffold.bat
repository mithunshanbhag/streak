
pushd %~dp0
dotnet ef dbcontext scaffold "Data Source=c:\Users\mithu\Downloads\temp-delete-me-later2.db" Microsoft.EntityFrameworkCore.Sqlite -p Streak.DbScaffold.csproj -f --use-database-names --no-onconfiguring -c StreakDbContext --context-dir ..\Streak.Core\Repositories\DbContexts --context-namespace Streak.Core.Repositories.DbContexts -n Streak.Core.Models.Storage -o ..\Streak.Core\Models\Storage
popd