# AGENTS.md — Notes for AI agents (gregMod.NoEOL)

Repo: https://github.com/mleem97/gregMod.NoEOL · License: Apache-2.0 · Version: see `VERSION` (2.0.0).

MelonMod for Data Center. Disables end-of-life (EOL) behavior for parts.
Panel key: **F5**.

## Duties

1. **Read first:** `README.md`, `docs/INDEX.md`, `docs/ARCHITECTURE.md` — only then make changes.
2. **Do not commit secrets** (keys, tokens, `.env`). Use keys only via environment variables.
3. **Preserve history:** no `push --force`, no history rewrite without instruction.
4. **Verify changes:** before reporting done, build the mod (`dotnet build gregMod.NoEOL.csproj -c Release` or `./build.sh NoEOL` from `ModRepositories/`).
5. **Keep docs in sync:** for new features update `README.md` + `docs/` + `CHANGELOG.md` (Unreleased).
6. **Conventions:** Conventional Commits (`feat:`, `fix:`, `docs:`, `chore:` …), one logical change per commit.
7. **When unsure:** stop and ask instead of guessing — especially for deletes, migrations, CI.

## Build and references

- Target: `net6.0`, x64. Game: Data Center (`MelonGame("Waseku", "Data Center")`).
- `references/` holds absolute symlinks into the Steam Data Center install.
  Never commit `references/*.dll`, `bin/`, or `obj/`.
- After a fresh clone, run `../tools/sync-melon-assemblies.sh`.
- Deploy only with `./build.sh NoEOL --deploy`.

## Hard rules

- **Panel key is F5** (F3 = Potato, F4 = FiberTrunk, F6 = Backplanes,
  F7 = Trainer, F8 = MultiCable, F9 = MusicPlayer, F10 = NotesHUD). Do not collide.
- EOL suppression stays a minimal override — never change catalog contents or
  economy behavior beyond disabling EOL gating.
- **Never** touch gregCore types outside a soft-probe/JIT-split bridge — the
  mod must load without `gregCore.dll`.

## Layout

- `src/NoMoreEOL/` — mod sources (entry, patches, prefs).
- `docs/` — `INDEX.md`, `ARCHITECTURE.md`, `SOURCE_LAYOUT.md`, `CHANGELOG.md`.
