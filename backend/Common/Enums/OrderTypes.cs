namespace RestaurantManagement.Api.Common.Enums;

public static class OrderTypes
{
    public const string DineIn = "DineIn";
    public const string TakeAway = "TakeAway";
    public const string Delivery = "Delivery";

    public static readonly string[] All = [DineIn, TakeAway, Delivery];
}
