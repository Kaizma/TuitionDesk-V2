# TuitionDesk — Product Requirements

| Field | Value |
| --- | --- |
| Document | Product requirements |
| Status | Draft v0.1 — for review |
| Date | 2026-11-08 |
| Scope | MVP (v0.1) |
| Audience | Product owner, engineering |
| Related | [Architecture](architecture.md) · [Roadmap](roadmap.md) |

**Conventions used in this document**

- **MUST** = required for v0.1. **SHOULD** = include if the schedule holds. **COULD** = backlog. **WON'T** = explicitly out of v0.1.
- IDs (`FR-S-*`, `BR-*`, `NFR-*`, `A-*`, `OQ-*`) are stable; tests and the roadmap refer back to them.

---

## 1. Summary

TuitionDesk is a local-first Windows desktop application for independent tutors and small tuition centres. It replaces the notebook-and-spreadsheet workflow with one local database covering three things:

1. **Who the students are** (a student registry).
2. **What classes are running** (a class registry).
3. **Who is in which class** (allocation / enrolment).

It installs without admin rights, opens in seconds, and works with no internet connection.

**MVP promise:** *"Open the app, find any student in seconds, see exactly which classes they attend, and see exactly who is in each class."*

---

## 2. Problem and opportunity

The target user runs a tuition practice from a laptop. Today their records live in some mix of:

- an exercise book or a spreadsheet per term,
- WhatsApp chats for contact details,
- memory for who joined which class.

That breaks down in predictable ways:

| Pain | Consequence | What TuitionDesk changes |
| --- | --- | --- |
| Student details are duplicated across a book, a spreadsheet, and a phone | Wrong number called; stale info | One student record, edited in one place |
| "Who is in my Saturday Maths class?" needs manual counting | Roster decisions are guesswork | A live roster with an enrolled count |
| "Which classes does this student attend?" is scattered | Front-desk questions take minutes | Student detail lists every class |
| Spreadsheets get corrupted or emailed around | Data loss, version confusion | Single local database with automatic backups |
| Cloud tools need accounts, subscriptions, and internet | Cost and friction for a one-person business | Offline install, no account, no subscription |

The opportunity is deliberately small: an excellent tool for **one tutor on one Windows machine**, extensible later to fees, attendance, and reporting without a rewrite.

---

## 3. Goals, success metrics, and non-goals

### 3.1 Goals

| ID | Goal |
| --- | --- |
| G-1 | Be the single source of truth for students, classes, and enrolment. |
| G-2 | Answer "who is in this class?" and "what is this student taking?" in under 10 seconds. |
| G-3 | Install and run offline on a normal Windows laptop with zero setup beyond the installer. |
| G-4 | Provide a foundation where fees and attendance can be added later without reshaping the data model. |
| G-5 | Stay small enough for one developer to build, test, package, and maintain. |

### 3.2 Success metrics for v0.1

| Metric | Target |
| --- | --- |
| Register 100 students and 20 classes through the UI without data loss | Pass on release check |
| Find a student from a partial name or phone | ≤ 3 keystrokes + Enter |
| Enrol or withdraw a student | ≤ 3 clicks from the class page |
| Cold start on a 4-core / 8 GB / SSD laptop | ≤ 3 seconds |
| Reopen the app after a crash | All committed data present |
| Upgrade v0.1.0 → v0.1.1 | Existing data preserved, no manual steps |

### 3.3 Non-goals for v0.1

Explicitly **not** in the MVP:

- Fee invoicing, payment tracking, receipts, arrears.
- Attendance registers, marking, or reports.
- Timetables, calendar integration, room booking.
- Teachers/tutors as a separate entity or login.
- Multi-user access, roles, permissions, cloud sync, or multi-device use.
- Email, SMS, or WhatsApp integration.
- Parent/student self-service portals.
- CSV import/export, PDF reports, printing.
- Mobile apps or a browser-hosted version.
- Auto-update.
- Localisation beyond English.

These are sequenced in the [roadmap](roadmap.md).

---

## 4. Users and workflows

### 4.1 Users

