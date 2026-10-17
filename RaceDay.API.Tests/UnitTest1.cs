
using System;
using RaceDay.Api.Models;

namespace RaceDay.API.Tests;

public class UnitTest1
{
    [Fact]
    public void Event_ShouldStoreEventDetailsCorrectly()
    {
        // Arrange: create an event for testing
        var raceEvent = new Event
        {
            EventID = 1,
            OrganiserID = 1,
            EventTypeID = 1,
            LocationID = 1,
            EventName = "Cape Town Road Run",
            Description = "A road running event in Cape Town.",
            EventDate = new DateTime(2027, 3, 14),
            DistanceKm = 21.1m
        };

        // Act: retrieve the event name and distance
        var eventName = raceEvent.EventName;
        var distance = raceEvent.DistanceKm;

        // Assert: verify that the details are correct
        Assert.Equal("Cape Town Road Run", eventName);
        Assert.Equal(21.1m, distance);
        Assert.Equal(1, raceEvent.OrganiserID);
    }

    [Fact]
    public void Category_ShouldStoreCategoryDetailsCorrectly()
    {
        // Arrange: create a category for testing
        var category = new Category
        {
            CategoryID = 1,
            EventID = 1,
            CategoryName = "Half Marathon",
            DistanceKm = 21.1m
        };

        // Assert: verify that the category details are correct
        Assert.Equal(1, category.CategoryID);
        Assert.Equal(1, category.EventID);
        Assert.Equal("Half Marathon", category.CategoryName);
        Assert.Equal(21.1m, category.DistanceKm);
    }

    [Fact]
    public void Enrolment_ShouldUseRegisteredAsDefaultStatus()
    {
        // Arrange: create a new enrolment
        var enrolment = new Enrolment();

        // Assert: verify the default status
        Assert.Equal("Registered", enrolment.Status);
    }
}
