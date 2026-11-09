
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

public class EventsControllerTests
{
    private static (
        RaceDayContext Context,
        EventsController Controller
    ) CreateTestEnvironment()
    {
        var options = new DbContextOptionsBuilder<RaceDayContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new RaceDayContext(options);

        // Simulate an authenticated organiser with database ID 100.
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "100"),
            new(ClaimTypes.Name, "organiser@test.com"),
            new(ClaimTypes.Role, "Organiser")
        };

        var identity = new ClaimsIdentity(
            claims,
            "TestAuthentication");

        var principal = new ClaimsPrincipal(identity);

        var controller = new EventsController(context)
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

    private static async Task SeedEvent(RaceDayContext context)
    {
        context.Events.Add(new Event
        {
            EventID = 1,
            OrganiserID = 100,
            EventTypeID = 1,
            LocationID = 1,
            EventName = "Bloemfontein Road Race",
            Description = "Original event description",
            EventDate = new DateTime(2027, 5, 1),
            DistanceKm = 10,
            CreatedAt = new DateTime(2026, 1, 1)
        });

        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task UpdateEvent_WhenOrganiserOwnsEvent_ReturnsNoContent()
    {
        // Arrange
        var (context, controller) = CreateTestEnvironment();

        await SeedEvent(context);

        var updatedEvent = new Event
        {
            EventID = 1,
            EventName = "Updated Bloemfontein Road Race",
            Description = "Updated event description",
            EventTypeID = 1,
            LocationID = 1,
            EventDate = new DateTime(2027, 6, 1),
            DistanceKm = 21
        };

        // Act
        var result = await controller.UpdateEvent(1, updatedEvent);

        // Assert
        Assert.IsType<NoContentResult>(result);

        var savedEvent = await context.Events.SingleAsync();

        Assert.Equal(
            "Updated Bloemfontein Road Race",
            savedEvent.EventName);

        Assert.Equal(21, savedEvent.DistanceKm);

        // Ownership must remain unchanged.
        Assert.Equal(100, savedEvent.OrganiserID);

        await context.DisposeAsync();
    }

    [Fact]
    public async Task UpdateEvent_WhenAnotherOrganiserOwnsEvent_ReturnsForbid()
    {
        // Arrange
        var (context, controller) = CreateTestEnvironment();

        await SeedEvent(context);

        // Change ownership to organiser 200.
        var existingEvent = await context.Events.SingleAsync();
        existingEvent.OrganiserID = 200;

        await context.SaveChangesAsync();

        var updatedEvent = new Event
        {
            EventID = 1,
            EventName = "Unauthorised Update",
            EventTypeID = 1,
            LocationID = 1,
            EventDate = new DateTime(2027, 6, 1),
            DistanceKm = 21
        };

        // Act
        var result = await controller.UpdateEvent(1, updatedEvent);

        // Assert
        Assert.IsType<ForbidResult>(result);

        var savedEvent = await context.Events.SingleAsync();

        Assert.Equal("Bloemfontein Road Race", savedEvent.EventName);
        Assert.Equal(200, savedEvent.OrganiserID);

        await context.DisposeAsync();
    }

    [Fact]
    public async Task DeleteEvent_WhenOrganiserOwnsEvent_ReturnsNoContent()
    {
        // Arrange
        var (context, controller) = CreateTestEnvironment();

        await SeedEvent(context);

        // Act
        var result = await controller.DeleteEvent(1);

        // Assert
        Assert.IsType<NoContentResult>(result);

        Assert.Empty(await context.Events.ToListAsync());

        await context.DisposeAsync();
    }

    [Fact]
    public async Task DeleteEvent_WhenAnotherOrganiserOwnsEvent_ReturnsForbid()
    {
        // Arrange
        var (context, controller) = CreateTestEnvironment();

        await SeedEvent(context);

        // Change ownership to organiser 200.
        var existingEvent = await context.Events.SingleAsync();
        existingEvent.OrganiserID = 200;

        await context.SaveChangesAsync();

        // Act
        var result = await controller.DeleteEvent(1);

        // Assert
        Assert.IsType<ForbidResult>(result);

        // The event must not be deleted.
        Assert.Single(await context.Events.ToListAsync());

        await context.DisposeAsync();
    }
}
