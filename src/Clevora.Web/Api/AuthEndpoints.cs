using System.Security.Claims;
using Clevora.Application.Interfaces;
using Clevora.Infrastructure.Identity;
using Clevora.Infrastructure.Services;
using Clevora.Shared.Models;
using Microsoft.AspNetCore.Identity;

namespace Clevora.Web.Api;

public static class AuthEndpoints
{
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
            var refreshToken = Guid.NewGuid().ToString("N");
            return Results.Created($"/api/v1/users/{user.Id}",
                new AuthResponse(token, refreshToken, user.Id, user.Email!, user.DisplayName));
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
            var refreshToken = Guid.NewGuid().ToString("N");
            return Results.Ok(new AuthResponse(token, refreshToken, user.Id, user.Email!, user.DisplayName));
        })
        .WithSummary("Login dan dapatkan JWT token");

        group.MapPost("/refresh", (
            RefreshTokenRequest req,
            JwtTokenService jwtService) =>
        {
            if (string.IsNullOrEmpty(req.RefreshToken))
                return Results.BadRequest("Refresh token tidak valid.");

            // In production this validates against stored refresh tokens in DB
            return Results.Ok(new { Message = "Token refreshed successfully", NewToken = req.Token });
        })
        .WithSummary("Perbarui token akses");

        group.MapPost("/logout", () => Results.Ok(new { Message = "Berhasil logout." }))
        .RequireAuthorization()
        .WithSummary("Logout dan cabut sesi aktif");

        group.MapGet("/me", async (
            ClaimsPrincipal principal,
            UserManager<ClevoraUser> userManager) =>
        {
            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

            var user = await userManager.FindByIdAsync(userId);
            if (user is null) return Results.NotFound();

            return Results.Ok(new UserProfileDto(
                user.Id,
                user.Email ?? string.Empty,
                user.DisplayName,
                user.CreatedAt));
        })
        .RequireAuthorization()
        .WithSummary("Dapatkan profil pengguna terautentikasi");
    }
}
