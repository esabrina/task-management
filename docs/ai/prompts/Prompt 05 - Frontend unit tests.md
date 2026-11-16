# AI-Assisted Development

## Prompt 05 — Frontend Unit Tests

You are an expert Senior Angular Frontend Architect and Developer

Generate unit tests for this Angular project. Strictly follow these rules:

1. Business Rules:
   - Thoroughly analyze all provided user stories, behaviors, and acceptance criteria in the scope.
   - Write dedicated test cases that systematically explore every acceptance criterion (including happy paths, boundary conditions, edge cases, and validation errors).
   - Ensure test suite descriptions or test case titles explicitly map back to the product behaviors or user story requirements being validated.

2. Isolation:
   - Isolate the unit under test. Mock all external dependencies (services, child components, stores, HTTP clients, routers) using spies or mock classes (`jasmine.createSpyObj` or Jest equivalents).
   - For Components, use `NO_ERRORS_SCHEMA` or shallow rendering where appropriate to avoid pulling in unneeded sub-components, unless testing direct component interaction.
   - Ensure tests are independent and can run in any order

3. Behavior-Focused Over Implementation-Focused:
   - Test public APIs, inputs (`@Input`), outputs (`@Output`), state mutations, and expected side effects/outcomes.
   - Do not assert private methods or internal state properties directly unless strictly necessary.
   - Test expected exceptions are thrown
   - Test edge cases and invalid input types 

4. Stability:
   - Handle asynchronous code, Promises, and RxJS Observables cleanly (e.g., using `of()`, `throwError()`, `fakeAsync` + `tick()`, or `waitForAsync` / `fixture.whenStable()`).

5. Clean TestBed Setup:
   - Keep `TestBed.configureTestingModule` lean and modular. Reset state properly between tests if mutations occur.
   - Inject dependencies cleanly via `TestBed.inject()` rather than accessing private fixture properties directly when possible.

6. Code Quality & Readability:
   - Follow standard `describe()`, `it()`, and `beforeEach()` nesting conventions.
   - Use descriptive test case titles that follow the pattern: `it('should [expected behavior] when [condition/action]', ...)`
   - The desired code coverage is 80%

---

### Context & Reference Files
Base your implementation, domain entities, and business rules on the contents of the following reference files:
- `README.md`
- `user-stories.md`

---

## Generated code

<details>
<summary>Expand</summary>

Added Jasmine/Karma test setup and 42 unit tests covering authentication, session expiry, task services, forms, and task management.

Tests: 42 passed
Coverage: 98.64% statements, 91.17% branches, 100% functions, 98.61% lines
Frontend build and test TypeScript check passed. 

12 file changed, +942 -1

</details>
---

## Validation

- Reviewed generated code 
- Removed uncessary methods and redundant logic
- Simplified assertions and improved structure
- Checked the code coverage


