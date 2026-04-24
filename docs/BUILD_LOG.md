# Build Log

## Issue #15 — Final Verification and Client-Facing Update

**Date:** 2026-02-17

### Overview

Re-tested all critical workflows end-to-end, confirmed no regressions from previous fixes, and finalized the client-facing progress report describing the completed MVP. All flows passed without bugs.

### Critical Workflow Re-Test Results

| # | Workflow | Result | Details |
|---|----------|--------|---------|
| 1 | Seed demo data via home page | ✅ Pass | Clicked "Seed Test Data" on `/`, received success toast: "Demo users and pipeline data seeded successfully." |
| 2 | Login as Sales Leader (Sarah Mitchell) | ✅ Pass | Clicked persona → MFA `123456` auto-submitted → redirected to `/app/pipeline-dashboard.html`, header shows "Sarah Mitchell". |
| 3 | View weekly report (2026-02-02) | ✅ Pass | CE €800,000→€865,000 (+€65,000), MS €450,000→€450,000 (€0), Total €1,250,000→€1,315,000 (+€65,000). All movement categories and counts correct. |
| 4 | View weekly report (2026-02-09) | ✅ Pass | CE €865,000→€860,000 (-€5,000), MS €450,000→€375,000 (-€75,000), Total €1,315,000→€1,235,000 (-€80,000). All movement categories correct. |
| 5 | Expand detail tables | ✅ Pass | New (CE, W1): 2 rows — Contoso Ltd (+€75,000), Fabrikam Inc (+€45,000). Lost (CE, W1 empty): "No opportunities in this category." |
| 6 | Logout | ✅ Pass | Clicked Logout → redirected to `/login.html` with all 7 personas visible. |
| 7 | Login as Sales Ops (Rachel Thompson) | ✅ Pass | Clicked persona → MFA → dashboard shows "Rachel Thompson". |

### Error Handling Spot-Checks

| Scenario | Input | Expected | Actual | Status |
|----------|-------|----------|--------|--------|
| No data (valid Monday) | 2026-01-05 | Info message | "No pipeline data found for the week of 2026-01-05…" + "↩ Latest week" button | ✅ Pass |
| Non-Monday date | 2026-02-10 | Validation warning | "Week start date must be a Monday. Please select a Monday." | ✅ Pass |
| Default (current week) | 2026-02-16 | 404 info message | "No pipeline data found…" — expected since only demo weeks are seeded | ✅ Pass |

### Responsiveness Verification

| Width | Layout | Status |
|-------|--------|--------|
| 375px (mobile) | Summary cards stacked single column, user name hidden, Logout visible, movement categories readable, data correct | ✅ Pass |
| 1280px (desktop) | Summary cards 3-column grid, full nav with user name and Logout, ample whitespace, data correct | ✅ Pass |

### Previously Fixed Bugs — Regression Check

| Bug | Original Issue | Status |
|-----|---------------|--------|
| Negative currency values displayed as `€-5,000` instead of `-€5,000` | #14 | ✅ Verified fixed — `-€5,000`, `-€75,000`, `-€80,000` all display correctly |

### Console Messages

- Tailwind CSS dev-mode warning (expected — vendor CDN build)
- 404 for weeks with no seeded data (expected behavior)
- No JavaScript errors

### Files Touched

| File | Action |
|------|--------|
| `docs/BUILD_LOG.md` | Updated (this file) — final testing summary |
| `docs/CLIENT_UPDATE.md` | Updated — comprehensive non-technical MVP summary |
| `docs/AGENT_PROBLEMS.md` | Updated — clean run confirmation |

---

## Issue #14 — Full Test Pass: Remaining Flows and Bug Fixes

**Date:** 2026-02-17

### Overview

Performed comprehensive testing of all remaining user flows, pages, and forms not covered by the smoke test. Verified all dashboard features including week selection, movement category breakdowns, opportunity-level detail views, and error/validation scenarios. Found and fixed one currency formatting bug.

### Pages Tested

| Page | URL | Status | Notes |
|------|-----|--------|-------|
| Home / Landing | `/` | ✅ Pass | Logo, description, "Seed Test Data" button, "Sign In" link all render correctly |
| Login | `/login.html` | ✅ Pass | 7 personas loaded, email form, MFA flow, "Seed Users"/"Reset Users" toggle |
| Pipeline Dashboard | `/app/pipeline-dashboard.html` | ✅ Pass | Summary cards, movement breakdown, expandable detail tables all functional |

### Week Selection Tests

| Scenario | Input | Expected | Actual | Status |
|----------|-------|----------|--------|--------|
| Valid seeded week 1 | 2026-02-02 | Data loads | CE €800K→€865K (+€65K), MS €450K→€450K (€0), Total +€65K | ✅ Pass |
| Valid seeded week 2 | 2026-02-09 | Data loads | CE €865K→€860K (-€5K), MS €450K→€375K (-€75K), Total -€80K | ✅ Pass |
| No data (valid Monday) | 2026-01-05 | Friendly info message | "No pipeline data found for the week of 2026-01-05…" | ✅ Pass |
| Non-Monday date | 2026-02-10 | Validation warning | "Week start date must be a Monday. Please select a Monday." | ✅ Pass |
| Default (current week) | 2026-02-16 | 404 info message | "No pipeline data found…" — expected since only demo weeks are seeded | ✅ Pass |
| Jump to latest week | Button click | Reloads last successful week | Correctly returns to last fetched week | ✅ Pass |

### Movement Category Breakdown Tests

| Opportunity Type | Week | Categories Verified | Status |
|-----------------|------|---------------------|--------|
| System Integration (CE) | 2026-02-02 | New (2, €120K), Won (1, -€85K), Lost (0), Increase (1, €30K), Decrease (0), Removed (0) | ✅ Pass |
| Managed Services | 2026-02-02 | New (1, €60K), Won (0), Lost (1, -€45K), Increase (0), Decrease (1, -€15K), Removed (0) | ✅ Pass |
| System Integration (CE) | 2026-02-09 | New (1, €55K), Won (0), Lost (0), Increase (0), Decrease (1, -€20K), Removed (1, -€40K) | ✅ Pass |
| Managed Services | 2026-02-09 | New (0), Won (1, -€70K), Lost (0), Increase (1, €25K), Decrease (0), Removed (1, -€30K) | ✅ Pass |

