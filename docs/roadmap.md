# TuitionDesk — Roadmap

| Field | Value |
| --- | --- |
| Document | Delivery roadmap |
| Status | Draft v0.1 — for review |
| Date | 2026-11-08 |
| Scope | v0.1 MVP, plus v0.2/v1.0 direction |
| Related | [Product requirements](product.md) · [Architecture](architecture.md) |

**Implementation status (2026-11-09):** M0–M3 complete. The walking skeleton, students registry,
classes registry, and allocation all work end to end, with 21 backend tests and 10 frontend tests
passing. M4 is nearly complete: the self-contained `win-x64` publish and Inno Setup installer were
built and verified with a silent per-user install, launch, and uninstall (data retained). Remaining:
clean-VM install, upgrade-over test, and the `v0.1.0` tag.

**Planning assumptions**

- One developer who knows React and .NET, working roughly 6 focused hours per day.
- No external design or QA resource; the same person tests and packages.
- Estimates include implementation, tests, and self-review — not waiting time.
- Target: a testable, installable v0.1 in **about three working weeks** (~15 dev days), with buffer.

---

## 1. Delivery strategy

1. **Walking skeleton first.** Prove the riskiest parts — WPF + WebView2 + embedded Kestrel + published assets + installer — before building features. If that combination fails, the whole plan changes; find out in week 1, not week 3.
2. **Vertical slices.** Students, then classes, then allocation, each taken end-to-end (domain → API → UI → tests) rather than layer-by-layer. Every milestone ends with something clickable.
3. **Release every milestone.** M0 produces a runnable app; each later milestone produces a build someone could use.
4. **Fix the rules once.** Business rules live in the domain and are covered by tests; the UI only mirrors them for fast feedback.
5. **Ship the boring hardening.** Backups, migrations, logs, and a clean install are part of the MVP, not an afterthought.
6. **No gold-plating.** Anything on the product's "cut lines" list is optional until the core flows are done.

---

## 2. Timeline at a glance

| Milestone | Theme | Estimate | Cumulative |
| --- | --- | --- | --- |
| **M0** | Walking skeleton and packaging spike | 2 days | Day 2 |
| **M1** | Students registry, end to end | 3 days | Day 5 |
| **M2** | Classes registry, end to end | 2.5 days | Day 7.5 |
| **M3** | Allocation (enrol / withdraw / rosters) | 2.5 days | Day 10 |
| **M4** | Hardening, installer, release candidate | 3 days | Day 13 |
| Buffer | Fixes from clean-VM testing, polish | 2 days | Day 15 |

Calendar shape (5-day weeks):

| Week | Days | Work |
| --- | --- | --- |
| 1 | 1–5 | M0, then M1 |
| 2 | 6–10 | Finish M1, M2, M3 |
| 3 | 11–15 | M4, clean-VM test, release, buffer |

If the schedule slips, use the product's cut lines in order: bulk enrol → class codes/duplicate warning → history toggle → class counts → student duplicate warning → fee fields. The MVP promise (students, classes, allocation) is non-negotiable.

---

## 3. Milestones

### M0 — Walking skeleton (2 days)

**Goal:** an installed `.exe` opens a window that renders React, calls the embedded .NET API, and reads/writes a SQLite file — with the same code path that will ship.

| # | Task | Notes |
| --- | --- | --- |
| 0.1 | Create solution, projects, `Directory.Build.props`, `Directory.Packages.props`, `global.json` | Domain/Application/Infrastructure/Server/Desktop + test projects |
| 0.2 | `AppDbContext` + first migration (`Students`, `Classes`, `Enrollments`, `Counters`) + pragmas | Verify the filtered unique index generates |
| 0.3 | `ServerHost.StartAsync` (bind `127.0.0.1:0`), `/api/health`, DI, exception handler shell | Read the assigned URL after start |
| 0.4 | WPF window + WebView2: environment, user-data folder, token/config injection, startup/shutdown | `AddScriptToExecuteOnDocumentCreatedAsync` |
| 0.5 | Vite app: React, TS, Router, TanStack Query, Tailwind; dev proxy to `:5179`; Health card | Prove fetch + ProblemDetails plumbing |
| 0.6 | Publish script copies `web/dist` into the published `wwwroot`; SPA fallback + `/api/*` 404 | Single origin |
| 0.7 | `publish.ps1` + minimal `installer.iss`; install and launch on a clean VM | **Go/no-go spike** |
| 0.8 | Single-instance mutex; data directory resolution; Serilog file sink | |

