using ArasERP.BuildingBlocks.Presentation.Middleware;
using ArasERP.Modules.Inventory;
using ArasERP.Modules.Inventory.Api.Endpoints;
using ArasERP.Modules.Inventory.Infrastructure;
using ArasERP.Modules.Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddInventoryModule();
builder.Services.AddInventoryInfrastructure(
    builder.Configuration.GetConnectionString("DefaultConnection")!
);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();

    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
    }
}

app.UseHttpsRedirection();

app.UseRequestLogging();
app.UseExceptionHandling();
app.UseRequestId();

app.MapGet("/health", () => Results.Ok(new { Status = "Healthy" }));

app.MapWarehouseEndpoints();

app.Run();
