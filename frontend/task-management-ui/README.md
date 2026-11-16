# Task Management UI

> Angular 18 standalone frontend for the Task Management API. It supports user registration and login, and lets authenticated users manage their tasks.

**Features**

- Reactive login and registration forms with inline validation and API error messages.
- JWT bearer token storage, automatic authorization headers, protected task routes, and session expiry handling.
- Task list ordered by due date, with task details, creation and editing, and deletion confirmation.
- Task status options loaded from the API.

**Project structure**

```text
src/
  app/
    core/           # Models, services, interceptor, and route guard
    features/       # Authentication and task components
    app.routes.ts
    app.config.ts
  environments/
```


## Run locally

Requirements:

- Node.js supported by Angular 18 (18.19.1+, 20.11.1+, or 22.0.0+)
- npm
- The Task Management API running locally

From the directory `frontend/task-management-ui`, run:

```bash
npm install
npm start
```

The Angular development server runs at `http://localhost:4200` (ensure the API is running and allows requests from that origin).

To create a production build:

```bash
npm run build
```

Build output is written to `dist/task-management-ui`.

## API configuration

The API base URL is set in `src/environments/environment.ts`, it defaults to `http://localhost:56030` (backend must be listening on this port).

Main API routes used by the frontend:

- `POST /api/auth/register` and `POST /api/auth/login`
- `GET /api/tasks` and `GET /api/tasks/{id}`
- `POST /api/tasks`, `PUT /api/tasks/{id}`, and `DELETE /api/tasks/{id}`
- `GET /api/tasks/status` to load task statuses


## Running tests

From the repository root, install the frontend dependencies (the first time only) and run the tests:

```bash
cd frontend/task-management-ui
npm install
npm test -- --watch=false --browsers=ChromeHeadless
```

To include code coverage, run:

```bash
npm test -- --watch=false --browsers=ChromeHeadless --code-coverage
```

The HTML report is generated under `coverage/task-management-ui`.
