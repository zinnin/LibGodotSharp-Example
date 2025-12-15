#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Builds Godot library for Linux using WSL2.

.DESCRIPTION
    This script automatically launches the GodotBuilder inside WSL2 to build
    the Linux native library. It handles path conversion and WSL2 detection.

.PARAMETER Configuration
    The build configuration (Release or Debug). Default is Release.

.EXAMPLE
    .\Build-Linux.ps1
    Builds Linux library in Release configuration

.EXAMPLE
    .\Build-Linux.ps1 -Configuration Debug
    Builds Linux library in Debug configuration
#>

param(
    [ValidateSet('Release', 'Debug')]
    [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Linux Build Script (via WSL2)" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Check if WSL is installed
Write-Host "Checking for WSL2..." -ForegroundColor Yellow
$wslCheck = wsl --list --verbose 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: WSL2 is not installed or not configured properly." -ForegroundColor Red
    Write-Host ""
    Write-Host "To install WSL2:" -ForegroundColor Yellow
    Write-Host "  1. Run in PowerShell as Administrator:" -ForegroundColor White
    Write-Host "     wsl --install" -ForegroundColor White
    Write-Host "  2. Restart your computer" -ForegroundColor White
    Write-Host "  3. Run this script again" -ForegroundColor White
    Write-Host ""
    exit 1
}

Write-Host "✓ WSL2 is installed" -ForegroundColor Green
Write-Host ""

# Check if a distribution is installed and running
$distributions = wsl --list --verbose | Select-String -Pattern "^\s*[\*\s]\s*(\S+)\s+(Running|Stopped)\s+(\d+)" -AllMatches
if ($distributions.Matches.Count -eq 0) {
    Write-Host "ERROR: No WSL2 distributions found." -ForegroundColor Red
    Write-Host ""
    Write-Host "To install a distribution:" -ForegroundColor Yellow
    Write-Host "  wsl --install -d Ubuntu" -ForegroundColor White
    Write-Host ""
    exit 1
}

Write-Host "Available WSL2 distributions:" -ForegroundColor Yellow
wsl --list --verbose
Write-Host ""

# Get the current directory and convert to WSL path
$currentDir = Get-Location
$driveLetter = $currentDir.Drive.Name.ToLower()
$pathWithoutDrive = $currentDir.Path.Substring(2).Replace('\', '/')
$wslPath = "/mnt/$driveLetter$pathWithoutDrive"

Write-Host "Current directory: $currentDir" -ForegroundColor Cyan
Write-Host "WSL2 path: $wslPath" -ForegroundColor Cyan
Write-Host ""

# Check if .NET is installed in WSL2
Write-Host "Checking if .NET SDK is installed in WSL2..." -ForegroundColor Yellow
$dotnetCheck = wsl dotnet --version 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "WARNING: .NET SDK is not installed in WSL2." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "To install .NET SDK in WSL2:" -ForegroundColor Yellow
    Write-Host "  1. Open WSL2 terminal: wsl" -ForegroundColor White
    Write-Host "  2. Run these commands:" -ForegroundColor White
    Write-Host "     wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh" -ForegroundColor White
    Write-Host "     chmod +x dotnet-install.sh" -ForegroundColor White
    Write-Host "     ./dotnet-install.sh --channel 8.0" -ForegroundColor White
    Write-Host "     echo 'export DOTNET_ROOT=`$HOME/.dotnet' >> ~/.bashrc" -ForegroundColor White
    Write-Host "     echo 'export PATH=`$PATH:`$HOME/.dotnet' >> ~/.bashrc" -ForegroundColor White
    Write-Host "     source ~/.bashrc" -ForegroundColor White
    Write-Host ""
    Write-Host "Attempting to continue anyway..." -ForegroundColor Yellow
    Write-Host ""
}
else {
    Write-Host "✓ .NET SDK is installed in WSL2" -ForegroundColor Green
    Write-Host ""
}

# Build the Linux library
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Starting Linux Build in WSL2" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "This will take 30-60 minutes..." -ForegroundColor Yellow
Write-Host ""

$wslCommand = "cd '$wslPath' && dotnet run --project GodotBuilder/GodotBuilder.csproj -- linuxbsd"

Write-Host "Executing in WSL2: $wslCommand" -ForegroundColor Cyan
Write-Host ""

# Run the build command in WSL2
wsl bash -c $wslCommand

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "============================================" -ForegroundColor Green
    Write-Host "  Linux Build Completed Successfully!" -ForegroundColor Green
    Write-Host "============================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Output location: lib/linux/" -ForegroundColor Cyan
    Write-Host ""
}
else {
    Write-Host ""
    Write-Host "============================================" -ForegroundColor Red
    Write-Host "  Linux Build Failed" -ForegroundColor Red
    Write-Host "============================================" -ForegroundColor Red
    Write-Host ""
    Write-Host "Common issues:" -ForegroundColor Yellow
    Write-Host "  1. .NET SDK not installed in WSL2" -ForegroundColor White
    Write-Host "  2. Missing build dependencies (gcc, g++, scons, etc.)" -ForegroundColor White
    Write-Host "  3. Insufficient disk space" -ForegroundColor White
    Write-Host ""
    Write-Host "To troubleshoot, open WSL2 and run manually:" -ForegroundColor Yellow
    Write-Host "  wsl" -ForegroundColor White
    Write-Host "  cd $wslPath" -ForegroundColor White
    Write-Host "  dotnet run --project GodotBuilder/GodotBuilder.csproj -- linuxbsd" -ForegroundColor White
    Write-Host ""
    exit $LASTEXITCODE
}
