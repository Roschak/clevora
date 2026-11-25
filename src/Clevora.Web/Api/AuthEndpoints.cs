using Clevora.Infrastructure.Identity;
using Clevora.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;

namespace Clevora.Web.Api;

public static class AuthEndpoints
{
    public record RegisterRequest(string Email, string Password, string? DisplayName);
    public record LoginRequest(string Email, string Password);
    public record AuthResponse(string Token, string UserId, string Email, string? DisplayName);

    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/auth").WithTags("Auth");

        group.MapPost("/register", async (
            RegisterRequest req,
            UserManager<ClevoraUser> userManager,
            JwtTokenService jwtService) =>
        {
            if (await userManager.FindByEmailAsync(req.Email) is not null)
                return Results.Conflict("Email sudah digunakan.");

            var user = new ClevoraUser
            {
                UserName = req.Email,
                Email = req.Email,
                DisplayName = req.DisplayName
            };

            var result = await userManager.CreateAsync(user, req.Password);
            if (!result.Succeeded)
                return Results.BadRequest(result.Errors.Select(e => e.Description));

            var token = jwtService.GenerateToken(user, []);
            return Results.Created($"/api/v1/users/{user.Id}",
                new AuthResponse(token, user.Id, user.Email!, user.DisplayName));
        })
        .WithSummary("Daftarkan akun baru");

        group.MapPost("/login", async (
            LoginRequest req,
            UserManager<ClevoraUser> userManager,
            SignInManager<ClevoraUser> signInManager,
            JwtTokenService jwtService) =>
        {
            var user = await userManager.FindByEmailAsync(req.Email);
            if (user is null) return Results.Unauthorized();

            var check = await signInManager.CheckPasswordSignInAsync(user, req.Password, lockoutOnFailure: false);
            if (!check.Succeeded) return Results.Unauthorized();

            var roles = await userManager.GetRolesAsync(user);
            var token = jwtService.GenerateToken(user, roles);
            return Results.Ok(new AuthResponse(token, user.Id, user.Email!, user.DisplayName));
        })
        .WithSummary("Login dan dapatkan JWT token");
    }
}
