# Mock Authentication System

This workspace includes a **mock authentication system** for demo and development purposes. It allows testing different user personas without requiring Azure AD or other external identity providers.

## ⚠️ Important: Demo Only

This mock authentication is designed for **demonstrations and local development only**. It is the **default configuration** for local development.

For production, switch to Azure Static Web Apps built-in authentication:
1. Set `AUTH_MODE=swa` (or remove the variable)
2. Use `staticwebapp.config.prod.json` instead of the demo config

---

## Quick Start

> **Mock auth is the default for local development.** Both `AUTH_MODE=mock` in `local.settings.json` and the demo `staticwebapp.config.json` are pre-configured.

### 1. Start SWA and Seed Data

1. Run the "swa start" VS Code task
2. Navigate to `http://127.0.0.1:4280/login.html`
3. Click "Seed Data" to create demo personas
4. Select a persona or enter an email to log in
5. Use MFA code: `123456`

---

## Architecture Overview

The implementation uses an **abstraction layer** that allows seamless switching between mock auth (demo) and SWA auth (production).

### Components

| Component | Location | Purpose |
|-----------|----------|---------|
| `IAuthProvider` | [api/Auth/IAuthProvider.cs](../api/Auth/IAuthProvider.cs) | Auth abstraction interface |
| `MockAuthProvider` | [api/Auth/MockAuthProvider.cs](../api/Auth/MockAuthProvider.cs) | Mock implementation with Table Storage |
| `SwaAuthProvider` | [api/Auth/SwaAuthProvider.cs](../api/Auth/SwaAuthProvider.cs) | Production SWA implementation |
| `AuthFunctions` | [api/Functions/AuthFunctions.cs](../api/Functions/AuthFunctions.cs) | API endpoints for auth |
| `SampleDataSeeder` | [api/Services/SampleDataSeeder.cs](../api/Services/SampleDataSeeder.cs) | Demo persona seeding |
| `authService.js` | [js/authService.js](../js/authService.js) | Frontend auth abstraction |
| `login.html` | [login.html](../login.html) | Login page with persona selection |

### Auth Mode Detection

The system automatically detects which auth mode is active:

```javascript
// Frontend
await authService.initialize();
if (authService.isMockAuth()) {
    // Mock auth mode - use API endpoints
} else {
    // SWA auth mode - use /.auth endpoints
}
```

```csharp
// Backend - configured in Program.cs
var authMode = Environment.GetEnvironmentVariable("AUTH_MODE") ?? "mock";
```

---

## Switching Between Modes

### Mock Mode (Demo/Development)

```bash
# In local.settings.json or environment
AUTH_MODE=mock
```

- Uses `/api/auth/*` endpoints
- Session stored in Azure Table Storage
- Login via `/login.html`
- Personas available for quick testing

### SWA Mode (Production)

```bash
# In Azure Static Web App settings
AUTH_MODE=swa
```

- Uses `/.auth/*` endpoints (SWA built-in)
- Session managed by Azure
- Login via `/.auth/login/aad`
- Standard Azure AD authentication

---

## API Endpoints

### Auth Endpoints

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/auth/mode` | GET | Get current auth mode and URLs |
| `/api/auth/me` | GET | Get current user (works in both modes) |
| `/api/auth/login` | POST | Start login (mock only) |
| `/api/auth/mfa` | POST | Verify MFA code `123456` (mock only) |
| `/api/auth/logout` | POST | Logout (mock only) |
| `/api/auth/personas` | GET | List demo personas (mock only) |

### Demo Admin Endpoints

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/demo/seed` | POST | Seed demo personas |
| `/api/demo/reset` | POST | Reset and re-seed all data |

**Note:** Demo endpoints are blocked in production environments.

---

## Demo Personas

Demo users are loaded from the CSV file at `data/demo-users.csv`. The seeder reads this file and creates the following test users:

| Name | Email | Description |
|------|-------|-------------|
| System Admin | admin@demo.example.com | System administrator |
| Sarah Mitchell | sarah.mitchell@example.com | Standard user in good standing |
| James Chen | james.chen@example.com | User with overdue items |
| Margaret O'Brien | margaret.obrien@example.com | Pensioner with concession |
| Rachel Thompson | rachel.thompson@example.com | Multiple accounts/properties |
| Tom Harrison | tom.harrison@example.com | Property manager (3 properties) |
| Lisa Wong | lisa.wong@acmecorp.example.com | Business owner (full access) |
| David Park | david.park@acmecorp.example.com | Business admin |
| Emma Scott | emma.scott@acmecorp.example.com | Billing manager (limited access) |
| Suspended User | suspended.user@example.com | Suspended account (cannot login) |

