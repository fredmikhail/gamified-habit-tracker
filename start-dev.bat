@echo off
setlocal EnableExtensions

rem Gamified Habit Tracker - Windows development launcher
rem Run this file from the repository root or double-click it.
rem It starts the ASP.NET Core API and Vite frontend in separate windows.

set "ROOT=%~dp0"
set "API_DIR=%ROOT%server\HabitTracker.Api"
set "CLIENT_DIR=%ROOT%client"

rem Child modes used by the two Command Prompt windows.
if /i "%~1"=="api" goto run_api
if /i "%~1"=="frontend" goto run_frontend

title Habit Tracker Launcher

echo.
echo ==========================================
echo   Gamified Habit Tracker - Development
echo ==========================================
echo.

where dotnet >nul 2>&1
if errorlevel 1 (
    echo ERROR: The .NET SDK was not found in PATH.
    echo Install the required .NET SDK, then reopen Command Prompt.
    echo.
    pause
    exit /b 1
)

where npm >nul 2>&1
if errorlevel 1 (
    echo ERROR: npm was not found in PATH.
    echo Install Node.js and npm, then reopen Command Prompt.
    echo.
    pause
    exit /b 1
)

if not exist "%API_DIR%\HabitTracker.Api.csproj" (
    echo ERROR: Could not find:
    echo %API_DIR%\HabitTracker.Api.csproj
    echo.
    echo Keep start-dev.bat in the repository root.
    pause
    exit /b 1
)

if not exist "%CLIENT_DIR%\package.json" (
    echo ERROR: Could not find:
    echo %CLIENT_DIR%\package.json
    echo.
    echo Keep start-dev.bat in the repository root.
    pause
    exit /b 1
)

echo Starting the ASP.NET Core API...
start "Habit Tracker API" "%ComSpec%" /k ""%~f0" api"

echo Starting the Vite frontend...
start "Habit Tracker Frontend" "%ComSpec%" /k ""%~f0" frontend"

echo.
echo Two development windows should now be open:
echo   1. Habit Tracker API
echo   2. Habit Tracker Frontend
echo.
echo PostgreSQL must already be running and local configuration must be valid.
echo Press Ctrl+C inside each development window to stop that process.
echo.
exit /b 0

:run_api
title Habit Tracker API

cd /d "%API_DIR%"
if errorlevel 1 (
    echo ERROR: Could not open the API directory:
    echo %API_DIR%
    pause
    exit /b 1
)

echo.
echo Starting API with hot reload...
echo Directory: %CD%
echo.

dotnet watch run

echo.
echo The API process stopped.
pause
exit /b

:run_frontend
title Habit Tracker Frontend

cd /d "%CLIENT_DIR%"
if errorlevel 1 (
    echo ERROR: Could not open the frontend directory:
    echo %CLIENT_DIR%
    pause
    exit /b 1
)

echo.
echo Frontend directory: %CD%
echo.

if not exist "node_modules\" (
    echo node_modules was not found. Installing dependencies with npm ci...
    echo.

    call npm ci

    if errorlevel 1 (
        echo.
        echo ERROR: npm ci failed. Review the error above.
        pause
        exit /b 1
    )
)

echo Starting Vite development server...
echo.

call npm run dev -- --open

echo.
echo The frontend process stopped.
pause
exit /b
