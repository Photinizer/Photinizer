@echo off
powershell -ExecutionPolicy Bypass -File "%~dp0remove-nuget.ps1" photinizer.cli
powershell -ExecutionPolicy Bypass -File "%~dp0remove-nuget.ps1" photinizer.build.own
pause