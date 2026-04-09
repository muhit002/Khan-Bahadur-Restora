namespace RestaurantManagement.Api.Common.Enums;

public static class AppRoles
{
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string Cashier = "Cashier";
    public const string Chef = "Chef";
    public const string Waiter = "Waiter";
    public const string Customer = "Customer";

    public static readonly string[] All =
    [
        Admin,
        Manager,
        Cashier,
        Chef,
        Waiter,
        Customer
    ];
}