### Opportunity Detail Expansion Tests

| Category | Type | Details Verified | Status |
|----------|------|------------------|--------|
| New (CE, W1) | Expand | 2 rows: Contoso Ltd (+€75K), Fabrikam Inc (+€45K) — Customer, Opportunity, Owner, Stage, Amount all correct | ✅ Pass |
| Won (CE, W1) | Expand | 1 row: Woodgrove Bank (-€85K) — all fields correct | ✅ Pass |
| Lost (CE, W1) | Expand (empty) | "No opportunities in this category." message shown | ✅ Pass |

### Auth Flow Tests

| Flow | Steps | Status |
|------|-------|--------|
| Login via persona | Click Sarah Mitchell → MFA 123456 → auto-submit → redirect to dashboard | ✅ Pass |
| User display | Header shows "Sarah Mitchell" | ✅ Pass |
| Logout | Click Logout → redirected to /login.html | ✅ Pass |
| Re-login (different user) | Click James Chen → MFA 123456 → dashboard shows "James Chen" | ✅ Pass |

### Seeding Tests

| Action | Status |
|--------|--------|
| Seed via API (`POST /api/demo/seed`) | ✅ Pass |
| Seed via home page button | ✅ Pass (verified in Issue #13) |
| Seed Users / Reset Users toggle on login page | ✅ Pass — dynamically switches based on user existence |

### Bugs Found and Fixed

| # | Bug | Severity | Fix | File |
|---|-----|----------|-----|------|
| 1 | Negative currency values displayed as `€-5,000` instead of `-€5,000` — sign appeared after euro symbol instead of before | Minor (UX) | Updated `formatCurrency()` to use `Math.abs()` and prepend sign before `€` symbol | `app/js/pipelineDashboard.js` |

### Console Messages

- Tailwind CSS dev-mode warning (expected — vendor CDN build)
- 404 for current week pipeline data (expected — only demo weeks 2026-02-02 and 2026-02-09 are seeded)
- No JavaScript errors

### Files Touched

| File | Action |
|------|--------|
| `app/js/pipelineDashboard.js` | Fixed `formatCurrency()` negative sign placement |
| `docs/BUILD_LOG.md` | Updated (this file) |
| `docs/AGENT_PROBLEMS.md` | Updated — agent problem log |

---

## Issue #13 — Smoke Test: Seed Data and Critical Path Testing

**Date:** 2026-02-17

### Overview

Performed end-to-end smoke testing of the three critical user workflows: seeding demo data, logging in as a demo persona, and viewing weekly pipeline data on the dashboard. All critical paths passed with no blocking bugs.

### Critical Path Results

| # | Workflow | Result | Details |
|---|----------|--------|---------|
| 1 | Seed demo data via home page | ✅ Pass | Navigated to `/`, clicked "Seed Test Data", received success toast: "Demo users and pipeline data seeded successfully." |
| 2 | Login as demo persona | ✅ Pass | Navigated to `/login.html`, 7 personas loaded, clicked "Sarah Mitchell", MFA code `123456` auto-submitted, redirected to `/app/pipeline-dashboard.html` with user name displayed in header. |
| 3 | View weekly pipeline summary | ✅ Pass | Selected week 2026-02-02: CE €800,000→€865,000 (+€65,000), MS €450,000→€450,000 (€0), Total €1,250,000→€1,315,000 (+€65,000). Selected week 2026-02-09: CE €865,000→€860,000 (-€5,000), MS €450,000→€375,000 (-€75,000), Total €1,315,000→€1,235,000 (-€80,000). Movement breakdown categories and expandable detail tables all rendering correctly. |

### Pages Tested

| Page | URL | Status |
|------|-----|--------|
| Home / Landing | `/` | ✅ No errors |
| Login | `/login.html` | ✅ No errors |
| Pipeline Dashboard | `/app/pipeline-dashboard.html` | ✅ No errors |

### Console Messages

- Tailwind CSS dev-mode warning (expected — vendor CDN build)
- 404 for current week pipeline data (expected — only demo weeks 2026-02-02 and 2026-02-09 are seeded)

### Bugs Fixed

None — all critical paths passed without blocking issues.

### Files Touched

| File | Action |
|------|--------|
| `docs/BUILD_LOG.md` | Updated (this file) |
| `docs/AGENT_PROBLEMS.md` | Updated — agent problem log |

---

## Issue #12 — Demo Data Seeding: Users and Application Data

**Date:** 2026-02-17

### Overview

Verified and confirmed that the full demo data seeding infrastructure is complete and working end-to-end. Both user personas and pipeline application data (snapshots, movements, lookups) are seeded and reset through a single action.

### Seeding Infrastructure Summary

| Component | Status | Details |
|-----------|--------|---------|
| `POST /api/demo/seed` | ✅ Complete | Seeds both users (via `SampleDataSeeder`) and pipeline data (via `AppDataSeeder`). Idempotent — safe to call multiple times. |
| `POST /api/demo/reset` | ✅ Complete | Clears and re-seeds all data (users + pipeline entities). |
| `AppDataSeeder.SeedAsync()` | ✅ Idempotent | Checks for existing data before inserting. Seeds 2 weeks of pipeline data covering both opportunity types and all 6 movement categories. |
| `AppDataSeeder.ResetAsync()` | ✅ Destructive | Deletes demo entries from all 3 tables, then re-seeds. |
| `index.html` "Seed Test Data" button | ✅ Wired | Calls `authService.seedDemoData()`, shows success/error toast feedback. |
| `login.html` Seed/Reset button | ✅ Wired | Dynamically shows "Seed Users" or "Reset Users" based on whether personas exist. Calls `seedDemoData()` or `resetDemoData()` accordingly. |
| `data/demo-users.csv` | ✅ Final | 7 personas with pipeline-relevant roles (admin, sales leaders, ops analyst, sales reps, suspended). |

### How to Use

1. **First-time setup**: Navigate to `http://127.0.0.1:4280` and click "Seed Test Data". This seeds both demo users and 2 weeks of pipeline data.
2. **From login page**: Click "Seed Users" (if no users exist) or "Reset Users" (if users already exist) in the Demo Personas section.
3. **Via API**: `curl -X POST http://127.0.0.1:4280/api/demo/seed` or `curl -X POST http://127.0.0.1:4280/api/demo/reset`.
4. **Dashboard**: After seeding, log in as any persona and view pipeline data for weeks of 2026-02-02 or 2026-02-09.

### Demo Data Coverage

| Week | CE Net Change | MS Net Change | Movement Categories Covered |
|------|--------------|---------------|----------------------------|
| 2026-02-02 | +$65,000 | $0 | New, Won, Increase (CE); New, Lost, Decrease (MS) |
| 2026-02-09 | −$5,000 | −$75,000 | New, Decrease, Removed (CE); Increase, Won, Removed (MS) |

All 6 movement categories (New, Won, Lost, Increase, Decrease, Removed) are represented across the two weeks for both opportunity types.

### Files Verified (No Code Changes Needed)

- `api/Services/AppDataSeeder.cs` — Confirmed idempotent seed and destructive reset
- `api/Functions/DemoAdminFunctions.cs` — Confirmed both seeders called in seed and reset endpoints
- `index.html` — Confirmed "Seed Test Data" button with toast feedback
- `login.html` — Confirmed dynamic Seed/Reset button
- `js/authService.js` — Confirmed `seedDemoData()` and `resetDemoData()` methods
- `data/demo-users.csv` — Confirmed pipeline-relevant personas
- `docs/BUILD_LOG.md` — Updated (this entry)
- `docs/AGENT_PROBLEMS.md` — Updated

---

## Issue #11 — Validation and UX Polish for Weekly Selection and Error Handling

**Date:** 2026-02-17

### Overview

Improved robustness and clarity of the week selector and error handling in the pipeline dashboard. Added client-side input validation, differentiated error messages by HTTP status code, and a "Jump to latest week" helper button.

### Validation Rules (Client-Side)

Before calling the API, `validateWeekStart(value)` checks:

| Rule | Condition | Message |
|------|-----------|---------|
| Non-empty | Value is falsy or not a string | "Please select a valid week start date." |
| Format | Does not match `YYYY-MM-DD` regex | "Date must be in YYYY-MM-DD format." |
| Valid calendar date | Month/day out of range, or `new Date()` round-trip fails (e.g., Feb 30) | "Please enter a valid calendar date." |
| Must be Monday | `parsed.getDay() !== 1` | "Week start date must be a Monday. Please select a Monday." |
| Not future | Date is after today | "Cannot select a future week. Please choose a past or current week." |

If any check fails, the warning message is shown immediately and no API call is made.

### Error Handling (API Responses)

| HTTP Status | Message Style | User Message |
|-------------|---------------|-------------|
| 400 | Warning (amber) | Server error message + "Please select a Monday in YYYY-MM-DD format." |
| 404 | Info (blue) | "No pipeline data found for the week of {date}. Try selecting a different week or seed demo data from the homepage." |
| Other errors | Error (red) | "Something went wrong while loading the report (HTTP {status}). Please try again later." |
| Network failure | Error (red) | "Unable to reach the server. Please check your connection and try again." |

### "Jump to Latest Week" Button

A secondary-styled "↩ Latest week" button appears next to the Apply button when:
- At least one week has been successfully loaded in the current session, **and**
- The current date input differs from that last successful week.

Clicking the button sets the date input to the last successfully loaded week and re-fetches data.

### HTML Changes

Added the `#jump-latest-week` button (hidden by default) to the week selector area in `app/pipeline-dashboard.html`, using the DESIGN.md secondary button pattern.

### Files Touched

- `app/js/pipelineDashboard.js` — Added `validateWeekStart()`, `updateLatestWeekButton()`, `_lastSuccessfulWeek` tracking; enhanced `fetchWeeklyReport()` and `initWeekSelector()`
- `app/pipeline-dashboard.html` — Added `#jump-latest-week` button
- `docs/screenshots/11-week-error-handling.png` — Screenshot showing no-data info message at 1280×720
- `docs/BUILD_LOG.md` — This entry
- `docs/CLIENT_UPDATE.md` — Updated with improved guidance description
- `docs/AGENT_PROBLEMS.md` — Clean-run note

---

## Issue #10 — Add "Seed Test Data" Button to Landing Page and Refine Demo Personas

**Date:** 2026-02-17

### Overview

Added a prominent "Seed Test Data" button to the public landing page (`index.html`) that calls `authService.seedDemoData()` and shows toast notifications for success/failure feedback. Updated `data/demo-users.csv` with pipeline-relevant personas.

### Landing Page Changes

A new "Seed Test Data" button is placed between the "What This App Does" card and the "Sign In to View Dashboard" link. Beneath the button, a helper text reads "Seeds demo users and pipeline data for testing" to clarify what the action does.

**Behavior:**
1. User clicks "Seed Test Data".
2. Button text changes to "Seeding…" and is disabled during the request.
3. On success: a green toast appears top-right — "Demo users and pipeline data seeded successfully."
4. On failure: a red toast appears top-right with the error message.
5. Toasts auto-dismiss after 5 seconds.

The button and toast use the DESIGN.md primary button and toast/notification component patterns respectively.

### Toast Implementation

A lightweight toast system is inlined in `index.html`:
- Container: `<div id="toast-container" class="fixed top-4 right-4 z-50 space-y-3">` (matching DESIGN.md pattern).
- Success toast: `bg-success-light border-success/60 text-success` with title and message.
- Error toast: `bg-error-light border-error/60 text-error` with title and message.
- Auto-removed after 5 seconds via `setTimeout`.

### Demo Personas (demo-users.csv)

Replaced generic template personas with pipeline-analysis-relevant roles:

| UserId | Name | Role/Description |
|--------|------|-----------------|
| user-admin | System Admin | Full admin access |
| user-001 | Sarah Mitchell | Sales Leader — System Integration (CE) focus |
| user-002 | James Chen | Sales Leader — Managed Services focus |
| user-003 | Rachel Thompson | Sales Ops / CRM Analyst — cross-type pipeline reporting |
| user-004 | Tom Harrison | Sales Rep — System Integration (CE) opportunities |
| user-005 | Lisa Wong | Sales Rep — Managed Services opportunities |
| user-suspended | Suspended User | Suspended account (cannot login) |

### Login Page Verification

The existing `login.html` seed/reset buttons continue to call `authService.seedDemoData()` and `authService.resetDemoData()`, which trigger combined user+pipeline seeding via the `/api/demo/seed` and `/api/demo/reset` endpoints (wired in Issue #8/#9). No changes to `login.html` were needed.

### Files Touched

| File | Action |
|------|--------|
| `index.html` | Updated — added "Seed Test Data" button, toast container, and inline script |
| `data/demo-users.csv` | Updated — replaced template personas with pipeline-relevant roles |
| `docs/screenshots/10-home-seed-button.png` | Created — landing page screenshot at 1280×720 |
| `docs/BUILD_LOG.md` | Updated (this file) |
| `docs/CLIENT_UPDATE.md` | Updated — client-facing description of homepage seeding |
| `docs/AGENT_PROBLEMS.md` | Updated — clean-run note |

---

## Issue #9 — Extend DemoAdminFunctions to Seed and Reset App Data

**Date:** 2026-02-17

### Overview

Verified and documented that `DemoAdminFunctions` correctly wires both `SampleDataSeeder` and `AppDataSeeder` so that the existing `/api/demo/seed` and `/api/demo/reset` endpoints manage both user personas and pipeline demo data in a single call.

### Combined Seeding Behavior

The `DemoAdminFunctions` class injects both seeders via constructor DI and calls them sequentially in each endpoint:

**`POST /api/demo/seed`** (`DemoAdminFunctions.SeedData`):
1. Calls `SampleDataSeeder.SeedAsync()` — loads demo user personas from `data/demo-users.csv` (idempotent: skips if users already exist).
2. Calls `AppDataSeeder.SeedAsync()` — seeds two weeks of pipeline snapshots, movements, and opportunity lookups (idempotent: skips if Week 1 CE snapshot already exists).

**`POST /api/demo/reset`** (`DemoAdminFunctions.ResetData`):
1. Calls `SampleDataSeeder.ResetAsync()` — clears all user data and re-seeds personas from CSV.
2. Calls `AppDataSeeder.ResetAsync()` — deletes all demo pipeline entities (snapshots, movements, lookups) and re-seeds both weeks.

### Idempotency

Both seeders implement idempotent `SeedAsync` methods:
- `SampleDataSeeder` checks `GetAllUsersAsync().Count > 0` before seeding.
- `AppDataSeeder` performs a point read for the Week 1 CE snapshot; if found, it skips seeding.
- `ResetAsync` on both seeders is destructive by design (clears and re-creates), so repeated calls produce the same result.

### Route and Auth Consistency

Routes and authorization remain unchanged from the template convention:
- Both endpoints use `AuthorizationLevel.Anonymous` (SWA handles auth at the proxy level).
- Both endpoints are blocked in production (`AZURE_FUNCTIONS_ENVIRONMENT == "Production"`) and require mock auth mode (`IsMockAuth == true`).
- Route paths: `demo/seed` and `demo/reset` (prefixed with `/api/` by SWA).

### Files Touched

| File | Action |
|------|--------|
| `api/Functions/DemoAdminFunctions.cs` | Verified — already wires both SampleDataSeeder and AppDataSeeder (done in Issue #8) |
| `docs/BUILD_LOG.md` | Updated (this file) — documented combined seeding behavior |
| `docs/CLIENT_UPDATE.md` | Updated — informed that seeding/reset endpoints now populate pipeline data |
| `docs/AGENT_PROBLEMS.md` | Updated — clean-run note |

---

## Issue #8 — AppDataSeeder for Demo Pipeline Data

**Date:** 2026-02-17

### Overview

Created `AppDataSeeder` to populate Azure Table Storage with demo weekly pipeline snapshots, movements, and lookup data. The seeder covers both opportunity types, all six movement categories, and two demo weeks with reconciling values.

### Seeding Strategy

`AppDataSeeder` is registered as a singleton in DI and wired into the existing `DemoAdminFunctions` endpoints (`POST /api/demo/seed` and `POST /api/demo/reset`). It operates alongside `SampleDataSeeder` (which handles user persona data).

- **SeedAsync(CancellationToken)**: Checks if data already exists (point read for Week 1 CE snapshot). If found, logs and returns (idempotent). Otherwise, inserts all demo entities using `UpsertEntityAsync` for safety.
- **ResetAsync(CancellationToken)**: Deletes all demo entities from the three tables by querying known demo partition keys, then calls `SeedAsync` to re-create.

### Demo Week Definitions

| Week | Start | End | CE Net Change | MS Net Change | Total Net |
|------|-------|-----|---------------|---------------|-----------|
| Week 1 | 2026-02-02 | 2026-02-08 | +€65,000 | €0 | +€65,000 |
| Week 2 | 2026-02-09 | 2026-02-15 | −€5,000 | −€75,000 | −€80,000 |

### Movement Category Coverage

Every movement category is covered at least once across the two weeks:

| Category | Week 1 CE | Week 1 MS | Week 2 CE | Week 2 MS |
|----------|-----------|-----------|-----------|-----------|
| New | ✅ (+75k, +45k) | ✅ (+60k) | ✅ (+55k) | — |
| Won | ✅ (−85k) | — | — | ✅ (−70k) |
| Lost | — | ✅ (−45k) | — | — |
| Increase | ✅ (+30k) | — | — | ✅ (+25k) |
| Decrease | — | ✅ (−15k) | ✅ (−20k) | — |
| Removed | — | — | ✅ (−40k) | ✅ (−30k) |

### Reconciliation

Movement sums reconcile to snapshot net change for each week and type:

- **Week 1 CE**: 75,000 + 45,000 − 85,000 + 30,000 = **+65,000** ✓
- **Week 1 MS**: 60,000 − 45,000 − 15,000 = **0** ✓
- **Week 2 CE**: 55,000 − 20,000 − 40,000 = **−5,000** ✓
- **Week 2 MS**: 25,000 − 70,000 − 30,000 = **−75,000** ✓

Snapshot start/end values chain correctly: Week 2 starting values equal Week 1 ending values.

### Demo Opportunities

13 unique opportunities are seeded across both types:

| ID | Type | Customer | Title |
|----|------|----------|-------|
| opp-ce-101 | CE | Contoso Ltd | Azure Migration |
| opp-ce-102 | CE | Fabrikam Inc | Power Platform Rollout |
| opp-ce-103 | CE | Proseware Inc | SharePoint Modernization |
| opp-ce-050 | CE | Woodgrove Bank | Teams Voice Deployment |
| opp-ce-030 | CE | Northwind Traders | Dynamics 365 Implementation |
| opp-ce-025 | CE | Consolidated Messenger | M365 Security Suite |
| opp-ce-015 | CE | Graphic Design Institute | Legacy CRM Migration |
| opp-ms-201 | MS | Tailspin Toys | Managed Security Operations |
| opp-ms-150 | MS | Adventure Works | Cloud Monitoring |
| opp-ms-120 | MS | Litware Inc | Helpdesk Outsourcing |
| opp-ms-110 | MS | Wide World Importers | Managed Cloud Infrastructure |
| opp-ms-100 | MS | Humongous Insurance | 24/7 IT Support Contract |
| opp-ms-090 | MS | Margie's Travel | Network Monitoring |

### DateTime Handling

All DateTime values use `DateTime.UtcNow` or `DateTime.SpecifyKind(..., DateTimeKind.Utc)` to satisfy Azure Table Storage requirements.

### DI Registration

`AppDataSeeder` is registered as a singleton in `Program.cs` alongside `TableStorageContext` and `PipelineReportService`.

### Files Touched

| File | Action |
|------|--------|
| `api/Services/AppDataSeeder.cs` | Created — demo pipeline data seeder with SeedAsync and ResetAsync |
| `api/Functions/DemoAdminFunctions.cs` | Updated — wired AppDataSeeder into seed/reset endpoints |
| `api/Program.cs` | Updated — registered AppDataSeeder in DI |
| `docs/BUILD_LOG.md` | Updated (this file) |
| `docs/CLIENT_UPDATE.md` | Updated — client-facing progress snapshot |
| `docs/AGENT_PROBLEMS.md` | Updated — agent problem log |

---

## Issue #7 — Frontend Movement Categories and Opportunity-Level Detail Rendering

**Date:** 2026-02-17

### Overview

Extended the dashboard UI to display movement category breakdowns and opportunity-level detail tables per category and opportunity type. Users can expand/collapse each category to see the individual opportunities that drove the weekly change.

### Rendering Architecture

The movement breakdown section is now fully dynamic. On API response, `renderMovementBreakdown(typeSummaries)` builds the entire DOM for both opportunity types (System Integration (CE) and Managed Services) with their six categories each.

**Key functions:**

| Function | Purpose |
|----------|---------|
| `renderMovementBreakdown(typeSummaries)` | Entry point — builds type cards and attaches expand/collapse listeners |
| `buildCategoryRows(typeKey, categories)` | Builds the six category rows for one opportunity type, each with a toggle header and a hidden detail area |
| `buildDetailTable(opportunities)` | Builds the HTML table for opportunity-level details within a category |
| `chevronSvg(expanded)` | Returns the appropriate SVG chevron icon for collapsed/expanded state |
| `escapeHtmlContent(text)` | XSS-safe HTML escaping for all dynamic content injected into the DOM |

### Category Metadata

Categories are rendered in a fixed order defined by `CATEGORY_ORDER` and styled via `CATEGORY_META`:

| Category | Badge Style | Description |
|----------|------------|-------------|
| New | `bg-info-light text-info` | New opportunities |
| Won | `bg-success-light text-success` | Closed won |
| Lost | `bg-error-light text-error` | Closed lost |
| Increase | `bg-success-light text-success` | Weighted value increase |
| Decrease | `bg-warning-light text-warning` | Weighted value decrease |
| Removed | `bg-warning-light text-warning` | Removed from pipeline |

### Expand/Collapse Interaction

- Each category row is a clickable toggle (`role="button"`, `tabindex="0"`, `aria-expanded`, `aria-controls`).
- Clicking or pressing Enter/Space on a category row toggles the hidden detail area below it.
- A chevron icon (right-pointing when collapsed, down-pointing when expanded) provides visual feedback.
- Multiple categories can be expanded simultaneously — they operate independently.
- Categories with opportunities show a count badge (e.g., "3") next to the category label.

### Opportunity Detail Table

Each expanded category shows a table with these columns, matching the `OpportunityMovementDetailDto`:

| Column | DTO Field |
|--------|-----------|
| Customer | `customerName` |
| Opportunity | `opportunityTitle` |
| Owner | `ownerName` |
| Final Stage | `finalSalesStage` |
| Amount | `weightedRevenueChange` (formatted with +/- sign) |

The table uses the standard DESIGN.md table pattern with `overflow-x-auto` for horizontal scrolling on small screens.

### Enum-to-Display Mapping

| API Value (opportunityType) | Display Name | ID Prefix |
|-----------------------------|-------------|-----------|
| `SystemIntegrationCE` | System Integration (CE) | `ce` |
| `ManagedServices` | Managed Services | `ms` |

| API Value (category) | Display Label |
|----------------------|--------------|
| `New` | New |
| `Won` | Won |
| `Lost` | Lost |
| `Increase` | Increase |
| `Decrease` | Decrease |
| `Removed` | Removed |

### Responsive Behavior

- **1280px**: Full layout with side-by-side summary cards, movement cards with expandable tables at comfortable widths.
- **375px**: Summary cards stack to single column. Movement category cards stack. Detail tables scroll horizontally via `overflow-x-auto`.

### Files Touched

| File | Action |
|------|--------|
| `app/pipeline-dashboard.html` | Updated — replaced static movement rows with dynamic container; removed old detail area placeholder |
| `app/js/pipelineDashboard.js` | Updated — added category rendering, expand/collapse logic, detail table generation |
| `docs/screenshots/07-movements-detail.png` | Created — dashboard screenshot at 1280×720 with expanded categories |
| `docs/BUILD_LOG.md` | Updated (this file) |
| `docs/CLIENT_UPDATE.md` | Updated — client-facing progress snapshot |
| `docs/AGENT_PROBLEMS.md` | Updated — agent problem log |

---

## Issue #6 — Frontend Week Selector and API Integration for Summary View

**Date:** 2026-02-17

### Overview

Wired the pipeline dashboard UI to the weekly report API. Users can now select a week and see starting/ending/net change summaries by opportunity type and total.

### Week Selector

- Date input (`<input type="date">`) with an "Apply" button in the page header.
- Default value: the most recent Monday on or before today (calculated client-side).
- Pressing Enter in the date input also triggers the report fetch.
- On page load, the report for the default week is fetched automatically.

### API Call Pattern

All authenticated API calls use `authService.fetchWithAuth(url, options)`, a new convenience method added to `js/authService.js`. It:
1. Ensures the auth service is initialized.
2. Merges session-token headers (for mock auth) into the request.
3. Delegates to the native `fetch()`.

```javascript
const response = await authService.fetchWithAuth(
    '/api/pipeline/weekly-report?weekStart=' + encodeURIComponent(weekStart)
);
```

### Summary Rendering

The `renderSummary(data)` function in `app/js/pipelineDashboard.js` maps the `WeeklyPipelineSummaryDto` response to DOM elements:

| DTO Field | DOM Element(s) |
|-----------|---------------|
| `totalStartingWeightedValue` | `#total-start` |
| `totalEndingWeightedValue` | `#total-end` |
| `totalNetChange` | `#total-net` (colored green/red by sign) |
| `typeSummaries[].startingWeightedValue` | `#ce-start` / `#ms-start` |
| `typeSummaries[].endingWeightedValue` | `#ce-end` / `#ms-end` |
| `typeSummaries[].netChange` | `#ce-net` / `#ms-net` (colored) |
| `typeSummaries[].movementCategories[].totalWeightedRevenueChange` | `#ce-new-total`, `#ce-won-total`, etc. |

Currency values are formatted as `€N` with locale-aware thousand separators via `toLocaleString('en-IE')`. Net change values are prefixed with `+` when positive.

### Error Handling

| Condition | HTTP Status | User Message |
|-----------|-------------|-------------|
| Missing/invalid weekStart | 400 | Server error message or "Invalid week start date…" |
| No data for the selected week | 404 | "No pipeline data available for the selected week…" |
| Network/server error | 5xx / network | "Unable to reach the server…" |
| Empty date input | n/a (client) | "Please select a valid week start date." |

Messages are displayed in a status banner (`#status-message`) styled with semantic Tailwind classes from DESIGN.md.

### Files Touched

| File | Action |
|------|--------|
| `js/authService.js` | Updated — added `fetchWithAuth(url, options)` method |
| `app/js/pipelineDashboard.js` | Created — week selector logic, API integration, summary rendering |
| `app/pipeline-dashboard.html` | Updated — added script tag, status message area, loading indicator |
| `docs/screenshots/06-summary-view.png` | Created — dashboard screenshot at 1280×720 |
| `docs/BUILD_LOG.md` | Updated (this file) |
| `docs/CLIENT_UPDATE.md` | Updated — client-facing progress snapshot |
| `docs/AGENT_PROBLEMS.md` | Updated — agent problem log |

---

## Issue #5 — WeeklyPipelineReportFunctions HTTP Endpoint

**Date:** 2026-02-17

### Overview

Exposed the weekly pipeline report via an HTTP-triggered Azure Function so the frontend can call it with a `weekStart` query parameter.

### API Surface

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/pipeline/weekly-report?weekStart=YYYY-MM-DD` | Retrieves the weekly pipeline report for the specified week |

### Query Parameter

| Parameter | Required | Format | Description |
|-----------|----------|--------|-------------|
| `weekStart` | Yes | `YYYY-MM-DD` | The Monday (start) of the reporting week |

### Response Codes

| Status | Condition | Response Body |
|--------|-----------|---------------|
| 200 OK | Data found for the specified week | `WeeklyPipelineSummaryDto` (camelCase JSON) |
| 400 Bad Request | Missing or invalid `weekStart` parameter | `{ "error": "..." }` |
| 404 Not Found | No pipeline data for the specified week | `{ "error": "..." }` |
| 500 Internal Server Error | Unexpected error during processing | `{ "error": "..." }` |

### Response Shape (200 OK)

```json
{
  "weekStartDate": "2026-02-09",
  "weekEndDate": "2026-02-15",
  "totalStartingWeightedValue": 1234567.89,
  "totalEndingWeightedValue": 1300000.00,
  "totalNetChange": 65432.11,
  "typeSummaries": [
    {
      "opportunityType": "SystemIntegrationCE",
      "opportunityTypeDisplayName": "System Integration (CE)",
      "startingWeightedValue": 800000.00,
      "endingWeightedValue": 850000.00,
      "netChange": 50000.00,
      "startingOpportunityCount": 42,
      "endingOpportunityCount": 44,
      "movementCategories": [
        {
          "category": "New",
          "categoryDisplayName": "New",
          "totalWeightedRevenueChange": 25000.00,
          "opportunityCount": 2,
          "opportunities": [...]
        }
      ]
    }
  ]
}
```

### Implementation Details

- Function class: `WeeklyPipelineReportFunctions` in `Api.Functions`
- Function name: `GetWeeklyReport`
- Authorization: `AuthorizationLevel.Anonymous` (SWA handles auth at the proxy level)
- DI: Injects `PipelineReportService` (already registered as singleton in `Program.cs`)
- JSON serialization: Uses `JsonNamingPolicy.CamelCase` via local `JsonSerializerOptions` (consistent with other function classes)
- Date parsing: Uses `DateTime.TryParseExact` with `InvariantCulture` and converts to UTC via `DateTime.SpecifyKind`

### Files Touched

| File | Action |
|------|--------|
| `api/Functions/WeeklyPipelineReportFunctions.cs` | Created — HTTP endpoint for weekly pipeline report |
| `docs/BUILD_LOG.md` | Updated (this file) |
| `docs/CLIENT_UPDATE.md` | Updated — client-facing progress snapshot |
| `docs/AGENT_PROBLEMS.md` | Updated — agent problem log |

---

## Issue #4 — PipelineReportService Business Logic

**Date:** 2026-02-17

### Overview

Implemented the core service that reads snapshot and movement entities from Table Storage and produces the weekly pipeline report DTO. This is the calculation engine behind the weekly pipeline change analysis.

### Service Design

`PipelineReportService` (in `Api.Services`) is an injectable singleton that accepts `TableStorageContext` and `ILogger<PipelineReportService>` via constructor injection.

**Primary method:** `GetWeeklyReportAsync(DateTime weekStartUtc)` → `WeeklyPipelineSummaryDto?`

### Aggregation Logic

1. **Snapshot retrieval**: Queries `WeeklyPipelineSnapshots` table for each known `OpportunityType` enum value using point reads (`GetEntityAsync` with PartitionKey = type, RowKey = weekKey in "yyyy-MM-dd" format).
2. **Movement retrieval**: For each opportunity type, queries `OpportunityMovements` table using PartitionKey = `"{OpportunityType}_{WeekStartDate}"` to get all movements for that type and week.
3. **Category grouping**: Movements are grouped by `MovementCategory` string. Each group produces a `MovementCategorySummaryDto` with summed `TotalWeightedRevenueChange` and individual `OpportunityMovementDetailDto` entries.
4. **Empty categories**: All six movement categories (New, Won, Lost, Increase, Decrease, Removed) are always included in the output, even if no movements exist for that category (shows 0 count and 0 change). Categories are sorted in enum-defined order.
5. **Per-type summaries**: Starting/ending weighted values and counts come directly from the snapshot entity. Net change is pre-computed in the snapshot.
6. **Totals**: Top-level totals are summed across all type summaries.

### Sign Conventions

| Category | Sign | Reasoning |
|----------|------|-----------|
| New | Positive | Adds to open pipeline |
| Increase | Positive | Increases existing pipeline value |
| Won | Negative | Removes from open pipeline (deal closed) |
| Lost | Negative | Removes from open pipeline |
| Decrease | Negative | Reduces existing pipeline value |
| Removed | Negative | Removed for other reasons |

Sign conventions are enforced at data entry time (the `WeightedRevenueChange` field on `OpportunityMovementEntity` is stored with the correct sign). The service sums values as-is — it does not re-apply signs.

### Opportunity Detail Enrichment

Movement entities (`OpportunityMovementEntity`) already contain denormalized fields: `OpportunityTitle`, `CustomerName`, `OwnerName`, `FinalSalesStage`, `WeightedRevenueChange`, `PreviousWeightedRevenue`, `CurrentWeightedRevenue`. These are mapped directly to `OpportunityMovementDetailDto` without a separate lookup join, since the movement entity is designed to carry all needed detail at write time.

The `OpportunityLookupEntity` table remains available for future use cases (e.g., real-time lookups, data sync verification) but is not queried during report generation to avoid unnecessary read overhead.

### Missing Data Handling

- If no snapshot entities exist for the requested week, the method returns `null`.
- Partial data (e.g., snapshot exists for one type but not another) is handled gracefully — only types with snapshots are included.
- Missing movements for a type result in all categories showing 0 values.
- 404 responses from Table Storage point reads are caught and logged at Debug level.

### Assumptions

- The `WeeklyPipelineSnapshotEntity` is the source of truth for starting/ending weighted values. Net change is pre-computed as `EndingWeightedValue - StartingWeightedValue`.
- Movement `WeightedRevenueChange` values are stored with correct signs at data ingestion time.
- The sum of all movement category totals for a type should equal the net change for that type (data consistency is assumed; the service does not enforce or validate this invariant).
- `OpportunityType` enum values define the complete set of types to query.
- Week start dates use "yyyy-MM-dd" format consistently across all tables.

### DI Registration

`TableStorageContext` and `PipelineReportService` are registered as singletons in `Program.cs`. The storage context was previously unregistered in DI (only used directly in some auth code); it is now available for injection throughout the application.

### Files Touched

| File | Action |
|------|--------|
| `api/Services/PipelineReportService.cs` | Created — core weekly report calculation service |
| `api/Program.cs` | Updated — registered `TableStorageContext` and `PipelineReportService` in DI |
| `docs/BUILD_LOG.md` | Updated (this file) |
| `docs/CLIENT_UPDATE.md` | Updated — client-facing progress snapshot |
| `docs/AGENT_PROBLEMS.md` | Updated — agent problem log |

---

## Issue #3 — Backend Data Models and Table Storage Context

**Date:** 2026-02-17

### Data Model Overview

Introduced core backend models for the weekly pipeline change analysis feature. No API endpoints or business logic yet — this establishes the data layer foundation.

### Storage Entities (Azure Table Storage)

Three tables are defined, all accessed via the `STORAGE` connection string (not `AzureWebJobsStorage`):

| Table Name | Entity Class | PartitionKey | RowKey | Purpose |
|------------|-------------|--------------|--------|---------|
| `WeeklyPipelineSnapshots` | `WeeklyPipelineSnapshotEntity` | OpportunityType (e.g., "SystemIntegrationCE") | WeekStartDate ("yyyy-MM-dd") | Stores weekly pipeline summary snapshots per opportunity type |
| `OpportunityMovements` | `OpportunityMovementEntity` | "{OpportunityType}_{WeekStartDate}" | "{MovementCategory}_{OpportunityId}" | Individual opportunity movements within a week |
| `OpportunityLookup` | `OpportunityLookupEntity` | OpportunityType | OpportunityId | Latest known state of each opportunity for comparison |

All entities implement `ITableEntity` from `Azure.Data.Tables`. All `DateTime` properties are documented and named with `Utc` suffix to enforce UTC usage per Azure Table Storage requirements.

### Enums

| Enum | Values | Usage |
|------|--------|-------|
| `OpportunityType` | `SystemIntegrationCE`, `ManagedServices` | Partitions data by opportunity type. String representation used as PartitionKey. |
| `MovementCategory` | `New`, `Won`, `Lost`, `Increase`, `Decrease`, `Removed` | Categorizes pipeline movements. String representation used in RowKey composition. |

### DTOs

| DTO | Purpose |
|-----|---------|
| `WeeklyPipelineReportRequest` | Request DTO — contains `WeekStartDate` (string, "yyyy-MM-dd") |
| `WeeklyPipelineSummaryDto` | Top-level response — week dates, totals, and list of `WeeklyPipelineTypeSummaryDto` |
| `WeeklyPipelineTypeSummaryDto` | Per-type summary — starting/ending values, net change, counts, and list of `MovementCategorySummaryDto` |
| `MovementCategorySummaryDto` | Per-category summary — total change, count, and list of `OpportunityMovementDetailDto` |
| `OpportunityMovementDetailDto` | Individual opportunity detail — customer, title, owner, final stage, revenue change |

### Table Storage Context

`TableStorageContext` (in `Api.Services`) provides:
- `WeeklyPipelineSnapshots` — `TableClient` for snapshot data
- `OpportunityMovements` — `TableClient` for movement records
- `OpportunityLookup` — `TableClient` for opportunity reference data
- `EnsureTablesExistAsync()` — creates tables if they don't exist

Uses `Environment.GetEnvironmentVariable("STORAGE")` for the connection string.

### Files Touched

| File | Action |
|------|--------|
| `api/StorageEntities/WeeklyPipelineSnapshotEntity.cs` | Created — weekly pipeline snapshot table entity |
| `api/StorageEntities/OpportunityMovementEntity.cs` | Created — opportunity movement table entity |
| `api/StorageEntities/OpportunityLookupEntity.cs` | Created — opportunity lookup table entity |
| `api/Models/OpportunityType.cs` | Created — opportunity type enum |
| `api/Models/MovementCategory.cs` | Created — movement category enum |
| `api/Models/WeeklyPipelineReportRequest.cs` | Created — report request DTO |
| `api/Models/WeeklyPipelineSummaryDto.cs` | Created — top-level summary DTO |
| `api/Models/WeeklyPipelineTypeSummaryDto.cs` | Created — per-type summary DTO |
| `api/Models/MovementCategorySummaryDto.cs` | Created — movement category summary DTO |
| `api/Models/OpportunityMovementDetailDto.cs` | Created — opportunity movement detail DTO |
| `api/Services/TableStorageContext.cs` | Created — Table Storage context with typed accessors |
| `docs/BUILD_LOG.md` | Updated (this file) |
| `docs/CLIENT_UPDATE.md` | Updated — client-facing progress snapshot |
| `docs/AGENT_PROBLEMS.md` | Updated — agent problem log |

---

## Issue #2 — Authenticated App Shell and Pipeline Dashboard Page Scaffold

**Date:** 2026-02-17

### Design Decisions

- **Authenticated app shell**: Built the top navigation bar from DESIGN.md with logo (left), "Weekly Pipeline" nav item (active, with `border-b-2 border-primary`), user name display, and logout button.
- **Logo in navigation**: Used the exact placement from DESIGN.md — `<a href="/app/pipeline-dashboard.html">` wrapping the logo image (`h-8 w-auto object-contain`).
- **Page layout**: Dashboard uses `min-h-screen bg-gray-50 flex flex-col` shell with `max-w-7xl mx-auto w-full px-4 sm:px-6 lg:px-8 py-6 space-y-8` content container, per DESIGN.md.
- **Summary cards**: Three-column responsive grid (`grid grid-cols-1 md:grid-cols-3 gap-4`) for System Integration (CE), Managed Services, and Total Pipeline — each showing Starting Value, Ending Value, and Net Change placeholders.
- **Movement breakdown**: Two cards (one per opportunity type) with category rows for New, Won, Lost, Increase, Decrease, and Removed — using semantic badge colors from DESIGN.md.
- **Week selector**: Page header includes date input and Apply button, matching the Page Header pattern from DESIGN.md.
- **Empty state**: Uses the standard empty state pattern from DESIGN.md.
- **Detail area**: Hidden placeholder table for opportunity-level details, using the standard table pattern.

### Authentication & Routing

- **app/app.js**: Replaced template code with focused dashboard logic using `authService.requireAuth()` to enforce authentication, `displayCurrentUser()` for header, and `setupLogout()` for logout button.
- **app/index.html**: Replaced template placeholder with a simple redirect page to `/app/pipeline-dashboard.html` using `window.location.replace()`.
- **SWA routing**: Added `/app/` → `/app/pipeline-dashboard.html` redirect (302) in all three config files (`staticwebapp.config.json`, `staticwebapp.config.mock.json`, `staticwebapp.config.swa.json`), placed before the `/app/*` wildcard route per SWA routing rules.
- **Login redirect**: Updated `login.html` to redirect to `/app/pipeline-dashboard.html` after successful MFA instead of `/app/`.

### Responsive Behavior

- At 1280px: Full top nav with logo, horizontal nav links, user info, and logout. Summary cards in 3 columns. Week selector inline with page title.
- At 375px: Logo remains. Nav links hidden below `md`. User name hidden below `sm`. Summary cards stack to single column. Week selector stacks below title.

### Files Touched

| File | Action |
|------|--------|
| `app/pipeline-dashboard.html` | Created — authenticated dashboard shell with all placeholder sections |
| `app/app.js` | Replaced — dashboard-specific auth, user display, and logout logic |
| `app/index.html` | Replaced — simple redirect to pipeline dashboard |
| `login.html` | Updated — redirect target changed to `/app/pipeline-dashboard.html` |
| `staticwebapp.config.json` | Updated — added `/app/` redirect route |
| `staticwebapp.config.mock.json` | Updated — added `/app/` redirect route |
| `staticwebapp.config.swa.json` | Updated — added `/app/` redirect route |
| `docs/BUILD_LOG.md` | Updated (this file) |
| `docs/CLIENT_UPDATE.md` | Updated — client-facing progress snapshot |
| `docs/AGENT_PROBLEMS.md` | Updated — agent problem log |
| `docs/screenshots/02-dashboard-shell.png` | Created — dashboard screenshot at 1280×720 |

---

## Issue #1 — Establish Design Foundation and Replace Public Landing + README

**Date:** 2026-02-17

### Design Decisions

- **Layout pattern**: Used the centered card layout from DESIGN.md for the public landing page (`min-h-screen bg-gray-50 flex items-center justify-center px-4` with inner card `max-w-xl mx-auto bg-white rounded-lg shadow-sm border border-gray-200 p-8 text-center`).
- **Head block**: Applied the standard `<head>` block from DESIGN.md with the full color system (primary/accent/gray/semantic tokens) and DM Sans + Inter font pairing.
- **Logo placement**: Centered above the hero title per DESIGN.md specification for the landing page.
- **Component demonstration**: Used the card pattern and badge/status pill pattern from DESIGN.md to showcase pipeline movement categories (New, Won, Lost, Changes) with semantic colors.
- **Navigation**: Public pages use a simple centered layout without the full app navigation bar, as specified in DESIGN.md ("Public pages use simpler headers without the app nav").
- **Login link**: Links to `/login.html` rather than `/app/` since unauthenticated users need to sign in first.

### Files Touched

| File | Action |
|------|--------|
| `index.html` | Replaced template placeholder with application-specific landing page |
| `README.md` | Replaced template placeholder with app-specific documentation |
| `docs/BUILD_LOG.md` | Created (this file) |
| `docs/CLIENT_UPDATE.md` | Created — client-facing progress snapshot |
| `docs/AGENT_PROBLEMS.md` | Created — agent problem log |
| `docs/screenshots/01-homepage.png` | Created — homepage screenshot |
