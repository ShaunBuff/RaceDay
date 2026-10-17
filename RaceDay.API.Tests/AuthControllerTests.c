
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RaceDay.Api.Controllers;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;
using RaceDay.Api.Services;
using Xunit;

namespace RaceDay.API.Tests;

public class AuthControllerTests
{
    private static (
        RaceDayContext Context,
        AuthController Controller,
        PasswordService PasswordService
    ) CreateTestEnvironment()
    {
        var options = new DbContextOptionsBuilder<RaceDayContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new RaceDayContext(options);

        var passwordService = new PasswordService();

        var settings = new Dictionary<string, string?>
        {
            ["Jwt:Key"] =
                "RaceDay-Test-Secret-Key-2026-Long-Enough!"
        };

        IConfiguration configuration =
            new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();

        var controller = new AuthController(
            context,
            passwordService,
            configuration);

        return (context, controller, passwordService);
    }

    [Fact]
    public async Task Register_WithNewEmail_ReturnsCreatedAndHashesPassword()
    {
        // Arrange
        var (context, controller, _) = CreateTestEnvironment();

        var request = new RegisterRequest
        {
            FirstName = "Test",
            LastName = "Participant",
            Email = "new.participant@test.com",
            Password = "RaceDayPass123!"
        };

        // Act
        var result = await controller.Register(request);

        // Assert
        var response = Assert.IsType<ObjectResult>(result);
        Assert.Equal(201, response.StatusCode);

        var savedUser = await context.Users.SingleAsync();

        Assert.Equal("new.participant@test.com", savedUser.Email);
        Assert.Equal("Participant", savedUser.Role);
        Assert.NotEqual("RaceDayPass123!", savedUser.PasswordHash);
        Assert.False(string.IsNullOrWhiteSpace(savedUser.PasswordHash));

        await context.DisposeAsync();
    }

    [Fact]
    public async Task Register_WithExistingEmail_ReturnsConflict()
    {
        // Arrange
        var (context, controller, passwordService) =
            CreateTestEnvironment();

        context.Users.Add(new User
        {
            FirstName = "Existing",
            LastName = "Participant",
            Email = "existing@test.com",
            PasswordHash = passwordService.HashPassword(
                "existing@test.com", "OriginalPass123!"),
            Role = "Participant",
            CreatedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync();

        var request = new RegisterRequest
        {
            FirstName = "Another",
            LastName = "Participant",
            Email = "EXISTING@test.com",
            Password = "AnotherPass123!"
        };

        // Act
        var result = await controller.Register(request);

        // Assert
        Assert.IsType<ConflictObjectResult>(result);
        Assert.Equal(1, await context.Users.CountAsync());

        await context.DisposeAsync();
    }

    [Fact]
    public async Task Login_WithUnknownEmail_ReturnsUnauthorized()
    {
        // Arrange
        var (context, controller, _) = CreateTestEnvironment();

        var request = new LoginRequest
        {
            Email = "unknown@test.com",
            Password = "SomePassword123!"
        };

        // Act
        var result = await controller.Login(request);

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result);

        await context.DisposeAsync();
    }

    [Fact]
    public async Task Login_WithIncorrectPassword_ReturnsUnauthorized()
    {
        // Arrange
        var (context, controller, passwordService) =
            CreateTestEnvironment();

        context.Users.Add(new User
        {
            FirstName = "Test",
            LastName = "Participant",
            Email = "participant@test.com",
            PasswordHash = passwordService.HashPassword(
                "participant@test.com", "CorrectPass123!"),
            Role = "Participant",
            CreatedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync();

        var request = new LoginRequest
        {
            Email = "participant@test.com",
            Password = "WrongPass123!"
        };

        // Act
        var result = await controller.Login(request);

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result);

        await context.DisposeAsync();
    }

    [Fact]
    public async Task Login_WithCorrectCredentials_ReturnsToken()
    {
        // Arrange
        var (context, controller, passwordService) =
            CreateTestEnvironment();

        context.Users.Add(new User
        {
            FirstName = "Test",
            LastName = "Participant",
            Email = "participant@test.com",
            PasswordHash = passwordService.HashPassword(
                "participant@test.com", "CorrectPass123!"),
            Role = "Participant",
            CreatedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync();

        var request = new LoginRequest
        {
            Email = "PARTICIPANT@test.com",
            Password = "CorrectPass123!"
        };

        // Act
        var result = await controller.Login(request);

        // Assert
        var response = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(response.Value);

        var tokenProperty = response.Value.GetType()
            .GetProperty("token");

        Assert.NotNull(tokenProperty);

        var token = tokenProperty!.GetValue(response.Value) as string;

        Assert.False(string.IsNullOrWhiteSpace(token));

        await context.DisposeAsync();
    }
}
