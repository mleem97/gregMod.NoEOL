# Changelog

## v1.7.0

- EolHider: hide EOL warning triangles on devices (by tindolt)
- Harmony prefix patch on `StaticUIElements.InstantiateErrorWarningSign`
- Toggle for warning triangle visibility in F5 overlay
- `PositionIndicator` hiding on scene load

## v1.6.5

- F5 configuration overlay (IPAM-style dark UI)
- Five toggle cards: Disable Switches EOL, Disable Servers EOL, Auto Repair Switches, Auto Repair Servers, Hide Warning Triangles
- MelonPreferences integration with immediate apply
- Procedural texture and GUIStyle system

## v1.6.1

- Code restructure: split into Core, NoEolOverlay, EolHider, ModReleaseLog
- Verbose release log (`noeol.latest.log`)
- Environment and mod metadata logging

## v1.6.0

- Initial release
- Disable switch end-of-life (restores default EOL every frame)
- Disable server end-of-life (restores default EOL every frame)
- Auto-repair broken switches
- Auto-repair broken servers
- Scene lifecycle management (cache on gameplay, clear on main menu)
