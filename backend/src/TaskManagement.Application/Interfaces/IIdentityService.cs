namespace TaskManagement.Application.Interfaces;

public sealed record IdentityRegistrationResult(bool Succeeded, bool EmailAlreadyExists, string[] Errors);

public interface IIdentityService
{
    Task<IdentityRegistrationResult> RegisterAsync(string email, string password, CancellationToken cancellationToken);
    Task<Guid?> ValidateCredentialsAsync(string email, string password, CancellationToken cancellationToken);
}
