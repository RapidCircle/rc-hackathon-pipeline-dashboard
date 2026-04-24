<p align="center">
  <img src="docs/assets/logo.jpg" alt="Rapidcircle Logo" height="60">
</p>

# Pipeline Change Analysis Dashboard

A weekly pipeline change analysis tool for Dynamics CRM. It helps sales leadership and operations teams understand how the open weighted sales pipeline changes week over week, with breakdowns by opportunity type.

**Client:** [Rapid Circle](docs/CLIENT.md)

## Project Status

🚧 **In Development** — MVP is actively being built.

## Features

- **Weekly Pipeline Overview** — View starting and ending open pipeline weighted value for a selected week, split by opportunity type (System Integration (CE) and Managed Services).
- **Movement Categorization** — Break down net change into categories: New, Won, Lost, Weighted Value Increase, Weighted Value Decrease, and Removed.
- **Opportunity-Level Detail** — Drill into individual opportunities behind each movement category, including customer, title, owner, final sales stage, and weighted revenue.

## Tech Stack

| Layer | Technology |
|-------|------------|
| Frontend | Vanilla JavaScript, HTML, CSS (Tailwind via CDN) |
| Backend | .NET 8 Azure Functions (isolated worker) |
| Storage | Azure Table Storage + Blob Storage |
| Hosting | Azure Static Web Apps |
| Authentication | Mock auth (development) / Azure AD (production) |

## Getting Started

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) (for SWA CLI)
- [Azure CLI](https://docs.microsoft.com/cli/azure/install-azure-cli)
- [SWA CLI](https://azure.github.io/static-web-apps-cli/)

### Local Development

1. Clone this repository
2. Run `./tools/init-local-settings.sh` to set up local configuration
3. Start the development server:
   ```bash
   ./scripts/swa-start.sh
   ```
4. Open [http://127.0.0.1:4280](http://127.0.0.1:4280)

### Authentication (Local)

Local development uses mock authentication. On the login page, select a demo persona or enter an email, then use MFA code `123456`.

## Documentation

- [Specification](docs/SPECIFICATION.md) — Detailed requirements and features
- [Client Information](docs/CLIENT.md) — Company details and branding
- [Design System](docs/DESIGN.md) — Tailwind config, component patterns, and layouts
- [Build Log](docs/BUILD_LOG.md) — Development decisions and progress

## Architecture

```
/                    Public landing page
/login.html          Authentication page
/app/                Authenticated application pages
/api/                .NET Azure Functions backend
/docs/               Documentation and design assets
/vendor/             Third-party libraries (Tailwind, fonts)
```

---

Built with ❤️ by [Rapid Circle](https://rapidcircle.com)
