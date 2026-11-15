using Microsoft.AspNetCore.Identity;
using TaskManagement.Application.Interfaces;

namespace TaskManagement.Infrastructure.Identity;

public sealed class IdentityService(UserManager<ApplicationUser> userManager) : IIdentityService
{
    public async Task<IdentityRegistrationResult> RegisterAsync(
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email
        };
        var result = await userManager.CreateAsync(user, password);
        var duplicateEmail = result.Errors.Any(error => error.Code is "DuplicateEmail" or "DuplicateUserName");

        return new IdentityRegistrationResult(
            result.Succeeded,
            duplicateEmail,
            result.Errors.Select(error => error.Description).ToArray());
    }

    public async Task<Guid?> ValidateCredentialsAsync(
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await userManager.FindByEmailAsync(email);
        if (user is null || !await userManager.CheckPasswordAsync(user, password))
        {
            return null;
        }

        return user.Id;
    }
}
