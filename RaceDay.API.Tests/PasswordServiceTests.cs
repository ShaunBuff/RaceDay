
using RaceDay.Api.Services;

namespace RaceDay.API.Tests;

public class PasswordServiceTests
{
    [Fact]
    public void HashPassword_ShouldReturnHashedPassword()
    {
        // Arrange
        var service = new PasswordService();
        var email = "participant@raceday.test";
        var password = "RaceDayTestPass2026!";

        // Act
        var passwordHash = service.HashPassword(email, password);

        // Assert
        Assert.NotNull(passwordHash);
        Assert.NotEqual(password, passwordHash);
        Assert.NotEmpty(passwordHash);
    }

    [Fact]
    public void VerifyPassword_ShouldReturnTrueForCorrectPassword()
    {
        // Arrange
        var service = new PasswordService();
        var email = "participant@raceday.test";
        var password = "RaceDayTestPass2026!";
        var passwordHash = service.HashPassword(email, password);

        // Act
        var result = service.VerifyPassword(
            email, password, passwordHash);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalseForIncorrectPassword()
    {
        // Arrange
        var service = new PasswordService();
        var email = "participant@raceday.test";
        var passwordHash = service.HashPassword(
            email, "CorrectPassword2026!");

        // Act
        var result = service.VerifyPassword(
            email, "WrongPassword2026!", passwordHash);

        // Assert
        Assert.False(result);
    }
}
