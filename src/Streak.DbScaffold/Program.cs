namespace Streak.DbScaffold;

/// <summary>
///     Note: This app only exists to work-around an issue in EF core.
///     We're hitting the following error when attempting to scaffold the dbContext inside
///     the FinSkew.AzureFunctionApp project:
///     Startup project 'FinSkew.AzureFunctionApp.csproj' targets framework '.NETStandard'.
///     There is no runtime associated with this framework, and projects targeting it cannot
///     be executed directly. To use the Entity Framework Core .NET Command Line Tools with
///     this project, add an executable project targeting .NET Core or .NET Framework that
///     references this project, and set it as the startup project using --startup-project;
///     or, update this project to cross-target .NET Core or .NET Framework.
///     The command to generate the scaffolding code is as follows:
///     dotnet ef dbcontext scaffold "{db connection string}" Microsoft.EntityFrameworkCore.SqlServer -o
///     ../Streak.Models/SQL -f --use-database-names -c StreakDbContext --no-pluralize
///     In case you see a 'command or file was not found' error, then run the following:
///     export PATH="$PATH:$HOME/.dotnet/tools/"
///     reference: https://stackoverflow.com/questions/56862089/cannot-find-command-dotnet-ef
/// </summary>
internal class Program
{
    private static void Main()
    {
    }
}