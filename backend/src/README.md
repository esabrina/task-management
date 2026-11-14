# Task Management Backend

The backend uses Clean Architecture, PostgreSQL, Entity Framework Core, ASP.NET Core Identity, and JWT bearer authentication.

## Requirements

- .NET 9 SDK
- Docker with the Compose plugin
- EF Core CLI 9.0.4

Install the EF Core CLI if needed:

```powershell
dotnet tool install --global dotnet-ef --version 9.0.4
```

If `dotnet-ef` is already installed, update it instead:

```powershell
dotnet tool update --global dotnet-ef --version 9.0.4
```

## Run locally

Run these commands from the repository root. PostgreSQL uses the local-development credentials configured in `docker-compose.yml` and `appsettings.json`.

```powershell
docker compose up -d

# Store a random signing key in .NET user-secrets (not in source control).
$key = [Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
dotnet user-secrets set "Jwt:Key" $key --project backend\src\TaskManagement.Api\TaskManagement.Api.csproj

# Create/update the PostgreSQL schema using the checked-in EF Core migration.
dotnet ef database update `
  --project backend\src\TaskManagement.Infrastructure\TaskManagement.Infrastructure.csproj `
  --startup-project backend\src\TaskManagement.Api\TaskManagement.Api.csproj `
  --context TaskManagementDbContext

dotnet run --project backend\src\TaskManagement.Api\TaskManagement.Api.csproj
```

The API requires `Jwt:Key` to be configured and validates JWTs with the configured issuer and audience. Tokens expire after 30 minutes. The default CORS origin is `http://localhost:4200`; change `AllowedOrigins` in the API configuration if the frontend uses a different origin.

Open Swagger UI at `/swagger` on the local address printed by `dotnet run`.

## API endpoints

All routes below are relative to the API host. The authentication routes are anonymous; every task route requires:

```http
Authorization: Bearer <accessToken>
```

### Authentication (anonymous)

| Method | Route | Description |
|---|---|---|
| `POST` | `/api/auth/register` | Register with a unique email and a password of at least 6 characters. |
| `POST` | `/api/auth/login` | Authenticate and receive an access token and its expiration time. |

Example registration/login request:

```json
{
  "email": "user@example.com",
  "password": "example-password"
}
```

The login response contains `accessToken` and `expiresAt`.

### Tasks (bearer token required)

| Method | Route | Description |
|---|---|---|
| `GET` | `/api/tasks/` | List the authenticated user's tasks, ordered by due date. |
| `POST` | `/api/tasks` | Create a task for the authenticated user. |
| `GET` | `/api/task/{id}` | Retrieve one of the authenticated user's tasks. |
| `PUT` | `/api/task/{id}` | Update one of the authenticated user's tasks. |
| `DELETE` | `/api/task/{id}` | Delete one of the authenticated user's tasks. |



## Solution structure

```text
backend/
└── src/
    ├── TaskManagement.sln
    ├── TaskManagement.Api/            # Controllers, middleware, Swagger, startup
    ├── TaskManagement.Application/    # DTOs, validation, and application services
    ├── TaskManagement.Domain/         # Entities and repository/unit-of-work contracts
    └── TaskManagement.Infrastructure/ # EF Core, PostgreSQL, Identity, JWT, migrations
```

Build the backend solution from the repository root:

```powershell
dotnet build backend\src\TaskManagement.sln
```

## Running tests
No unit or integration test projects are included yet.
