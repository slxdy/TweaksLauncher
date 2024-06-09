@echo off

dotnet restore
if %ERRORLEVEL% NEQ 0 exit %ERRORLEVEL%

dotnet build -c Release --no-restore
if %ERRORLEVEL% NEQ 0 exit %ERRORLEVEL%

dotnet build TweaksLauncher.Game/TweaksLauncher.Game.csproj -c Publish -r win-x86 --no-restore
if %ERRORLEVEL% NEQ 0 exit %ERRORLEVEL%

dotnet publish -c Publish --no-restore
if %ERRORLEVEL% NEQ 0 exit %ERRORLEVEL%