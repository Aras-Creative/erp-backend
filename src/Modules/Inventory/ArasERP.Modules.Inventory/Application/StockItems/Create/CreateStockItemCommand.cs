using ArasERP.BuildingBlocks.Application;

namespace ArasERP.Modules.Inventory.Application.StockItems.Create;

public sealed class CreateStockItemCommand : ICommand
{
    public required string Name { get; init; }
    public required string Sku { get; init; }
    public required string Unit { get; init; }
    public required string CostingMethod { get; init; }
}
