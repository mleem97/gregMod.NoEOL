# Changelog — gregMod.NoEOL

Format: [Keep a Changelog](https://keepachangelog.com/de/1.0.0/). Version: see [`VERSION`](VERSION).

## [2.0.0] — 2026-09-24

### Changed

- Hard dependency on gregCore (like HexViewer 1.0.8): probe, guards and
  JIT-split helpers removed; fail-fast with a clear error when
  `gregCore.dll` is missing. Unused `ModImGuiTheme` clone deleted
  (core `GregImGuiTheme` covers it).

## [1.9.0] — 2026-09-24

### Added

- Configurable toggle hotkey (`ToggleKey` pref, default F5), mod contract, key-HUD entry, and opener for the F1 hub (only with gregCore).
- Unified open-source layout (README, docs, badges) following the gregCore template.

## [0.1.0] — 2026-09-22

- Initial standardized baseline.
