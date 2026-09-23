# Quickstart — gregMod.NoEOL

> gregMod.NoEOL** prevents servers and switches from reaching end-of-life and automatically repairs broken devices. It keeps your network running without constant

Repo: [https://github.com/mleem97/gregMod.NoEOL](https://github.com/mleem97/gregMod.NoEOL) · Version: `0.1.0` · Lizenz: Apache-2.0.

## 1. Klonen

```bash
git clone git@github.com:mleem97/gregMod.NoEOL.git
cd gregMod.NoEOL
```

## 2. Bauen / Starten

Je nach Tech-Stack **einen** Weg wählen:

```bash
# .NET
dotnet build -c Release
dotnet run --project src/

# Node / pnpm
pnpm install
pnpm build
pnpm start

# Python
python -m venv .venv && source .venv/bin/activate
pip install -r requirements.txt
python -m <modul>
```

## 3. Testen

```bash
dotnet test            # .NET
pnpm test              # Node
pytest                 # Python
```

Details stehen in [README.md](README.md) und [docs/INDEX.md](docs/INDEX.md).
Bei Problemen: Issue anlegen ([Issues](https://github.com/mleem97/gregMod.NoEOL/issues)) oder [CONTRIBUTING.md](CONTRIBUTING.md) lesen.
