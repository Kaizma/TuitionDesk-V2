# TuitionDesk

A local-first Windows desktop app for independent tutors: student registry, class registry, and
allocation in one place. No account, no cloud, works offline.

**Status:** v0.1 MVP feature-complete (2026-11-09) — students, classes, and allocation are
implemented and verified, with 21 backend tests and 10 frontend tests passing. A self-contained
`win-x64` publish and the per-user Inno Setup installer were built and verified (silent install,
launch, uninstall with data retained). Remaining release checks: clean-VM install, upgrade-over
test, and code signing.

## Features

- **Students:** create, edit, search/filter/sort, paginate, archive/restore, duplicate warnings,
  guardian and school details, per-student class history.
- **Classes:** create, edit, search, fee and currency, schedule text, capacity, archive/restore.
- **Allocation:** enrol from a class (multi-select) or from a student, withdraw, active/withdrawn
  roster filters, live enrolled counts, capacity warnings.
- **Local-first:** SQLite database, automatic pre-migration and daily backups, loopback-only API with
  a per-launch session token, no telemetry and no outbound network calls.

## Stack

- **Desktop shell:** WPF (.NET 10) hosting WebView2
- **Backend:** ASP.NET Core minimal API, embedded in the same process, bound to loopback
- **Data:** SQLite via EF Core
- **Frontend:** React 19, TypeScript, Vite 8, TanStack Query, React Hook Form + Zod, Tailwind CSS
- **Tests:** xUnit + built-in asserts (backend), Vitest (frontend)

See [docs/product.md](docs/product.md), [docs/architecture.md](docs/architecture.md), and
[docs/roadmap.md](docs/roadmap.md).

## Prerequisites

- Windows 10 21H2+ or Windows 11, x64
- .NET 10 SDK
- Node.js 24 LTS + pnpm 10
- WebView2 Evergreen runtime (preinstalled on Windows 11)
- Inno Setup 6 (only when building the installer)

## Run

### API only (useful for frontend work)

```powershell
$env:TUITIONDESK_DATA_DIR = "$PWD\artifacts\dev-data"
dotnet run --project src/TuitionDesk.Server
# http://127.0.0.1:5179/api/health
```

### Frontend only

```powershell
pnpm --dir web install
pnpm --dir web dev
# http://127.0.0.1:5173 — proxies /api to 127.0.0.1:5179
```

### Desktop app

The desktop shell loads the built SPA from its `wwwroot`, so build the web app first:

```powershell
pnpm --dir web build
dotnet run --project src/TuitionDesk.Desktop
```

Set `TUITIONDESK_DATA_DIR` before launching to use a throwaway database.

## Test

```powershell
dotnet test
pnpm --dir web test
pnpm --dir web typecheck
```

## Publish and install

```powershell
./build/publish.ps1
# artifacts/app/TuitionDesk.exe
# artifacts/TuitionDesk-Setup-0.1.0.exe (when Inno Setup 6 is installed)
```

The installer is per-user (`%LOCALAPPDATA%\Programs\TuitionDesk`), needs no admin rights, and keeps
the data folder on uninstall. It checks for the WebView2 runtime and explains how to install it if
missing.

## Data and recovery

| Item | Path |
| --- | --- |
| Database | `%LOCALAPPDATA%\TuitionDesk\data\tuitiondesk.db` |
| Backups | `%LOCALAPPDATA%\TuitionDesk\data\backups\` |
| Logs | `%LOCALAPPDATA%\TuitionDesk\logs\` |
| WebView2 profile | `%LOCALAPPDATA%\TuitionDesk\webview2\` |

Backups are taken before every schema migration and once per day when the app starts. The newest 10
are kept.

**Restore:** close TuitionDesk, copy the chosen backup over `data\tuitiondesk.db` (remove the
`-wal`/`-shm` files first), then start the app. Back up the data folder before moving to another
machine.

`TUITIONDESK_DATA_DIR` overrides the root for development and tests.

## Repository layout

```text
src/                             .NET application projects, grouped by layer
tests/                           Domain unit and server integration tests
web/                             React SPA and its package manifest/lockfile
docs/                            Product, architecture, and roadmap documentation
build/                           Publish script and Inno Setup installer
.github/workflows/               CI workflows
```

Within `src`, keep domain rules in `TuitionDesk.Domain`, use cases in
`TuitionDesk.Application`, persistence and integrations in `TuitionDesk.Infrastructure`, HTTP
handling in `TuitionDesk.Server`, and the WPF/WebView2 lifecycle in `TuitionDesk.Desktop`. Add tests
under the matching project in `tests`, and update the relevant docs when behavior or architecture
decisions change. Keep generated output and local data under ignored output locations such as
`artifacts/`; do not add build products, databases, package caches, credentials, or machine-specific
configuration to source control.

Before committing, review `git status` and the staged diff. Confirm every added file is intentional,
generated or local files are not staged, and configuration examples contain no secrets. Use
`.gitignore` for generated and local files, and `.editorconfig` and `.gitattributes` for formatting
and line endings. CI runs `build/verify-repo-hygiene.ps1` as a backstop for tracked artifacts and
recognizable secret formats; it does not replace reviewing staged changes.

## Notes

- The API binds to `127.0.0.1` on an ephemeral port in the desktop app and requires a per-launch
  token injected into the WebView. In development it runs on `:5179` without a token.
- `nuget.config` uses nuget.org. `nuget.offline.config` restores from a local feed/cache for
  restricted or offline machines.
