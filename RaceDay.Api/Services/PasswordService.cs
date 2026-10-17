
using Microsoft.AspNetCore.Identity;

namespace RaceDay.Api.Services;

public class PasswordService
{
    private readonly PasswordHasher<string> _passwordHasher = new();

    public string HashPassword(string email, string password)
    {
        return _passwordHasher.HashPassword(email, password);
    }

    public bool VerifyPassword(
        string email,
        string password,
        string passwordHash)
    {
        var result = _passwordHasher.VerifyHashedPassword(
            email,
            passwordHash,
            password);

        return result != PasswordVerificationResult.Failed;
    }
}
