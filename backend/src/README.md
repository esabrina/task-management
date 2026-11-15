# Task Management Backend

The backend uses Clean Architecture, PostgreSQL, Entity Framework Core, ASP.NET Core Identity, and JWT bearer authentication.

## Requirements

- .NET 9 SDK
- Docker with the Compose plugin (must be running)
- EF Core command-line tools 9.0.4 (will be installed automatically by the setup script)


## Run locally

Run the following command from the repository root, it will create the environment and run the application. 

```powershell
.\setup-and-run-app.ps1
```

Open Swagger UI at `/swagger` on the local address (https://localhost:56029/swagger/index.html)

## API endpoints

The API listens for the Angular development server at `http://localhost:4200`.

The `auth` routes are anonymous and `tasks` route requires access token like this:

```http
Authorization: Bearer <accessToken>
```

### Authentication routes (anonymous)

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

### Tasks routes (bearer token required)

| Method | Route | Description |
|---|---|---|
| `GET` | `/api/tasks/` | List the authenticated user's tasks, ordered by due date. |
| `POST` | `/api/tasks` | Create a task for the authenticated user. |
| `GET` | `/api/tasks/{id}` | Retrieve one of the authenticated user's tasks. |
| `PUT` | `/api/tasks/{id}` | Update one of the authenticated user's tasks. |
| `DELETE` | `/api/tasks/{id}` | Delete one of the authenticated user's tasks. |
| `GET` | `/api/tasks/status` | List all available task statuses. |

Tokens expire after 30 minutes.

### Health check

Health check endpoint checks application and database health. It's available at `/health`

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

The Compose database credentials are for local development only.

## Running tests
No unit or integration test projects are included yet.
