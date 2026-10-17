
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Controllers;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using Xunit;

namespace RaceDay.API.Tests;

public class EnrolmentsControllerTests
{
    private static (
        RaceDayContext Context,
        EnrolmentsController Controller
    ) CreateTestEnvironment()
    {
        var options = new DbContextOptionsBuilder<RaceDayContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new RaceDayContext(options);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "100"),
            new(ClaimTypes.Name, "participant@test.com"),
            new(ClaimTypes.Role, "Participant")
        };

        var identity = new ClaimsIdentity(
            claims,
            "TestAuthentication");

        var principal = new ClaimsPrincipal(identity);

        var controller = new EnrolmentsController(context)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = principal
                }
            }
        };

        return (context, controller);
    }

    private static async Task SeedEventAndCategory(
        RaceDayContext context)
    {
        context.Events.AddRange(
            new Event
            {
                EventID = 1,
                OrganiserID = 10,
                EventTypeID = 1,
                LocationID = 1,
                EventName = "Bloemfontein Road Race",
                EventDate = new DateTime(2027, 5, 1),
                DistanceKm = 10
            },
            new Event
            {
                EventID = 2,
                OrganiserID = 10,
                EventTypeID = 1,
                LocationID = 1,
                EventName = "Welkom Road Race",
                EventDate = new DateTime(2027, 6, 1),
                DistanceKm = 21
            });

        context.Categories.AddRange(
            new Category
            {
                CategoryID = 1,
                EventID = 1,
                CategoryName = "10 km",
                DistanceKm = 10
            },
            new Category
            {
                CategoryID = 2,
                EventID = 2,
                CategoryName = "Half Marathon",
                DistanceKm = 21
            });

        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task CreateEnrolment_WithInvalidEventId_ReturnsNotFound()
    {
        // Arrange
        var (context, controller) = CreateTestEnvironment();

        await SeedEventAndCategory(context);

        var request = new CreateEnrolmentRequest
        {
            EventID = 999,
            CategoryID = 1
        };

        // Act
        var result = await controller.CreateEnrolment(request);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);

        Assert.Empty(
            await context.Enrolments.ToListAsync());

        await context.DisposeAsync();
    }

    [Fact]
    public async Task CreateEnrolment_WithCategoryFromAnotherEvent_ReturnsBadRequest()
    {
        // Arrange
        var (context, controller) = CreateTestEnvironment();

        await SeedEventAndCategory(context);

        var request = new CreateEnrolmentRequest
        {
            EventID = 1,
            CategoryID = 2
        };

        // Act
        var result = await controller.CreateEnrolment(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);

        Assert.Empty(
            await context.Enrolments.ToListAsync());

        await context.DisposeAsync();
    }

    [Fact]
    public async Task CreateEnrolment_WhenAlreadyEnrolled_ReturnsConflict()
    {
        // Arrange
        var (context, controller) = CreateTestEnvironment();

        await SeedEventAndCategory(context);

        context.Enrolments.Add(new Enrolment
        {
            ParticipantID = 100,
            EventID = 1,
            CategoryID = 1,
            EnrolmentDate = DateTime.UtcNow,
            Status = "Registered"
        });

        await context.SaveChangesAsync();

        var request = new CreateEnrolmentRequest
        {
            EventID = 1,
            CategoryID = 1
        };

        // Act
        var result = await controller.CreateEnrolment(request);

        // Assert
        Assert.IsType<ConflictObjectResult>(result);

        Assert.Single(
            await context.Enrolments.ToListAsync());

        await context.DisposeAsync();
    }

    [Fact]
    public async Task CreateEnrolment_WithValidRequest_ReturnsCreated()
    {
        // Arrange
        var (context, controller) = CreateTestEnvironment();

        await SeedEventAndCategory(context);

        var request = new CreateEnrolmentRequest
        {
            EventID = 1,
            CategoryID = 1
        };

        // Act
        var result = await controller.CreateEnrolment(request);

        // Assert
        var response = Assert.IsType<ObjectResult>(result);

        Assert.Equal(201, response.StatusCode);

        var savedEnrolment =
            await context.Enrolments.SingleAsync();

        Assert.Equal(100, savedEnrolment.ParticipantID);
        Assert.Equal(1, savedEnrolment.EventID);
        Assert.Equal(1, savedEnrolment.CategoryID);
        Assert.Equal("Registered", savedEnrolment.Status);

        await context.DisposeAsync();
    }
}