**Exit criteria**

- `dotnet run` + `pnpm dev` shows the React app reading `/api/health` with TanStack Query.
- The **published, installed** app opens its own window and shows the same health data with no browser chrome.
- The database file appears under `%LOCALAPPDATA%\TuitionDesk\data`, migrations applied, log file written.
- A second launch reports that the app is already running.
- Installer installs and uninstalls per-user without elevation.
- Go/no-go note recorded: WebView2 presence, publish size, startup time.

**Demo:** double-click the installed app; it opens, shows "API v0.1.0 · database v1", and closes cleanly.

---

### M1 — Students registry (3 days)

**Goal:** create, find, edit, archive, and restore students entirely through the UI.

| # | Task | Notes |
| --- | --- | --- |
| 1.1 | Domain: `Student`, `StudentStatus`, validation, `Archive`/`Restore`, `StudentCode` formatting | Unit tests for BR-01/02/04/05 |
| 1.2 | Application: `CreateStudent`, `UpdateStudent`, `ArchiveStudent`, `RestoreStudent`, `GetStudent`, `SearchStudents` + validators | Handlers, DTOs, manual mapping |
| 1.3 | Infrastructure: `StudentRepository`, `StudentQueries` (search/filter/sort/page, class counts), `CounterService`, `SystemClock`, `UnitOfWork` | Projection query; real SQLite tests |
| 1.4 | API: student endpoints, `ProblemDetails`, validation filter, archive/restore (enrolment cascade completed in M3) | Integration tests: CRUD + 400/404/409 |
| 1.5 | Web: students list (search, status filter, sort, pagination in URL), form, detail, archive/restore, duplicate warning | Zod schema, RHF, error mapping |
| 1.6 | Web: shared `DataTable`, `EmptyState`, `PageHeader`, `ConfirmDialog`, toasts, skeletons | Reused by M2 |
| 1.7 | About panel v1 (version, schema, folders, copy diagnostics) | Needed for support early |
| 1.8 | Tests: domain, handlers, API integration; frontend tests for schema + list params | |

**Exit criteria**

- Student CRUD and archive/restore work from the installed app.
- Search by partial name, code, or phone returns correct results; filters and paging survive Back.
- Validation errors appear per field; the form retains values; first invalid field is focused.
- Archive/restore works; the enrolment cascade is finished in M3.
- Empty, loading, and error states exist on list and detail; no console errors.
- All product rules for students are covered by at least one automated test.

**Demo:** register 10 students, search by phone, edit one, archive one, restore it.

---

### M2 — Classes registry (2.5 days)

**Goal:** create, find, edit, archive, and restore classes, with fee and capacity visible.

| # | Task | Notes |
| --- | --- | --- |
| 2.1 | Domain: `Class`, `ClassStatus`, fee/currency/capacity/schedule/date rules, archive/restore | Unit tests for BR-06–BR-08 |
| 2.2 | Application: class commands/queries + validators; lookups query (subjects, grade levels) | |
| 2.3 | Infrastructure: `ClassRepository`, `ClassQueries` (search/detail with active counts), `LookupQueries` | Correlated count in one query |
| 2.4 | API: class endpoints, archive/restore (enrolment cascade completed in M3), lookups endpoint | Integration tests |
| 2.5 | Web: classes list (counts, `Full` badge), form (money input, currency, capacity, schedule), detail shell | |
| 2.6 | Web: subject/grade comboboxes fed by `useLookups` with free-text entry | |
| 2.7 | Tests: domain, handlers, API, form schema | |

**Exit criteria**

- Class CRUD and archive/restore work from the installed app.
- Fee displays correctly from minor units in LKR; capacity badge shows `n / capacity`, `Full`, or `Unlimited`.
- Date validation (end ≥ start) is enforced on client and server.
- Class list shows active enrolled counts (0 until M3).
- Subjects and grade levels are suggested from existing data but free text is accepted.
- Empty/loading/error states present.

**Demo:** create three classes with different schedules and fees; search by subject; archive one.

---

### M3 — Allocation (2.5 days)

**Goal:** connect students and classes; rosters and counts stay correct everywhere.

