# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

Run commands from repository root. Solution file is `src/CivicConnect.slnx`.

```bash
# Restore and build (analyzers and warnings-as-errors run during build)
dotnet restore src/CivicConnect.slnx
dotnet build src/CivicConnect.slnx

# Run web application
dotnet run --project src/CivicConnect.Web

# Run all tests
dotnet test src/CivicConnect.slnx

# Run one test class
dotnet test src/CivicConnect.Tests/CivicConnect.Tests.csproj --filter "FullyQualifiedName~CivicConnect.Tests.RequestControllerTests"

# Run one test method
dotnet test src/CivicConnect.Tests/CivicConnect.Tests.csproj --filter "FullyQualifiedName=CivicConnect.Tests.RequestControllerTests.UpdateState_ClosedToOpen_ReturnsBadRequest"

# Check formatting (repository's lint/format gate)
dotnet format src/CivicConnect.slnx --verify-no-changes --no-restore

# Apply EF Core migrations
dotnet ef database update --project src/CivicConnect.Web --startup-project src/CivicConnect.Web

# Check NuGet vulnerabilities as CI does
dotnet list src/CivicConnect.slnx package --vulnerable --include-transitive --format json --output-version 1 > vulnerabilities.json
python scripts/check-vulnerabilities.py vulnerabilities.json
```

CI builds Release with `--no-restore`, applies migrations, verifies formatting, runs vulnerability self-tests/checks, then runs tests with TRX and XPlat coverage. See `.github/workflows/test.yml` for exact CI commands.

`DatabaseConnectionTests` requires a reachable PostgreSQL database. Set `ConnectionStrings__DefaultConnection`, or legacy `DB_IP` plus `DB_NAME` (and normally `DB_USER`, `DB_PASSWORD`, `DB_PORT`, `DB_SSLMODE`). Other controller tests use EF Core InMemory locally when `ConnectionStrings__DefaultConnection` is absent. In CI, `TestWebApplicationFactory` deliberately uses the disposable PostgreSQL service and refuses missing CI database configuration.

## Architecture

CivicConnect is a .NET 10 ASP.NET Core application split into four projects with dependencies flowing `Web -> Application -> Domain`; Web also references Domain directly, and Tests reference all three.

- **Domain** contains persistence-shaped entities (`Request`, `Category`, `Comment`, `StatusHistory`) and `RequestState`. Entities are mutable POCOs with navigation collections; domain behavior currently lives outside them.
- **Application** contains access-control logic and Observer/Strategy abstractions. `CategoryValidationContext` selects an `ICategoryValidationStrategy` by category ID and falls back to non-empty title/description validation. `RequestStateNotifier` invokes all injected `IRequestStateObserver` implementations sequentially. Repository interfaces exist, but no repository implementations or consumers currently use them.
- **Web** is both API/composition root and infrastructure layer. `Program.cs` configures JWT authentication, PostgreSQL/Npgsql, Swagger in Development, `/health`, and DI. `RequestsController` performs workflow and EF Core access directly: create requests, fetch requests subject to ownership/admin access, update state/history, and notify observers after persistence. `CivicConnectDbContext` exposes all entity sets and seeds three categories; migrations live beside it in `CivicConnect.Web/Migrations`.
- **Tests** combine unit tests for entities/patterns with in-process API tests through `WebApplicationFactory<Program>`. `TestWebApplicationFactory` replaces Npgsql with EF Core InMemory outside CI and replaces JWT with `TestAuthenticationHandler`; use `AsRequester`, `AsStaff`, `AsManager`, or `AsAnonymous` helpers to set test identity headers. Production JWT validation is not exercised by these API tests.

Concrete `Category1ValidationStrategy` and `LogRequestStateObserver` classes exist, but `Program.cs` currently registers only `CategoryValidationContext` and `RequestStateNotifier`, not those concrete implementations. Do not assume their behavior runs through HTTP requests unless registrations are added.

## Configuration and conventions

`EnvLoader` loads the first `.env` found from content/current/base directories up to repository root, without overwriting existing process variables. Database configuration precedence is individual `DB_*` values when both host and database are present, then `ConnectionStrings:DefaultConnection`, then local PostgreSQL defaults. JWT settings come from `JWT_ISSUER`, `JWT_AUDIENCE`, and `JWT_SECRET`.

`Directory.Build.props` enables .NET recommended analyzers, treats warnings as errors, and enforces code style for every project. `.editorconfig` contains narrow exceptions for underscore-style xUnit test names and generated EF migration code.

Changes must follow `docs/CONTRIBUTING.md`: work on a lowercase kebab-case prefixed branch, use imperative commit subjects with issue/defect references where applicable, and retain PR traceability/verification and AI-usage records. `main` requires two non-author approvals.
