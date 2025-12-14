@echo off
REM Build Godot library for Linux using WSL2
REM This is a simple wrapper that calls the PowerShell script

setlocal enabledelayedexpansion

echo ============================================
echo   Linux Build Script (via WSL2)
echo ============================================
echo.

REM Check if PowerShell is available
where pwsh >nul 2>nul
if %ERRORLEVEL% EQU 0 (
    echo Using PowerShell Core...
    pwsh -ExecutionPolicy Bypass -File "%~dp0Build-Linux.ps1" %*
    exit /b %ERRORLEVEL%
)

where powershell >nul 2>nul
if %ERRORLEVEL% EQU 0 (
    echo Using Windows PowerShell...
    powershell -ExecutionPolicy Bypass -File "%~dp0Build-Linux.ps1" %*
    exit /b %ERRORLEVEL%
)

echo ERROR: PowerShell not found!
echo Please install PowerShell or use the direct WSL2 method.
echo.
echo Direct method:
echo   1. Open WSL2: wsl
echo   2. Navigate to project: cd /mnt/c/path/to/LibGodotSharp-Example
echo   3. Run: dotnet run --project GodotBuilder/GodotBuilder.csproj -- linuxbsd
echo.
exit /b 1