### Customizing Personas

Edit [data/demo-users.csv](../data/demo-users.csv) to add or modify personas. The CSV format is:

```csv
UserId,Email,FullName,Status,IsSystemAdmin,Roles,PersonaDescription
user-custom,custom.user@example.com,Custom User,Active,false,"anonymous,authenticated,custom-role",Custom scenario for testing
```

**Fields:**
- `UserId` - Unique identifier for the user
- `Email` - Login email address
- `FullName` - Display name
- `Status` - `Active` or `Suspended`
- `IsSystemAdmin` - `true` or `false`
- `Roles` - Comma-separated list (quote if contains commas)
- `PersonaDescription` - Description shown on login page

### Reloading After CSV Changes

After editing `data/demo-users.csv`:
1. Go to the login page at `http://127.0.0.1:4280/login.html`
2. Click the **"Reset Users"** button (replaces all users from CSV)

**Note:** The "Reset Users" button appears when users already exist. It clears all existing demo users and reloads them from the CSV file.

### Tips for CSV Editing

- **Roles with commas**: Wrap in double quotes: `"anonymous,authenticated,admin"`
- **Special characters**: Avoid quotes within fields, or escape with double-quotes
- **Admin users**: Set `IsSystemAdmin` to `true` and include `admin` in Roles
- **Test suspended accounts**: Set `Status` to `Suspended` - user will see "Account is not active"

---

## Frontend Integration

### Using authService.js

```javascript
// Initialize (call once on page load)
await authService.initialize();

// Check if authenticated
const state = await authService.getCurrentUser();
if (!state.isAuthenticated) {
    await authService.requireAuth(); // Redirects to login
}

// Get auth headers for API calls
const headers = authService.getAuthHeaders();
fetch('/api/my-endpoint', { headers });

// Logout
await authService.logout();
```

### Making Authenticated API Calls

```javascript
async function callApi() {
    const headers = {
        'Content-Type': 'application/json',
        ...authService.getAuthHeaders()
    };
    
    const response = await fetch('/api/my-data', { headers });
    return response.json();
}
```

---

## Session Management

### Mock Mode

- Sessions stored in `AuthSessions` table in Azure Table Storage
- Temporary session (5 min) created at login, extended to 8 hours after MFA
- Session token passed via `X-Session-Token` header
- Token stored in `localStorage` as `mock_session_token`

### SWA Mode

- Sessions managed by Azure Static Web Apps
- Uses secure HTTP-only cookies
- No custom headers needed

---

## Configuration Files

### staticwebapp.config.json (Default - Demo Mode)

The default configuration allows anonymous access to auth endpoints for mock authentication:

```json
{
  "routes": [
    { "route": "/api/auth/*", "allowedRoles": ["anonymous"] },
    { "route": "/api/demo/*", "allowedRoles": ["anonymous"] },
    { "route": "/app/*", "allowedRoles": ["anonymous"] },
    { "route": "/api/*", "allowedRoles": ["anonymous"] }
  ],
  "responseOverrides": {
    "401": { "redirect": "/login.html" }
  }
}
```

### staticwebapp.config.prod.json (Production)

For production deployments, use the production config which protects all routes:

```json
{
  "routes": [
    { "route": "/app/*", "allowedRoles": ["authenticated"] },
    { "route": "/api/*", "allowedRoles": ["authenticated"] }
  ],
  "responseOverrides": {
    "401": { "redirect": "/.auth/login/aad" }
  }
}
```

**To switch to production config:**
```bash
cp staticwebapp.config.prod.json staticwebapp.config.json
```

---

## Troubleshooting

### "No personas found"

1. Click the "Seed Data" button on the login page
2. Or call `POST /api/demo/seed` directly

### "User not found" on login

The email must match a seeded persona exactly. Check the personas list or seed the data.

### Session expires quickly

Temporary sessions (before MFA) expire in 5 minutes. Complete MFA to get an 8-hour session.

### Mock auth not working in production

Mock auth endpoints return 404 in production (`AZURE_FUNCTIONS_ENVIRONMENT=Production`). This is by design.

---

## Security Notes

1. **Never use mock auth in production** - Set `AUTH_MODE=swa` for production deployments
2. **MFA code is fixed** - Always `123456`, provides no real security
3. **No password validation** - Login is email-only
4. **Demo endpoints blocked in production** - `/api/demo/*` returns 404 in production

---

## File Structure
