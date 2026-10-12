using CivicConnect.Web.Configuration;
using CivicConnect.Web.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;

// Load environment variables from .env if present
EnvLoader.Load();

var builder = WebApplication.CreateBuilder(args);

// Ensure .env in content root is also checked
EnvLoader.Load(builder.Environment.ContentRootPath);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<CivicConnectDbContext>(options =>
    options.UseNpgsql(EnvLoader.GetConnectionString(builder.Configuration)));

        // Ensure .env in content root is also checked
        EnvLoader.Load(builder.Environment.ContentRootPath);

        // Add services to the container.
        builder.Services.AddControllersWithViews();

        builder.Services.AddDbContext<CivicConnectDbContext>(options =>
            options.UseNpgsql(EnvLoader.GetConnectionString(builder.Configuration)));

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

        app.MapGet("/health", async (CivicConnectDbContext db) =>
        {
            try
            {
                var canConnect = await db.Database.CanConnectAsync();
                return canConnect
                    ? Results.Ok(new { status = "Healthy", database = "Connected" })
                    : Results.Problem("Cannot connect to database.", statusCode: 503);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Database connection error: {ex.Message}", statusCode: 503);
            }
        });

app.MapGet("/health", async (CivicConnectDbContext db) =>
{
    try
    {
        var canConnect = await db.Database.CanConnectAsync();
        return canConnect
            ? Results.Ok(new { status = "Healthy", database = "Connected" })
            : Results.Problem("Cannot connect to database.", statusCode: 503);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Database connection error: {ex.Message}", statusCode: 503);
    }
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

        app.Run();
    }
}