| ID | Role | Description | In v0.1? |
| --- | --- | --- | --- |
| U-1 | **Tutor / owner-operator** (primary) | Runs the practice, teaches most classes, does the admin. Uses a Windows laptop at home or at the class venue. Moderate computer literacy; expects desktop-app behaviour. | Yes — the only user |
| U-2 | **Centre administrator / assistant** | Takes enquiries and updates records for a larger centre. | No — no separate account, no concurrency |
| U-3 | **Student or parent** (data subject) | Appears in the registry, never logs in. Provides name, contact, guardian details. | No access |

**Primary persona (U-1): "The tutor".** Runs 5–20 classes a week for 30–150 students. Knows every student by name but not every phone number. Works in short bursts: between lessons, in the evening, at the start of a term. Motivation is low for data entry; the app must be faster than the notebook for the tasks below.

### 4.2 Jobs to be done

| ID | When… | I want to… | So that… |
| --- | --- | --- | --- |
| JTBD-1 | A new student enquires | Add them once with contact details | I can call them back and add them to a class |
| JTBD-2 | A new term starts | Set up the classes I'm running | I have a clean list with schedule and fee |
| JTBD-3 | A student joins | Put them in the right class(es) | The roster and the student record agree |
| JTBD-4 | Before a lesson | Check who is in the class | I know the headcount and who to expect |
| JTBD-5 | A student asks what they're taking | Open their record | I can answer without searching three places |
| JTBD-6 | A student leaves | Withdraw them from classes / archive the record | History stays accurate and counts stay right |

### 4.3 Workflows

#### WF-1 — Register a new student

- **Trigger:** enquiry by phone, WhatsApp, or walk-in.
- **Steps:** Students → **New student** → name + phone (required) → optional email, guardian, school, grade, date of birth, joined date, notes → Save.
- **Outcome:** student exists with an auto-generated code (`STU-00001`), status **Active**, appears in the list.
- **Rules:** BR-01 to BR-05.

#### WF-2 — Set up a class

- **Trigger:** start of term, or a new class being added mid-term.
- **Steps:** Classes → **New class** → name, subject, grade/level, schedule text, start date, monthly fee, optional capacity → Save.
- **Outcome:** class exists with code (`CLS-0001`), status **Active**, enrolled count 0.
- **Rules:** BR-06 to BR-09.

#### WF-3 — Allocate students to a class

- **Trigger:** student joins a class.
- **Steps (class-first):** open the class → **Add students** → search by name/phone → select one or more non-enrolled active students → **Add**. Alternatively student-first: open a student → **Add to class** → search active classes → **Add**.
- **Outcome:** active enrolment rows exist; roster count updates immediately; student detail shows the class.
- **Rules:** BR-10 to BR-14.

#### WF-4 — Review a class roster

- **Trigger:** before a lesson, or checking capacity.
- **Steps:** Classes → open class → roster table (name, code, phone, grade, enrolled on).
- **Outcome:** complete, searchable list of active students, plus a withdrawn-history toggle.
- **Rules:** BR-15.

#### WF-5 — Review a student's classes

- **Trigger:** student asks, or a fee/attendance conversation.
- **Steps:** Students → search → open student → classes section lists active and past enrolments.
- **Outcome:** one screen with contact details and class history.

#### WF-6 — Withdraw a student from a class

- **Trigger:** student stops attending one class.
- **Steps:** class roster → row action **Withdraw** → confirm.
- **Outcome:** enrolment becomes **Withdrawn** with a date; it disappears from the active roster and count but remains in history. The student record is untouched.
- **Rules:** BR-13.

#### WF-7 — Archive a student or class at end of term

- **Trigger:** student leaves the practice, or a class finishes.
- **Steps:** open record → **Archive** → confirmation explains side effects.
- **Outcome:** status **Archived**, hidden from default lists and allocation search, active enrolments automatically withdrawn. Restorable.
- **Rules:** BR-04, BR-08.

#### WF-8 — Correct a mistake

