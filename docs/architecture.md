# Architecture

## Overview

>  This project is a simple task management web application.

The solution follows **Clean Architecture** principles and is composed of:

- **Angular** frontend
- **ASP.NET Core Web API** backend
- **PostgreSQL** database
- **Entity Framework Core** for data access and database schema migrations
- **ASP.NET Core Identity** for user management and authentication
- **JWT Bearer Authentication** for securing API endpoints
- **Docker Compose** for running the local PostgreSQL database

The application is intentionally designed to run locally without dependencies on cloud services. 

The main goal is to keep the solution simple and maintainable while demonstrating separation of concerns, testability, API design, authentication, authorization, and frontend integration.

---

## Architecture Overview

```mermaid
flowchart LR
    UI[Angular Frontend]
    API[ASP.NET Core Web API]
    APP[Application Layer]
    DOMAIN[Domain Layer]
    INFRA[Infrastructure Layer]
    DB[(PostgreSQL)]

    UI -->|HTTP / JSON| API
    API --> APP
    APP --> DOMAIN
    APP --> INFRA
    INFRA --> DB
```

The frontend communicates exclusively with the backend through HTTP/REST APIs.

The backend is organized according to Clean Architecture principles. Business rules are kept independent from the API and infrastructure concerns.

---

## Solution Structure

The repository is organized into three main areas:

```text
task-management/
│
├── backend/
│   ├── src/
│   │   ├── TaskManagement.Api/
│   │   ├── TaskManagement.Application/
│   │   ├── TaskManagement.Domain/
│   │   └── TaskManagement.Infrastructure/
│   │   │
│   │   └── tests/
│   │       ├── TaskManagement.UnitTests/
│   │       ├── TaskManagement.IntegrationTests/
│
├── frontend/
│   └── task-management/
│
├── infrastructure/
│   ├── docker/
│   │   └── docker-compose.yml
│   
│
├── docs/
│   ├── architecture.md
│   ├── user-story.md
│   └── ai/
│       ├── prompts.md
│
└── README.md
```

The `infrastructure` directory contains files related to overall local environment.

---

# Clean Architecture

The backend follows a simplified Clean Architecture approach with four projects:

```mermaid
flowchart TB
    API[Api]
    APP[Application]
    DOMAIN[Domain]
    INFRA[Infrastructure]

    API --> APP
    API --> INFRA
    APP --> DOMAIN
    INFRA --> APP
    INFRA --> DOMAIN
```


---

# Authentication and Authorization

Authentication is implemented using **ASP.NET Core Identity** and **JWT Bearer Authentication**.

Users can register themselves and subsequently log in. Authentication sessions must expire after 30 minutes.

The authentication flow is:

```mermaid
sequenceDiagram
    participant User
    participant Angular
    participant API
    participant Identity
    participant DB as PostgreSQL

    User->>Angular: Register
    Angular->>API: POST /api/auth/register
    API->>Identity: Create user
    Identity->>DB: Store user/password hash
    API-->>Angular: Registration result

    User->>Angular: Login
    Angular->>API: POST /api/auth/login
    API->>Identity: Validate credentials
    Identity->>DB: Retrieve user
    API-->>Angular: JWT access token

    Angular->>API: Request with Bearer token
    API->>API: Validate JWT
    API-->>Angular: Protected resource
```

The application does not introduce administrative roles because user administration is outside the scope of the technical exercise. The authorization model is intentionally simple:

> An authenticated user can only manage their own tasks.


---

# Data Model

PostgreSQL is used as the relational data store.

The main application entities are:

```mermaid
erDiagram
    AspNetUser ||--o{ Task : owns
    TaskStatus ||--o{ Task : classifies

    AspNetUser {
        uuid Id PK "required"
        string Email "required"
        string PasswordHash "required"
        datetime CreatedAt "required"
    }

    Task {
        uuid Id PK "required"
        string Title "required"
        string Description
        int StatusId FK "required"
        datetime DueDate "required"
        uuid UserId FK "required"
        datetime CreatedAt "required"
        datetime UpdatedAt
    }

    TaskStatus {
        int Id PK "required"
        string Name "required"
    }
```

The existing statuses are:

| Id | Name |
|---:|---|
| 1 | To-do |
| 2 | Doing |
| 3 | Done |
| 4 | Cancelled |


---

# Database Schema Management

EF Core Migrations were selected for this technical exercise to manage database schema changes because they provide a simple and integrated way to manage the database schema in a local environment.

For a production environment, database schema changes would typically be integrated into the deployment pipeline using a controlled migration strategy, such as reviewed SQL migration scripts or migration bundles. A dedicated database deployment tool such as DbUp could also be considered depending on the organization's deployment practices.

---

# Infrastructure

I intentionally kept the infrastructure local because the objective is to demonstrate software architecture, clean code, testing and API design rather than cloud infrastructure.

PostgreSQL runs containerized using Docker Compose.

```mermaid
flowchart TB
    Developer[Environment]

    Angular[Angular<br/>localhost:4200]
    API[ASP.NET Core API<br/>localhost:5000/5001]
    PostgreSQL[(PostgreSQL<br/>Docker Container)]

    Developer --> Angular
    Developer --> API
    API --> PostgreSQL
```


---

# Architectural Decisions

The following decisions were intentionally made to keep the solution focused on the requirements of the technical exercise.

| Decision | Choice | Reason |
|---|---|---|
| Frontend | Angular | Familiarity and suitable for the required CRUD UI |
| Backend | ASP.NET Core / C# | Required by the exercise |
| API | REST | Simple and appropriate for the CRUD requirements |
| Architecture | Clean Architecture | Required by the exercise and improves separation of concerns |
| Database | PostgreSQL | Relational model fits the domain and runs easily in Docker |
| ORM | Entity Framework Core | Native .NET integration and productivity |
| Authentication | ASP.NET Core Identity | Provides secure user management without introducing another service |
| API authentication | JWT Bearer | Appropriate for a stateless REST API |
| Authorization | User/resource ownership | Satisfies the application's security requirement without unnecessary RBAC |
| Database migrations | EF Core Migrations | Simple and integrated for this exercise |
| Cloud | None | Keeps the solution locally reproducible and focused on the exercise |
| Containers | Docker Compose | Provides reproducible local PostgreSQL infrastructure |

---

# Scope and Deliberate Simplifications

This solution intentionally does not introduce:

- administrative users or RBAC;
- external identity providers;
- cloud infrastructure;
- API gateway;
- Refresh-token rotation or blacklist


The architecture is therefore designed to demonstrate sound engineering principles while remaining small enough to understand, test, run, and review easily.