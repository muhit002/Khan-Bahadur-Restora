namespace RestaurantManagement.Api.Entities;

public class MenuItem : BaseEntity
{
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal DiscountPercentage { get; set; }
    public bool IsAvailable { get; set; } = true;
    public string? ImageUrl { get; set; }
    public int PreparationTimeMinutes { get; set; }

    public Category? Category { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; } = [];
}