- **Trigger:** wrong phone number, misspelled name, wrong fee.
- **Steps:** open the record → **Edit** → Save.
- **Outcome:** record updated with an updated timestamp; lists refresh.
- **Rules:** BR-16.

---

## 5. Core domain entities

The product model has three entities. Everything else is an attribute or a reference value.

| Entity | Definition | Key attributes | Lifecycle |
| --- | --- | --- | --- |
| **Student** | A person the tutor teaches or has taught. | Code, full name, phone, email, guardian name/phone, date of birth, school, grade level, joined date, notes | Active ⇄ Archived |
| **Class** | A recurring teaching group for a subject at a level, with a schedule and a fee. Not a single lesson. | Code, name, subject, grade level, schedule text, start/end date, monthly fee + currency, capacity, notes | Active ⇄ Archived |
| **Enrolment** | The link that says a student attends a class. Carries its own dates and status. | Student, class, enrolled on, status, withdrawn on, notes | Active → Withdrawn |

**Relationships**

```text
Student 1 ──── * Enrolment * ──── 1 Class
```

- A student can attend **many** classes (A-02).
- A class has **many** students.
- A student can be enrolled in the same class **at most once while active**; withdrawing and re-joining creates a new enrolment row so history is preserved (BR-12).

**Reference values (not entities in v0.1)**

- **Subject** — free text (`Mathematics`, `Physics`, `ICT`), suggested from existing values.
- **Grade level** — free text (`Grade 10`, `Year 11`, `A/L`, `Adult`), suggested from existing values.
- **Schedule** — free text in v0.1 (`Wed 4–6pm, Sat 9–11am`). A structured timetable is deliberately deferred.
- **Money** — monthly fee stored as integer minor units plus ISO currency code; never floating point.

**Why not a Teacher entity, Course, or Session?** The MVP serves a single tutor, so a teacher reference adds a table, a screen, and a filter with no user value today. "Class" already means the recurring group; individual lessons and attendance are a later feature that will reference the class. This is recorded as assumption A-07 and revisited in the roadmap.

---

## 6. Functional requirements

### 6.1 Students

| ID | Pri | Requirement | Acceptance notes |
| --- | --- | --- | --- |
| FR-S-01 | MUST | Create a student with required full name and phone; optional email, guardian name/phone, date of birth, school, grade level, joined date, notes. | Save returns the new record; code auto-generated; duplicate name+phone warns (FR-S-06). |
| FR-S-02 | MUST | List students with text search (name, code, phone), status filter (Active default, Archived, All), sort (name, code), and server-side pagination. | Search is case-insensitive for ASCII; list reflects saved changes without a manual refresh. |
| FR-S-03 | MUST | Show a student detail page with all profile fields, active classes, and past enrolments. | Active classes show class name, subject, schedule, enrolled date; history shows withdrawn date. |
| FR-S-04 | MUST | Edit an active student's fields. | Archived students are read-only until restored (BR-19). |
| FR-S-05 | MUST | Archive and restore a student. | Archive confirmation states that active enrolments will be withdrawn. |
| FR-S-06 | MUST | Warn (not block) when a new or edited student matches an existing student's name and phone. | Warning dialog offers "Save anyway" and "Open existing". |
| FR-S-07 | MUST | Generate a unique, human-readable student code. | Format `STU-00001`; never reused; shown in list and detail; not user-editable. |
| FR-S-08 | MUST | Validate all inputs on both client and server with specific, field-level messages. | Invalid save keeps entered values; first invalid field receives focus. |
| FR-S-09 | SHOULD | Show the number of active classes per student in the list. | Count stays correct after enrol/withdraw/archive. |
| FR-S-10 | SHOULD | Allow sorting by most recently updated. | Useful for "what did I just change?". |
| FR-S-11 | COULD | Export the student list to CSV. | Deferred; see roadmap. |
| FR-S-12 | WON'T | Bulk import students from a spreadsheet. | Highest-value fast follow after v0.1. |

### 6.2 Classes

