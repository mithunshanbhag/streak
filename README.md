# Streak

[![Build](https://img.shields.io/badge/build-placeholder-lightgrey)](#build-and-run-locally)
[![Tests](https://img.shields.io/badge/tests-placeholder-lightgrey)](#run-tests)
[![Coverage](https://img.shields.io/badge/coverage-placeholder-lightgrey)](#run-tests)

Streak is a .NET MAUI + Blazor habit tracking app for Android with local SQLite storage, focused on quick daily check-ins, streak visibility, habit management, and reminder settings.

![Screenshot / GIF placeholder](https://via.placeholder.com/960x540.png?text=Streak+App+Screenshot+or+GIF)

## Installation

1. Install **.NET 10 SDK**.
2. Install MAUI Android workload (for app run): `dotnet workload install maui-android`.
3. From repo root, restore dependencies: `dotnet restore .\Streak.slnx`.

## Usage

After launching the app on Android/emulator:
- Add, edit, delete, and reorder habits on **Manage Habits**.
- Toggle today’s check-in on **Home**.
- Open **Trends** for streak count + heatmap.
- Configure reminder enable/time in **Settings**.

## Build and run locally

Use the convenience script from repo root:

- Build + generate local SQLite db: `pwsh .\run-local.ps1 -Task build`
- Run unit tests: `pwsh .\run-local.ps1 -Task test`
- Run app on Android target: `pwsh .\run-local.ps1 -Task run`
- Build + test: `pwsh .\run-local.ps1 -Task all`

Equivalent direct commands:

- `dotnet build .\tests\Streak.Ui.UnitTests\Streak.Ui.UnitTests.csproj -c Debug`
- `dotnet test .\tests\Streak.Ui.UnitTests\Streak.Ui.UnitTests.csproj -c Debug --no-build`
- `dotnet build -t:Run .\src\Streak.Ui\Streak.Ui.csproj -f net10.0-android -c Debug`

## Run tests

From repo root:

1. `dotnet build .\tests\Streak.Ui.UnitTests\Streak.Ui.UnitTests.csproj -c Debug`
2. `dotnet test .\tests\Streak.Ui.UnitTests\Streak.Ui.UnitTests.csproj -c Debug --no-build`

## SQLite SQL-first + scaffolding workflow

- SQL schema: `src\Streak.Ui\Repositories\Implementations\Sqlite\streak-schema.sql`
- DbContext: `src\Streak.Ui\Repositories\Implementations\Sqlite\StreakDbContext.cs`
- Scaffolded entities: `src\Streak.Ui\Repositories\Implementations\Sqlite\Entities\`
- SQLite generation script: `src\Streak.Ui\Repositories\Implementations\Sqlite\CreateLocalSqliteDb.ps1`

Workflow when schema changes:

1. Update `streak-schema.sql`.
2. Regenerate db file from SQL schema:
   - `pwsh .\src\Streak.Ui\Repositories\Implementations\Sqlite\CreateLocalSqliteDb.ps1`
3. Regenerate EF Core model (DbContext + Entities):
   - Install EF tool (once): `dotnet tool install --global dotnet-ef --version 10.*`
   - From `src\Streak.Ui`, run:
   - `dotnet ef dbcontext scaffold "Data Source=Repositories\Implementations\Sqlite\streak.local.db" Microsoft.EntityFrameworkCore.Sqlite --context StreakDbContext --context-dir Repositories\Implementations\Sqlite --output-dir Repositories\Implementations\Sqlite\Entities --force --no-onconfiguring`
