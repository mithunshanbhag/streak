param(
    [ValidateSet('build', 'test', 'run', 'all')]
    [string]$Task = 'all',
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug'
)

$ErrorActionPreference = 'Stop'

$solutionFilePath = Join-Path -Path $PSScriptRoot -ChildPath 'Streak.slnx'
$testProjectFilePath = Join-Path -Path $PSScriptRoot -ChildPath 'tests\Streak.Ui.UnitTests\Streak.Ui.UnitTests.csproj'
$appProjectFilePath = Join-Path -Path $PSScriptRoot -ChildPath 'src\Streak.Ui\Streak.Ui.csproj'
$sqliteScriptFilePath = Join-Path -Path $PSScriptRoot -ChildPath 'src\Streak.Ui\Repositories\Implementations\Sqlite\CreateLocalSqliteDb.ps1'

dotnet restore $solutionFilePath

if ($Task -in @('build', 'all'))
{
    dotnet build $testProjectFilePath -c $Configuration
    & $sqliteScriptFilePath
}

if ($Task -in @('test', 'all'))
{
    if ($Task -eq 'all')
    {
        dotnet test $testProjectFilePath -c $Configuration --no-build
    }
    else
    {
        dotnet test $testProjectFilePath -c $Configuration
    }
}

if ($Task -eq 'run')
{
    dotnet build -t:Run $appProjectFilePath -f net10.0-android -c $Configuration
}
