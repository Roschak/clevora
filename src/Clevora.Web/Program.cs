using System.Text;
using Clevora.Application.Interfaces;
using Clevora.Domain.Entities;
using Clevora.Infrastructure;
using Clevora.Infrastructure.Data;
using Clevora.Infrastructure.Identity;
using Clevora.Web.Api;
using Clevora.Web.Components;
using Clevora.Web.Components.Account;
using Clevora.Web.Data;
using Clevora.Web.Hubs;
using Clevora.Web.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Bind to 0.0.0.0 so that mobile devices, emulators, and LAN clients can connect
if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASPNETCORE_URLS")))
{
    builder.WebHost.UseUrls("http://0.0.0.0:5000;http://localhost:5000;https://localhost:5001");
}

// ─── Infrastructure (EF + Identity + Services) ───────────────────────────────
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSingleton<IForumWebService, ForumWebService>();

// ─── HttpContext / CurrentUser ────────────────────────────────────────────────
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, HttpContextCurrentUserService>();

// ─── ASP.NET Core Identity (cookie auth for Blazor Server) ───────────────────
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

// Also configure JWT Bearer for the REST API
var jwtKey = builder.Configuration["Jwt:Key"] ?? "clevora-dev-secret-key-must-be-at-least-32chars!";
builder.Services.AddAuthentication()
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "clevora",
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "clevora-clients",
            ValidateLifetime = true
        };
        // Allow SignalR connections to pass the token via query string
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                    context.Token = accessToken;
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder(
        IdentityConstants.ApplicationScheme)
        .RequireAuthenticatedUser()
        .Build();

    options.AddPolicy("ApiPolicy", policy =>
    {
        policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme, IdentityConstants.ApplicationScheme);
        policy.RequireAuthenticatedUser();
    });
});

// Identity helpers
builder.Services.AddScoped<IUserClaimsPrincipalFactory<ClevoraUser>, UserClaimsPrincipalFactory<ClevoraUser, IdentityRole>>();
builder.Services.AddScoped<SignInManager<ClevoraUser>>();
builder.Services.AddScoped<ISecurityStampValidator, SecurityStampValidator<ClevoraUser>>();
builder.Services.AddScoped<ITwoFactorSecurityStampValidator, TwoFactorSecurityStampValidator<ClevoraUser>>();
builder.Services.AddSingleton<IEmailSender<ClevoraUser>, IdentityNoOpEmailSender>();

// ─── SignalR ──────────────────────────────────────────────────────────────────
builder.Services.AddSignalR();

// ─── Swagger / OpenAPI ────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Clevora Chat API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Masukkan token JWT: Bearer {token}"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// ─── Blazor ───────────────────────────────────────────────────────────────────
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ─── Health Checks ────────────────────────────────────────────────────────────
builder.Services.AddHealthChecks();

var app = builder.Build();

