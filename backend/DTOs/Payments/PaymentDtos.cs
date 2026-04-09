using System.ComponentModel.DataAnnotations;

namespace RestaurantManagement.Api.DTOs.Payments;

public class CreatePaymentDto
{
    [Required]
    public Guid OrderId { get; set; }

    [Range(0.01, 100000)]
    public decimal Amount { get; set; }

    [Required]
    public string PaymentMethod { get; set; } = string.Empty;

    [StringLength(100)]
    public string? TransactionId { get; set; }
}

public record PaymentDto(
    Guid Id,
    Guid OrderId,
    string OrderNumber,
    decimal Amount,
    string PaymentMethod,
    string Status,
    string? TransactionId,
    string InvoiceNumber,
    DateTime PaidAtUtc);
