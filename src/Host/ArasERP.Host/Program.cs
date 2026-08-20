using ArasERP.BuildingBlocks.Application;
using ArasERP.BuildingBlocks.Presentation.Middleware;
using ArasERP.Modules.Inventory;
using ArasERP.Modules.Inventory.Api.Warehouses.Endpoints;
using ArasERP.Modules.Inventory.Infrastructure;
using ArasERP.Modules.Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using ArasERP.Modules.Address;
using ArasERP.Modules.Address.Api.Endpoints;
using ArasERP.Modules.Address.Infrastructure;
using ArasERP.Modules.Address.Infrastructure.Persistence;
using ArasERP.Integrations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddMediator();
builder.Services.AddInventoryModule();
builder.Services.AddAddressModule();
builder.Services.AddAddressInfrastructure(
    builder.Configuration.GetConnectionString("DefaultConnection")!,
    builder.Configuration
);
builder.Services.AddInventoryInfrastructure(
    builder.Configuration.GetConnectionString("DefaultConnection")!
);
builder.Services.AddIntegrations(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();

    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        await dbContext.Database.MigrateAsync();

        var addressDbContext = scope.ServiceProvider.GetRequiredService<AddressDbContext>();
        await addressDbContext.Database.MigrateAsync();
    }
}

app.UseHttpsRedirection();

app.UseRequestLogging();
app.UseExceptionHandling();
app.UseRequestId();

app.MapGet("/health", () => Results.Ok(new { Status = "Healthy" }));

app.MapWarehouseEndpoints();
app.MapAddressEndpoints();

app.Run();
