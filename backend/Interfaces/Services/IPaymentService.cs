using RestaurantManagement.Api.DTOs.Common;
using RestaurantManagement.Api.DTOs.Payments;

namespace RestaurantManagement.Api.Interfaces.Services;

public interface IPaymentService
{
    Task<PagedResult<PaymentDto>> GetPaymentsAsync(QueryParameters query);
    Task<PaymentDto?> GetByIdAsync(Guid id);
    Task<PaymentDto> CreateAsync(CreatePaymentDto request);
}
