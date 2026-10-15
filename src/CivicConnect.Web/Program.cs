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

builder.Services.AddDbContext<CivicConnectDbContext>(options =>
    options.UseNpgsql(
        EnvLoader.GetConnectionString(builder.Configuration)));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();

// ponytail: local demo identities only; replace with a real identity provider before release.
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = "CivicConnectAuth";
        options.DefaultChallengeScheme = "CivicConnectAuth";
        options.DefaultForbidScheme = "CivicConnectAuth";
    })
    .AddPolicyScheme("CivicConnectAuth", "JWT or development test headers", options =>
    {
        options.ForwardDefaultSelector = context =>
            context.RequestServices.GetRequiredService<IHostEnvironment>().IsDevelopment() &&
            context.RequestServices.GetRequiredService<IConfiguration>().GetValue<bool>("DemoAuth:Enabled") &&
            !context.Request.Headers.ContainsKey("Authorization") &&
            context.Request.Headers.ContainsKey("X-Test-Role")
                ? "TestHeader"
                : JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER"),
            ValidAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET") ?? "super_secret_default_key_change_me"))
        };
    })
    .AddScheme<AuthenticationSchemeOptions, TestHeaderAuthenticationHandler>("TestHeader", _ => { });

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

// Keep missing API endpoints out of the SPA fallback.
app.MapFallback("/api/{**path}", () => Results.NotFound());
app.MapFallbackToFile("/index.html");

app.Run();
