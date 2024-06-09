set -e

dotnet restore

dotnet build -c Release --no-restore

dotnet build TweaksLauncher.Game/TweaksLauncher.Game.csproj -c Publish -r win-x86 --no-restore

dotnet publish -c Publish --no-restore