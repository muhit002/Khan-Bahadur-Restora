using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Api.Common.Enums;
using RestaurantManagement.Api.Data;
using RestaurantManagement.Api.DTOs.Common;
using RestaurantManagement.Api.DTOs.Payments;
using RestaurantManagement.Api.Entities;
using RestaurantManagement.Api.Helpers;
using RestaurantManagement.Api.Interfaces.Services;

namespace RestaurantManagement.Api.Services;

public class PaymentService(ApplicationDbContext dbContext) : IPaymentService
{
    public async Task<PagedResult<PaymentDto>> GetPaymentsAsync(QueryParameters query)
    {
        var paymentsQuery = dbContext.Payments.AsNoTracking()
            .Include(x => x.Order)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            paymentsQuery = paymentsQuery.Where(x =>
                x.InvoiceNumber.ToLower().Contains(search) ||
                (x.Order != null && x.Order.OrderNumber.ToLower().Contains(search)) ||
                (x.TransactionId != null && x.TransactionId.ToLower().Contains(search)));
        }

        return await paymentsQuery
            .OrderByDescending(x => x.PaidAtUtc)
            .Select(x => x.ToDto())
            .ToPagedResultAsync(query.PageNumber, query.PageSize);
    }

    public async Task<PaymentDto?> GetByIdAsync(Guid id)
    {
        var payment = await dbContext.Payments.AsNoTracking()
            .Include(x => x.Order)
            .FirstOrDefaultAsync(x => x.Id == id);
        return payment?.ToDto();
    }

    public async Task<PaymentDto> CreateAsync(CreatePaymentDto request)
    {
        var paymentMethod = NormalizePaymentMethod(request.PaymentMethod);

        var order = await dbContext.Orders
            .Include(x => x.Table)
            .Include(x => x.Payments)
            .FirstOrDefaultAsync(x => x.Id == request.OrderId) ??
            throw new KeyNotFoundException("Order not found.");

        if (order.Status == OrderStatuses.Cancelled)
        {
            throw new InvalidOperationException("Payments cannot be recorded for a cancelled order.");
        }

        var totalPaid = order.Payments.Sum(x => x.Amount);
        var remaining = order.TotalAmount - totalPaid;
        if (remaining <= 0)
        {
            throw new InvalidOperationException("This order has already been paid in full.");
        }

        if (request.Amount > remaining)
        {
            throw new InvalidOperationException("Payment amount exceeds the remaining balance.");
        }

        var payment = new Payment
        {
            OrderId = order.Id,
            Order = order,
            Amount = request.Amount,
            PaymentMethod = paymentMethod,
            Status = PaymentStatuses.Completed,
            TransactionId = request.TransactionId,
            InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(100, 999)}",
            PaidAtUtc = DateTime.UtcNow
        };

        await dbContext.Payments.AddAsync(payment);

        if (totalPaid + request.Amount >= order.TotalAmount)
        {
            order.Status = OrderStatuses.Paid;
            if (order.Table is not null && order.Table.Status != TableStatuses.Maintenance)
            {
                order.Table.Status = TableStatuses.Available;
            }
        }

        await dbContext.SaveChangesAsync();
        return payment.ToDto();
    }

    private static string NormalizePaymentMethod(string paymentMethod)
    {
        var normalized = PaymentMethods.All.FirstOrDefault(value =>
            string.Equals(value, paymentMethod, StringComparison.OrdinalIgnoreCase));

        return normalized ?? throw new InvalidOperationException("Invalid payment method.");
    }
}
