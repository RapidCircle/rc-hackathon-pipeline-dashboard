# Design System

## Design Tone
Professional, clean, simple, modern. Designed for business users.

## Typography

### Font Pairing
| Role | Font Family | Weights | Rationale |
|------|-------------|---------|-----------|
| Headings | "DM Sans" | 600, 700 | Modern, slightly warm geometric sans that fits a contemporary analytics dashboard and aligns with Rapid Circle’s digital/tech positioning. Strong at larger sizes for clear section and card titles. |
| Body | "Inter" | 400, 500, 600 | Highly readable and neutral, ideal for dense tabular data, labels, and explanatory copy in a reporting UI used by sales leaders and ops. Pairs well with DM Sans while keeping the interface businesslike. |

## Color System

### Brand Color Mapping
Using Rapid Circle’s palette, extended with neutral grays for enterprise-style UI.

| Token | Hex | Usage |
|-------|-----|-------|
| primary | #0050A0 | Main brand blue. Primary buttons, key links, active nav, key metrics highlights. |
| primary-light | #00AEEF | Lighter blue. Hovers, subtle highlights, secondary emphasis, chart accents. |
| primary-dark | #003A75 | Darker blue derived from primary. Button hover/active, strong emphasis text, focus states. |
| accent | #F15A24 | Brand highlight orange. Badges, warning/high-impact highlights, secondary CTAs. |
| neutral | #FFFFFF | Card and surface backgrounds. |
| gray-50 | #F9FAFB | App background, subtle section backgrounds. |
| gray-100 | #F3F4F6 | Table header background, subtle borders. |
| gray-200 | #E5E7EB | Standard borders, dividers. |
| gray-300 | #D1D5DB | Input borders, disabled states. |
| gray-500 | #6B7280 | Muted text, secondary labels. |
| gray-700 | #374151 | Body text. |
| gray-900 | #111827 | Headings, high-emphasis text (also matches brand accent dark). |
| success | #16A34A | Success states, positive changes. |
| success-light | #DCFCE7 | Success background. |
| error | #DC2626 | Error states. |
| error-light | #FEE2E2 | Error background. |
| warning | #D97706 | Warning states. |
| warning-light | #FFFBEB | Warning background. |
| info | #2563EB | Informational states. |
| info-light | #EFF6FF | Info background. |

### Color Usage Rules
| Context | Token / Class |
|---------|--------------|
| Page background | `bg-gray-50` |
| Card background | `bg-white` |
| Primary button | `bg-primary text-white hover:bg-primary-dark disabled:bg-gray-300` |
| Secondary button | `border-gray-300 text-gray-700 hover:bg-gray-50` |
| Destructive button | `bg-error text-white hover:bg-red-700` |
| Body text | `text-gray-700` |
| Headings | `text-gray-900` |
| Muted text | `text-gray-500` |
| Links | `text-primary hover:text-primary-dark` |
| Borders | `border-gray-200` |
| Table header background | `bg-gray-50` |
| Table row hover | `hover:bg-gray-50` |
| Error | `text-error bg-error-light border-error/40` |
| Warning | `text-warning bg-warning-light border-warning/40` |
| Success | `text-success bg-success-light border-success/40` |
| Info | `text-info bg-info-light border-info/40` |
| Active nav item | `bg-primary/10 text-primary border-l-2 border-primary` (for sidebar) or `border-b-2 border-primary text-gray-900` (for top nav) |
| Selected table row | `bg-primary/5` |
| Category badges (e.g., “New”, “Won”) | Use `bg-*` and `text-*` based on semantic meaning (success/info/warning/error). |

## Standard Head Block

The exact HTML every page must include:

```html
<meta charset="UTF-8">
<meta name="viewport" content="width=device-width, initial-scale=1.0">
<link rel="stylesheet" href="/vendor/fonts.css">
<script src="/vendor/tailwindcss.js"></script>
<script>
tailwind.config = {
  theme: {
    extend: {
      colors: {
        primary: { DEFAULT: '#0050A0', light: '#00AEEF', dark: '#003A75' },
        accent: '#F15A24',
        neutral: '#FFFFFF',
        'gray-50': '#F9FAFB',
        'gray-100': '#F3F4F6',
        'gray-200': '#E5E7EB',
        'gray-300': '#D1D5DB',
        'gray-500': '#6B7280',
        'gray-700': '#374151',
        'gray-900': '#111827',
        success: '#16A34A',
        'success-light': '#DCFCE7',
        error: '#DC2626',
        'error-light': '#FEE2E2',
        warning: '#D97706',
        'warning-light': '#FFFBEB',
        info: '#2563EB',
        'info-light': '#EFF6FF'
      },
      fontFamily: {
        heading: ['"DM Sans"', 'system-ui', 'sans-serif'],
        body: ['Inter', 'system-ui', 'sans-serif'],
      }
    }
  }
}
</script>
```