| ID | Pri | Requirement | Acceptance notes |
| --- | --- | --- | --- |
| FR-C-01 | MUST | Create a class with name, subject, start date, monthly fee (≥ 0), and currency; optional grade level, schedule text, end date, capacity, notes. | End date must be ≥ start date; capacity, if set, must be ≥ 1. |
| FR-C-02 | MUST | List classes with text search (name, subject, code), status filter, and server-side pagination. | Each row shows active enrolled count and capacity. |
| FR-C-03 | MUST | Show a class detail page with class facts, enrolled/capacity indicator, and a searchable roster. | Roster shows active students by default with a toggle for withdrawn. |
| FR-C-04 | MUST | Edit an active class's fields. | Archived classes are read-only until restored. |
| FR-C-05 | MUST | Archive and restore a class. | Archiving withdraws active enrolments after confirmation (BR-08). |
| FR-C-06 | MUST | Show enrolled vs capacity, and a "Full" indicator when at or over capacity. | Capacity is advisory, not a hard block (BR-11). |
| FR-C-07 | SHOULD | Generate a unique class code. | Format `CLS-0001`; shown in list/detail. |
| FR-C-08 | MUST | Capture monthly fee and currency without implementing billing. | Fee formatted for display; stored as minor units. |
| FR-C-09 | SHOULD | Warn (not block) on a new class whose name, subject, and schedule match an existing active class. | Prevents accidental duplicates. |
| FR-C-10 | WON'T | Structured timetable with recurring day/time slots. | Free-text schedule now; structured slots in a later release. |

### 6.3 Allocation (enrolment)

| ID | Pri | Requirement | Acceptance notes |
| --- | --- | --- | --- |
| FR-A-01 | MUST | Add a student to a class from the class detail page. | Search excludes students already active in the class and archived students. |
| FR-A-02 | MUST | Add a class to a student from the student detail page. | Search excludes classes the student is already active in and archived classes. |
| FR-A-03 | MUST | Prevent duplicate active enrolment in the same class. | Server returns 409 with a readable message; UI shows it inline/toast. |
| FR-A-04 | MUST | Warn when enrolling beyond capacity and require explicit confirmation. | Server allows the action; capacity is the tutor's call (BR-11). |
| FR-A-05 | MUST | Withdraw an enrolment, recording the withdrawal date. | Row leaves active roster; count decrements; history retained. |
| FR-A-06 | MUST | Search or filter a class roster. | By name, code, or phone. |
| FR-A-07 | SHOULD | Show enrolment history (withdrawn students and dates) in class detail. | Toggle "Show withdrawn". |
| FR-A-08 | SHOULD | Enrol several students in one action. | All-or-nothing; conflicts reported before anything is written. |
| FR-A-09 | COULD | Transfer a student between classes in one action. | Withdraw + enrol; deferred. |
| FR-A-10 | WON'T | Waitlists and automatic promotion from waitlist. | Later release. |

### 6.4 Application shell and shared behaviour

| ID | Pri | Requirement | Acceptance notes |
| --- | --- | --- | --- |
| FR-G-01 | MUST | Persistent navigation between Students and Classes. | Current section is visually obvious; window title shows app name. |
| FR-G-02 | MUST | Loading, empty, and error states for every list and detail view. | Empty states explain the next action and offer a CTA. |
| FR-G-03 | MUST | Inline validation, toast feedback for successful actions, and confirmation for destructive actions. | No silent failures; error messages say what to do next. |
| FR-G-04 | MUST | An About panel showing app version, database schema version, data folder, log folder, and a copy-diagnostics action. | Supports the tutor without a developer. |
| FR-G-05 | SHOULD | Warn before leaving a dirty form. | Browser-native or in-app dialog. |
| FR-G-06 | COULD | Keyboard shortcuts (e.g. `/` to focus search, `n` for new). | Nice for power users. |
| FR-G-07 | WON'T | Global search or command palette across entities. | Per-page search is sufficient for MVP scale. |

---

## 7. Business rules

