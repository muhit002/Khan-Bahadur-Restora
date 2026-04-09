namespace RestaurantManagement.Api.Common.Enums;

public static class OrderStatuses
{
    public const string Pending = "Pending";
    public const string Cooking = "Cooking";
    public const string Ready = "Ready";
    public const string Served = "Served";
    public const string Cancelled = "Cancelled";
    public const string Paid = "Paid";

    public static readonly string[] All = [Pending, Cooking, Ready, Served, Cancelled, Paid];
}