| # | Task | Notes |
| --- | --- | --- |
| 3.1 | Domain: `Enrollment`, transitions, withdrawal, date rules | Unit tests for BR-10–BR-14 |
| 3.2 | Application: `EnrollStudent`, `EnrollStudents` (bulk, all-or-nothing), `WithdrawEnrollment`, `GetClassRoster` | Duplicate + archived + capacity checks |
| 3.3 | Infrastructure: `EnrollmentRepository`, roster query (paged, searchable, active/withdrawn), counts | Partial unique index catch → 409 |
| 3.4 | API: enrol, bulk enrol, withdraw, roster endpoints | Integration tests incl. race on unique index |
| 3.5 | Web: `AddStudentsDialog` on class detail (search, exclude enrolled/archived, multi-select) | Bulk if time allows |
| 3.6 | Web: `AddToClassDialog` on student detail; classes section with history | |
| 3.7 | Web: roster table, withdraw confirmation, counts on class list; invalidation matrix wired | Verify no stale counts |
| 3.8 | Tests: enrolment rules, bulk atomicity, roster filters, invalidation behaviour | |
| 3.9 | Wire archive/restore cascades: student or class archive withdraws active enrolments in one transaction, and the confirmation names affected classes | Completes the M1/M2 placeholders |

**Exit criteria**

- Enrol from both directions; the other side reflects it without a manual refresh.
- Duplicate active enrolment is blocked with a readable 409; no duplicate row exists.
- Withdrawal removes the student from the active roster and decrements the count; history is retained and viewable.
- Bulk enrol either adds everyone or nobody, and reports conflicts.
- Capacity warning appears and can be acknowledged; the server permits the enrolment.
- Archiving a class/student withdraws active enrolments exactly once.
- Student detail shows active and past classes.

**Demo:** build a class of 12 from 30 students, withdraw one, re-enrol them, verify counts and history.

---

### M4 — Hardening and release (3 days)

**Goal:** a release candidate that survives a clean machine, a crash, and an upgrade.

| # | Task | Notes |
| --- | --- | --- |
| 4.1 | Backup service: daily + pre-migration + manual; retention 10; verify `BackupDatabase` output | Restore instructions in About/README |
| 4.2 | Startup integrity check + corrupt-DB dialog; migration-failure path with backup pointer | Do not auto-recreate |
| 4.3 | Global error handling: WPF startup failures, WebView2 `ProcessFailed`, React error boundary, 500 retry | Native dialogs with log path |
| 4.4 | Polish: window size memory, dirty-form guard, keyboard focus, empty states, wording pass | SHOULD items from product |
| 4.5 | Accessibility pass: labels, focus rings, dialog focus restore, table semantics, contrast | Keyboard-only run-through |
| 4.6 | Installer finalisation: WebView2 detection/bootstrapper, uninstall data choice, icon, version metadata, Start Menu | |
| 4.7 | Clean-VM test: no .NET installed, network disabled, standard user | Full acceptance script |
| 4.8 | Upgrade test: install v0.1.0, add data, install v0.1.1 build, verify migration | Requires two builds |
| 4.9 | Release docs: README (install, data location, backup/restore, known issues), CHANGELOG, tag | |
| 4.10 | Optional: CI workflow (`build`, `test`, `lint`, `typecheck`) | |

**Exit criteria**

