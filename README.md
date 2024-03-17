# W.I.P.
This project is still in development.

# Il2CppLauncher
A lightweight Unity Il2Cpp mod launcher.

## How it works
Il2CppLauncher is a game launcher, meaning it does NOT inject itself into the game, but rather starts it manually through `UnityPlayer.dll`.
Mods can interact with the game through the generated proxy assemblies from [Il2CppInterop](https://github.com/BepInEx/Il2CppInterop), just like with any other known Il2Cpp mod loaders.

## Credits
[Il2CppInterop](https://github.com/BepInEx/Il2CppInterop) - A tool interoperate between CoreCLR and Il2Cpp at runtime<br>
[BepInEx/Dobby](https://github.com/BepInEx/Dobby) - Fork of jmpews/Dobby with stability edits for Windows<br>