# Stage 1: React Tier-1 SPA (Vite builds straight into ../wwwroot).
FROM node:22-bookworm-slim AS frontend
WORKDIR /src/CivicConnect.Web/ClientApp
COPY src/CivicConnect.Web/ClientApp/package.json src/CivicConnect.Web/ClientApp/package-lock.json ./
RUN npm ci --no-audit --no-fund
COPY src/CivicConnect.Web/ClientApp/ ./
RUN npm run build

# Stage 2: .NET backend publish (includes the built wwwroot below).
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS backend
WORKDIR /src
COPY src/CivicConnect.slnx ./
COPY src/CivicConnect.Domain/CivicConnect.Domain.csproj ./CivicConnect.Domain/
COPY src/CivicConnect.Application/CivicConnect.Application.csproj ./CivicConnect.Application/
COPY src/CivicConnect.Web/CivicConnect.Web.csproj ./CivicConnect.Web/
COPY src/CivicConnect.Tests/CivicConnect.Tests.csproj ./CivicConnect.Tests/
COPY src/CivicConnect.Perf/CivicConnect.Perf.csproj ./CivicConnect.Perf/
RUN dotnet restore CivicConnect.slnx
COPY src/ ./
COPY --from=frontend /src/CivicConnect.Web/wwwroot ./CivicConnect.Web/wwwroot
RUN dotnet publish CivicConnect.Web/CivicConnect.Web.csproj -c Release -o /app/publish --no-restore

# Stage 3: runtime (M3-STG-01: compose-based local staging target).
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=backend /app/publish ./
ENV ASPNETCORE_ENVIRONMENT=Staging
EXPOSE 8080
ENTRYPOINT ["dotnet", "CivicConnect.Web.dll"]
