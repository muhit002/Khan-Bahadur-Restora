using System.ComponentModel.DataAnnotations;

namespace RestaurantManagement.Api.DTOs.Inventory;

public record InventoryItemDto(
    Guid Id,
    string Name,
    string Unit,
    decimal QuantityInStock,
    decimal ReorderLevel,
    decimal CostPerUnit,
    string? SupplierName,
    string? Notes,
    DateTime LastUpdatedAtUtc);

public record LowStockAlertDto(Guid Id, string Name, decimal QuantityInStock, decimal ReorderLevel, string Unit);

public class CreateInventoryItemDto
{
    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(40)]
    public string Unit { get; set; } = string.Empty;

    [Range(0, 100000)]
    public decimal QuantityInStock { get; set; }

    [Range(0, 100000)]
    public decimal ReorderLevel { get; set; }

    [Range(0, 100000)]
    public decimal CostPerUnit { get; set; }

    [StringLength(120)]
    public string? SupplierName { get; set; }

    [StringLength(250)]
    public string? Notes { get; set; }
}

public class UpdateInventoryItemDto : CreateInventoryItemDto;
