# RemoteLoader
A universal [MelonLoader](https://github.com/LavaGang/MelonLoader) **Plugin** that loads mods from arbitrary locations on your computer.

### Usage / Info
* Put `RemoteLoader.dll` into the `Plugins` folder
* Either Launch the game once to generate the default config file `UserData/RemoteLoaderConfig.json` or [create it yourself](#config-file)
* \(You might have to place `Newtonsoft.Json.dll` into the `UserLibs` folder for some games where it doesn't exist\)
* Direct paths to \(`.dll`\) files are assumed to be working Melons and loaded in MelonLoader
* Paths to a folder attempt to load every file ending with `.dll` in said folder

### Config File
```json
{
  "PathsToModsToLoad": [
    "C:/Path/To/A/Mod.dll",
    "C:/Path/To/A/Folder/"
  ]
}
```

### Building the Project
1. Make sure to place `MelonLoader.dll` into the `_ref/` folder
2. Open the solution `RemoteLoader.sln` in Visual Studio
3. Hit `CTRL + Shift + B` on your keyboard or alternatively use the `Build > Build Solution` menubar option
4. The project is now building and the final dll is going to be placed into the `out/` directory

#### Folder structure:
```
.
├── _ref/                                   # References / put your MelonLoader.dll in here
│   ├── .gitkeep
│   └── MelonLoader.dll
├── out/                                    # dll output directory
├── RemoteLoader/                           # Project Folder
├── RemoteLoader.sln
└── .../                                    # Other Folders / Files
```
