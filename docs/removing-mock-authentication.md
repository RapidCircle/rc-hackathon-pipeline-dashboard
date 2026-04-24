# Removing Mock Authentication

This document describes how to permanently remove mock authentication from the application, leaving only Azure Static Web Apps (SWA) authentication.

## Overview

The application is structured so that mock authentication can be completely removed by **running a script** — no code modifications are required.

## Quick Removal (Recommended)

Run the removal script from the repository root:

```bash
./tools/remove-mock-auth.sh
```

This script will:
1. Delete all mock authentication backend files
2. Delete the mock login page and demo data
3. Remove the mock config template
4. Rename `staticwebapp.config.swa.json` → `staticwebapp.config.json`

## Configuration Files

The application uses three SWA configuration files:

| File | Purpose |
|------|---------|
| `staticwebapp.config.mock.json` | Template for mock auth (supports both modes) |
| `staticwebapp.config.swa.json` | Template for SWA-only auth |
| `staticwebapp.config.json` | **Active config** (copy of mock by default) |

After running the removal script:
- `staticwebapp.config.mock.json` is deleted
- `staticwebapp.config.json` (active) is deleted  
- `staticwebapp.config.swa.json` is renamed to `staticwebapp.config.json`

## Manual Removal

If you prefer to remove files manually:

### Backend Files

```bash
rm api/Auth/MockAuthProvider.cs
rm api/Auth/MockAuthRegistration.cs
rm api/Functions/MockAuthFunctions.cs
rm api/Functions/DemoAdminFunctions.cs
rm api/Services/SampleDataSeeder.cs
rm api/Models/AuthSession.cs
```

### Frontend Files

```bash
rm login.html
rm -rf data/
```

### Configuration Files

```bash
rm staticwebapp.config.mock.json
rm staticwebapp.config.json
mv staticwebapp.config.swa.json staticwebapp.config.json
```

## What Happens After Removal

1. **Backend**: The `AuthRegistration.AddAuthProvider()` method uses reflection to look for `MockAuthRegistration`. When not found, it automatically registers `SwaAuthProvider` instead.

2. **API Endpoints**: The Azure Functions runtime auto-discovers function classes. When `MockAuthFunctions.cs` and `DemoAdminFunctions.cs` are deleted, those routes simply return 404.

3. **Frontend**: The `authService.js` automatically detects the auth mode from `/api/auth/mode` and adapts accordingly. It will use SWA authentication URLs (`/.auth/login/aad`, `/.auth/logout`).

4. **Routing**: The SWA config requires authentication for `/app/*` and `/api/*` routes (except auth endpoints).

## Files That Remain (SWA Auth)

These files handle SWA authentication and should NOT be deleted:

| File | Purpose |
|------|---------|
| `api/Auth/IAuthProvider.cs` | Auth interface |
| `api/Auth/SwaAuthProvider.cs` | SWA authentication implementation |
| `api/Auth/AuthRegistration.cs` | DI registration with automatic fallback |
| `api/Functions/AuthFunctions.cs` | Universal auth endpoints (`/auth/mode`, `/auth/me`) |
| `js/authService.js` | Frontend auth service (auto-detects mode) |
| `staticwebapp.config.json` | Active SWA config (was SWA template) |

## Reverting to Mock Auth

If you need to restore mock authentication, recover the deleted files from git:

```bash
git checkout HEAD -- \
    api/Auth/MockAuthProvider.cs \
    api/Auth/MockAuthRegistration.cs \
    api/Functions/MockAuthFunctions.cs \
    api/Functions/DemoAdminFunctions.cs \
    api/Services/SampleDataSeeder.cs \
    api/Models/AuthSession.cs \
    login.html \
    staticwebapp.config.mock.json \
    data/

# Restore mock config as active
cp staticwebapp.config.mock.json staticwebapp.config.json
```

## Architecture Notes

The delete-only removal is possible because:

1. **Reflection-based discovery**: `AuthRegistration.cs` uses `Type.GetType()` to find `MockAuthRegistration`. If the class doesn't exist, it falls back to SWA auth.

2. **Azure Functions auto-discovery**: The Functions runtime finds all `[Function]` attributes at startup. Deleted function files = deleted endpoints.

3. **Frontend auto-detection**: `authService.js` calls `/api/auth/mode` to detect the current auth mode and adapts its behavior automatically.

4. **Template-based configs**: Three config files allow switching between mock and SWA modes by simply changing which template is active.
