
using System;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Services;
using RaceDay.Mvc.ViewModels;

namespace RaceDay.Mvc.Controllers
{
    public class AuthController : Controller
    {
        private readonly RaceDayApiService _api;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            RaceDayApiService api,
            ILogger<AuthController> logger)
        {
            _api = api;
            _logger = logger;
        }

        // GET: Auth/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: Auth/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var response = await _api.PostAsync(
                    "api/Auth/register", model);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] =
                        "Registration successful. Please log in.";

                    return RedirectToAction(nameof(Login));
                }

                var errorMessage =
                    await response.Content.ReadAsStringAsync();

                _logger.LogWarning(
                    "Registration failed. API status: {StatusCode}. Response: {Response}",
                    response.StatusCode,
                    errorMessage);

                ModelState.AddModelError(
                    "",
                    "Registration failed. This email may already be registered, or the supplied details are invalid.");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An error occurred while registering a RaceDay user.");

                ModelState.AddModelError(
                    "",
                    "An unexpected error occurred during registration. Please try again.");
            }

            return View(model);
        }

        // GET: Auth/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var response = await _api.PostAsync(
                    "api/Auth/login", model);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage =
                        await response.Content.ReadAsStringAsync();

                    _logger.LogWarning(
                        "Login rejected. API status: {StatusCode}. Response: {Response}",
                        response.StatusCode,
                        errorMessage);

                    ModelState.AddModelError(
                        "",
                        "Invalid email address or password.");

                    return View(model);
                }

                var json =
                    await response.Content.ReadAsStringAsync();

                using var document = JsonDocument.Parse(json);

                var root = document.RootElement;

                if (!root.TryGetProperty("token", out var tokenElement) ||
                    !root.TryGetProperty("user", out var userElement))
                {
                    throw new JsonException(
                        "The API login response is missing the token or user object.");
                }

                var token = tokenElement.GetString();

                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new JsonException(
                        "The API returned an empty authentication token.");
                }

                if (!userElement.TryGetProperty(
                        "userID", out var userIdElement) ||
                    !userElement.TryGetProperty(
                        "firstName", out var firstNameElement) ||
                    !userElement.TryGetProperty(
                        "role", out var roleElement))
                {
                    throw new JsonException(
                        "The API user response is missing required user details.");
                }

                var firstName = firstNameElement.GetString() ?? "";
                var role = roleElement.GetString() ?? "";

                HttpContext.Session.SetString("Token", token);

                HttpContext.Session.SetString(
                    "UserID",
                    userIdElement.ToString());

                HttpContext.Session.SetString(
                    "FirstName",
                    firstName);

                HttpContext.Session.SetString(
                    "Role",
                    role);

                TempData["Success"] = "Welcome to RaceDay!";

                _logger.LogInformation(
                    "User {UserID} logged in successfully with role {Role}.",
                    userIdElement,
                    role);

                if (role.Equals(
                        "Organiser",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return RedirectToAction(
                        "Dashboard",
                        "Organiser");
                }

                return RedirectToAction(
                    "Index",
                    "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An error occurred while logging in to RaceDay.");

                ModelState.AddModelError(
                    "",
                    "An unexpected error occurred during login. Please try again.");

                return View(model);
            }
        }

        // POST: Auth/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            TempData["Success"] = "You have logged out successfully.";

            return RedirectToAction(
                "Index",
                "Home");
        }
    }
}
