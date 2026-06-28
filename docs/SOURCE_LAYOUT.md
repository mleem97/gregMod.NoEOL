# Source layout

All source lives under `NoMoreEOL/` with root namespace **`GregModNoEOL`**. No sub-namespaces.

## Tree

```
NoMoreEOL/
├── Core.cs                    # MelonLoader entry point, EOL handling, device repair
├── NoEolOverlay.cs            # F5 configuration overlay (IPAM-style dark UI)
├── EolHider.cs                # EOL warning triangle hiding (Harmony patch by tindolt)
├── ModReleaseLog.cs           # Verbose file logging (noeol.latest.log)
└── Enums/
    └── EOLDeviceType.cs       # Device type enum (Switch, Server)
```

## File descriptions

### `Core.cs`
MelonLoader entry point (`GregModNoEOLMod : MelonMod`). Registers MelonPreferences for five settings (DisableSwitchesEOL, DisableServersEOL, AutoRepairSwitches, AutoRepairServers, HideWarningTriangles). On each `OnUpdate` frame: repairs broken devices, resets EOL values to defaults. Manages scene lifecycle — caches `NetworkMap` and `MainGameManager` on gameplay scene load, clears on main menu. Handles F5 key input to toggle the overlay.

### `NoEolOverlay.cs`
IMGUI configuration overlay (toggled with F5). Renders a dark IPAM-style window with five toggle cards — one per setting. Each card shows title, description, and an ON/OFF button that writes to MelonPreferences immediately. Uses procedural textures and custom `GUIStyle` instances. Blocks game input while visible.

### `EolHider.cs`
Based on EolHider by [tindolt](https://github.com/tindolt). Uses Harmony to prefix-patch `StaticUIElements.InstantiateErrorWarningSign`, returning early when the "Hide Warning Triangles" setting is enabled. Also hides existing `PositionIndicator` objects on scene load and when the setting is toggled.

### `ModReleaseLog.cs`
Writes a verbose log (`noeol.latest.log`) next to MelonLoader's `Latest.log`. Recreated on each game launch. Logs environment info (OS, .NET, Unity version, game version, screen resolution), mod metadata, and categorized events (EOL resets, repairs, scene changes, config changes, errors).

### `Enums/EOLDeviceType.cs`
Simple enum: `Switch = 0`, `Server`. Used for device type classification.
