@echo off
title VOID PROTOCOL - Launching Game...
cd /d "%~dp0"
if exist "VoidProtocol.exe" (
    start "" "VoidProtocol.exe"
    exit /b 0
)
python "Playable_Game\launcher.py"
if %errorlevel% neq 0 (
    echo Python failed or not found. Attempting direct browser launch...
    start "" "Playable_Game\index.html"
)
