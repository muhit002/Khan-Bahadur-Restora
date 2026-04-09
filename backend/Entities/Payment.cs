namespace RestaurantManagement.Api.Entities;

public class Payment : BaseEntity
{
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? TransactionId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime PaidAtUtc { get; set; } = DateTime.UtcNow;

    public Order? Order { get; set; }
}
