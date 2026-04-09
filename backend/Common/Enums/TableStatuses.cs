namespace RestaurantManagement.Api.Common.Enums;

public static class TableStatuses
{
    public const string Available = "Available";
    public const string Reserved = "Reserved";
    public const string Occupied = "Occupied";
    public const string Maintenance = "Maintenance";

    public static readonly string[] All = [Available, Reserved, Occupied, Maintenance];
}
