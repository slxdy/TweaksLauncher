<div>
  <img src="https://github.com/slxdy/TweaksLauncher/assets/61495410/7a4c686b-d092-4f6c-9aef-64b3421457f1" width="160">
  <h1>TweaksLauncher</h1>
</div>

A lightweight Unity mod launcher for IL2CPP games built for windows x64 and x86.

## How it works
TweaksLauncher is a game launcher, meaning it does NOT inject itself into the game, but rather starts it manually through the UnityPlayer module.
Mods can interact with the game through the generated proxy assemblies from [Il2CppInterop](https://github.com/BepInEx/Il2CppInterop), just like with any other known Il2Cpp mod loaders.

## Requirements
- [.NET 8.0 Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-8.0.3-windows-x64-installer)

## How to use
**If you've already installed the launcher before, skip steps 1 and 2.**
1. Grab the latest zip from the [Releases Page](https://github.com/slxdy/TweaksLauncher/releases).
2. Unzip it to an empty folder (NOT IN A GAME FOLDER).
3. Start the launcher, and follow the instruction displayed in the console. Do not use the x86 version unless you know what you're doing. This will allow you to start the game through the launcher.

## How to install mods
Depending on the mod, the installation instructions may vary. However, most zipped mods should be extracted to the launcher's folder. **Make sure to extract the entire zip, including all folders within it. Do NOT create any extra folders if prompted.** If the mod is a single DLL, follow instructions provided by the developer.

## Developing Mods
TweaksLauncher provides useful features for developers, such as auto-build and debugging.<br>
To get started, refer to the [Developer Guide](Guides/DeveloperGuide.md).

## Compilation Guide
The project should be compiled using Visual Studio. Make sure to build the entire solution. The builds are saved to the `output` directory.

## Credits
[Il2CppInterop](https://github.com/BepInEx/Il2CppInterop) - A tool interoperate between CoreCLR and Il2Cpp at runtime<br>
[Cpp2IL](https://github.com/SamboyCoding/Cpp2IL) - Work-in-progress tool to reverse unity's IL2CPP toolchain.<br>
[BepInEx/Dobby](https://github.com/BepInEx/Dobby) - Fork of jmpews/Dobby with stability edits for Windows<br>
[HarmonyX](https://github.com/BepInEx/HarmonyX) - Harmony built on top of MonoMod.RuntimeDetours with additional features<br>
[Pastel](https://github.com/silkfire/Pastel) - A tiny utility class that makes colorizing console output a breeze.<br>
[GameFinder.StoreHandlers.Steam](https://github.com/erri120/GameFinder) - Library for finding games installed with Steam.