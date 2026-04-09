using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Api.Common.Enums;
using RestaurantManagement.Api.Data;
using RestaurantManagement.Api.DTOs.Common;
using RestaurantManagement.Api.DTOs.Orders;
using RestaurantManagement.Api.Entities;
using RestaurantManagement.Api.Helpers;
using RestaurantManagement.Api.Interfaces.Services;

namespace RestaurantManagement.Api.Services;

public class OrderService(ApplicationDbContext dbContext) : IOrderService
{
    public async Task<PagedResult<OrderListItemDto>> GetOrdersAsync(QueryParameters query, string? status, Guid? customerId = null, Guid? waiterId = null)
    {
        var ordersQuery = dbContext.Orders.AsNoTracking()
            .Include(x => x.Customer)
            .Include(x => x.Waiter)
            .Include(x => x.Table)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            ordersQuery = ordersQuery.Where(x =>
                x.OrderNumber.ToLower().Contains(search) ||
                (x.Customer != null && x.Customer.FullName.ToLower().Contains(search)) ||
                (x.Table != null && x.Table.TableNumber.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            ordersQuery = ordersQuery.Where(x => x.Status == status);
        }

        if (customerId.HasValue)
        {
            ordersQuery = ordersQuery.Where(x => x.CustomerId == customerId.Value);
        }

        if (waiterId.HasValue)
        {
            ordersQuery = ordersQuery.Where(x => x.WaiterId == waiterId.Value);
        }

        return await ordersQuery
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => x.ToListDto())
            .ToPagedResultAsync(query.PageNumber, query.PageSize);
    }

    public async Task<OrderDetailDto?> GetByIdAsync(Guid id)
    {
        var order = await dbContext.Orders.AsNoTracking()
            .Include(x => x.Customer)
            .Include(x => x.Waiter)
            .Include(x => x.Table)
            .Include(x => x.Items).ThenInclude(x => x.MenuItem)
            .FirstOrDefaultAsync(x => x.Id == id);

        return order?.ToDetailDto();
    }

    public async Task<OrderDetailDto> CreateAsync(CreateOrderDto request, Guid? currentUserId, string? currentUserRole)
    {
        if (request.Items.Count == 0)
        {
            throw new InvalidOperationException("Order must contain at least one item.");
        }

        var orderType = NormalizeOrderType(request.OrderType);
        var customerId = request.CustomerId;
        var waiterId = request.WaiterId;
        var tableId = orderType == OrderTypes.DineIn ? request.TableId : null;

        if (currentUserRole == AppRoles.Customer)
        {
            customerId = currentUserId ?? throw new UnauthorizedAccessException("Invalid customer context.");
        }

        if (currentUserRole == AppRoles.Waiter)
        {
            waiterId = currentUserId ?? throw new UnauthorizedAccessException("Invalid waiter context.");
        }

        if (currentUserRole == AppRoles.Customer && request.CustomerId.HasValue && request.CustomerId != customerId)
        {
            throw new UnauthorizedAccessException("Customers can only place orders for themselves.");
        }

        if (currentUserRole == AppRoles.Waiter && request.WaiterId.HasValue && request.WaiterId != waiterId)
        {
            throw new UnauthorizedAccessException("Waiters can only create orders assigned to themselves.");
        }

        RestaurantTable? table = null;

        if (customerId.HasValue)
        {
            var customerExists = await dbContext.Users
                .AsNoTracking()
                .AnyAsync(x => x.Id == customerId.Value && x.Role == AppRoles.Customer);

            if (!customerExists)
            {
                throw new KeyNotFoundException("Customer not found.");
            }
        }

        if (waiterId.HasValue)
        {
            var waiterExists = await dbContext.Users
                .AsNoTracking()
                .AnyAsync(x => x.Id == waiterId.Value && x.Role == AppRoles.Waiter);

            if (!waiterExists)
            {
                throw new KeyNotFoundException("Waiter not found.");
            }
        }

        if (tableId.HasValue)
        {
            table = await dbContext.Tables.FirstOrDefaultAsync(x => x.Id == tableId.Value);
            if (table is null)
            {
                throw new KeyNotFoundException("Table not found.");
            }

            if (table.Status == TableStatuses.Maintenance)
            {
                throw new InvalidOperationException("Selected table is under maintenance.");
            }

            if (table.Status != TableStatuses.Available)
            {
                throw new InvalidOperationException("Selected table is not currently available.");
            }
        }

        var requestedMenuItemIds = request.Items.Select(x => x.MenuItemId).Distinct().ToHashSet();
        var availableMenuItems = await dbContext.MenuItems
            .AsNoTracking()
            .Where(x => x.IsAvailable)
            .ToListAsync();

        var menuItems = availableMenuItems
            .Where(x => requestedMenuItemIds.Contains(x.Id))
            .ToDictionary(x => x.Id);

        if (menuItems.Count != requestedMenuItemIds.Count)
        {
            throw new InvalidOperationException("One or more menu items are unavailable.");
        }

        decimal subtotal = 0;
        decimal discountAmount = 0;
        var orderItems = new List<OrderItem>();

        foreach (var requestItem in request.Items)
        {
            var menuItem = menuItems[requestItem.MenuItemId];
            var lineSubtotal = menuItem.Price * requestItem.Quantity;
            var lineDiscount = Math.Round(lineSubtotal * (menuItem.DiscountPercentage / 100m), 2);
            var lineTotal = Math.Round(lineSubtotal - lineDiscount, 2);

            subtotal += lineSubtotal;
            discountAmount += lineDiscount;

            orderItems.Add(new OrderItem
            {
                MenuItemId = menuItem.Id,
                Quantity = requestItem.Quantity,
                UnitPrice = menuItem.Price,
                DiscountAmount = lineDiscount,
                TotalPrice = lineTotal,
                Notes = requestItem.Notes?.Trim()
            });
        }

        var taxableAmount = subtotal - discountAmount;
        var taxAmount = Math.Round(taxableAmount * 0.08m, 2);
        var serviceCharge = request.OrderType == OrderTypes.DineIn ? Math.Round(taxableAmount * 0.05m, 2) : 0m;
        var totalAmount = taxableAmount + taxAmount + serviceCharge;

        var order = new Order
        {
            OrderNumber = $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(100, 999)}",
            CustomerId = customerId,
            WaiterId = waiterId,
            TableId = tableId,
            Table = table,
            Status = OrderStatuses.Pending,
            OrderType = orderType,
            Notes = request.Notes?.Trim(),
            Subtotal = Math.Round(subtotal, 2),
            DiscountAmount = Math.Round(discountAmount, 2),
            TaxAmount = taxAmount,
            ServiceCharge = serviceCharge,
            TotalAmount = Math.Round(totalAmount, 2),
            Items = orderItems
        };