| ID | Rule |
| --- | --- |
| BR-01 | A student requires a full name (2–120 chars) and a phone number. Email is optional but must be valid when present. |
| BR-02 | Student codes are unique, system-generated, and immutable. |
| BR-03 | Phone numbers need not be unique (siblings may share one), but a matching name + phone pair triggers a duplicate warning. |
| BR-04 | Archiving a student sets status to Archived and automatically withdraws all their active enrolments, recording the withdrawal date. A confirmation names the affected classes. |
| BR-05 | Restoring a student makes the record Active and editable again; enrolments are **not** restored automatically. |
| BR-06 | A class requires a name, subject, start date, and non-negative monthly fee. End date, if set, cannot be before start date. |
| BR-07 | Class codes are unique, system-generated, and immutable. |
| BR-08 | Archiving a class sets status to Archived and automatically withdraws all active enrolments, recording the withdrawal date. |
| BR-09 | Archived students and classes cannot be enrolled; they must be restored first. |
| BR-10 | A student can have at most one **active** enrolment per class. Withdrawn enrolments do not block a new one. |
| BR-11 | Capacity is advisory. At or above capacity the UI warns and requires confirmation, but the server permits enrolment. |
| BR-12 | Withdrawal preserves the enrolment row (status Withdrawn + date). Re-joining creates a new enrolment row; history is never deleted. |
| BR-13 | Only active enrolments can be withdrawn. Withdrawing twice is rejected. |
| BR-14 | Enrolment date defaults to today and cannot be in the future; past dates are allowed for back-filling. |
| BR-15 | Rosters show active students by default, sorted by name; withdrawn students are available behind a filter. |
| BR-16 | Records with no history (never enrolled) may be hard-deleted only by a future admin tool; v0.1 exposes no delete action. |
| BR-17 | "Today" is the operating system's local date; all audit timestamps are stored in UTC. |
| BR-18 | Money is stored as integer minor units with an ISO 4217 currency code; the default currency for new classes is LKR and is editable per class. |
| BR-19 | Archived students and classes are read-only until restored; only Restore is available, and any edit attempt is rejected. |

> **Assumption A-08:** The user is one person on one machine, so no record locking, optimistic concurrency UI, or conflict resolution is needed. If a second user or device is added later, this becomes a design change (see roadmap).

---

## 8. Non-functional requirements

| ID | Requirement | Target / measure |
| --- | --- | --- |
| NFR-01 | Fully offline | No outbound network calls at runtime; works with Wi-Fi and Ethernet disabled. |
| NFR-02 | Fast start | Cold start ≤ 3 s, warm start ≤ 1.5 s on a 4-core / 8 GB / SSD reference laptop. |
| NFR-03 | Responsive lists | List and roster queries p95 ≤ 150 ms at 5,000 students, 200 classes, 20,000 enrolments. |
| NFR-04 | Durability | WAL journal, automatic pre-migration and daily backups; no committed data lost on process kill or power loss. |
| NFR-05 | Simple install | Per-user install, no admin rights, ≤ 5 minutes end to end, Start Menu shortcut, clean uninstall. |
| NFR-06 | Small footprint | Installed size ≤ 250 MB; typical working set ≤ 400 MB. |
| NFR-07 | Local-only security | API bound to loopback only; no inbound firewall rule required; local session token on API calls. |
| NFR-08 | Privacy | No telemetry, analytics, crash upload, or cloud storage. Data never leaves the machine. |
| NFR-09 | Accessibility | Core flows keyboard-operable; visible focus; labelled fields; WCAG 2.1 AA contrast; dialogs trap and restore focus. |
| NFR-10 | Maintainability | Layered solution with dependency rules enforced by project references; automated tests for domain and API flows; a clean build under 2 minutes. |
| NFR-11 | Compatibility | Windows 10 21H2+ and Windows 11, x64, 1366×768 minimum resolution. |
| NFR-12 | Recoverability | 10 rolling backups retained; a documented manual restore procedure; integrity check on startup. |
| NFR-13 | Upgrade safety | Migrations run on startup after an automatic backup; a failed migration stops startup and leaves the pre-migration backup available for restore. |
| NFR-14 | Diagnostics | Rolling local logs (7 days) with enough context to diagnose a failed save without a debugger. |
| NFR-15 | Data ownership | The user can locate the database and backups from the app and copy them as files. |

