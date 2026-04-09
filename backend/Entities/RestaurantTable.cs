namespace RestaurantManagement.Api.Entities;

public class RestaurantTable : BaseEntity
{
    public string TableNumber { get; set; } = string.Empty;
    public int Seats { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Location { get; set; }

    public ICollection<Order> Orders { get; set; } = [];
}