## Logo

### Placement
| Location | HTML | Notes |
|----------|------|-------|
| Navigation (top-left, app shell) | `<a href="/app/pipeline-dashboard.html" class="flex items-center gap-2"><img src="/docs/assets/logo.jpg" alt="Rapidcircle Logo" class="h-8 w-auto object-contain" /></a>` | Placed in the left side of the top nav bar. Clicking returns to main dashboard. |
| Landing page header / hero | `<img src="/docs/assets/logo.jpg" alt="Rapidcircle Logo" class="h-10 w-auto object-contain mx-auto mb-6" />` | Centered above hero title on `index.html`. |

### Logo Path
`/docs/assets/logo.jpg` — sourced from CLIENT.md

## Navigation

### Pattern
Top navigation bar (3 main pages: landing, login, pipeline dashboard; authenticated app shell only for `/app/*`).

### Logo in Navigation
```html
<header class="border-b border-gray-200 bg-white">
  <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 h-14 flex items-center justify-between">
    <div class="flex items-center gap-6">
      <a href="/app/pipeline-dashboard.html" class="flex items-center gap-2">
        <img src="/docs/assets/logo.jpg" alt="Rapidcircle Logo" class="h-8 w-auto object-contain" />
      </a>
      <nav class="hidden md:flex items-center gap-4 text-sm font-medium">
        <a href="/app/pipeline-dashboard.html" class="border-b-2 border-primary text-gray-900 pb-3">Weekly Pipeline</a>
      </nav>
    </div>
    <div class="flex items-center gap-4">
      <span class="hidden sm:inline text-sm text-gray-500" id="current-user-name"></span>
      <button id="logout-button" class="text-sm text-gray-600 hover:text-gray-900">Logout</button>
    </div>
  </div>
</header>
```

### Structure
For the authenticated app shell (`/app/pipeline-dashboard.html`):

| Section | Route | Icon (optional) |
|---------|-------|-----------------|
| Weekly Pipeline | `/app/pipeline-dashboard.html` | None in MVP; text-only nav item. |

Public pages (`/index.html`, `/login.html`) use simpler headers without the app nav; logo appears in the hero/header as specified.

### Responsive Behavior
- On `md` and up: full top nav with logo, horizontal nav links, user info, logout.
- Below `md`:
  - Logo remains left-aligned.
  - Nav links collapse to a single primary link or hidden; for MVP, only “Weekly Pipeline” is needed, so it can remain visible as a single text button.
  - User name can be hidden on very small screens (`hidden sm:inline`), leaving only logout icon/text.
- No hamburger menu needed for MVP due to single main app section.

## Page Layouts

| Page Type | Layout | Key Classes |
|-----------|--------|-------------|
| Landing (`/index.html`) | Centered column with logo, title, short description, “Seed Test Data” primary button, and link to login. Plenty of whitespace. | `min-h-screen bg-gray-50 flex items-center justify-center px-4` for outer; inner `max-w-xl mx-auto bg-white rounded-lg shadow-sm border border-gray-200 p-8 text-center` |
| Login (`/login.html`) | Simple centered form card with app name, fields, and actions. | `min-h-screen bg-gray-50 flex items-center justify-center px-4`; card `max-w-md w-full bg-white rounded-lg shadow-sm border border-gray-200 p-8` |
| Dashboard (`/app/pipeline-dashboard.html`) | Top nav + main content: page header with week selector, summary stat cards (System Integration (CE), Managed Services, Total) in responsive grid, then movement breakdown per opportunity type with category sections and expandable tables. | Shell: `min-h-screen bg-gray-50 flex flex-col`; content container: `max-w-7xl mx-auto w-full px-4 sm:px-6 lg:px-8 py-6 space-y-8`; summary grid: `grid grid-cols-1 md:grid-cols-3 gap-4`; movement sections: `space-y-6` with cards. |
| Detail within dashboard (movement categories) | Each opportunity type has a card with tabs or stacked sections for categories (New, Won, Lost, Increase, Decrease, Removed). Each category has a header row with total amount and a collapsible detail table. | Category header: `flex items-center justify-between cursor-pointer py-3 border-b border-gray-200`; detail: `mt-3 overflow-x-auto` with table pattern. |
| Form (week selection) | Inline form controls in page header: week start date input or select plus “Apply” button. | Header: `flex flex-col gap-4 md:flex-row md:items-center md:justify-between`; controls: `flex flex-wrap items-center gap-3` with inputs using form pattern. |