- The full acceptance script in [product.md §14](product.md#14-mvp-acceptance-script) passes on a clean Windows 11 VM with the network disabled.
- Killing the process with Task Manager loses no committed data; relaunch recovers.
- A backup can be located and restored using only the documented steps.
- Installer runs without admin rights and uninstalls cleanly while keeping data by default.
- No unhandled exceptions in the log during the smoke run.
- v0.1.0 tagged and release notes written.

**Demo:** hand the installer to someone who has never seen the app; they install and complete the acceptance script.

---

## 4. v0.1 release checklist

### Functional

- [ ] Students: create, list/search/filter/sort/page, edit, archive, restore.
- [ ] Classes: create, list/search/filter/page, edit, archive, restore.
- [ ] Allocation: enrol from class and student, bulk enrol, withdraw, roster search, history.
- [ ] Counts (class enrolled, student class count) update after every mutation.
- [ ] Duplicate warning (student) and duplicate/conflict errors (enrolment) behave correctly.
- [ ] Capacity warning and confirmation work; over-capacity is stored.
- [ ] Archived records are read-only, hidden by default, blocked from allocation.
- [ ] Fee and currency display correctly from minor units.

### Quality

- [ ] Domain rules have unit tests; handlers have SQLite-backed tests; API flows have integration tests.
- [ ] Validation messages are specific and appear per field.
- [ ] Loading, empty, and error states exist on every screen.
- [ ] Dirty-form warning works.
- [ ] Keyboard-only run-through of the core flows succeeds.
- [ ] No console errors or unhandled promise rejections in a normal session.
- [ ] `dotnet build`, `dotnet test`, `pnpm lint`, `pnpm typecheck`, `pnpm test` pass.

### Data and operations

- [ ] Migrations apply from an empty database and from the previous release.
- [ ] Pre-migration backup is created and named correctly.
- [ ] Daily backup runs; retention keeps 10; a restore has been tested.
- [ ] Corrupt database produces a helpful dialog, not a crash.
- [ ] Logs contain request IDs and no full PII.

### Packaging

- [ ] Self-contained win-x64 build runs on a machine with no .NET SDK/runtime.
- [ ] Installer is per-user, no UAC, Start Menu shortcut, correct icon/version.
- [ ] WebView2 detection works; missing-runtime path tested (simulated).
- [ ] Uninstall removes binaries and keeps data by default.
- [ ] Installer and app are scanned by Windows Defender without a false positive.
- [ ] Installer size and startup time recorded.

### Documentation

- [ ] README: what it is, install, first run, where data lives, backup/restore, known issues.
- [ ] CHANGELOG for v0.1.0.
- [ ] Product/architecture docs updated for any decisions made during the build.
- [ ] Release notes written in plain language.

---

## 5. Definition of done

A task is done when:

1. Code is implemented in the correct layer with dependencies pointing inward.
2. Automated tests cover the behaviour and each business rule it implements.
3. Validation exists on both client and server, with field-level messages.
4. Loading, empty, and error states are handled; failures are not silent.
5. Keyboard and screen-reader basics work; focus is managed.
6. Logs and error messages are useful without a debugger.
7. Docs (product/architecture/README) are updated if behaviour or decisions changed.
8. The packaged app still starts and the core flows still work.

---

## 6. Test and QA plan

### Automated

| Suite | Runs on | Gate |
| --- | --- | --- |
| `dotnet test` | Every change | Must pass |
| `pnpm typecheck`, `pnpm lint` | Every change | Must pass |
| `pnpm test` | Every change | Must pass |
| Optional CI workflow | Push/PR | Build + all tests |

### Manual smoke script (clean VM, network off)

1. Install as a standard user; no UAC prompt; Start Menu shortcut present.
2. Launch; window opens ≤ 3 s; no error dialog; data folder created.
3. Create a class with schedule, fee, capacity; verify it appears with `0 / n`.
4. Create three students; verify codes are sequential and unique.
5. Submit an invalid student (blank phone, bad email); field errors appear; values retained.
6. Search by a partial name and a partial phone; both find the student.
7. Enrol five students; count becomes `5 / n` without reload.
8. Re-enrol one; conflict message; count unchanged.
9. Withdraw one; count drops; the student's detail lists the class as past with a withdrawal date.
10. Open a student; class appears with enrolment date.
11. Archive the student; confirmation names the class; count drops; student hidden under Active filter.
12. Restore the student; record is editable; not re-enrolled.
13. Archive the class; it disappears from the active list and its enrolments are withdrawn.
14. Kill the process with Task Manager; relaunch; all data present.
15. Inspect `data/backups`; at least one backup exists; copy it aside.
16. Install the next build over the top; data intact; schema version current.
17. Uninstall; binaries removed; data retained (or removed only when chosen).
18. Restore the copied backup over the database; app opens with that data.

### Exploratory focus areas

- Very long names, Unicode names, long notes.
- Rapid double-click on Save / Enrol (no duplicates).
- Enrol while the class is archived in another view (stale UI) → server rejects; UI explains.
- Many students (500+) in a class for roster paging.
- Changing filters while a request is in flight (stale responses, abort behaviour).

---

## 7. Post-MVP backlog

Ordered by expected value. "Now" is the first release after v0.1.

### Now — v0.2 (fees and attendance foundations)

| Item | Why | Notes |
| --- | --- | --- |
| CSV import of students | Day-one migration from spreadsheets; highest user value | Map columns, preview, duplicate handling |
| CSV export (students, rosters) | Backup and sharing | |
| Fee tracking per student/class | Natural next question after "who is in my class" | Charges + payments, monthly period, balance; no invoicing yet |
| Attendance marking per class session | Tutor's second-biggest need | Session/date + present/absent per enrolment |
| Print/PDF class roster | Practical for the classroom | |
| Structured class schedule slots | Better lists and future calendar | `ClassSlot` table |
| Global search | Faster navigation at scale | Revisit command palette |
| Student photo / custom fields | Personalisation | Optional |

### Next — v0.3

| Item | Notes |
| --- | --- |
| Payments ledger, receipts, arrears report | Builds on v0.2 charges |
| Attendance reports and monthly summaries | |
| Teacher/tutor entity and per-class assignment | Single-tutor default retained |
| Terms/sessions with term-scoped enrolments | |
| Waitlists and capacity automation | |
| Import/export full backup archive | Zip with database + attachments |
| Auto-backup to a user-chosen folder (e.g. OneDrive) | Still local-first |

### Later — v1.0 and beyond

| Item | Notes |
| --- | --- |
| Auto-update (Velopack) | Removes manual reinstall |
| Code signing | Removes SmartScreen friction |
| Multi-user on one machine or LAN | Needs auth, concurrency, server DB evaluation |
| Cloud sync / multi-device | The big architectural step; GUID keys and the API help |
| Parent/student portal | Separate app or hosted service |
| Localisation (Sinhala/Tamil) | UI strings externalised first |
| Theming and density options | |
| SMS/email reminders | Requires third-party services and consent |
| Mobile companion | Out of the desktop scope |

---

## 8. Risks to the schedule

| Risk | Early warning | Response |
| --- | --- | --- |
| WebView2/Kestrel packaging burns time in M0 | Installer or publish issues on the clean VM | Time-box the spike to 2 days; if blocked, fall back to a local-server + thin shell approach and record the decision |
| Scope creep (fees, attendance, import) | New fields appearing in M1/M2 | Point at product non-goals; park in v0.2 backlog |
| Underestimated UI polish | M1 takes > 4 days | Cut SHOULD items per product cut lines; keep flows functional |
| Migration or backup bugs found late | M4 surprises | Pull backup/migration tests into M1 as soon as the schema exists |
| Clean-VM environment differs (WebView2 missing, AV block) | Only tested on the dev machine | Test on a clean VM at end of M0 and M4, not just at release |
| Single developer unavailable | No commits for several days | Keep docs current; small vertical slices mean a pause loses little context |

---

## 9. Versioning and release policy

- **SemVer:** `MAJOR.MINOR.PATCH`. v0.x may change internals freely; the database schema still migrates forward.
- **Database:** forward-only EF migrations, each with a pre-migration backup. No destructive migration without an explicit data-mapping step.
- **Release cadence:** v0.1.0 at M4; patch releases for defects; v0.2 when the first post-MVP feature set lands.
- **Release artefacts:** `TuitionDesk-Setup-{version}.exe`, `CHANGELOG.md`, release notes, tagged commit.
- **Support:** About panel provides version, schema version, log/data folders, and copy-diagnostics so a user can report an issue without a developer on site.

---

## 10. Decision log

| Date | Decision | Reason | Owner |
| --- | --- | --- | --- |
| 2026-11-08 | React + .NET + SQLite, Clean Architecture, TanStack Query | Declared stack; local-first desktop | Product owner |
| 2026-11-08 | WPF + WebView2 single process with in-process minimal API | Simplest way to ship React + .NET as one installable desktop app | Engineering |
| 2026-11-08 | Many-to-many students/classes via Enrolment | Real tuition practice; see A-02 | Engineering |
| 2026-11-08 | Archive instead of delete; capacity advisory | Preserve history; tutor judgement | Product |
| 2026-11-08 | Fee captured but not billed in v0.1 | Keeps MVP focused on the three registries | Product |
| 2026-11-08 | Inno Setup per-user installer, self-contained publish | No admin rights, no .NET prerequisite | Engineering |

---

## 11. Next actions

1. Install Inno Setup 6 and run `build/publish.ps1` to produce `TuitionDesk-Setup-0.1.0.exe`.
2. Test the installer on a clean Windows 11 VM with the network disconnected: per-user install,
   student/class/allocation flows, backup folder, kill-and-relaunch durability.
3. Install over the previous build to verify migration and data retention; test uninstall keeps
   the data folder.
4. Tag `v0.1.0`, write release notes, and update the changelog.
5. Confirm the product assumptions in [product.md §12](product.md#12-assumptions), especially
   many-classes-per-student and CSV import for the next release.

*The per-user install, launch, and uninstall-with-data-retained path has been verified on the
development machine. The clean-VM and upgrade-over checks above are still outstanding.*
