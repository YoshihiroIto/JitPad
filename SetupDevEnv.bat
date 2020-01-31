echo off
cd /d %~dp0

pwsh ./script/SetupDevEnv.ps1 -NoProfile -ExecutionPolicy ByPass -NonInteractive

