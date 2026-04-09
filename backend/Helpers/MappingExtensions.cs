using RestaurantManagement.Api.DTOs.Categories;
using RestaurantManagement.Api.DTOs.Inventory;
using RestaurantManagement.Api.DTOs.MenuItems;
using RestaurantManagement.Api.DTOs.Orders;
using RestaurantManagement.Api.DTOs.Payments;
using RestaurantManagement.Api.DTOs.Tables;
using RestaurantManagement.Api.DTOs.Users;
using RestaurantManagement.Api.Entities;

namespace RestaurantManagement.Api.Helpers;

public static class MappingExtensions
{
    public static UserSummaryDto ToSummaryDto(this AppUser user) =>
        new(user.Id, user.FullName, user.Email, user.Role, user.IsActive, user.PhoneNumber);

    public static UserDetailDto ToDetailDto(this AppUser user) =>
        new(user.Id, user.FullName, user.Email, user.Role, user.IsActive, user.PhoneNumber, user.Address, user.CreatedAtUtc, user.LastLoginAtUtc);

    public static CategoryDto ToDto(this Category category) =>
        new(category.Id, category.Name, category.Description, category.ImageUrl, category.IsActive, category.SortOrder, category.MenuItems.Count);

    public static MenuItemDto ToDto(this MenuItem menuItem) =>
        new(
            menuItem.Id,
            menuItem.CategoryId,
            menuItem.Category?.Name ?? string.Empty,
            menuItem.Name,
            menuItem.Description,
            menuItem.Price,
            menuItem.DiscountPercentage,
            Math.Round(menuItem.Price * (1 - (menuItem.DiscountPercentage / 100)), 2),
            menuItem.IsAvailable,
            menuItem.ImageUrl,
            menuItem.PreparationTimeMinutes);

    public static TableDto ToDto(this RestaurantTable table) =>
        new(table.Id, table.TableNumber, table.Seats, table.Status, table.Location);

    public static OrderItemDto ToDto(this OrderItem item) =>
        new(item.MenuItemId, item.MenuItem?.Name ?? string.Empty, item.Quantity, item.UnitPrice, item.DiscountAmount, item.TotalPrice, item.Notes);

    public static OrderListItemDto ToListDto(this Order order) =>
        new(
            order.Id,
            order.OrderNumber,
            order.Status,
            order.OrderType,
            order.Customer?.FullName,
            order.Waiter?.FullName,
            order.Table?.TableNumber,
            order.TotalAmount,
            order.CreatedAtUtc);

    public static OrderDetailDto ToDetailDto(this Order order) =>
        new(
            order.Id,
            order.OrderNumber,
            order.Status,
            order.OrderType,
            order.CustomerId,
            order.Customer?.FullName,
            order.WaiterId,
            order.Waiter?.FullName,
            order.Table?.TableNumber,
            order.Notes,
            order.Subtotal,
            order.DiscountAmount,
            order.TaxAmount,
            order.ServiceCharge,
            order.TotalAmount,
            order.CreatedAtUtc,
            order.Items.Select(ToDto).ToArray());

    public static InvoiceDto ToInvoiceDto(this Order order)
    {
        var latestPayment = order.Payments.OrderByDescending(x => x.PaidAtUtc).FirstOrDefault();
        return new InvoiceDto(
            latestPayment?.InvoiceNumber ?? $"INV-{order.OrderNumber}",
            order.OrderNumber,
            order.Customer?.FullName,
            order.Table?.TableNumber,
            order.Subtotal,
            order.DiscountAmount,
            order.TaxAmount,
            order.ServiceCharge,
            order.TotalAmount,
            order.Payments.Sum(x => x.Amount),
            latestPayment?.PaidAtUtc ?? DateTime.UtcNow,
            order.Items.Select(ToDto).ToArray());
    }

    public static KitchenOrderDto ToKitchenDto(this Order order) =>
        new(
            order.Id,
            order.OrderNumber,
            order.Status,
            order.Table?.TableNumber,
            order.CreatedAtUtc,
            order.Items.Select(x => $"{x.Quantity}x {x.MenuItem?.Name}").ToArray());

    public static PaymentDto ToDto(this Payment payment) =>
        new(payment.Id, payment.OrderId, payment.Order?.OrderNumber ?? string.Empty, payment.Amount, payment.PaymentMethod, payment.Status, payment.TransactionId, payment.InvoiceNumber, payment.PaidAtUtc);

    public static InventoryItemDto ToDto(this InventoryItem inventoryItem) =>
        new(inventoryItem.Id, inventoryItem.Name, inventoryItem.Unit, inventoryItem.QuantityInStock, inventoryItem.ReorderLevel, inventoryItem.CostPerUnit, inventoryItem.SupplierName, inventoryItem.Notes, inventoryItem.LastUpdatedAtUtc);

    public static LowStockAlertDto ToLowStockDto(this InventoryItem inventoryItem) =>
        new(inventoryItem.Id, inventoryItem.Name, inventoryItem.QuantityInStock, inventoryItem.ReorderLevel, inventoryItem.Unit);
}
