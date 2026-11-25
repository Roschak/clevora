using System.Security.Claims;
using Clevora.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Clevora.Web.Services;

public class ApiKeyMiddleware(RequestDelegate next, IConfiguration config)
{
    private const string ApiKeyHeaderName = "X-API-Key";

    public async Task InvokeAsync(HttpContext context, ClevoraDbContext db)
    {
        if (context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
        {
            var configuredApiKey = config["ApiKey"] ?? "clevora-dev-secret-key-2026";
            if (string.Equals(extractedApiKey, configuredApiKey, StringComparison.Ordinal))
            {
                // Authenticate as a system or default user
                var user = await db.Users.FirstOrDefaultAsync();
                var userId = user?.Id ?? "system-api-user";
                var email = user?.Email ?? "system@clevora.local";

                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId),
                    new Claim(ClaimTypes.Email, email),
                    new Claim(ClaimTypes.Name, "ApiKeyUser"),
                    new Claim("auth_method", "api_key")
                };

                var identity = new ClaimsIdentity(claims, "ApiKey");
                context.User = new ClaimsPrincipal(identity);
            }
        }

        await next(context);
    }
}
