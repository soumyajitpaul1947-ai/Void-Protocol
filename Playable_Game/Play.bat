@echo off
title VOID PROTOCOL - Launching Game...
cd /d "%~dp0"
python "launcher.py"
if %errorlevel% neq 0 (
    echo Python failed or not found. Attempting direct browser launch...
    start "" "index.html"
)