        if (table is not null && table.Status != TableStatuses.Maintenance)
        {
            table.Status = TableStatuses.Occupied;
        }

        await dbContext.Orders.AddAsync(order);
        await dbContext.SaveChangesAsync();
        return await GetByIdAsync(order.Id) ?? throw new InvalidOperationException("Failed to load the newly created order.");
    }

    public async Task<OrderDetailDto> UpdateStatusAsync(Guid id, UpdateOrderStatusDto request)
    {
        var status = NormalizeOrderStatus(request.Status);

        var order = await dbContext.Orders
            .Include(x => x.Customer)
            .Include(x => x.Waiter)
            .Include(x => x.Table)
            .Include(x => x.Items).ThenInclude(x => x.MenuItem)
            .FirstOrDefaultAsync(x => x.Id == id) ??
            throw new KeyNotFoundException("Order not found.");

        order.Status = status;

        if (order.Table is not null && order.Table.Status != TableStatuses.Maintenance)
        {
            if (status is OrderStatuses.Cancelled or OrderStatuses.Paid)
            {
                order.Table.Status = TableStatuses.Available;
            }
            else
            {
                order.Table.Status = TableStatuses.Occupied;
            }
        }

        await dbContext.SaveChangesAsync();
        return order.ToDetailDto();
    }

    public async Task<InvoiceDto?> GetInvoiceAsync(Guid id)
    {
        var order = await dbContext.Orders.AsNoTracking()
            .Include(x => x.Customer)
            .Include(x => x.Table)
            .Include(x => x.Items).ThenInclude(x => x.MenuItem)
            .Include(x => x.Payments)
            .FirstOrDefaultAsync(x => x.Id == id);

        return order?.ToInvoiceDto();
    }

    private static string NormalizeOrderType(string orderType)
    {
        var normalized = OrderTypes.All.FirstOrDefault(value =>
            string.Equals(value, orderType, StringComparison.OrdinalIgnoreCase));

        return normalized ?? throw new InvalidOperationException("Invalid order type.");
    }

    private static string NormalizeOrderStatus(string status)
    {
        var normalized = OrderStatuses.All.FirstOrDefault(value =>
            string.Equals(value, status, StringComparison.OrdinalIgnoreCase));

        return normalized ?? throw new InvalidOperationException("Invalid order status.");
    }
}
