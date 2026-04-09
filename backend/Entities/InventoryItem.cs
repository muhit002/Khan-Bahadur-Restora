namespace RestaurantManagement.Api.Entities;

public class InventoryItem : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal QuantityInStock { get; set; }
    public decimal ReorderLevel { get; set; }
    public decimal CostPerUnit { get; set; }
    public string? SupplierName { get; set; }
    public string? Notes { get; set; }
    public DateTime LastUpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
