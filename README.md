# gregMod.NoEOL

> Removes end-of-life expiration and auto-repairs broken devices in **Data Center** — quality-of-life automation for your network.

[![Discord](https://img.shields.io/badge/Discord-Join-5865F2?style=for-the-badge&logo=discord&logoColor=white)](https://discord.gg/greg)
[![gregFramework](https://img.shields.io/badge/gregFramework-Website-blue?style=for-the-badge)](https://gregframework.eu)
[![License](https://img.shields.io/badge/License-Apache%202.0-green?style=for-the-badge)](./LICENSE)
[![Version](https://img.shields.io/badge/Version-1.7.0-orange?style=for-the-badge)]()
[![GameVersion](https://img.shields.io/badge/Game%20Version-1.0.50.15-yellow?style=for-the-badge)]()
[![Unity](https://img.shields.io/badge/Unity-6000.5-black?style=for-the-badge&logo=unity&logoColor=white)]()

## Links

- **Repository:** [github.com/mleem97/gregMod.NoEOL](https://github.com/mleem97/gregMod.NoEOL)
- **Discord / Support:** [discord.gg/greg](https://discord.gg/greg)
- **Website:** [gregframework.eu](https://gregframework.eu)

## Overview

**gregMod.NoEOL** prevents servers and switches from reaching end-of-life and automatically repairs broken devices. It keeps your network running without constant micromanagement.

## Features

- Disable switch end-of-life (restores default EOL value every frame)
- Disable server end-of-life (restores default EOL value every frame)
- Auto-repair broken switches
- Auto-repair broken servers
- Hide EOL warning triangles on devices
- Real-time behavior — applies continuously during gameplay
- **F5 configuration overlay** (IPAM-style dark UI)
- Configurable via MelonPreferences (F5 menu)
- Safe scene reset when returning to main menu
- Verbose release log (`noeol.latest.log`)

## Installation

1. Install **MelonLoader** (v0.7.2+) for **Data Center**
2. Copy the release DLL into the mod folder:

   ```text
   Game/Mods/gregMod.NoEOL.dll
   ```

3. Start the game
4. Press **F5** to open the configuration overlay

## Configuration

All settings are available in-game via MelonPreferences (F5 menu).

| Setting | Default | Description |
|---------|---------|-------------|
| `DisableSwitchesEOL` | `true` | Prevents switches from reaching end-of-life |
| `DisableServersEOL` | `true` | Prevents servers from reaching end-of-life |
| `AutoRepairBrokenSwitches` | `true` | Automatically repairs broken switches |
| `AutoRepairBrokenServers` | `true` | Automatically repairs broken servers |
| `HideWarningTriangles` | `false` | Hides EOL warning triangles on devices |

## How It Works

1. On scene load, the mod caches `NetworkMap` and `MainGameManager` singletons
2. Each frame in `OnUpdate()`:
   - Repairs broken switches/servers (if enabled)
   - Restores default EOL values (if enabled)
3. On main menu load, scene references are cleared
4. EOL values are resolved from device prefabs and cached by type

## Dependencies

- **MelonLoader** (v0.7.2+)

### Build only

- **Il2CppInterop**
- Unity / game interop assemblies from the local Data Center installation

## Build from Source

Requirements:

- .NET 6 SDK
- local Data Center / MelonLoader installation

> **Note:** This mod was built on Linux using Proton-GE 10-34. The `references/` directory contains the required game and MelonLoader DLLs. When building on a different system, adjust the `<HintPath>` entries in the `.csproj` to point to your local MelonLoader and game interop assemblies.

Build:

```bash
git clone https://github.com/mleem97/gregMod.NoEOL.git
cd gregMod.NoEOL
dotnet build -c Release
```

Release output:

```text
bin/Release/net6.0/gregMod.NoEOL.dll
```

## Project Structure

```
gregMod.NoEOL/
├── NoMoreEOL/                  # Source code
│   ├── Core.cs                 # MelonLoader entry point, EOL handling, device repair
│   ├── NoEolOverlay.cs         # F5 configuration overlay (IPAM-style UI)
│   ├── EolHider.cs             # EOL warning triangle hiding (Harmony patch)
│   ├── ModReleaseLog.cs        # Verbose release logging
│   └── Enums/
│       └── EOLDeviceType.cs    # Device type enum
├── references/                 # Game & MelonLoader interop DLLs
├── gregMod.NoEOL.csproj        # Project file
├── LICENSE                     # Apache 2.0
└── README.md
```

## Credits

| Role | Contributor |
|------|-------------|
| **Original Author** | [Neox](https://github.com/MartelSimon) (TeamGreg Modding) |
| **gregMod Fork** | [mleem97](https://github.com/mleem97) ([TeamGreg Modding](https://github.com/teamGregModding)) |
| **EOL Hider** | [tindolt](https://github.com/tindolt) |

## License

This project is licensed under the **Apache License 2.0**. See [`LICENSE`](./LICENSE).

---

**gregFramework — powered by the community.**
