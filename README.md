<h1 align="center">🌌 Hyperion<br/></h1>
<h2 align="center">Data collection plugin for Terra Invicta</h1>

## ⚠️ Disclaimer

This plugin is provided **as-is**, with no warranties or guarantees of any kind.  
By using this software, you acknowledge that:

- You are using it **at your own risk**
- The author(s) are **not liable** for any damage to your system, game installation, save files, or other data
- There is **no official support or warranty**

Always back up your game and save files before installing or testing mods.

<br><br>
<p align="center">🌑🌒🌓🌔🌕🌖🌗🌘🌑</p>
<br>

## 🔧 Setting up the plug-in

To use or develop this plugin, you'll need to install [BepInEx](https://github.com/BepInEx/BepInEx) into your Terra Invicta install folder. Here's how:

### ✅ Step 1: Download BepInEx 5

- Go to the [BepInEx Releases page](https://github.com/BepInEx/BepInEx/releases)
- Download the latest **BepInEx x64 (Unity IL2CPP)** version  
  ⚠️ Terra Invicta uses **IL2CPP**, not Mono!

Example:  
`BepInEx_x64_5.X.X.0.zip`

---

### ✅ Step 2: Extract to Game Folder

Extract the contents of the ZIP into your Terra Invicta install directory, where `TerraInvicta.exe` is located.

Your folder should now look like (other files/folders will also be present):

```
Terra Invicta/
├── TerraInvicta.exe
└── BepInEx/
```

---

### ✅ Step 3: Run the Game Once

Launch Terra Invicta once after installing BepInEx.

This will:
- Create the full plugin folder structure
- Generate logs to confirm it's working

Your folder should now look like (other files/folders will also be present):

```
Terra Invicta/
└── BepInEx/
    ├── plugins/
    └── LogOutput.log

```

---

### ✅ Step 4: Setting up your Terra Invicta path

1. Open `.vscode/settings.json`
2. Set your local TI install path:

```json
{
  "hyperion.tiPath": "F:\\SteamLibrary\\steamapps\\common\\Terra Invicta"
}
```

---

### ✅ Step 5: Build and Deploy the Plugin

Once BepInEx is installed, you can:
- Build the plugin using `Ctrl+Shift+B` in VS Code
- Deploy it into `BepInEx/plugins/`
- Launch Terra Invicta and verify it's loaded

Your plugin will appear in the console log or `BepInEx\LogOutput.log`.

<br><br>
<p align="center">☀️ ✦  ✦ 🌎 ✦ ✦ 🌕 ✦  ✦ 🪐 ✦ ✦ 👽</p>
<br>

## 🛡️ License

This project is licensed under the [Creative Commons Attribution-NonCommercial 4.0 International License](https://creativecommons.org/licenses/by-nc/4.0/).

You may use, modify, and distribute it **for non-commercial purposes only**, with attribution.  
**Commercial use is prohibited without permission.**

Attribution: © 2025 Nova9
