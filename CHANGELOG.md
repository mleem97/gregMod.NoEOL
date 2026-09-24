# Changelog — gregMod.NoEOL

Format: [Keep a Changelog](https://keepachangelog.com/de/1.0.0/). Version: see [`VERSION`](VERSION).

## [2.0.1] — 2026-09-24

### Changed

- EOL patch via GregPatches.TryPatchPrefix.
- English strings throughout.

## [2.0.0] — 2026-09-24

### Changed

- F1-hub wiring via `GregMenuBinding.BindToggle`; EOL patch via shared
  `GregPatches.TryPatchPrefix` helper (with manual-reflection fallback).
  Soft dependency on gregCore preserved: probe, guards and JIT-split
  helpers kept, standalone theme mirror (`ModImGuiTheme`) kept — the mod
  stays fully usable without `gregCore.dll`.

## [1.9.0] — 2026-09-24

### Added

- Configurable toggle hotkey (`ToggleKey` pref, default F5), mod contract, key-HUD entry, and opener for the F1 hub (only with gregCore).
- Unified open-source layout (README, docs, badges) following the gregCore template.

## [0.1.0] — 2026-09-22

- Initial standardized baseline.
