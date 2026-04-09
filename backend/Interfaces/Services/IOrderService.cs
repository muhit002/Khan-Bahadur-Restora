using RestaurantManagement.Api.DTOs.Common;
using RestaurantManagement.Api.DTOs.Orders;

namespace RestaurantManagement.Api.Interfaces.Services;

public interface IOrderService
{
    Task<PagedResult<OrderListItemDto>> GetOrdersAsync(QueryParameters query, string? status, Guid? customerId = null, Guid? waiterId = null);
    Task<OrderDetailDto?> GetByIdAsync(Guid id);
    Task<OrderDetailDto> CreateAsync(CreateOrderDto request, Guid? currentUserId, string? currentUserRole);
    Task<OrderDetailDto> UpdateStatusAsync(Guid id, UpdateOrderStatusDto request);
    Task<InvoiceDto?> GetInvoiceAsync(Guid id);
}
