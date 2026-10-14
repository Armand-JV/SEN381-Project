using CivicConnect.Application.Patterns.Observer;
using CivicConnect.Application.Patterns.Strategy;
using CivicConnect.Application.Services;
using CivicConnect.Web.Authentication;
using CivicConnect.Web.Configuration;
using CivicConnect.Web.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

// Load environment variables from .env if present.
EnvLoader.Load();

var builder = WebApplication.CreateBuilder(args);

// Ensure .env in the content root is also checked.
EnvLoader.Load(builder.Environment.ContentRootPath);

// Register services.
builder.Services.AddControllersWithViews();

const string testHeaderScheme = "TestHeader";
const string applicationAuthenticationScheme = "CivicConnectAuthentication";
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET");
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");
var hasJwtConfiguration = !string.IsNullOrWhiteSpace(jwtSecret)
    && !string.IsNullOrWhiteSpace(jwtIssuer)
    && !string.IsNullOrWhiteSpace(jwtAudience);

var authenticationBuilder = builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = applicationAuthenticationScheme;
        options.DefaultChallengeScheme = applicationAuthenticationScheme;
    })
    .AddPolicyScheme(applicationAuthenticationScheme, null, options =>
    {
        options.ForwardDefaultSelector = context =>
        {
            var authorization = context.Request.Headers.Authorization.ToString();
            return hasJwtConfiguration
                && authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                ? JwtBearerDefaults.AuthenticationScheme
                : testHeaderScheme;
        };
    })
    .AddScheme<AuthenticationSchemeOptions, TestHeaderAuthenticationHandler>(testHeaderScheme, _ => { });

if (hasJwtConfiguration)
{
    authenticationBuilder.AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret!)),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });
}

builder.Services.AddAuthorization();

builder.Services.AddScoped<CategoryValidationContext>();
builder.Services.AddScoped<ICategoryValidationStrategy, Category1ValidationStrategy>();
builder.Services.AddScoped<IRequestStateObserver, LogRequestStateObserver>();
builder.Services.AddScoped<RequestStateNotifier>();

builder.Services.AddDbContext<CivicConnectDbContext>(options =>
    options.UseNpgsql(
        EnvLoader.GetConnectionString(builder.Configuration)));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();

builder.Services.AddAuthorization();
builder.Services.AddScoped<IAccessControlService, AccessControlService>();
builder.Services.AddScoped<CategoryValidationContext>();
builder.Services.AddScoped<ICategoryValidationStrategy, Category1ValidationStrategy>();
builder.Services.AddScoped<IRequestStateObserver, LogRequestStateObserver>();
builder.Services.AddScoped<RequestStateNotifier>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler();
    app.UseHsts();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapControllers();

// Health check endpoint.
app.MapGet("/health", async (CivicConnectDbContext db) =>
{
    try
    {
        var canConnect = await db.Database.CanConnectAsync();

        return canConnect
            ? Results.Ok(new
            {
                status = "Healthy",
                database = "Connected"
            })
            : Results.Problem(
                "Cannot connect to database.",
                statusCode: StatusCodes.Status503ServiceUnavailable);
    }
    catch (Exception)
    {
        // Avoid exposing database connection details to callers.
        return Results.Problem(
            "Database connection error.",
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }
});

// M3-UI-01 (Issue #21): API-only demo path — no Razor UI.
// Root no longer 404s: Swagger in Development, API pointer otherwise.
app.MapGet("/", () =>
{
    return app.Environment.IsDevelopment()
        ? Results.Redirect("/swagger")
        : Results.Ok(new { service = "CivicConnect API", health = "/health", demo = "See docs/P3-Decisions.md M3-UI-01" });
});

app.Run();
