@echo off
chcp 65001 >nul
title Build App_ConsultaStocks - Android
echo.
echo ╔═══════════════════════════════════════════════════════════════╗
echo ║  Build Automatizado - App_ConsultaStocks (Android)            ║
echo ╚═══════════════════════════════════════════════════════════════╝
echo.

:: Verificar si PowerShell está disponible
where powershell >nul 2>nul
if %errorlevel% neq 0 (
    echo ERROR: PowerShell no encontrado
    pause
    exit /b 1
)

:: Ejecutar el script de PowerShell
powershell -ExecutionPolicy Bypass -File "%~dp0build-android.ps1"

if %errorlevel% neq 0 (
    echo.
    echo ═══════════════════════════════════════════════════════════════
    echo  ERROR: El build fallo
    echo ═══════════════════════════════════════════════════════════════
    pause
    exit /b 1
)
