# NoMoreEOL

**NoMoreEOL** is a quality-of-life mod for *Data Center* that removes repetitive infrastructure maintenance.

It prevents servers and switches from reaching end-of-life and can automatically repair broken devices, keeping your network running without constant micromanagement.

The mod integrates with the in-game modular menu system via DataCenter-RustBridge for easy configuration.

---

## ✨ Features

- Disable switch end-of-life
- Disable server end-of-life
- Automatically repair broken switches
- Automatically repair broken servers
- Real-time behavior (applies continuously during gameplay)
- In-game configuration menu

---

## 📦 Requirements

Before installing, make sure you have:

- **MelonLoader (latest version)**
- **DataCenter-RustBridge**

---

## 📥 Installation

1. Install **MelonLoader** into *Data Center*
2. Install **DataCenter-RustBridge**
3. Download the latest release of **NoMoreEOL**
4. Place `NoMoreEOL.dll` into your `Mods` folder:

```
GameFolder/
└── Mods/
    └── NoMoreEOL.dll
```

5. Launch the game

---

## ⚙️ Configuration

All settings are available in-game through the modular menu (RustBridge).

### Available Options

- **DisableSwitchesEOL** *(default: true)*  
  Prevents switches from reaching end-of-life.

- **DisableServersEOL** *(default: true)*  
  Prevents servers from reaching end-of-life.

- **AutoRepairBrokenSwitches** *(default: true)*  
  Automatically repairs switches after they break.

- **AutoRepairBrokenServers** *(default: true)*  
  Automatically repairs servers after they break.

---

## 🧠 How It Works

- The mod activates when entering the main gameplay scene
- It monitors servers and switches continuously
- It restores original EOL values to prevent expiration
- It automatically repairs broken devices when enabled
- It resets safely when returning to the main menu

---

## 🎯 Scope

This is a **quality-of-life automation mod**.

It does **not**:
- Add new content
- Change game balance
- Modify UI beyond the config menu

---

## ⚠️ Notes

- This mod is designed for convenience and reduced micromanagement
- It may reduce intended maintenance gameplay mechanics

---

## 🛠️ Development

- Mod Loader: MelonLoader
- Framework: .NET 6
- Language: C#

---

## 📜 License

MIT License © 2026 Neox
