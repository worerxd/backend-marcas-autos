# MarcasAutos API

REST API for listing car brands. The application stores brands in PostgreSQL, applies Entity Framework Core migrations on startup, and exposes a small HTTP surface for local development and technical review.

## Technology stack

- .NET 10 and ASP.NET Core
- Entity Framework Core 10
- PostgreSQL 17 with Npgsql
- xUnit and `Microsoft.AspNetCore.Mvc.Testing`
- Coverlet collector
- Docker and Docker Compose

## Project structure

```text
.
├── Dockerfile
├── docker-compose.yml
├── .env.example
├── MarcasAutos.slnx
├── dotnet-tools.json
├── src/MarcasAutos.Api
│   ├── Controllers/MarcasAutosController.cs
│   ├── Data/MarcasAutosDbContext.cs
│   ├── Data/DatabaseMigrationExtensions.cs
│   ├── Models/MarcaAuto.cs
│   ├── Migrations/
│   └── Program.cs
└── tests/MarcasAutos.Tests
    ├── MarcasAutosControllerTests.cs
    ├── MarcasAutosApiFactory.cs
    ├── MarcasAutosApiTests.cs
    └── coverlet.runsettings
```

`MarcasAutos.slnx` is the solution file. The API project publishes on its own; the image does not require a `.sln`.

## Prerequisites

- .NET 10 SDK
- Docker Engine with Docker Compose v2

The local `dotnet-ef` tool is declared in `dotnet-tools.json` (version 10.0.4). Install it when you need to add migrations from the host:

```bash
dotnet tool restore
```

## Environment configuration

Copy the development template and keep the real file local:

```bash
cp .env.example .env
```

`.env` is ignored by Git. `.env.example` is committed and contains local development values only. Do not reuse that password outside this environment.

| Variable | Used by |
|---|---|
| `POSTGRES_USER` | PostgreSQL container |
| `POSTGRES_PASSWORD` | PostgreSQL container and the API connection string |
| `POSTGRES_DB` | PostgreSQL database name. The development value is `MarcasAutosDb`. |

Compose builds the API connection string as:

```text
Host=postgres;Port=5432;Database=${POSTGRES_DB};Username=${POSTGRES_USER};Password=${POSTGRES_PASSWORD}
```

`Host=postgres` is the Compose service name. Containers do not use `localhost` to reach each other.

`appsettings.json` does not store a connection string. To run the API on the host, outside Docker, set `ConnectionStrings__DefaultConnection` yourself. The commented example in `.env.example` points at `localhost`.

## Docker Compose

From the repository root, with `.env` present:

```bash
docker compose build
docker compose up -d
docker compose ps
```

Compose starts two services on the `marcas-autos` network:

| Service | Image | Host port | Role |
|---|---|---|---|
| `postgres` | `postgres:17` | `5432` | Database. Data is stored in the `postgres_data` volume. |
| `api` | Built from `Dockerfile` | `8080` | ASP.NET Core app listening on port `8080` inside the container. |

PostgreSQL defines a `pg_isready` healthcheck. The API starts only after that check reports `service_healthy`.

The image is multi-stage. `mcr.microsoft.com/dotnet/sdk:10.0` publishes the API. `mcr.microsoft.com/dotnet/aspnet:10.0` runs it. `ASPNETCORE_HTTP_PORTS` is set to `8080`.

The Compose file sets `ASPNETCORE_ENVIRONMENT` to `Development`, so the OpenAPI document is mapped.

## API endpoints

| Method | Path | Response |
|---|---|---|
| `GET` | `/api/marcasautos` | `200` and a JSON array of brands (`id`, `nombre`, `paisOrigen`). |
| `GET` | `/weatherforecast` | `200` and five forecast objects. This endpoint comes from the ASP.NET Core template and is covered by integration tests. |
| `GET` | `/openapi/v1.json` | `200` in Development. Not mapped in Production. |

Example:

```bash
curl http://localhost:8080/api/marcasautos
```

## Database migrations

Migrations live in `src/MarcasAutos.Api/Migrations`, including `MarcasAutosDbContextModelSnapshot`.

On startup, `Program.cs` calls `ApplyMigrationsAsync()`. That extension applies `Database.MigrateAsync()` only when the provider is relational. The EF Core InMemory provider used by tests is skipped. If Npgsql cannot connect, the extension retries up to five times.

The published assembly already contains the migrations, so the runtime image does not include the .NET SDK or `dotnet-ef`.

## Seed data

`InitialCreate` inserts three brands:

| Id | Nombre | PaisOrigen |
|---|---|---|
| 1 | Toyota | Japón |
| 2 | BMW | Alemania |
| 3 | Ford | Estados Unidos |

The same rows are declared with `HasData` on `MarcasAutosDbContext`. A database that already has `20261124053145_InitialCreate` in `__EFMigrationsHistory` is left as-is.

## Tests

Unit tests construct `MarcasAutosController` with an isolated InMemory database (`Guid` database name per test).

Integration tests boot the real pipeline through `WebApplicationFactory<Program>`. Each factory replaces the Npgsql registration with its own InMemory database. `Program` is a public partial class so the test project can reference the entry point. Development and Production share one factory type; OpenAPI is the behavior that changes between those environments.

```bash
dotnet test
```

## Code coverage

`coverlet.collector` 6.0.4 is configured in `tests/MarcasAutos.Tests/coverlet.runsettings`. The test project sets `RunSettingsFilePath`, so this command uses that file:

```bash
dotnet test --collect:"XPlat Code Coverage"
```

Coverlet excludes:

- `[MarcasAutos.Api]MarcasAutos.Api.Migrations.*`
- `[MarcasAutos.Api]Microsoft.AspNetCore.OpenApi.Generated.*`

Generated migration code and the OpenAPI source generator stay out of the percentage. Application code, including `Program.cs`, stays in the report. Coverage output is written under `TestResults/`, which is gitignored.

## Architectural decisions

- One web project is enough for a single read endpoint, a `DbContext`, and the startup pipeline. The controller depends on `MarcasAutosDbContext` directly.
- PostgreSQL remains the production provider. Tests replace `IDbContextOptionsConfiguration<MarcasAutosDbContext>` in the test host so Npgsql and InMemory are not registered together. EF Core 10 accumulates every `AddDbContext` configuration.
- Migrations run before the process serves traffic, outside the controller, and only for a relational provider.
- Credentials are supplied with environment variables. The Compose file and `appsettings.json` do not hard-code the database password.

## Runtime messages

Two messages can appear while the container is healthy:

- `Cannot load library libgssapi_krb5.so.2`. Npgsql probes for Kerberos/GSSAPI. This setup authenticates with a username and password, and the query still runs.
- `Failed to determine the https port for redirect`. The container listens on HTTP port `8080` only. `UseHttpsRedirection()` does not redirect the request. The `https` profile in `Properties/launchSettings.json` remains available for `dotnet run` on the host.
