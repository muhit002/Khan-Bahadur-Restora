using System.ComponentModel.DataAnnotations;

namespace RestaurantManagement.Api.DTOs.Orders;

public class CreateOrderItemDto
{
    [Required]
    public Guid MenuItemId { get; set; }

    [Range(1, 100)]
    public int Quantity { get; set; }

    [StringLength(250)]
    public string? Notes { get; set; }
}

public class CreateOrderDto
{
    public Guid? CustomerId { get; set; }
    public Guid? WaiterId { get; set; }
    public Guid? TableId { get; set; }

    [Required]
    public string OrderType { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Notes { get; set; }

    [Required, MinLength(1)]
    public List<CreateOrderItemDto> Items { get; set; } = [];
}

public class UpdateOrderStatusDto
{
    [Required]
    public string Status { get; set; } = string.Empty;
}

public record OrderItemDto(
    Guid MenuItemId,
    string MenuItemName,
    int Quantity,
    decimal UnitPrice,
    decimal DiscountAmount,
    decimal TotalPrice,
    string? Notes);

public record OrderListItemDto(
    Guid Id,
    string OrderNumber,
    string Status,
    string OrderType,
    string? CustomerName,
    string? WaiterName,
    string? TableNumber,
    decimal TotalAmount,
    DateTime CreatedAtUtc);

public record OrderDetailDto(
    Guid Id,
    string OrderNumber,
    string Status,
    string OrderType,
    Guid? CustomerId,
    string? CustomerName,
    Guid? WaiterId,
    string? WaiterName,
    string? TableNumber,
    string? Notes,
    decimal Subtotal,
    decimal DiscountAmount,
    decimal TaxAmount,
    decimal ServiceCharge,
    decimal TotalAmount,
    DateTime CreatedAtUtc,
    IReadOnlyCollection<OrderItemDto> Items);

public record InvoiceDto(
    string InvoiceNumber,
    string OrderNumber,
    string? CustomerName,
    string? TableNumber,
    decimal Subtotal,
    decimal DiscountAmount,
    decimal TaxAmount,
    decimal ServiceCharge,
    decimal TotalAmount,
    decimal PaidAmount,
    DateTime GeneratedAtUtc,
    IReadOnlyCollection<OrderItemDto> Items);

public record KitchenOrderDto(
    Guid Id,
    string OrderNumber,
    string Status,
    string? TableNumber,
    DateTime CreatedAtUtc,
    IReadOnlyCollection<string> Items);
