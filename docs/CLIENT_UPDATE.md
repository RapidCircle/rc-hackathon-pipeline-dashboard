# Client Update

## Current Status — 2026-02-17

**Status: MVP Complete ✅**

The Weekly Pipeline Change Analysis dashboard is fully built, tested, and ready for use. All core features described below have been verified end-to-end.

---

## What the Application Does

This application gives your sales leadership and sales operations team a clear, repeatable weekly view of how the open weighted sales pipeline has changed. Each week, you can see:

- **Starting and ending pipeline values** — the total open weighted pipeline (Weighted Revenue) at the beginning and end of the selected week.
- **Net change** — whether the pipeline grew or shrank, and by how much.
- **What drove the change** — every pipeline movement is grouped into one of six categories:
  - **New** — opportunities that entered the pipeline during the week.
  - **Won** — deals that were closed-won (reducing open pipeline).
  - **Lost** — deals that were closed-lost (reducing open pipeline).
  - **Increase** — existing open opportunities whose weighted value went up.
  - **Decrease** — existing open opportunities whose weighted value went down.
  - **Removed** — opportunities removed from the pipeline for other reasons.
- **Split by opportunity type** — all numbers are broken out for **System Integration (CE)** and **Managed Services** separately, plus a combined total.
- **Drill-down to individual deals** — each movement category can be expanded to show the specific opportunities behind it, including customer name, opportunity title, owner, final sales stage, and the weighted revenue amount.

---

## How to Use It Weekly

### First-Time Setup (One-Time)

1. Open the application homepage.
2. Click **"Seed Test Data"** to load demo users and two weeks of sample pipeline data.
3. Click **"Sign In to View Dashboard"**.
4. Choose a demo persona (e.g., Sarah Mitchell — Sales Leader) and complete the sign-in.

### Weekly Review Workflow

1. **Sign in** at the login page by selecting your persona.
2. Enter the MFA verification code (`123456` for the demo environment).
3. You'll land on the **Weekly Pipeline Change** dashboard.
4. **Select a week**: Enter the Monday start date (YYYY-MM-DD format) in the date picker and click **Apply**.
5. **Review the summary cards**: Starting value, ending value, and net change for System Integration (CE), Managed Services, and Total.
6. **Examine movement categories**: Scroll down to see the six movement categories for each opportunity type. The total weighted revenue change and opportunity count are shown for each category.
7. **Drill into details**: Click any movement category row to expand it and see the individual opportunities — customer, title, owner, final stage, and amount.
8. **Switch weeks**: Change the date and click Apply to compare different weeks.
9. **Sign out** when done by clicking Logout in the top-right corner.

### Tips

- The week start date must be a **Monday**. The system will warn you if you select a different day.
- If no data exists for a selected week, a friendly message will appear with an option to jump back to the last viewed week.
- Positive net changes appear in **green**; negative changes appear in **red**.
- On mobile devices, the layout adapts — summary cards stack vertically and detail tables scroll horizontally.

---

## Available Demo Data

Two weeks of sample data are pre-loaded when you seed:

| Week Starting | CE Net Change | MS Net Change | Total Net Change |
|---------------|--------------|---------------|-----------------|
| 2026-02-02 | +€65,000 | €0 | +€65,000 |
| 2026-02-09 | -€5,000 | -€75,000 | -€80,000 |

Seven demo user personas are available, representing different roles:

| Name | Role |
|------|------|
| Sarah Mitchell | Sales Leader — System Integration (CE) |
| James Chen | Sales Leader — Managed Services |
| Rachel Thompson | Sales Ops / CRM Analyst |
| Tom Harrison | Sales Rep — System Integration (CE) |
| Lisa Wong | Sales Rep — Managed Services |
| System Admin | Administrator |
| Suspended User | Test account (cannot log in) |

---

## What's Included in the MVP

- ✅ Weekly pipeline change analysis by opportunity type
- ✅ Six movement categories with weighted revenue totals
- ✅ Drill-down to individual opportunity details
- ✅ Week selector with validation and error handling
- ✅ Responsive layout for desktop (1280px) and mobile (375px)
- ✅ Demo data seeding from the homepage
- ✅ Mock authentication with demo personas
- ✅ Logout and re-login support

## What's Out of Scope (Future)

- 🔲 Multi-week trend views (e.g., last 12 weeks side-by-side)
- 🔲 Monthly aggregation view
- 🔲 Additional segmentation (by region, industry, account owner)
- 🔲 Automated weekly report scheduling and email distribution
- 🔲 Full historical change log for individual opportunities
- 🔲 Non-weighted (nominal) value reporting
- 🔲 Opportunity types beyond System Integration (CE) and Managed Services
- 🔲 Live Dynamics CRM integration (currently uses seeded demo data)
- 🔲 Production authentication (Azure AD / Entra ID)

---

## Preview

- Homepage: `docs/screenshots/01-homepage.png`
- Homepage with seed button: `docs/screenshots/10-home-seed-button.png`
- Dashboard summary: `docs/screenshots/06-summary-view.png`
- Movement details: `docs/screenshots/07-movements-detail.png`
- Week error handling: `docs/screenshots/11-week-error-handling.png`
