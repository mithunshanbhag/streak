param(
    [ValidateSet('app', 'tests', 'unit-tests', 'e2e-tests')]
    [string]$Target = 'app',
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug',
    [string]$WindowsTargetFramework = 'net10.0-windows10.0.19041.0',
    [string]$WindowsRuntimeIdentifier = 'win-x64'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$solutionFilePath = Join-Path -Path $PSScriptRoot -ChildPath 'Streak.slnx'
$appProjectFilePath = Join-Path -Path $PSScriptRoot -ChildPath 'src\Streak.Ui\Streak.Ui.csproj'
$testsRootPath = Join-Path -Path $PSScriptRoot -ChildPath 'tests'

$allTestProjects = @(
    Get-ChildItem -Path $testsRootPath -Filter *.csproj -Recurse |
        Sort-Object -Property FullName
)

$unitTestProjects = @(
    $allTestProjects |
        Where-Object { $_.BaseName -match 'UnitTests' }
)

$e2eTestProjects = @(
    $allTestProjects |
        Where-Object { $_.BaseName -match 'E2E|E2e' }
)

function Invoke-DotnetCommand
{
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$Arguments
    )

    Write-Host ("`n> dotnet {0}" -f ($Arguments -join ' '))
    & dotnet @Arguments

    if ($LASTEXITCODE -ne 0)
    {
        throw "The dotnet command failed with exit code $LASTEXITCODE."
    }
}

function Invoke-TestProjects
{
    param(
        [System.IO.FileInfo[]]$Projects = @(),
        [Parameter(Mandatory = $true)]
        [string]$Label
    )

    if ($Projects.Count -eq 0)
    {
        Write-Host "No $Label projects were found."
        return
    }

    foreach ($project in $Projects)
    {
        Invoke-DotnetCommand -Arguments @(
            'test'
            $project.FullName
            '-c'
            $Configuration
            '--no-restore'
            '--nologo'
        )
    }
}

Invoke-DotnetCommand -Arguments @(
    'restore'
    $solutionFilePath
    '--nologo'
)

switch ($Target)
{
    'app'
    {
        Invoke-DotnetCommand -Arguments @(
            'build'
            $appProjectFilePath
            '-t:Run'
            '-f'
            $WindowsTargetFramework
            '-c'
            $Configuration
            "-p:RuntimeIdentifier=$WindowsRuntimeIdentifier"
            '--no-restore'
            '--nologo'
        )
    }
    'tests'
    {
        Invoke-TestProjects -Projects $allTestProjects -Label 'test'
    }
    'unit-tests'
    {
        Invoke-TestProjects -Projects $unitTestProjects -Label 'unit test'
    }
    'e2e-tests'
    {
        Invoke-TestProjects -Projects $e2eTestProjects -Label 'E2E test'
    }
}