---

## 9. MVP feature set

### 9.1 In scope

| Area | Included |
| --- | --- |
| Students | Create, list/search/filter/sort/paginate, view, edit, archive/restore, duplicate warning, auto code, validation |
| Classes | Create, list/search/filter/paginate, view, edit, archive/restore, fee fields, capacity indicator, auto code |
| Allocation | Enrol from class or student, bulk enrol (SHOULD), withdraw, roster search, history (SHOULD), counts |
| Shell | Sidebar navigation, About/diagnostics, toasts, confirmations, empty/loading/error states, window state memory (SHOULD) |
| Data | Local SQLite database, migrations, automatic backups, integrity check |
| Packaging | Self-contained Windows build, per-user installer, Start Menu shortcut, uninstall |

### 9.2 Out of scope

Fees/payments, attendance, timetable, teachers, reports, printing, CSV import/export, cloud sync, multi-user, auth, auto-update, mobile, localisation. See the [roadmap](roadmap.md) for sequencing.

### 9.3 Cut lines if the schedule slips

In priority order, drop these first without endangering the MVP promise:

1. Bulk enrolment (FR-A-08) — single-add still works.
2. Class codes (FR-C-07) and duplicate class warning (FR-C-09).
3. Enrolment history toggle (FR-A-07) — data is still retained.
4. Student class counts in list (FR-S-09).
5. Duplicate student warning (FR-S-06) — validation still prevents bad data.
6. Fee and currency fields (FR-C-08) — add in v0.2; schema can gain columns later.

Do **not** cut: student/class CRUD, enrolment/withdrawal, search, backups, installer, migrations, validation.

---

## 10. Screens and UX

### 10.1 Screen inventory

| ID | Screen | Route | Purpose | Primary action |
| --- | --- | --- | --- | --- |
| S-1 | Students list | `/students` | Find and triage students | New student |
| S-2 | Student form | `/students/new`, `/students/:id/edit` | Create or edit a student | Save |
| S-3 | Student detail | `/students/:id` | Contact details + classes | Add to class / Edit |
| S-4 | Classes list | `/classes` | Find classes and see fill levels | New class |
| S-5 | Class form | `/classes/new`, `/classes/:id/edit` | Create or edit a class | Save |
| S-6 | Class detail | `/classes/:id` | Facts + roster | Add students |
| S-7 | Add-students dialog | overlay on S-3 / S-6 | Search and enrol | Add selected |
| S-8 | About / diagnostics | `/about` | Version, folders, backups | Open folder / Copy diagnostics |

### 10.2 Field specification

**Student (S-2)**

| Field | Required | Rules | Input |
| --- | --- | --- | --- |
| Full name | Yes | 2–120 chars, trimmed | Text |
| Phone | Yes | 7–20 chars, digits and `+ - ( ) space` | Tel |
| Email | No | Valid email, ≤ 200 chars | Email |
| Guardian name | No | ≤ 120 chars | Text |
| Guardian phone | No | Same as phone | Tel |
| Date of birth | No | Not in the future | Date |
| School | No | ≤ 120 chars | Text |
| Grade level | No | ≤ 40 chars, suggestions from existing values | Combobox |
| Joined on | Yes | Default today, not in the future | Date |
| Notes | No | ≤ 1,000 chars | Multiline |

**Class (S-5)**

| Field | Required | Rules | Input |
| --- | --- | --- | --- |
| Name | Yes | 2–120 chars | Text |
| Subject | Yes | 2–60 chars, suggestions from existing values | Combobox |
| Grade level | No | ≤ 40 chars, suggestions | Combobox |
| Schedule | No | ≤ 200 chars, e.g. "Wed 4–6pm, Sat 9–11am" | Text |
| Start date | Yes | Default today | Date |
| End date | No | On or after start date | Date |
| Monthly fee | Yes | ≥ 0, max 2 decimals | Currency input |
| Currency | Yes | Default LKR; ISO 4217 | Select |
| Capacity | No | ≥ 1 when set; blank = unlimited | Number |
| Notes | No | ≤ 1,000 chars | Multiline |

