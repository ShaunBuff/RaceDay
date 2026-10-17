
using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Controllers;
using Xunit;

namespace RaceDay.API.Tests;

public class ControllerAuthorizationTests
{
    private static AuthorizeAttribute? GetAuthorizeAttribute(
        Type controllerType,
        string methodName)
    {
        var method = controllerType.GetMethod(methodName);

        if (method == null)
        {
            return null;
        }

        // First check authorization on the action method.
        var methodAttribute = method
            .GetCustomAttributes<AuthorizeAttribute>(true)
            .FirstOrDefault();

        if (methodAttribute != null)
        {
            return methodAttribute;
        }

        // If not found on the action, check the controller.
        return controllerType
            .GetCustomAttributes<AuthorizeAttribute>(true)
            .FirstOrDefault();
    }

    [Fact]
    public void ResultsController_ShouldRequireAuthentication()
    {
        var attributes = typeof(ResultsController)
            .GetCustomAttributes<AuthorizeAttribute>(true);

        Assert.NotEmpty(attributes);
    }

    [Fact]
    public void ResultsPost_ShouldRequireOrganiserRole()
    {
        var attribute = GetAuthorizeAttribute(
            typeof(ResultsController),
            "CreateResult");

        Assert.NotNull(attribute);
        Assert.Equal("Organiser", attribute!.Roles);
    }

    [Fact]
    public void EnrolmentsPost_ShouldRequireParticipantRole()
    {
        // Find the action decorated with HttpPost instead of
        // assuming its method name.
        var method = typeof(EnrolmentsController)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(m =>
                m.GetCustomAttributes<HttpPostAttribute>(true).Any());

        Assert.NotNull(method);

        var methodAttribute = method!
            .GetCustomAttributes<AuthorizeAttribute>(true)
            .FirstOrDefault();

        var controllerAttribute = typeof(EnrolmentsController)
            .GetCustomAttributes<AuthorizeAttribute>(true)
            .FirstOrDefault();

        var attribute = methodAttribute ?? controllerAttribute;

        Assert.NotNull(attribute);
        Assert.Equal("Participant", attribute!.Roles);
    }

    [Fact]
    public void CategoriesPost_ShouldRequireOrganiserRole()
    {
        var attribute = GetAuthorizeAttribute(
            typeof(CategoriesController),
            "CreateCategory");

        Assert.NotNull(attribute);
        Assert.Equal("Organiser", attribute!.Roles);
    }

    [Fact]
    public void ProfileController_ShouldAllowBothRequiredRoles()
    {
        var attributes = typeof(ProfileController)
            .GetCustomAttributes<AuthorizeAttribute>(true);

        var attribute = attributes.FirstOrDefault();

        Assert.NotNull(attribute);
        Assert.NotNull(attribute!.Roles);

        var roles = attribute.Roles!
            .Split(',')
            .Select(role => role.Trim())
            .ToArray();

        Assert.Contains("Organiser", roles);
        Assert.Contains("Participant", roles);
    }

    [Fact]
    public void EventsPost_ShouldRequireOrganiserRole()
    {
        var attribute = GetAuthorizeAttribute(
            typeof(EventsController),
            "CreateEvent");

        Assert.NotNull(attribute);
        Assert.Equal("Organiser", attribute!.Roles);
    }
}
