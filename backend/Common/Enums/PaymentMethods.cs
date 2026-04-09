namespace RestaurantManagement.Api.Common.Enums;

public static class PaymentMethods
{
    public const string Cash = "Cash";
    public const string Card = "Card";
    public const string Bkash = "Bkash";
    public const string Nagad = "Nagad";

    public static readonly string[] All = [Cash, Card, Bkash, Nagad];
}