### 10.3 List behaviour

- Search box debounced ~250 ms; filter, sort, and page live in the URL so Back restores the view.
- Page size 25 (SHOULD: 25/50/100 selector).
- Archived records are hidden under the default Active filter; the Archived filter is explicit.
- Row click opens detail; row actions (Edit, Archive/Restore, Withdraw) are keyboard reachable.
- Counts shown in a class row must update after an enrolment change without a page reload.

### 10.4 UX principles

1. **Table first.** The list is the working surface; detail pages are for editing and context.
2. **Never dead-end.** Every empty state has a next action; every error says what to do.
3. **Confirm the irreversible.** Archive, withdraw, and over-capacity enrolment require confirmation that names the consequence.
4. **Say what changed.** Success toasts name the object ("Kasun Perera added to Grade 10 Mathematics").
5. **Don't lose typing.** Warn before discarding a dirty form; keep values on validation failure.
6. **Desktop manners.** Native window chrome, remembered size/position, fast keyboard flow, no browser-looking page reloads.
7. **Readable at a glance.** Enrolled/capacity and status use text plus colour, never colour alone.

### 10.5 Copy and tone

Plain, direct, second person. "Student archived." not "The student entity has been successfully archived." Errors state cause + action: "Another student already has this name and phone number. Open that record, or save anyway if they are different people."

---

## 11. Data, privacy, and ownership

- **What is stored:** names, phone numbers, email addresses, guardian contacts, dates of birth, schools, grades, notes, class and enrolment data.
- **Where:** a single SQLite database in the current Windows user's local app data folder. No cloud, no sync, no telemetry.
- **Protection:** the database is not encrypted in v0.1; it relies on Windows account security and disk encryption (BitLocker recommended). This is documented for the user in the About panel.
- **Backups:** automatic local copies in the app data folder; these contain the same personal data and inherit the same protections.
- **Retention:** the tutor decides. Archiving hides records but keeps history; there is no automatic deletion.
- **Third parties:** none. The app makes no network requests after installation.
- **PII in logs:** logs record identifiers and error context, never full student records; phone numbers and emails are redacted.

---

## 12. Assumptions

| ID | Assumption | Rationale | If wrong |
| --- | --- | --- | --- |
| A-01 | One user, one Windows machine, no login | Target is a solo tutor; auth would add cost and friction | Multi-user needs auth, concurrency, and likely a server DB (roadmap) |
| A-02 | A student can attend many classes; many-to-many via Enrolment | Real tuition practices teach multiple subjects | If one-class-only, add a nullable FK and simplify; low cost now, higher later |
| A-03 | A "class" is a recurring group, not a single lesson | Matches how tutors describe their work | Session-level scheduling is a later feature; class remains the parent |
| A-04 | Classes charge a monthly fee, but v0.1 does not bill | Fee is useful context; billing is a separate product area | If billing is needed immediately, v0.2 grows (see roadmap) |
| A-05 | English-only UI; default currency LKR, editable per class | User is in Sri Lanka; English is the working language | Localisation is additive; currency already per class |
| A-06 | Subject and grade level are free text with suggestions | Avoids maintaining lookup tables for a solo tutor | Can be normalised later with a data migration |
| A-07 | No Teacher entity in v0.1 | Single tutor; adds no value yet | Add `TeacherId` later; default teacher can be back-filled |
| A-08 | No concurrency control needed | Single process, single user | Multi-device/multi-user changes the architecture |
| A-09 | Windows 10 21H2+/11 x64, WebView2 runtime available | WebView2 ships with Windows 11 and current Windows 10 | Installer offers the WebView2 bootstrapper; documented in architecture |
| A-10 | Phone required, email optional | Phone is the primary contact channel | Making phone optional is a validation change only |
| A-11 | Archive instead of delete | Preserves history and prevents accidental loss | A hard-delete admin tool can be added later |
| A-12 | Capacity is advisory | Tutor should not be blocked by software | Can become a hard rule with an override flag |
| A-13 | Data volumes stay small (< 5k students, < 20k enrolments) | Solo practice | Indexes and pagination scale further; SQLite handles this comfortably |
| A-14 | No structured timetable in v0.1 | Free text is faster to ship and flexible | Schema can add `ClassSlots` without touching enrolments |

