# AI-Assisted Development

## Prompt 01 — User Stories

You are an assistant that reads user stories and generates clear, concise, and extensive acceptance criteria in Gherkin (BDD) format for developers. 
- Carefully analyze each reads user storie description and identify all relevant requirements and scenarios.
- If any information is missing, unclear, or incomplete, ask the user clarifying questions about the fields, data, or context before proceeding. 
- Ensure the acceptance criteria are actionable, unambiguous, and easy for developers to understand. 
- Format all acceptance criteria using Gherkin syntax (Given, When, Then). 
- Maintain a professional, collaborative, and detail-oriented tone in your responses.
- The scenarios can have a name, but they should be enumerated.
- If a user story has more than 6 acceptance criteria, the story should be broken down.

Feel free to propose new user stories or split any of the existing one.

### Scope files: [architecture.md](../../architecture.md)

### Stories:
<details>
<summary>Expand user stories</summary>

#### User Story 1 — User Registration

As a user, 
I want to register myself in the system,  
so that I can securely log in and manage my tasks.

Notes: 
 - E-mail should be in a valid format (e.g., "user@domain.com"). When invalid, the system should highlight the field and display an error message.
 - Email and Password are required
 - When the user is successfully registered, the user is redirected to the login page

#### User Story 2 — User Login

As a registered user,
I want to log in to the system,  
so that I can securely access all my tasks and manage them.

#### User Story 3 — User Logout

As a logged-in user,
I want to log out of the system,  
so that I can securely end my session.

Notes: 
 - When logout is completed, the user is redirected to the login page

#### User Story 4 — Task Management

As a logged-in user,
I want to create and manage my tasks,  
so that I can keep track of my work and deadlines.

Notes: 
 - DueDate should be in a valid date. When invalid, the system should highlight the field and display an error message.
 - Task itle, DueDate and Status are required

</details>

---

## Generated code

<details>
<summary>Expand result</summary>

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
And redirect the user to the authenticated application area.

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
And an appropriate validation message should be displayed.

**AC4 — Authenticated user accessing login page**

Given the user is already authenticated  
When the user attempts to access the login page  
Then the system should prevent access to the login page  
And the frontend should remove the access token  
And the user should be redirected to the login page.

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

## 4. Task Management

As a logged-in user,  
I want to create and manage my tasks,  
so that I can keep track of my work and deadlines.

### Acceptance criteria

#### Task Creation

**AC1 — Successful task creation**

Given the user is authenticated  
And the user provides a task title  
And the user provides a valid due date  
And the user selects a predefined status  
When the user submits the task  
Then the system should create the task  
And associate the task with the authenticated user  
And assign the task's creation date  
And return the created task successfully.

**AC2 — Required task fields**

Given the user is creating a task  
When the user submits the form without a title, due date, or status  
Then the system should prevent the task from being created  
And the corresponding field should be highlighted  
And an appropriate validation message should be displayed.

**AC3 — Invalid due date**

Given the user is creating or editing a task  
When the user provides an invalid date  
Then the system should prevent the operation  
And the due date field should be highlighted  
And an appropriate validation message should be displayed.

**AC4 — Optional description**

Given the user is creating or editing a task  
When the user does not provide a description  
Then the system should allow the operation to proceed successfully.

#### Task Listing

**AC5 — User sees only their own tasks**

Given the user is authenticated  
And tasks exist for multiple users  
When the user accesses the task list  
Then the system should return only tasks belonging to the authenticated user  
And tasks belonging to other users must not be displayed.

**AC6 — Task ordering**

Given the user has multiple tasks  
When the user accesses the task list  
Then the tasks should be ordered by DueDate.

**AC7 — Predefined task statuses**

Given the user is creating or editing a task  
When the user selects a task status  
Then the available statuses should come from the predefined status values  
And the user should not be able to assign an undefined status.

The predefined statuses are:

- To-do
- Doing
- Done
- Cancelled

#### Task Update

**AC8 — Update existing task**

Given the user is authenticated  
And the requested task belongs to the authenticated user  
When the user updates the task with valid data  
Then the system should persist the changes  
And return the updated task successfully.

#### Task Deletion

**AC9 — Delete existing task**

Given the user is authenticated  
And the requested task belongs to the authenticated user  
When the user deletes the task  
Then the system should remove the task  
And return a successful response.

#### Task Access Control

**AC10 — Non-existent task**

Given the user is authenticated  
When the user attempts to retrieve, update, or delete a task that does not exist  
Then the API should return 404 Not Found.

**AC11 — Task belonging to another user**

Given the user is authenticated  
And the requested task belongs to another user  
When the user attempts to retrieve, update, or delete that task  
Then the API should return 404 Not Found  
And the API should not reveal whether the task exists for another user.


</details>

---

## Validation

- Clarified some questions:
    1. Registration
        - Should the password have any minimum length? 
        - Should email addresses be unique? 
    2. Login
        - Should login be performed with email + password?
        - Should an already authenticated user be prevented from accessing the login page, or can they simply be redirected to the task list?    
- Removed unnecessary acceptance criteria
- Refined a few acceptance criteria
- Splitted "Task Management" story. I was generated with more than 6 acceptance criteria
