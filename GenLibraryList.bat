rem echo off
cd /d %~dp0

tools\GenLibraryList\GenLibraryList.exe --sln=JitPad.sln --output=JitPad.Core\Resources\OssList.json --external=externalLibraries.list