## Component Patterns

### Card
```html
<div class="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
  <!-- content -->
</div>
```

### Button (Primary)
```html
<button class="inline-flex items-center justify-center bg-primary text-white px-4 py-2 rounded-lg text-sm font-medium hover:bg-primary-dark focus:outline-none focus:ring-2 focus:ring-primary/50 focus:ring-offset-1 focus:ring-offset-white disabled:bg-gray-300 disabled:cursor-not-allowed transition">
  Label
</button>
```

### Button (Secondary)
```html
<button class="inline-flex items-center justify-center border border-gray-300 text-gray-700 bg-white px-4 py-2 rounded-lg text-sm font-medium hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-gray-200 focus:ring-offset-1 focus:ring-offset-white disabled:text-gray-400 disabled:border-gray-200 disabled:bg-gray-50 disabled:cursor-not-allowed transition">
  Label
</button>
```

### Table
```html
<div class="overflow-x-auto">
  <table class="min-w-full text-left text-sm">
    <thead>
      <tr class="bg-gray-50 border-b border-gray-200 text-xs font-semibold text-gray-600 uppercase tracking-wider">
        <th class="px-4 py-3">Customer</th>
        <th class="px-4 py-3">Opportunity</th>
        <th class="px-4 py-3">Owner</th>
        <th class="px-4 py-3">Final Stage</th>
        <th class="px-4 py-3 text-right">Amount</th>
      </tr>
    </thead>
    <tbody class="divide-y divide-gray-100">
      <tr class="hover:bg-gray-50">
        <td class="px-4 py-3 text-gray-700">Contoso</td>
        <td class="px-4 py-3 text-gray-700">CRM Upgrade</td>
        <td class="px-4 py-3 text-gray-700">Alex Smith</td>
        <td class="px-4 py-3 text-gray-700">Closed - Won</td>
        <td class="px-4 py-3 text-gray-700 text-right">€120,000</td>
      </tr>
    </tbody>
  </table>
</div>
```

### Form Input
```html
<div>
  <label class="block text-sm font-medium text-gray-700 mb-1">Reporting week (start date)</label>
  <input type="date" class="w-full max-w-xs border border-gray-300 rounded-lg px-3 py-2 text-sm text-gray-700 bg-white focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary transition" />
</div>
```

### Empty State
```html
<div class="text-center py-12">
  <p class="text-gray-500 text-lg font-medium">No data for this week</p>
  <p class="text-gray-400 mt-1 text-sm">Select a different week or seed demo data to view pipeline changes.</p>
</div>
```

### Loading Spinner
```html
<div class="flex items-center justify-center py-8">
  <div class="animate-spin rounded-full h-8 w-8 border-2 border-gray-200 border-t-2 border-t-primary"></div>
</div>
```

### Toast / Notification
```html
<!-- Container (fixed, top-right) -->
<div id="toast-container" class="fixed top-4 right-4 z-50 space-y-3"></div>

<!-- Success -->
<div class="bg-success-light border border-success/60 text-success px-4 py-3 rounded-lg text-sm shadow-sm">
  <p class="font-medium">Success</p>
  <p class="mt-1">Message</p>
</div>

<!-- Error -->
<div class="bg-error-light border border-error/60 text-error px-4 py-3 rounded-lg text-sm shadow-sm">
  <p class="font-medium">Error</p>
  <p class="mt-1">Message</p>
</div>

<!-- Info -->
<div class="bg-info-light border border-info/60 text-info px-4 py-3 rounded-lg text-sm shadow-sm">
  <p class="font-medium">Info</p>
  <p class="mt-1">Message</p>
</div>
```

