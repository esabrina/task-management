# AI-Assisted Development

## Prompt 02 — Backend project

You are an expert Senior .NET Backend Engineer. Your task is to generate the complete code and folder structure for a backend Web API project named `TaskManagement`, adhering strictly to Clean Architecture and SOLID principles without adding unnecessary over-engineering.

### Context & Reference Files
Base your implementation, domain entities, and business rules on the contents of the following reference files:
- `README.md`
- `architecture.md`
- `user-stories.md`

---

**Project Architecture & Design Rules**

1. Solution Structure (Clean Architecture):
   - `TaskManagement.Domain`: Enterprise entities, domain models, and interfaces (repositories, unit of work).
   - `TaskManagement.Application`: Business logic, application services, DTOs, validators, and mapping configurations.
   - `TaskManagement.Infrastructure`: Database context, Entity Framework Core configurations, repository implementations, unit of work, and authentication/authorization services.
   - `TaskManagement.Api`: Presentation layer, controllers, middleware, and Program.cs configuration.

2. Design Patterns & Principles:
   - Repository Pattern & Unit of Work: Implement abstract repositories and a Unit of Work pattern for data persistence transactions. Do **not** use CQRS to keep the project straightforward.
   - DTOs (Data Transfer Objects): Use dedicated DTOs for data input and output across all layers. Never expose domain entities directly through controllers.
   - SOLID Principles: Keep classes small, cohesive, and single-responsibility. Avoid unnecessary design patterns or abstraction layers that do not add direct value.
   - IoC & Dependency Injection: Register all services, repositories, and Unit of Work cleanly in the Dependency Injection container.

3. Testing Constraints:
   - **Do NOT** generate unit tests or integration tests for the project implementation at this stage. (Note: XUnit is specified for future testing, but no test projects or test files should be created right now).

4. Security, Validation & Documentation:
   - Data Validation: Implement robust data validation using FluentValidation or Data Annotations on all incoming DTOs.
   - Authorization: Implement proper authorization checks (e.g., JWT-based authentication) protecting endpoints where necessary.
   - Swagger/OpenAPI: Enable and configure Swagger in `Program.cs` so API documentation is available at the root/swagger endpoint.


---

**Required REST API Endpoints**

Implement controllers supporting the following exact routes and operations:

- Tasks:
  - `GET /api/tasks/` - Retrieve tasks.
  - `GET /api/task/{id}` - Retrieve a specific task by its unique identifier.
  - `PUT /api/task/{id}` - Update an existing task.
  - `DELETE /api/task/{id}` - Delete a task.

- Authentication:
  - `POST /api/auth/register` - Register a new user account.
  - `POST /api/auth/login` - Authenticate a user and return a JWT token.

Feel free to propose new endpoints based on the user stories.

---

**Output Requirements**

Provide the complete directory tree structure, followed by the full, working source code files for each project layer, ensuring all namespaces, project references, NuGet package dependencies, and `Program.cs` wire-ups are fully configured and ready to build.

---

### Scope files: [architecture.md](../../architecture.md), [README.md](../../../README.md), [user-stories.md](../../user-stories.md)

---

## Generated code

<details>
<summary>Expand</summary>
It includes the required task and authentication routes, Identity-based registration, 30-minute JWTs, owner-scoped task access, PostgreSQL/EF Core configuration, a migration with seeded statuses, and Swagger. Local setup is documented in README.md, with PostgreSQL in infrastructure/docker/docker-compose.yml.

Verification: Release build succeeded with zero warnings or errors. API smoke checks confirmed Swagger routes and authorization metadata, unauthenticated task access returns 401, and empty login returns the specified validation message. No test projects were added, as requested. The migration was generated but not applied; PostgreSQL was not running here.


46 files changed +1097 -29


</details>
---

## Validation

- Reviewed generated architecture
- Removed seed data from the DBContext and move it to a standart SQL file to be executed in the migrations.
- Moved configuration from the codebase to AppSettings
- Cleaned up the codebase by removing unnecessary code and directives
- Split up DTO files with many classes
- Merged Controllers
- Created HealthCheck endpoint
- Added more information to the readme files with AI