// ─── Migrate & seed DB on startup ────────────────────────────────────────────
await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ClevoraDbContext>();
    await db.Database.EnsureCreatedAsync();

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ClevoraUser>>();

    // Seed User 1: Kang Fadhil
    var kf = await userManager.FindByEmailAsync("fadhil@gravicode.com");
    if (kf is null)
    {
        kf = new ClevoraUser
        {
            UserName = "fadhil@gravicode.com",
            Email = "fadhil@gravicode.com",
            DisplayName = "Kang Fadhil (Gravicode Studios)",
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };
        await userManager.CreateAsync(kf, "Password123!");
    }

    // Seed User 2: Demo User
    var demo = await userManager.FindByEmailAsync("user@clevora.com");
    if (demo is null)
    {
        demo = new ClevoraUser
        {
            UserName = "user@clevora.com",
            Email = "user@clevora.com",
            DisplayName = "Pengguna Clevora",
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };
        await userManager.CreateAsync(demo, "Password123!");
    }

    // Seed User 3: Iruzzz67
    var iruzzz = await userManager.FindByEmailAsync("iruzzz@gravicode.com");
    if (iruzzz is null)
    {
        iruzzz = new ClevoraUser
        {
            UserName = "iruzzz@gravicode.com",
            Email = "iruzzz@gravicode.com",
            DisplayName = "Iruzzz67 (DevOps)",
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };
        await userManager.CreateAsync(iruzzz, "Password123!");
    }

    // Seed User 4: Budi Santoso
    var budi = await userManager.FindByEmailAsync("budisantoso@gravicode.com");
    if (budi is null)
    {
        budi = new ClevoraUser
        {
            UserName = "budisantoso@gravicode.com",
            Email = "budisantoso@gravicode.com",
            DisplayName = "Budi Santoso",
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };
        await userManager.CreateAsync(budi, "Password123!");
    }

    // Seed Default Conversations if none exist
    if (!await db.Conversations.AnyAsync())
    {
        // 1. Private Chat: Kang Fadhil <-> Demo User
        var conv1 = new Conversation
        {
            IsGroup = false,
            Name = "Kang Fadhil (Gravicode Studios)",
            CreatedAt = DateTime.UtcNow.AddHours(-3),
            UpdatedAt = DateTime.UtcNow
        };
        db.Conversations.Add(conv1);
        await db.SaveChangesAsync();

        db.ConversationMembers.Add(new ConversationMember { ConversationId = conv1.Id, UserId = kf.Id });
        db.ConversationMembers.Add(new ConversationMember { ConversationId = conv1.Id, UserId = demo.Id });

        db.Messages.Add(new Message
        {
            ConversationId = conv1.Id,
            SenderId = kf.Id,
            Body = "Halo! Selamat datang di Clevora Web & Mobile WhatsApp. Aplikasi siap digunakan secara offline dan real-time 👍",
            Type = MessageType.Text,
            Status = MessageStatus.Read,
            SentAt = DateTime.UtcNow.AddHours(-2)
        });
        db.Messages.Add(new Message
        {
            ConversationId = conv1.Id,
            SenderId = demo.Id,
            Body = "Terima kasih Kang Fadhil! Desain dan performanya sangat responsif.",
            Type = MessageType.Text,
            Status = MessageStatus.Read,
            SentAt = DateTime.UtcNow.AddMinutes(-30)
        });

        // 2. Group Chat: Tim Clevora Gravicode 🚀
        var conv2 = new Conversation
        {
            IsGroup = true,
            Name = "Tim Clevora Gravicode 🚀",
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow
        };
        db.Conversations.Add(conv2);
        await db.SaveChangesAsync();

        db.ConversationMembers.Add(new ConversationMember { ConversationId = conv2.Id, UserId = kf.Id });
        db.ConversationMembers.Add(new ConversationMember { ConversationId = conv2.Id, UserId = demo.Id });
        db.ConversationMembers.Add(new ConversationMember { ConversationId = conv2.Id, UserId = iruzzz.Id });

        db.Messages.Add(new Message
        {
            ConversationId = conv2.Id,
            SenderId = iruzzz.Id,
            Body = "Server SignalR hub dan database SQLite WAL siap melayani sinkronisasi data real-time.",
            Type = MessageType.Text,
            Status = MessageStatus.Delivered,
            SentAt = DateTime.UtcNow.AddMinutes(-45)
        });
        db.Messages.Add(new Message
        {
            ConversationId = conv2.Id,
            SenderId = kf.Id,
            Body = "Pastikan seluruh fitur chat web dan mobile dapat digunakan dengan mulus oleh pengguna.",
            Type = MessageType.Text,
            Status = MessageStatus.Delivered,
            SentAt = DateTime.UtcNow.AddMinutes(-15)
        });

        await db.SaveChangesAsync();
    }
}

// ─── Middleware Pipeline ──────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
    app.UseHttpsRedirection();
}
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Clevora Chat API v1"));
app.UseMiddleware<ApiKeyMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

// ─── API Endpoints ────────────────────────────────────────────────────────────
app.MapAuthEndpoints();
app.MapChatEndpoints();

// ─── SignalR Hub ──────────────────────────────────────────────────────────────
app.MapHub<ChatHub>("/hubs/chat");

// ─── Health ───────────────────────────────────────────────────────────────────
app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");

// ─── Identity UI endpoints ────────────────────────────────────────────────────
app.MapAdditionalIdentityEndpoints();

app.MapPost("/Account/QuickLogin", async (
    HttpContext context,
    SignInManager<ClevoraUser> signInManager,
    UserManager<ClevoraUser> userManager) =>
{
    var form = await context.Request.ReadFormAsync();
    var identifier = form["email"].ToString()?.Trim();
    if (string.IsNullOrWhiteSpace(identifier)) identifier = "fadhil@gravicode.com";

    var user = await userManager.FindByEmailAsync(identifier)
               ?? await userManager.FindByNameAsync(identifier)
               ?? await userManager.FindByIdAsync(identifier)
               ?? await userManager.Users.FirstOrDefaultAsync(u => u.DisplayName == identifier || u.UserName == identifier);

    if (user is null)
    {
        var isEmail = identifier.Contains('@');
        var email = isEmail ? identifier : $"{identifier.ToLowerInvariant().Replace(" ", "_")}@clevora.local";
        var userName = isEmail ? identifier.Split('@')[0] : identifier;

        user = new ClevoraUser
        {
            UserName = userName,
            Email = email,
            DisplayName = identifier,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };
        await userManager.CreateAsync(user, "Password123!");
    }

    await signInManager.SignInAsync(user, isPersistent: true);
    return Results.LocalRedirect("/");
}).DisableAntiforgery();

// ─── Blazor ───────────────────────────────────────────────────────────────────
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
