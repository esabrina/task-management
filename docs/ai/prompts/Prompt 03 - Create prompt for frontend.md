# AI-Assisted Development

## Prompt 03 — Generate prompt for frontend



Create a comprehensive andstructured AI prompt designed to instruct an AI coding assistant to generate an Angular 18 frontend project. 

The prompt you generate must instruct the coding assistant to adhere to the following specifications:


**Scope**
- Project Name: `task-management-ui`. Angular project should be generated inside the folder `task-management/frontend`.
- Tech Stack: Angular 18 frontend integrating with an existing .NET Web API backend.
- Reference Files: The code must be based on the attached context files: `README.md`, `architecture.md`, and `user-stories.md`.
- Exclusions: Generate only the frontend project. Do not generate backend code or unit tests at this stage.
- Follow modern Angular 18 best practices and clean code standards, avoiding unnecessary complexity or over-engineering.
- Implement a simple, clean, and effective UI based on standard Bootstrap.
- Ensure all validation and system error messages are clearly displayed in red.
- Implement all data validations specified in `user-stories.md`, as well as proper route guards and authorization handling (JWT token management).
- The backend expects the Angular development server to run on `http://localhost:4200`.
- Provide immediate visual feedback (red error text/borders) for invalid form inputs.

**Requirements and Flow**
- Authentication: Include a login screen that also allows users to register. Once registered, users can log in and get redirected to their task list.
- Task Listing: Display the authenticated user's tasks ordered by due date, along with a button to create a new task.
- Task Details & Editing: Clicking a task in the list must open a modal showing its details, allowing the user to either edit and save changes or simply view and close the modal.
- Task Deletion: Each task in the list must have a delete icon/button that requires user confirmation before deleting.


**Backend REST API Reference**

| Method | Endpoint | Description |
| :--- | :--- | :--- |
| `POST` | `/api/auth/register` | Register with a unique email and a password (min. 6 characters). |
| `POST` | `/api/auth/login` | Authenticate and receive an access token and its expiration time. |
| `GET` | `/api/tasks/` | List the authenticated user's tasks, ordered by due date. |
| `POST` | `/api/tasks` | Create a task for the authenticated user. |
| `GET` | `/api/tasks/{id}` | Retrieve a specific task of the authenticated user. |
| `PUT` | `/api/tasks/{id}` | Update a specific task of the authenticated user. |
| `DELETE` | `/api/tasks/{id}` | Delete a specific task of the authenticated user. |

---

### Scope files: [architecture.md](../../architecture.md), [README.md](../../../README.md), [user-stories.md](../../user-stories.md)

---

## Generated code

<details>
<summary>Expand</summary>
You are an expert Angular 18 Frontend Architect and Developer. Your task is to generate a complete, production-ready Angular 18 frontend project named `task-management-ui`. 

The backend is an ASP.NET Core Web API running at `http://localhost:56030` (or `56029`), which expects requests from the Angular development server running at `http://localhost:4200`. 

### Core Constraints & Guidelines
1. **Scope**: Create ONLY the frontend Angular 18 project (`task-management-ui`). Do not generate backend files or unit tests.
2. **Coding Standards**: Use modern Angular 18 best practices (Standalone Components, Signals/Reactive Forms where appropriate, strict TypeScript, clean directory structure, shared services, and HTTP interceptors). Avoid unnecessary complexity.
3. **Styling & UI**: Use Bootstrap 5 for a clean, simple, and effective visual identity. All validation error messages must be clearly rendered in red.

---

### API Endpoints & Integration
The application must communicate with the following REST endpoints:
- `POST /api/auth/register` – Register with a unique email and a password of at least 6 characters.
- `POST /api/auth/login` – Authenticate, receive an access token and its 30-minute expiration time.
- `GET /api/tasks/` – List the authenticated user's tasks ordered by due date.
- `POST /api/tasks` – Create a task for the authenticated user.
- `GET /api/tasks/{id}` – Retrieve a specific task.
- `PUT /api/tasks/{id}` – Update a specific task.
- `DELETE /api/tasks/{id}` – Delete a specific task.

---

### Functional Requirements & User Flows

1. **Authentication & Authorization (JWT & Guards)**:
   - **Login & Registration Screen**: A clean authentication page allowing users to switch between Login and Registration.
   - **Registration Validations**: Valid email format, password minimum 6 characters, required fields highlighting, and handling duplicate email error responses from the API in red text.
   - **Login Validations**: Required fields, incorrect credential handling showing "Invalid email or password" in red.
   - **Token Management**: Store the JWT access token securely (e.g., `localStorage`). Implement an HTTP Interceptor to automatically attach the `Authorization: Bearer <token>` header to all outgoing API requests.
   - **Session Expiry & Guards**: Handle 30-minute token expiration / 401 Unauthorized responses by clearing stored tokens and redirecting the user back to the login page. Protect authenticated routes using functional route guards.

2. **Task List View (`/tasks`)**:
   - Accessible only after successful login.
   - Displays only the authenticated user's tasks ordered by due date.
   - Includes a **"Create Task"** button that opens a modal or form.
   - Each task item in the list features:
     - Title, description summary, status (To-do, Doing, Done, Cancelled), and due date.
     - A click event on a task item that opens a modal window displaying task details, allowing the user to view or edit the task data.
     - A delete icon or button that triggers a confirmation dialog before calling the delete endpoint, showing a success message upon completion and updating the list.

3. **Task Creation & Editing Modal**:
   - Form fields: Title (required), Description, Status (select dropdown: 1 - To-do, 2 - Doing, 3 - Done, 4 - Cancelled), and Due Date (required, valid date validation).
   - Instant validation feedback with required fields highlighted and error messages displayed in **red**.

---

### Project Structure & Deliverables
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
</details>
---

## Validation

- Reviewed generated prompt
- Added information regarding project folder
- Added missing requirements regarding validation and error messages
- Noted I forgot to create an endpoint to return all statuses, so I added a new endpoint to the backend and updated the prompt accordingly.

