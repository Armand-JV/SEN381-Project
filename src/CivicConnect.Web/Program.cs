```csharp
using CivicConnect.Web.Configuration;
using CivicConnect.Web.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

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

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
```