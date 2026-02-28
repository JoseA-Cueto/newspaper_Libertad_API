using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Libertad.Api.Endpoints.Auth;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithName("Auth");

        group.MapPost("/login", Login)
            .WithName("Login")
            .WithSummary("Authenticate admin user and return JWT")
            .AllowAnonymous()
            .Produces<LoginResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status500InternalServerError);
    }

    private static IResult Login(
        [FromServices] IConfiguration configuration,
        [FromBody] LoginRequest request)
    {
        var configuredUsername = configuration["Auth:Admin:Username"];
        var configuredPassword = configuration["Auth:Admin:Password"];

        if (string.IsNullOrWhiteSpace(configuredUsername) || string.IsNullOrWhiteSpace(configuredPassword))
        {
            return Results.Problem("Auth admin credentials are not configured.", statusCode: StatusCodes.Status500InternalServerError);
        }

        if (!string.Equals(request.Username, configuredUsername, StringComparison.Ordinal) ||
            !string.Equals(request.Password, configuredPassword, StringComparison.Ordinal))
        {
            return Results.Unauthorized();
        }

        var issuer = configuration["Auth:Jwt:Issuer"];
        var audience = configuration["Auth:Jwt:Audience"];
        var signingKey = configuration["Auth:Jwt:SigningKey"];

        if (string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(audience) || string.IsNullOrWhiteSpace(signingKey))
        {
            return Results.Problem("JWT configuration is incomplete.", statusCode: StatusCodes.Status500InternalServerError);
        }

        if (signingKey.Length < 32)
        {
            return Results.Problem("JWT signing key must be at least 32 characters.", statusCode: StatusCodes.Status500InternalServerError);
        }

        var expiresAt = DateTimeOffset.UtcNow.AddHours(12);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, configuredUsername),
            new(ClaimTypes.Role, "Author"),
            new(ClaimTypes.Role, "Editor")
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        return Results.Ok(new LoginResponse(accessToken, expiresAt));
    }
}

public sealed record LoginRequest(string Username, string Password);

public sealed record LoginResponse(string AccessToken, DateTimeOffset ExpiresAt);
