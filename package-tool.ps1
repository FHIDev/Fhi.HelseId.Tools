#!/usr/bin/env pwsh
# Script to package the HelseID CLI as a .NET tool

# Navigate to the CLI project directory
$cliProjectDir = "$PSScriptRoot\src\Fhi.HelseIdSelvbetjening.CLI"
Push-Location $cliProjectDir

# Clean any previous packages
if (Test-Path "nupkg") {
    Write-Host "Cleaning previous packages..."
    Remove-Item -Recurse -Force "nupkg"
}

# Build and pack the tool
Write-Host "Building and packaging the tool..."
dotnet pack -c Release

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Failed to create package" -ForegroundColor Red
    exit 1
}

Write-Host "`nPackage created successfully in $cliProjectDir\nupkg" -ForegroundColor Green

# Display installation instructions
Write-Host "`nTo install the tool globally, run:" -ForegroundColor Yellow
Write-Host "dotnet tool install --global --add-source $cliProjectDir\nupkg Fhi.HelseIdSelvbetjening.CLI"

Write-Host "`nTo install the tool in a local tool manifest, run:" -ForegroundColor Yellow
Write-Host "dotnet new tool-manifest   # if you don't have one"
Write-Host "dotnet tool install --add-source $cliProjectDir\nupkg Fhi.HelseIdSelvbetjening.CLI"

# Return to the original directory
Pop-Location