### Modal
```html
<div class="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
  <div class="bg-white rounded-xl shadow-xl max-w-lg w-full mx-4 p-6">
    <div class="flex items-start justify-between mb-4">
      <h3 class="text-lg font-semibold text-gray-900 font-heading">Title</h3>
      <button class="text-gray-400 hover:text-gray-600 focus:outline-none" aria-label="Close">
        <span class="sr-only">Close</span>
        <!-- Simple X icon -->
        <svg class="h-5 w-5" viewBox="0 0 20 20" fill="none">
          <path d="M6 6l8 8M14 6l-8 8" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" />
        </svg>
      </button>
    </div>
    <!-- content -->
    <div class="mt-2 text-sm text-gray-600">
      Modal body
    </div>
    <div class="mt-6 flex justify-end gap-3">
      <button class="border border-gray-300 text-gray-700 bg-white px-4 py-2 rounded-lg text-sm font-medium hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-gray-200 transition">
        Cancel
      </button>
      <button class="bg-primary text-white px-4 py-2 rounded-lg text-sm font-medium hover:bg-primary-dark focus:outline-none focus:ring-2 focus:ring-primary/50 transition">
        Confirm
      </button>
    </div>
  </div>
</div>
```

### Page Header
```html
<div class="flex flex-col gap-4 md:flex-row md:items-center md:justify-between mb-8">
  <div>
    <h1 class="text-2xl font-bold text-gray-900 font-heading">Weekly Pipeline Change</h1>
    <p class="text-gray-500 mt-1 text-sm">Analyze open weighted pipeline movements by opportunity type.</p>
  </div>
  <div class="flex flex-wrap items-center gap-3">
    <!-- Week selector -->
    <div>
      <label for="week-start" class="block text-xs font-medium text-gray-700 mb-1">Reporting week (start date)</label>
      <input id="week-start" type="date" class="w-full max-w-xs border border-gray-300 rounded-lg px-3 py-2 text-sm text-gray-700 bg-white focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary transition" />
    </div>
    <button id="apply-week" class="inline-flex items-center justify-center bg-primary text-white px-4 py-2 rounded-lg text-sm font-medium hover:bg-primary-dark focus:outline-none focus:ring-2 focus:ring-primary/50 focus:ring-offset-1 focus:ring-offset-gray-50 transition">
      Apply
    </button>
  </div>
</div>
```

### Badge / Status Pill
```html
<!-- Generic status pills -->
<span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-success-light text-success">Won</span>
<span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-error-light text-error">Lost</span>
<span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-info-light text-info">New</span>
<span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-warning-light text-warning">Removed</span>

<!-- Opportunity type pill -->
<span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-primary/10 text-primary">System Integration (CE)</span>
<span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-primary-light/10 text-primary-dark">Managed Services</span>
```

### Sidebar Nav Item
*(For this app we use top nav, but define pattern for any vertical sub-nav, e.g., settings or movement category filters if needed.)*
```html
<!-- Active -->
<a class="flex items-center gap-3 px-3 py-2 rounded-lg bg-primary/10 text-primary font-medium text-sm">
  Item
</a>
<!-- Inactive -->
<a class="flex items-center gap-3 px-3 py-2 rounded-lg text-gray-600 hover:bg-gray-100 hover:text-gray-900 text-sm transition">
  Item
</a>
```

## Spacing

| Context | Value |
|---------|-------|
| Page padding (outer) | `px-4 sm:px-6 lg:px-8 py-6` |
| Card/section padding | `p-6` (use `p-4` on small cards if needed) |
| Section gap | `space-y-8` between major sections |
| Summary card gap | `gap-4` in grids |
| Form field gap | `space-y-4` in vertical forms; `gap-3` for inline controls |
| Table cell padding | `px-4 py-3` |
| Between page header and content | `mb-8` |
| Between category header and table | `mt-3` |

## Responsive Breakpoints

| Breakpoint | Usage |
|------------|-------|
| `sm` (640px) | Increase horizontal padding, allow two-column layouts for smaller cards if needed. |
| `md` (768px) | Switch page header to side-by-side layout; show full top nav links; summary cards in 2–3 columns depending on content. |
| `lg` (1024px) | Use `max-w-7xl` content width; summary cards in 3 columns (System Integration, Managed Services, Total); movement sections more spacious. |
| `xl` (1280px) | Maintain `max-w-7xl` for comfortable reading; additional whitespace on sides. |