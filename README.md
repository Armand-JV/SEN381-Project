# CivicConnect

CivicConnect is a citizen service request management platform built for the SEN381 project.

## Implementation Status (M3)
- **Architecture**: Separated Domain, Application, and Web modules.
- **Persistence**: PostgreSQL via Entity Framework Core with schema migrations created.
- **Authentication**: JWT bearer with role-based authorization (`REQUESTER`, `STAFF`, `MANAGER`/`ADMIN`); test-header identities in the `Testing` environment only.
- **Design Patterns**:
  - *Observer*: Request status changes emit events that are caught by observers (e.g. notification). View `RequestStateNotifier` and `IRequestStateObserver`. Log-only, best-effort delivery (see TD-03 in `docs/Technical-Debt-Register.md`).
  - *Strategy*: Category-specific validations have abstracted strategies. View `CategoryValidationContext` and `ICategoryValidationStrategy`.
  - *State policy*: Legal status transitions live in the central `RequestStateTransitionPolicy` table (not State objects).
- **Frontend**: React 19 + Vite SPA (`ClientApp`), built output served from `wwwroot` with SPA fallback; `/` serves the app, `/swagger` the API docs.
- **Management slice (P3)**: dashboard aggregation, append-only audit trail, stalled oversight + reassignment, category administration, staging/perf/release evidence.

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
All endpoints return JSON problem details on errors. Role abbreviations: REQ = Requester, STF = Staff, MGR = Manager/Admin.
- `POST /api/requests` (auth): submit a request (validated, requester from principal).
- `GET /api/requests/{id}` (REQ own / STF / MGR): request detail; internal comments hidden from requesters.
- `GET /api/requests/mine` (REQ): own requests, paged.
- `GET /api/requests/{id}/history` (owner / STF / MGR): status history timeline.
- `PUT /api/requests/{id}/state` (STF / MGR): transition through the policy table (reason required for reject/cancel).
- `PUT /api/requests/{id}/assign` (STF / MGR): assign to staff, Open becomes Assigned.
- `POST /api/requests/{id}/comments` (REQ public-only own / STF): public/internal notes.
- `POST /api/requests/{id}/feedback` (REQ owner, Resolved/Closed only): rating 1-5, one per request.
- `GET /api/categories` (auth): seeded category list.
- `GET /api/staff/requests` (STF / MGR): queue with state/category/assignee filters + paging.
- `GET /api/reports/requests-summary` (MGR): counts by state/category with filters.
- `GET /api/audit` (MGR): append-only audit trail, filterable by entity.
- `GET /api/management/stalled` (MGR): Open/Assigned requests older than N days.
- `PUT /api/management/requests/{id}/reassign` (MGR): reassign with history + audit.
- `POST /api/management/categories`, `PUT /api/management/categories/{id}` (MGR): category administration.
- `GET /health` (anonymous): database connectivity. `GET /api/version` (anonymous): build/commit/environment.

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
6. Access Swagger UI at `http://localhost:5287/swagger` (http profile) to interact with the API endpoints. The API is served on `http://localhost:5287` (or `https://localhost:7121` with the https profile).

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
3. Run with the staging environment (no launch profile, so the environment is honest):
   ```powershell
   $env:ASPNETCORE_ENVIRONMENT = "Staging"
   dotnet run --project src/CivicConnect.Web --no-launch-profile --urls http://localhost:5287
   ```
4. Verify: `/swagger/index.html` serves the API docs, `/health` reports database connectivity.

| Variable | Required | Purpose |
|---|---|---|
| `DB_NAME`, `DB_USER`, `DB_PASSWORD`, `DB_IP`, `DB_PORT`, `DB_SSLMODE` | Yes | PostgreSQL connection (no defaults committed) |
| `JWT_ISSUER`, `JWT_AUDIENCE`, `JWT_SECRET` | Yes | JWT signing/validation |
| `ASPNETCORE_ENVIRONMENT` | Yes | `Development`, `Staging`, or `Production` (controls Swagger + error pages) |

## Known limitations
- No production hosting, TLS certificate story, or real identity provider (JWT demo scheme only).
- Notifications are log-only; no outbox/retry (TD-03). No optimistic concurrency token (TD-02).
- Docker staging (`docker-compose.staging.yml`) is written but `compose up` still needs a Docker host to verify.
- FR-015/FR-016 acceptance criteria were authored by P3 (no baseline wording existed); dashboard age buckets deferred.

## Design documents and registers
- PED v2.0: `docs/Milestone 2/CivicConnect_PED_v2.pdf` (PED v3.0 assembly in progress — sections land per milestone issues).
- Requirements traceability: `docs/RTM-M3.md` (continued by owners; FR-014–FR-016 + NFR-020 rows: P3).
- P3 decisions: `docs/P3-Decisions.md`. AI usage: `docs/P3-AI-Usage-Register.md`.
- Technical Debt Register: `docs/Technical-Debt-Register.md`. Test evidence: `docs/Tests/`.

## Governance
This project follows strict branch protection on `main`. Pull requests require at least two approvals. All substantive code must trace back to the PED requirements.

See [CONTRIBUTING.md](CONTRIBUTING.md) for branch naming standards (e.g. `feature/staff-rbac-slice`, `feature/ci-pipeline`), PR templates, verification checklists, and AI usage logging.
