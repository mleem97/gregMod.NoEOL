# Quickstart — gregMod.NoEOL

> gregMod.NoEOL** prevents servers and switches from reaching end-of-life and automatically repairs broken devices. It keeps your network running without constant

Repo: [https://github.com/mleem97/gregMod.NoEOL](https://github.com/mleem97/gregMod.NoEOL) · Version: `0.1.0` · License: Apache-2.0.

## 1. Clone

```bash
git clone git@github.com:mleem97/gregMod.NoEOL.git
cd gregMod.NoEOL
```

## 2. Build / Run

Depending on the tech stack, choose **one** path:

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

## 3. Test

```bash
dotnet test            # .NET
pnpm test              # Node
pytest                 # Python
```

Details are in [README.md](README.md) and [docs/INDEX.md](docs/INDEX.md).
If you run into problems: open an issue ([Issues](https://github.com/mleem97/gregMod.NoEOL/issues)) or read [CONTRIBUTING.md](CONTRIBUTING.md).