---

## 13. Open questions

Defaults below are what the plan proceeds with. Answering them changes scope, not the overall architecture.

| ID | Question | Default if unanswered | Impact if different |
| --- | --- | --- | --- |
| OQ-1 | Can a student be in more than one class at a time? | Yes (A-02) | Removing it simplifies allocation but changes the schema |
| OQ-2 | Should classes record a fee now, or only in v0.2? | Record the field, no billing (A-04) | Dropping it saves a form field; adding later is a migration |
| OQ-3 | Is capacity a hard limit or advisory? | Advisory (A-12) | Hard limit adds an override flow |
| OQ-4 | Do you need to import an existing spreadsheet for day one? | Not in v0.1; CSV import is the first fast follow | If yes, it becomes the highest-priority post-MVP item and may pull forward |
| OQ-5 | Will anyone else ever use this install (assistant, second tutor)? | No (A-01) | If yes, plan auth/multi-user before fees |
| OQ-6 | Should archived students still appear on class rosters? | No; withdraw on archive, visible in history | If yes, roster shows archived badges instead of withdrawing |
| OQ-7 | Install scope: per-user (no admin) or machine-wide? | Per-user, `%LOCALAPPDATA%\Programs\TuitionDesk` | Machine-wide needs admin and a shared data location |
| OQ-8 | Currency: LKR only, or selectable per class? | Selectable, LKR default | Locking to LKR removes a dropdown |

---

## 14. MVP acceptance script

A release candidate passes when this script works on a clean Windows 11 machine, with networking disabled, using the packaged installer.

1. Install as a standard user; no admin prompt; launch from the Start Menu.
2. Window opens in ≤ 3 s; Students and Classes navigation is visible.
3. Create class **Grade 10 Mathematics**, subject Mathematics, Wed 4–6pm, start today, fee 4,000 LKR, capacity 15. It appears in the class list with 0/15 enrolled.
4. Create 30 students; the list paginates; searching a partial name and a partial phone both find the right student.
5. Attempt to save a student without a phone; a field-level error appears and the form keeps its values.
6. Open the class, add 12 students via the add-students dialog; the count becomes 12/15 without a manual refresh.
7. Open one of those students; the class is listed with the enrolment date.
8. Try adding the same student to the same class again; a clear "already enrolled" message appears and no duplicate row is created.
9. Add 4 more students one at a time; the addition that would take the class past 15 shows a full-capacity warning and requires confirmation.
10. Withdraw one student; the roster count shows 15/15 and the student leaves the active roster. Open that student and confirm the class appears under past enrolments with a withdrawal date.
11. Open the student list, archive a student with an active enrolment; the confirmation names the class, and the class count drops by one.
12. Disable the network adapter and repeat a create, a search, and an enrolment; everything behaves identically.
13. Kill the app with Task Manager, relaunch; all saved data is present.
14. Install the next build over the top; the app starts, data is intact, schema version is current.
15. Uninstall; the app is removed and the user's data folder is retained (or explicitly offered for deletion).

---

## 15. Glossary

| Term | Meaning |
| --- | --- |
| **Active** | Record in normal use: visible in default lists and eligible for enrolment. |
| **Archived** | Soft-deleted record: hidden by default, read-only, restorable, blocked from enrolment. |
| **Class** | A recurring teaching group (subject + level + schedule + fee), not a single lesson. |
| **Enrolment** | The record linking one student to one class, with dates and status. |
| **Roster** | The list of students in a class. |
| **Withdrawn** | An enrolment that ended; retained for history. |
| **Minor units** | Integer representation of money (cents), e.g. LKR 4,000.00 = 400000. |
| **Duplicate warning** | A non-blocking notice that a similar record may already exist. |

---

*Changes to this document should be reflected in [architecture.md](architecture.md) and [roadmap.md](roadmap.md) in the same change.*
