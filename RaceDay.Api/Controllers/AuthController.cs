
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;
using RaceDay.Api.Services;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly RaceDayContext _context;
    private readonly PasswordService _passwordService;
    private readonly IConfiguration _configuration;

    public AuthController(
        RaceDayContext context,
        PasswordService passwordService,
        IConfiguration configuration)
    {
        _context = context;
        _passwordService = passwordService;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var emailExists = await _context.Users
            .AnyAsync(u => u.Email.ToLower() == email);

        if (emailExists)
        {
            return Conflict(new
            {
                message = "An account with this email address already exists."
            });
        }

        var user = new User
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = email,
            PasswordHash = _passwordService.HashPassword(
                email, request.Password),
            Role = "Participant",
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return StatusCode(201, new
        {
            message = "Registration successful.",
            user.UserID,
            user.FirstName,
            user.LastName,
            user.Email,
            user.Role
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _context.Users
            .SingleOrDefaultAsync(u => u.Email.ToLower() == email);

        if (user == null ||
            !_passwordService.VerifyPassword(
                email, request.Password, user.PasswordHash))
        {
            return Unauthorized(new
            {
                message = "Invalid email address or password."
            });
        }

        var jwtKey = _configuration["Jwt:Key"];

        if (string.IsNullOrWhiteSpace(jwtKey))
        {
            return StatusCode(500, new
            {
                message = "Authentication is not configured."
            });
        }

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey));

        var credentials = new SigningCredentials(
            key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
            new Claim(ClaimTypes.Name, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: "RaceDay.Api",
            audience: "RaceDay.Client",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return Ok(new
        {
            message = "Login successful.",
            token = new JwtSecurityTokenHandler().WriteToken(token),
            expiresAt = token.ValidTo,
            user = new
            {
                user.UserID,
                user.FirstName,
                user.LastName,
                user.Email,
                user.Role
            }
        });
    }
}

