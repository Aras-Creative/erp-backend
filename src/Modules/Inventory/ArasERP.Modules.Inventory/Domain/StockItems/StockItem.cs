using ArasERP.BuildingBlocks.Domain.Abstractions;
using ArasERP.Modules.Inventory.Domain.StockItems.ValueObjects;

namespace ArasERP.Modules.Inventory.Domain.StockItems;

public sealed class StockItem : AggregateRoot<StockItemId>
{
    public string Name { get; private set; } = null!;
    public string Sku { get; private set; } = null!;
    public string Unit { get; private set; } = null!;
    public CostingMethod CostingMethod { get; private set; } = null!;
    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    public string? CreatedBy { get; private set; }
    public string? UpdatedBy { get; private set; }
    public string? DeletedBy { get; private set; }

    private StockItem(
        StockItemId id,
        string name,
        string sku,
        string unit,
        CostingMethod costingMethod,
        string? createdBy = null
    )
        : base(id)
    {
        Name = name;
        Sku = sku;
        Unit = unit;
        CostingMethod = costingMethod;
        IsActive = true;

        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        CreatedBy = createdBy;
    }

    private StockItem() { }

    public static StockItem Create(
        StockItemId id,
        string name,
        string sku,
        string unit,
        CostingMethod costingMethod,
        string? createdBy = null
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(sku);
        ArgumentException.ThrowIfNullOrWhiteSpace(unit);
        ArgumentNullException.ThrowIfNull(costingMethod);

        return new StockItem(id, name.Trim(), sku.Trim(), unit.Trim(), costingMethod, createdBy);
    }

    public void Rename(string newName, string? updatedBy = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newName);
        Name = newName.Trim();
        UpdateTimestamp(updatedBy);
    }

    public void ChangeUnit(string newUnit, string? updatedBy = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newUnit);
        Unit = newUnit.Trim();
        UpdateTimestamp(updatedBy);
    }

    public void ChangeCostingMethod(CostingMethod newMethod, string? updatedBy = null)
    {
        ArgumentNullException.ThrowIfNull(newMethod);
        CostingMethod = newMethod;
        UpdateTimestamp(updatedBy);
    }

    public void Deactivate(string? updatedBy = null)
    {
        IsActive = false;
        UpdateTimestamp(updatedBy);
    }

    public void Activate(string? updatedBy = null)
    {
        IsActive = true;
        UpdateTimestamp(updatedBy);
    }

    public void Delete(string? deletedBy = null)
    {
        if (DeletedAt.HasValue)
            return;

        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;
        IsActive = false;
        UpdateTimestamp(deletedBy);
    }

    public void Restore(string? updatedBy = null)
    {
        if (!DeletedAt.HasValue)
            return;

        DeletedAt = null;
        DeletedBy = null;
        IsActive = true;
        UpdateTimestamp(updatedBy);
    }

    public bool IsDeleted() => DeletedAt.HasValue;

    private void UpdateTimestamp(string? updatedBy = null)
    {
        UpdatedAt = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(updatedBy))
        {
            UpdatedBy = updatedBy;
        }
    }
}
