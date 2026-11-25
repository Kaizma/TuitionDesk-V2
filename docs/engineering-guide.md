# Engineering Guide for Contributors and Agents

**Audit date:** 2026-11-25

This guide is a compact orientation and audit handoff, not a replacement for product or architecture decisions. See [product requirements](product.md) for behavior and scope, [architecture](architecture.md) for system design and guardrails, and [roadmap](roadmap.md) for delivery and release status.

## Repository map

| Area | Responsibility |
| --- | --- |
| `src/TuitionDesk.Domain` | Entity invariants and state transitions |
| `src/TuitionDesk.Application` | Use cases, validation, DTOs, and persistence abstractions |
| `src/TuitionDesk.Infrastructure` | EF Core, SQLite, repositories, backups, and system clock |
| `src/TuitionDesk.Server` | Minimal API, middleware, and composition root |
| `src/TuitionDesk.Desktop` | WPF/WebView2 lifecycle and embedded server startup |
| `web/src/features` | Feature UI, API calls, forms, and query hooks |
| `web/src/shared` | Shared API client, UI components, and utilities |
| `tests` | Domain unit tests and server integration tests |
| `docs`, `build` | Product/architecture/release docs and Windows publish/installer scripts |

Follow the existing direction: Web → HTTP → Server → Application → Domain; Infrastructure implements Application abstractions, and Desktop composes the Server. Keep rules enforced by the server even when the UI provides early feedback.

## Verification commands

Run from the repository root:

```powershell
dotnet restore TuitionDesk.slnx
dotnet build TuitionDesk.slnx --no-restore
dotnet test TuitionDesk.slnx --no-build
pnpm --dir web install --frozen-lockfile
pnpm --dir web build # includes TypeScript typecheck
pnpm --dir web test
```

CI runs the hygiene check and these build/test gates on Windows. There is no `pnpm lint` script; do not add it to checklists unless a linter is introduced. `build/publish.ps1` builds the web app and self-contained Windows desktop app; Inno Setup is optional when unavailable. Clean-machine install and upgrade checks remain manual release work.

### Choose checks by change

| Change | Minimum focused checks |
| --- | --- |
| Domain rule or Application handler | `dotnet test TuitionDesk.slnx` |
| API endpoint, validation, middleware, or HTTP contract | `dotnet test TuitionDesk.slnx` (integration tests use the real server and SQLite) |
| Persistence, migration, or backup | `dotnet test TuitionDesk.slnx`; add/extend SQLite-backed tests for the changed recovery or data behavior |
| Frontend feature, form, or API handling | `pnpm --dir web build` and `pnpm --dir web test` |
| Desktop startup, packaging, or installer | `dotnet build TuitionDesk.slnx`, then the relevant Windows publish/smoke check; install and upgrade checks are manual |
| Cross-layer or release change | Run the full verification commands above |

Use the full command in the table for routine work; do not run `pnpm typecheck` separately when `pnpm build` already covers it.

## Feature change recipe

Use the closest existing vertical slice and modify only the layers required by the behavior. Example: class-first enrollment flows from `web/src/features/enrollments/components/AddStudentsDialog.tsx` through the feature API/query code, `src/TuitionDesk.Server/Endpoints/EnrollmentEndpoints.cs`, `EnrollStudentsHandler`, repository abstractions, and Infrastructure implementations. The domain owns enrollment state transitions; UI checks improve feedback but do not replace server checks.

For a feature task, write down a brief contract before editing: user-visible outcome, relevant `FR-*`/`BR-*` IDs from `docs/product.md`, invalid/conflict cases, and observable acceptance checks. Treat API response shapes and error codes as contracts: inspect the endpoint and Application DTO, then extend the existing server integration tests when changing them. TypeScript API types are maintained separately, so update them alongside the backend contract. Do not introduce a generated client or a new abstraction just to avoid this small explicit mapping.

## Audit findings

### Dependencies and configuration

- **Confirmed:** .NET package versions are centrally pinned in `Directory.Packages.props`; the SDK is pinned in `global.json`; frontend direct dependencies are exact versions and `web/pnpm-lock.yaml` is checked in.
- **Confirmed:** NuGet has a normal nuget.org source and a separate offline config using `.nuget/feed` and a local package cache. CI uses the normal source; the offline config is an opt-in developer path.
- **No defect identified in this review:** No unpinned direct dependency or credential-bearing configuration was found. This was a repository configuration review, not a current vulnerability/advisory scan.

### Tests and highest-value gaps

- **Confirmed:** Backend tests cover domain/API behavior for students, classes, enrollments, and token middleware. Domain tests are currently grouped in `tests/TuitionDesk.Domain.Tests/StudentTests.cs`, including class and enrollment rules.
- **Confirmed:** The only frontend test files are student and class schema tests; they do not exercise page, dialog, query-cache, or API-error behavior.
- **Priority 1:** Add focused infrastructure tests for database migration, integrity failure, backup creation, daily backup, and retention. These are data-recovery paths with high impact and no current test files.
- **Priority 2:** Add an integration case for archiving a class withdrawing its active enrollments, complementing the existing student-archive case.
- **Priority 3:** Add a small number of frontend behavior tests for critical mutation flows and server errors before expanding UI test coverage generally.
- **Maintenance suggestion:** Split the mixed domain test file by aggregate when it next needs substantial editing; avoid a file-move-only cleanup.

### Architecture

- **Reviewed flow:** Enrollment is exposed through the feature API, mapped by a Server endpoint, handled in Application, and persisted through Infrastructure abstractions. The domain owns enrollment transitions; the UI mirrors capacity and validation feedback.
- **Finding:** The sampled dependency direction matches `docs/architecture.md`; no confirmed boundary violation or unnecessary abstraction was found in the reviewed flows.
- **Guidance:** Preserve the explicit handlers and repositories already used. Do not add a mediator, generic repository, or extra layer for consistency alone; revisit only when a concrete requirement justifies it.

### Release readiness and documentation mismatches

- **Confirmed:** The roadmap and README record that local install/launch/uninstall was checked, while clean-VM and upgrade-over testing remain outstanding. Ignored files under `artifacts/` are local build outputs, not evidence that those release checks passed.
- **Confirmed:** `web/package.json` has no `pnpm lint` script; `pnpm build` already runs `tsc --noEmit`, so separate typecheck entries are redundant. The release checklist and CI milestone are aligned with the actual commands.
- **Confirmed:** The roadmap expects a `CHANGELOG.md`, but none is present in the repository. The installer script defines version `0.1.0` directly in `build/installer.iss`.
- **Priority 1:** Complete clean-VM install and upgrade-over checks and record their results before claiming release readiness.
- **Priority 2:** Add a changelog when preparing a release rather than as part of unrelated feature work.

## Agent handoff

Before implementation, read the relevant product rule and architecture flow, inspect adjacent code and tests, and make the smallest complete change. For behavior changes, add tests at the narrowest meaningful layer and run the matching CI commands. Keep the product and architecture docs authoritative; update the roadmap only when delivery status changes. Report checks actually run and distinguish confirmed defects from recommendations.

## Latest agent task

Updated agent workflow with a concise task contract, vertical-slice example, focused check matrix, and a rule to update authoritative Markdown or replace this handoff after every coding task. The roadmap quality checklist now matches the actual CI scripts. No runtime behavior changed.
