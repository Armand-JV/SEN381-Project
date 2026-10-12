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
cd src/CivicConnect.Web
dotnet run
```

### Tests
```bash
dotnet test
```

## Setup Instructions

### Prerequisites
- .NET 10.0 SDK
- PostgreSQL 18
- `dotnet ef` tools (`dotnet tool install --global dotnet-ef`)

### Running locally
1. Clone the repository and navigate to `src/CivicConnect.Web`.
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
   dotnet ef database update
   ```
5. Run the application:
   ```bash
   dotnet run
   ```
6. Access Swagger UI at `https://localhost:<port>/swagger` to interact with the API endpoints.

## Governance
This project follows strict branch protection on `main`. Pull requests require at least two approvals. All substantive code must trace back to the PED requirements.

See [CONTRIBUTING.md](CONTRIBUTING.md) for branch naming standards (e.g. `feature/staff-rbac-slice`, `feature/ci-pipeline`), PR templates, verification checklists, and AI usage logging.
