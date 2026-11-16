using System.ComponentModel.DataAnnotations;
using TaskManagement.Application.DTOs;

namespace TaskManagement.UnitTests.Application;

public sealed class RequestDtoValidationTests
{
    [Fact]
    [Trait("Category", "Auth")]
    public void RegisterRequestDto_WhenRequiredFieldsAreMissing_ReportsEachField()
    {
        var request = new RegisterRequestDto { Email = string.Empty, Password = string.Empty };

        var errors = Validate(request);

        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(RegisterRequestDto.Email)));
        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(RegisterRequestDto.Password)));
    }

    [Fact]
    [Trait("Category", "Auth")]
    public void RegisterRequestDto_WhenEmailIsInvalid_ReportsEmailValidationError()
    {
        var request = new RegisterRequestDto { Email = "invalid-email", Password = "password" };

        var errors = Validate(request);

        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(RegisterRequestDto.Email)));
    }

    [Fact]
    [Trait("Category", "Auth")]
    public void RegisterRequestDto_WhenPasswordIsShorterThanSixCharacters_ReportsPasswordValidationError()
    {
        var request = new RegisterRequestDto { Email = "user@example.com", Password = "12345" };

        var errors = Validate(request);

        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(RegisterRequestDto.Password)));
    }

    [Fact]
    [Trait("Category", "Auth")]
    public void RegisterRequestDto_WhenPasswordHasSixCharacters_PassesValidation()
    {
        var request = new RegisterRequestDto { Email = "user@example.com", Password = "123456" };

        var errors = Validate(request);

        Assert.Empty(errors);
    }

    [Fact]
    [Trait("Category", "Auth")]
    public void LoginRequestDto_WhenCredentialsAreMissing_ReportsInvalidCredentialsForBothFields()
    {
        var request = new LoginRequestDto { Email = string.Empty, Password = string.Empty };

        var errors = Validate(request);

        Assert.Contains(errors, error =>
            error.MemberNames.Contains(nameof(LoginRequestDto.Email)) &&
            error.ErrorMessage == "Invalid email or password");
        Assert.Contains(errors, error =>
            error.MemberNames.Contains(nameof(LoginRequestDto.Password)) &&
            error.ErrorMessage == "Invalid email or password");
    }

    [Fact]
    [Trait("Category", "Auth")]
    public void LoginRequestDto_WhenEmailIsInvalid_ReportsInvalidCredentials()
    {
        var request = new LoginRequestDto { Email = "invalid-email", Password = "password" };

        var errors = Validate(request);

        Assert.Contains(errors, error =>
            error.MemberNames.Contains(nameof(LoginRequestDto.Email)) &&
            error.ErrorMessage == "Invalid email or password");
    }

    [Fact]
    [Trait("Category", "Auth")]
    public void RegisterRequestDto_WhenEmailExceedsMaximumLength_ReportsEmailValidationError()
    {
        var request = new RegisterRequestDto
        {
            Email = $"{new string('a', 245)}@example.com",
            Password = "password"
        };

        var errors = Validate(request);

        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(RegisterRequestDto.Email)));
    }

    [Fact]
    [Trait("Category", "Auth")]
    public void LoginRequestDto_WhenEmailExceedsMaximumLength_ReportsEmailValidationError()
    {
        var request = new LoginRequestDto
        {
            Email = $"{new string('a', 245)}@example.com",
            Password = "password"
        };

        var errors = Validate(request);

        Assert.Contains(errors, error =>
            error.MemberNames.Contains(nameof(LoginRequestDto.Email)) &&
            error.ErrorMessage == "Invalid email or password");
    }

    [Fact]
    [Trait("Category", "Task")]
    public void CreateTaskRequestDto_WhenRequiredFieldsAreMissing_ReportsEachField()
    {
        var request = new CreateTaskRequestDto
        {
            Title = string.Empty,
            StatusId = null,
            DueDate = null
        };

        var errors = Validate(request);

        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(CreateTaskRequestDto.Title)));
        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(CreateTaskRequestDto.StatusId)));
        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(CreateTaskRequestDto.DueDate)));
    }

    [Fact]
    [Trait("Category", "Task")]
    public void CreateTaskRequestDto_WhenTitleExceedsMaximumLength_ReportsTitleValidationError()
    {
        var request = new CreateTaskRequestDto
        {
            Title = new string('a', 201),
            StatusId = 1,
            DueDate = DateTimeOffset.UtcNow
        };

        var errors = Validate(request);

        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(CreateTaskRequestDto.Title)));
    }

    [Fact]
    [Trait("Category", "Task")]
    public void CreateTaskRequestDto_WhenDescriptionExceedsMaximumLength_ReportsDescriptionValidationError()
    {
        var request = new CreateTaskRequestDto
        {
            Title = "Task",
            Description = new string('a', 4001),
            StatusId = 1,
            DueDate = DateTimeOffset.UtcNow
        };

        var errors = Validate(request);

        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(CreateTaskRequestDto.Description)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    [Trait("Category", "Task")]
    public void CreateTaskRequestDto_WhenStatusIsOutsideAllowedRange_ReportsStatusValidationError(int statusId)
    {
        var request = new CreateTaskRequestDto
        {
            Title = "Task",
            StatusId = statusId,
            DueDate = DateTimeOffset.UtcNow
        };

        var errors = Validate(request);

        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(CreateTaskRequestDto.StatusId)));
    }

    [Fact]
    [Trait("Category", "Task")]
    public void UpdateTaskRequestDto_WhenRequiredFieldsAreMissing_ReportsEachField()
    {
        var request = new UpdateTaskRequestDto
        {
            Title = string.Empty,
            StatusId = null,
            DueDate = null
        };

        var errors = Validate(request);

        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(UpdateTaskRequestDto.Title)));
        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(UpdateTaskRequestDto.StatusId)));
        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(UpdateTaskRequestDto.DueDate)));
    }

    [Fact]
    [Trait("Category", "Task")]
    public void UpdateTaskRequestDto_WhenTitleExceedsMaximumLength_ReportsTitleValidationError()
    {
        var request = new UpdateTaskRequestDto
        {
            Title = new string('a', 101),
            StatusId = 1,
            DueDate = DateTimeOffset.UtcNow
        };

        var errors = Validate(request);

        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(UpdateTaskRequestDto.Title)));
    }

    [Fact]
    [Trait("Category", "Task")]
    public void UpdateTaskRequestDto_WhenDescriptionExceedsMaximumLength_ReportsDescriptionValidationError()
    {
        var request = new UpdateTaskRequestDto
        {
            Title = "Task",
            Description = new string('a', 1001),
            StatusId = 1,
            DueDate = DateTimeOffset.UtcNow
        };

        var errors = Validate(request);

        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(UpdateTaskRequestDto.Description)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    [Trait("Category", "Task")]
    public void UpdateTaskRequestDto_WhenStatusIsOutsideAllowedRange_ReportsStatusValidationError(int statusId)
    {
        var request = new UpdateTaskRequestDto
        {
            Title = "Task",
            StatusId = statusId,
            DueDate = DateTimeOffset.UtcNow
        };

        var errors = Validate(request);

        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(UpdateTaskRequestDto.StatusId)));
    }

    private static IReadOnlyList<ValidationResult> Validate(object request)
    {
        var errors = new List<ValidationResult>();
        Validator.TryValidateObject(request, new ValidationContext(request), errors, validateAllProperties: true);
        return errors;
    }
}
