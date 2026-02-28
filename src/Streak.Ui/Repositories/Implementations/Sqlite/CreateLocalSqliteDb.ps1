param(
    [string]$SchemaPath = "$PSScriptRoot\streak-schema.sql",
    [string]$DatabasePath = "$PSScriptRoot\streak.local.db"
)

$resolvedSchemaPath = (Resolve-Path -Path $SchemaPath).Path

if (Test-Path -Path $DatabasePath)
{
    Remove-Item -Path $DatabasePath -Force
}

$toolingDirectoryPath = Join-Path -Path $env:TEMP -ChildPath "Streak.SqliteDbGenerator"
$toolingProjectFilePath = Join-Path -Path $toolingDirectoryPath -ChildPath "Streak.SqliteDbGenerator.csproj"
$toolingProgramFilePath = Join-Path -Path $toolingDirectoryPath -ChildPath "Program.cs"

if (-not (Test-Path -Path $toolingDirectoryPath))
{
    New-Item -Path $toolingDirectoryPath -ItemType Directory | Out-Null
}

@"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.Data.Sqlite" Version="10.0.3" />
  </ItemGroup>
</Project>
"@ | Set-Content -Path $toolingProjectFilePath -Encoding UTF8

@"
using Microsoft.Data.Sqlite;

if (args.Length != 2)
{
    throw new InvalidOperationException("Expected args: <schemaPath> <databasePath>.");
}

var schemaPath = args[0];
var databasePath = args[1];

var sql = await File.ReadAllTextAsync(schemaPath);
var directoryPath = Path.GetDirectoryName(databasePath);
if (!string.IsNullOrWhiteSpace(directoryPath))
{
    Directory.CreateDirectory(directoryPath);
}

await using var connection = new SqliteConnection($"Data Source={databasePath}");
await connection.OpenAsync();
await using var command = connection.CreateCommand();
command.CommandText = sql;
await command.ExecuteNonQueryAsync();
"@ | Set-Content -Path $toolingProgramFilePath -Encoding UTF8

dotnet run --project $toolingProjectFilePath --configuration Release -- $resolvedSchemaPath $DatabasePath
if ($LASTEXITCODE -ne 0)
{
    throw 'Failed to generate SQLite database from schema.'
}

Write-Host "SQLite database created at: $DatabasePath"
