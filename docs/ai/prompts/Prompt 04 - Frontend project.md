# AI-Assisted Development

## Prompt 04 — Frontend project

You are an expert Angular 18 Frontend Architect and Developer. Your task is to generate a complete Angular 18 frontend project named `task-management-ui`. 

The backend is an ASP.NET Core Web API running at `http://localhost:56030` (or `56029`), which expects requests from the Angular development server running at `http://localhost:4200`. 

**Core Constraints & Guidelines**
1. Scope: Create ONLY the frontend Angular 18 project (`task-management-ui`). Do not generate backend files or unit tests.
2. Coding Standards: Use modern Angular 18 best practices (Standalone Components, Signals/Reactive Forms where appropriate, strict TypeScript, clean directory structure, shared services, and HTTP interceptors). Avoid unnecessary complexity.
3. Styling & UI: Use Bootstrap 5 for a clean, simple, and effective visual identity. All validation error messages must be clearly rendered in red providing immediate visual feedback (red error text/borders) for invalid form inputs..
4. Angular project should be generated inside the folder `task-management/frontend`.

**API Endpoints & Integration**
The application must communicate with the following REST endpoints:
- `POST /api/auth/register` – Register with a unique email and a password of at least 6 characters.
- `POST /api/auth/login` – Authenticate, receive an access token and its 30-minute expiration time.
- `GET /api/tasks/` – List the authenticated user's tasks ordered by due date.
- `POST /api/tasks` – Create a task for the authenticated user.
- `GET /api/tasks/{id}` – Retrieve a specific task.
- `PUT /api/tasks/{id}` – Update a specific task.
- `DELETE /api/tasks/{id}` – Delete a specific task.

**Functional Requirements & User Flows**

1. Authentication & Authorization (JWT & Guards):
   - Login & Registration Screen: A clean authentication page allowing users to switch between Login and Registration.
   - Registration Validations: Valid email format, password minimum 6 characters, required fields highlighting, and handling duplicate email error responses from the API in red text.
   - Login Validations: Required fields, incorrect credential handling showing "Invalid email or password" in red.
   - Token Management: Store the JWT access token securely (e.g., `localStorage`). Implement an HTTP Interceptor to automatically attach the `Authorization: Bearer <token>` header to all outgoing API requests.
   - Session Expiry & Guards: Handle 30-minute token expiration / 401 Unauthorized responses by clearing stored tokens and redirecting the user back to the login page. Protect authenticated routes using functional route guards.

2. Task List View (`/tasks`):
   - Accessible only after successful login.
   - Displays only the authenticated user's tasks ordered by due date.
   - Includes a **"Create Task"** button that opens a modal or form.
   - Each task item in the list features:
     - Title, description summary, status (To-do, Doing, Done, Cancelled), and due date.
     - A click event on a task item that opens a modal window displaying task details, allowing the user to view or edit the task data.
     - A delete icon or button that triggers a confirmation dialog before calling the delete endpoint, showing a success message upon completion and updating the list.

3. Task Creation & Editing Modal:
   - Form fields: Title (required), Description, Status (select dropdown), and Due Date (required, valid date validation).
   - The status values for the select dropdown should be retrieved from the backend via a new endpoint `GET /api/tasks/status` that returns all available task statuses.
   - Instant validation feedback with required fields highlighted and error messages displayed in red.

**Project Structure & Deliverables**
Please generate the complete source code files for the Angular 18 project, including:
1. `angular.json`, `package.json` (with Bootstrap 5 dependency), and `tsconfig.json`.
2. Environment configuration pointing to the backend API (`http://localhost:56030`).
3. Core models/interfaces for User, AuthResponse, and Task.
4. Services: `AuthService` and `TaskService`.
5. Interceptors: `AuthInterceptor` for JWT injection and error handling.
6. Guards: `AuthGuard` for protecting routes.
7. Components:
   - `LoginComponent` / `RegisterComponent` (Auth layout)
   - `TaskListComponent`
   - `TaskModalComponent` (handles both creation and editing)
8. App routing configuration (`app.routes.ts`) with default redirects.

Provide clean, modular code blocks for each required file so the project can be immediately set up and executed with `npm install` and `ng serve`.

---

### Context & Reference Files
Base your implementation, domain entities, and business rules on the contents of the following reference files:
- `README.md`
- `architecture.md`
- `user-stories.md`
- `openapi.json`

---


### Scope files: [architecture.md](../../architecture.md), [README.md](../../../README.md), [user-stories.md](../../user-stories.md)

---

## Generated code

<details>
<summary>Expand</summary>
Implemented the Angular 18 frontend in task-management-ui, with standalone components, Bootstrap, authentication and registration, JWT handling and expiry, protected routes, and task list/create/edit/delete flows.

The API URL defaults to http://localhost:56030 in environment.ts. To run it:

24 files changed, added 1152 deleted 0


</details>
---

## Validation

- Reviewed generated architecture.
- Readme file was not created. Add a step in this prompt to generate proper readme file.
- For each user story, verify that the LLM-generated frontend validation messages and behaviors match the acceptance criteria.
- Created components HTML files and removed inline HTML templates from Typescript files generated by the LLM
- Changed some UI styling

