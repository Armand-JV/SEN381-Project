# CivicConnect

CivicConnect is a citizen service request management platform built for the SEN381 project.

## Milestone 2 Implementation Status
- **Architecture**: Separated Domain, Application, and Web modules.
- **Persistence**: PostgreSQL via Entity Framework Core with initial schema migrations created.
- **Initial Design Patterns**:
  - *Observer*: Request status changes emit events that are caught by observers (e.g. notification). View `RequestStateNotifier` and `IRequestStateObserver`.
  - *Strategy*: Category-specific validations have abstracted strategies. View `CategoryValidationContext` and `ICategoryValidationStrategy`.

## Technology Stack
- **Framework:** .NET 10 (ASP.NET Core API / MVC)
- **Database:** PostgreSQL (Npgsql)
- **ORM:** Entity Framework Core
- **API Documentation:** Swagger

## Project Structure
- `CivicConnect.Domain`: Core entities (Request, Category, Comment, Enums)
- `CivicConnect.Application`: Business logic, interfaces, repositories, and design patterns (Observer, Strategy)
- `CivicConnect.Web`: API Controllers, EF Core DbContext, Migrations
- `src/CivicConnect.Web/ClientApp`: React + TypeScript + Vite SPA frontend
- `CivicConnect.Tests`: Unit tests (xUnit, Moq, FluentAssertions)

## API Endpoints
- `POST /api/requests`: Create a new request.
- `GET /api/requests/{id}`: Get a request by ID.
- `PUT /api/requests/{id}/state`: Update the state of a request.

## Commands

### Build
```bash
dotnet build
```

### Run
```bash
# Backend
dotnet run --project src/CivicConnect.Web

# Frontend (dev server with API proxy)
cd src/CivicConnect.Web/ClientApp
npm install
npm run dev
```

### Tests
```bash
dotnet test
```

### CI-equivalent checks
```bash
dotnet restore src/CivicConnect.slnx
dotnet build src/CivicConnect.slnx --configuration Release --no-restore
dotnet ef database update --project src/CivicConnect.Web --startup-project src/CivicConnect.Web --configuration Release --no-build
dotnet format src/CivicConnect.slnx --verify-no-changes --no-restore
dotnet test src/CivicConnect.slnx --configuration Release --no-build --logger "trx;LogFileName=test-results.trx" --results-directory TestResults --collect "XPlat Code Coverage"
dotnet list src/CivicConnect.slnx package --vulnerable --include-transitive
```

CI uses disposable PostgreSQL service values through `ConnectionStrings__DefaultConnection`, applies migrations before tests, and uploads TRX/Cobertura results. Local relational checks require PostgreSQL and `dotnet-ef`; the normal controller tests use an in-memory database. CI credentials are test-only environment values. Never commit production credentials or `.env` files.

## Setup Instructions

### Prerequisites
- .NET 10.0 SDK
- PostgreSQL 18
- `dotnet ef` tools (`dotnet tool install --global dotnet-ef`)
- Node.js 22+ (for the React/Vite frontend)

### Running the backend locally
1. Clone the repository and navigate to the repository root.
2. Ensure PostgreSQL is running.
3. Configure database credentials in `.env` (copy from `.env.example`):
   ```env
   DB_NAME: civicconnect_db
   DB_USER: civicconnect_admin
   DB_PASSWORD: your_password_here
   DB_IP: armanddatabase.webhop.me
   DB_PORT: 5432
   DB_SSLMODE: require
   ```
4. Run EF database migrations:
   ```bash
   dotnet ef database update --project src/CivicConnect.Web --startup-project src/CivicConnect.Web
   ```
5. Run the backend application:
   ```bash
   dotnet run --project src/CivicConnect.Web
   ```
6. Access Swagger UI at `https://localhost:<port>/swagger` to interact with the API endpoints. The API is served on `https://localhost:5287` (or `http://localhost:5287` if HTTPS is disabled).

### Running the frontend locally
The frontend is a React + TypeScript + Vite single-page application in `src/CivicConnect.Web/ClientApp/`. It proxies API requests to the backend during development.

1. Ensure Node.js 22+ is installed.
2. Install dependencies (from the `src/CivicConnect.Web/ClientApp/` directory):
   ```bash
   cd src/CivicConnect.Web/ClientApp
   npm install
   ```
3. Start the Vite dev server:
   ```bash
   npm run dev
   ```
4. Open `http://localhost:5173` in a browser. The dev server proxies `/api` and `/health` requests to `http://localhost:5287`.

### Building the frontend for production
The Vite build outputs static assets to `src/CivicConnect.Web/wwwroot/`, which ASP.NET Core serves in production via `MapFallbackToFile("/index.html")`.

```bash
cd src/CivicConnect.Web/ClientApp
npm run build
```

In production, start only the backend:
```bash
dotnet run --project src/CivicConnect.Web
```
The backend serves both the API and the statically-built React UI.

### Running staging (`ASPNETCORE_ENVIRONMENT=Staging`)
1. Copy `.env.example` to `.env` and fill in real values. Secrets come **only** from
   the environment (`.env` is gitignored); `appsettings.Staging.json` holds
   non-secret settings (logging, hosts) and Swagger is enabled for Staging.
2. Apply migrations: `dotnet ef database update` (run against the staging database).
3. Run with the staging environment:
   ```powershell
   $env:ASPNETCORE_ENVIRONMENT = "Staging"
   dotnet run --project src/CivicConnect.Web --launch-profile http
   ```
4. Verify: `/swagger/index.html` serves the API docs, `/health` reports database connectivity.

| Variable | Required | Purpose |
|---|---|---|
| `DB_NAME`, `DB_USER`, `DB_PASSWORD`, `DB_IP`, `DB_PORT`, `DB_SSLMODE` | Yes | PostgreSQL connection (no defaults committed) |
| `JWT_ISSUER`, `JWT_AUDIENCE`, `JWT_SECRET` | Yes | JWT signing/validation |
| `ASPNETCORE_ENVIRONMENT` | Yes | `Development`, `Staging`, or `Production` (controls Swagger + error pages) |

## Governance
This project follows strict branch protection on `main`. Pull requests require at least two approvals. All substantive code must trace back to the PED requirements.

See [CONTRIBUTING.md](CONTRIBUTING.md) for branch naming standards (e.g. `feature/staff-rbac-slice`, `feature/ci-pipeline`), PR templates, verification checklists, and AI usage logging.
