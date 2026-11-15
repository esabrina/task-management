# User Stories

> Detailed user stories and acceptance criteria defining the expected system behavior using Gherkin (BDD) format.

---

## 1. User Registration

As a user,  
I want to register myself in the system,  
so that I can securely log in and manage my tasks.

### Acceptance criteria

**AC1 — Successful registration**

Given the user is on the registration page  
And the user provides a valid email address  
And the user provides a password with at least 6 characters  
When the user submits the registration form  
Then the system should create the user account  
And the user should be redirected to the login page.

**AC2 — Required fields**

Given the user is on the registration page  
When the user submits the form without providing an email or password  
Then the system should prevent the registration  
And the missing field should be highlighted  
And an appropriate validation message should be displayed.

**AC3 — Invalid email format**

Given the user is on the registration page  
When the user enters an invalid email address  
And submits the form  
Then the system should prevent the registration  
And the email field should be highlighted  
And an appropriate validation message should be displayed.

**AC4 — Password minimum length**

Given the user is on the registration page  
When the user enters a password with fewer than 6 characters  
And submits the form  
Then the system should prevent the registration  
And the password field should be highlighted  
And an appropriate validation message should be displayed.

**AC5 — Duplicate email**

Given an account already exists for an email address  
When another user attempts to register using the same email address  
Then the system should prevent the registration  
And an appropriate message should inform the user that the email is already registered.

---

## 2. User Login

As a registered user,  
I want to log in to the system,  
so that I can securely access all my tasks and manage them.

### Acceptance criteria

**AC1 — Successful login**

Given a registered user is on the login page  
And the user provides the registered email address  
And the user provides the correct password  
When the user submits the login form  
Then the system should authenticate the user  
And issue an access token with a 30-minute expiration time  
And redirect the user to the authenticated area displaying the appropriate task list.

**AC2 — Invalid credentials**

Given the user is on the login page  
When the user submits an incorrect email or password  
Then the system should reject the authentication  
And display the message "Invalid email or password"  
And the user should remain unauthenticated.

**AC3 — Required credentials**

Given the user is on the login page  
When the user submits the form without an email or password  
Then the system should prevent the login  
And the missing field should be highlighted  
And display the message "Invalid email or password"


---

## 3. User Logout

As a logged-in user,  
I want to log out of the system,  
so that I can securely end my session.

### Acceptance criteria

**AC1 — Successful logout**

Given the user is authenticated  
And a valid access token is stored by the frontend  
When the user chooses to log out  
Then the frontend should remove the access token  
And the user should be redirected to the login page.

**AC2 — Expired access token**

Given the user has an access token  
And 30 minutes have elapsed since the token was issued  
When the user attempts to access an authenticated resource  
Then the API should reject the request as unauthenticated  
And the frontend should remove the expired access token  
And the user should be redirected to the login page.

---

## 4. Create Task

As a logged-in user,  
I want to create a task,
so that I can organize my work and monitor my progress.

### Acceptance criteria

**AC1 — Successful task creation**

Given the user is authenticated  
And is on the task list page
When the user clicks on "Create" buttom
Then the system should display the task creation form 
When the user provides a task title, valid due date, selects the status and submits the task
Then the system should create the task  
And associate the task with the authenticated user  
And the user should be redirected to the task list.

**AC2 — Required task fields**

Given the user is creating a task  
When the user submits the form without a title, due date, or status  
Then the system should prevent the task from being created  
And the corresponding field should be highlighted  
And a message for required fields should be displayed.

**AC3 — Invalid due date**

Given the user is creating a task  
When the user provides an invalid date  
Then the system should prevent the operation  
And the due date field should be highlighted  
And an error message ("Invalid field.") should be displayed.

---

## 5. View Tasks

As a logged-in user,
I want to view my tasks,
So that I can keep track of my work and deadlines.

### Acceptance criteria

**AC1 — View task list**

Given the user is authenticated  
When the user accesses the task list
Then the system should return only tasks (title, description, status and dueDate) belonging to the authenticated user, ordered by due date

**AC2 — View task**

Given the user is authenticated
And is on the task list page  
And the task list is not empty  
When the user clicks on a task
Then the system should display the task details (title, description, due date and status) in a modal window

**AC3 — Task belonging to another user**

Given the user is authenticated
And the requested task belongs to another user
When the user attempts to view the task
Then the API should return 404 Not Found

---

## 6. Task Update

As a logged-in user,
I want to update my tasks,
So that I can keep their information and status up to date.

### Acceptance criteria

**AC1 — Update existing task**

Given the user is authenticated 
And is on the task list page  
And the task list is not empty  
When the user clicks on a task
Then the system should display the task details (title, description, due date and status) in a modal window
When the user updates the task with valid data  
Then the system should persist the changes  
And return the updated task successfully.

**AC2 — Validate updated task data**

Given the user is updating a task
When the user submits the task without a title, due date, or status
Or the user provides an invalid due date
Then the system should prevent the update
And the corresponding field should be highlighted
And an appropriate validation message should be displayed.

**AC3 — Task belonging to another user**

Given the user is authenticated
And the requested task belongs to another user
When the user attempts to update the task
Then the API should return 404 Not Found

---

## 7. Task Deletion

As a logged-in user,
I want to delete my tasks,
so that I can remove tasks that are no longer needed.

### Acceptance criteria

**AC1 — Delete existing task**

Given the user is authenticated
And is on the task list page
And the task list is not empty
When the user clicks the task's delete button
Then the system should ask the user to confirm the deletion
When the user confirms the deletion
Then the system should delete the task
And display a success message
And remove the task from the task list.

**AC2 — Non-existent or unauthorized task**

Given the user is authenticated
When the user attempts to delete a task that does not exist
Or the requested task belongs to another user
Then the API should return 404 Not Found.

**AC3 — Delete task not confirmed**

Given the user is authenticated
And is on the task list page
And the task list is not empty
When the user clicks the task's delete button
Then the system should ask the user to confirm the deletion
When the user cancels the deletion
Then the system should not delete the task
