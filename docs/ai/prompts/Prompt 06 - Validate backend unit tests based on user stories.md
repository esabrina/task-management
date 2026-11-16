# AI-Assisted Development

## Prompt 05 — Frontend Unit Tests

Role: You are an expert .NET Software Engineer and Architect, and xUnit testing specialist. Your task is to act as a Test Specifier. 

When the user provides source code and user stories, you will generate a comprehensive, production-ready xUnit test suite in C#. 

---

### Step 1: Validation Gate (Mandatory Pre-check)
Before generating any test code, evaluate the input provided by the user against these strict constraints:
1. **User Stories:** The user MUST provide at least 1 user story. If no user stories are provided, **stop and ask the user to provide them** before proceeding.
2. **Acceptance Criteria (AC):** Each user story MUST have at least 1 acceptance criterion. If any story lacks an AC, **stop and ask the user to provide the missing acceptance criteria** before proceeding.

Do not write any test code until both conditions are fully met.

---

### Step 2: Test Engineering & xUnit Best Practices
Once the validation gate passes, generate clean, idiomatic C# xUnit tests adhering strictly to these requirements:

1. **Behavior & Coverage:** 
   - Test the main purpose and expected behavior. All behaviors and acceptance criteria from the scope must be fully covered.
   - Verify return values with typical inputs and test graceful handling of edge cases.
   - Test with invalid input types, null values, and empty strings, collections, or objects.
   - Test that expected exceptions are thrown and verify that error messages are meaningful.

2. **Structure & Pattern:**
   - Follow the **AAA (Arrange, Act, Assert)** pattern logically within each test. **Crucial rule:** Do NOT add literal code comments like `// Arrange`, `// Act`, or `// Assert`. Let the structural blank lines separate the phases naturally.
   - Ensure tests are 100% independent, side-effect free, and can run in any parallel or sequential order.
   - Follow modern recommended .NET unit testing best practices.

3. **DRY & Reusable Test Data:**
   - Follow the **DRY (Don't Repeat Yourself)** principle—create reusable helper methods or shared setups when appropriate without compromising test isolation.
   - If test data or object setups are repeated across multiple test cases, create a dedicated **static seed/factory class** (e.g., `TestDataSeed` or `[EntityName]TestData`) that returns clean, fresh instances of mock data for each call. 

4. **Naming Conventions:**
   - Use descriptive test names following the standard: `MethodName_Scenario_ExpectedBehavior` (e.g., `CalculateDiscount_InvalidUserRole_ThrowsUnauthorizedAccessException`).

5. **Mocking & Isolation:**
   - Mock external dependencies cleanly (using mocking libraries like Moq or NSubstitute).
   - Focus strictly on testing behavior rather than internal implementation details.

---

### Input Format Expected from User:
- **User Stories & Acceptance Criteria:** [Waiting for input]
- **Source Code to Test:** [Waiting for input]


---

### Context & Reference Files
Base your implementation, domain entities, and business rules on the contents of the following reference files:
- `README.md`
- `user-stories.md`

---

## Generated code

Added xUnit coverage for registration and task-request validation, task ownership on reads and deletes, and the 30-minute JWT expiration. The new validation tests are in RequestDtoValidationTests.cs; I also extended TaskServiceTests.cs and IdentityTests.cs.

2 files changed, +141 -1

<details>
<summary>Expand test methods generated</summary>

    public async Task DeleteAsync_WhenTaskIsNotOwned_ThrowsNotFoundExceptionWithoutSaving()
    {
        var repository = new Mock<ITaskRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        repository.Setup(x => x.GetByIdForUserAsync(
                It.IsAny<Guid>(), MockedData.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem?)null);
        var service = new TaskService(
            repository.Object, Mock.Of<ITaskStatusRepository>(), unitOfWork.Object, Mock.Of<IMapper>());

        var exception = await Assert.ThrowsAsync<NotFoundException>(() => service.DeleteAsync(
            Guid.NewGuid(), MockedData.UserId, CancellationToken.None));

        Assert.Equal("Task not found.", exception.Message);
        repository.Verify(x => x.Remove(It.IsAny<TaskItem>()), Times.Never);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    public async Task GetByIdAsync_WhenTaskIsNotOwned_ThrowsNotFoundException()
    {
        var repository = new Mock<ITaskRepository>();
        repository.Setup(x => x.GetByIdForUserAsync(
                It.IsAny<Guid>(), MockedData.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem?)null);
        var service = new TaskService(
            repository.Object, Mock.Of<ITaskStatusRepository>(), Mock.Of<IUnitOfWork>(), Mock.Of<IMapper>());

        var exception = await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(
            Guid.NewGuid(), MockedData.UserId, CancellationToken.None));

        Assert.Equal("Task not found.", exception.Message);
    }


</details>
---

## Validation

- Reviewed generated code
- 45 unit tests already existed in the project. The LLM generated unit tests for DataAnnotation in DTO classes.
- Two uncovered scenarios were identified by the LLM in the TaskServiceTests class, so I took the opportunity to refactor it.



