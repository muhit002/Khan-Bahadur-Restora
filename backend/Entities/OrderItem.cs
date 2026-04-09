namespace RestaurantManagement.Api.Entities;

public class OrderItem : BaseEntity
{
    public Guid OrderId { get; set; }
    public Guid MenuItemId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalPrice { get; set; }
    public string? Notes { get; set; }

    public Order? Order { get; set; }
    public MenuItem? MenuItem { get; set; }
}
