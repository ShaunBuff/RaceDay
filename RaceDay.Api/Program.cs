
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RaceDay.Api.Data;
using RaceDay.Api.Services;
using System;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Register API controllers
builder.Services.AddControllers();

// Register the database context
builder.Services.AddDbContext<RaceDayContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("RaceDayConnection")));

// Register the password hashing service
builder.Services.AddScoped<PasswordService>();

// Read the JWT signing key from User Secrets or configuration
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException(
        "JWT signing key is missing. Configure Jwt:Key using User Secrets.");

if (Encoding.UTF8.GetByteCount(jwtKey) < 32)
{
    throw new InvalidOperationException(
        "The JWT signing key must contain at least 32 bytes.");
}

// Configure JWT authentication
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "RaceDay.Api",

            ValidateAudience = true,
            ValidAudience = "RaceDay.Client",

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey))
        };
    });

// Register authorization
builder.Services.AddAuthorization();

// Configure Swagger and JWT authorization
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "RaceDay API",
        Version = "v1",
        Description = "API for RaceDay running, walking and cycling events."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste your JWT access token here. Do not type 'Bearer ' before it."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Authentication must run before authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
