namespace RestaurantManagement.Api.Common.Enums;

public static class PaymentStatuses
{
    public const string Pending = "Pending";
    public const string Completed = "Completed";
    public const string Failed = "Failed";
    public const string Refunded = "Refunded";

    public static readonly string[] All = [Pending, Completed, Failed, Refunded];
}
