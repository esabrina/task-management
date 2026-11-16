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

    private static IReadOnlyList<ValidationResult> Validate(object request)
    {
        var errors = new List<ValidationResult>();
        Validator.TryValidateObject(request, new ValidationContext(request), errors, validateAllProperties: true);
        return errors;
    }
}
