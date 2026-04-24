# .NET Version Upgrade/Downgrade Checklist

This document provides a comprehensive checklist of all files that need to be reviewed and updated when changing the .NET SDK version for this project.

## Quick Reference

| Current Version | Target Version | Date Updated |
|-----------------|----------------|--------------|
| .NET 9.0 | .NET 8.0 | January 29, 2026 |

---

## Files Requiring Updates

### 1. API Project Configuration

#### `api/api.csproj`
**Purpose**: Defines the target framework for the Azure Functions API project.

**What to change**:
```xml
<TargetFramework>net8.0</TargetFramework>
```

**Notes**:
- This is the primary project file that controls which .NET runtime the API targets
- After changing, run `dotnet restore` and `dotnet build` to verify compatibility
- Check that all NuGet packages support the target framework version

---

### 2. Dev Container Configuration

#### `.devcontainer/Dockerfile`
**Purpose**: Defines the development container environment with all required tools.

**What to change**:
1. Update the comment at the top (documentation)
2. Change the `--channel` parameter in the dotnet-install.sh command

**Lines to modify**:
```dockerfile
# Comment (line 2-3)
# Using Debian bookworm base with .NET 8 SDK installed manually

# Install command (line 12)
&& /tmp/dotnet-install.sh --channel 8.0 --install-dir /usr/share/dotnet \
```

**Post-change actions**:
- Rebuild the dev container: `Dev Containers: Rebuild Container` command in VS Code
- Verify with `dotnet --version` after container rebuild

---

### 3. GitHub Actions Workflows

#### `.github/workflows/copilot-setup-steps.yml`
**Purpose**: Configures the environment for GitHub Copilot coding agent.

**What to change**:
```yaml
# Comment (line 39)
# .NET 8.0 SDK Setup

# Step name (line 41)
- name: Setup .NET 8.0 SDK

# Version (line 44)
dotnet-version: "8.0.x"
```

**Notes**:
- This workflow runs when Copilot agent needs to make changes
- Must match the dev container environment for consistency

---

#### `.github/workflows/azure-static-web-apps.yml`
**Purpose**: Deploys the application to Azure Static Web Apps.

**What to change**:
- For .NET 8.0: No manual pre-build needed - Oryx supports .NET 8.0 natively
- Set `API_LOCATION: "api"` to point to source code (Oryx will build)
- For newer .NET versions not yet supported by Oryx: Use manual pre-build approach

**Notes**:
- Azure's Oryx builder natively supports .NET 8.0, so no manual pre-build is required
- If upgrading to a newer .NET version, check Oryx supported versions first

---

#### `.github/workflows/azure-function-byo.yml.disabled`
**Purpose**: (Disabled) Deploys API to Azure Functions Flex Consumption as BYO Function App.

**What to change**:
```yaml
# Environment variable (line 16)
DOTNET_VERSION: '8.0.x'
```

**Notes**:
- This file is currently disabled (`.disabled` extension)
- Still update it to maintain consistency if it's re-enabled later

---

## Files NOT Requiring Updates

### `staticwebapp.config.json`
**Why no changes needed**: 
- Contains security headers, route configurations, and authentication settings
- No .NET version references
- Independent of backend runtime version

### `swa-cli.config.json`
**Why no changes needed**:
- The `"apiVersion": "9.0"` refers to the **SWA CLI API version**, not the .NET version
- Controls how SWA CLI communicates with the API, not the runtime
- `"apiLanguage": "dotnetisolated"` specifies the runtime type, not version

### `api/Properties/launchSettings.json`
**Why no changes needed**:
- Contains debugging/launch profiles
- No version-specific settings

### `api/host.json`
**Why no changes needed**:
- Azure Functions host configuration
- Runtime version is determined by project file, not host.json

---

## Post-Upgrade Checklist

After updating all files, complete these steps:

### Local Development

- [ ] Delete `api/bin/` folder (contains old compiled artifacts)
- [ ] Delete `api/obj/` folder (contains old build intermediates)
- [ ] Rebuild dev container (`Dev Containers: Rebuild Container`)
- [ ] Verify SDK version: `dotnet --version`
- [ ] Restore packages: `dotnet restore` (in api folder)
- [ ] Build project: `dotnet build` (in api folder)
- [ ] Run SWA locally and test: Use "swa start" VS Code task
- [ ] Verify API endpoints work correctly

### CI/CD Verification

- [ ] Push changes to a feature branch
- [ ] Verify `copilot-setup-steps.yml` workflow passes
- [ ] Create a PR to verify `azure-static-web-apps.yml` workflow passes
- [ ] Check deployment logs for any .NET version errors

### Package Compatibility

- [ ] Check all NuGet packages support the target .NET version
- [ ] Run `dotnet list package --outdated` to check for updates
- [ ] Update packages if newer versions are available for the target framework

---

## Troubleshooting

### Build Errors After Version Change

1. **Clean and rebuild**:
   ```bash
   cd api
   dotnet clean
   dotnet restore
   dotnet build
   ```

2. **Delete all build artifacts**:
   ```bash
   rm -rf api/bin api/obj
   ```

3. **Check package compatibility**:
   ```bash
   dotnet list package
   ```

### Dev Container Issues

If the dev container fails to build after Dockerfile changes:

1. Check Docker logs for specific errors
2. Verify the .NET channel exists: https://dotnet.microsoft.com/download/dotnet
3. Try using an exact version instead of channel: `--version 9.0.100`

### Workflow Failures

If GitHub Actions fail:

1. Check that `actions/setup-dotnet@v4` supports your target version
2. Verify the version string format (e.g., `9.0.x` not `9.0`)
3. Check the Actions log for specific error messages

---

## Version History

| Date | From Version | To Version | Updated By |
|------|--------------|------------|------------|
| 2026-01-29 | .NET 9.0 | .NET 8.0 | GitHub Copilot |
| 2026-01-27 | .NET 10.0 | .NET 9.0 | GitHub Copilot |

---

## Related Documentation

- [.NET Version Support Policy](https://dotnet.microsoft.com/platform/support/policy)
- [Azure Functions Supported Languages](https://docs.microsoft.com/azure/azure-functions/supported-languages)
- [Azure Static Web Apps API Support](https://docs.microsoft.com/azure/static-web-apps/apis-overview)
