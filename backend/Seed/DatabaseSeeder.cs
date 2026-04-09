using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Api.Common.Enums;
using RestaurantManagement.Api.Data;
using RestaurantManagement.Api.Entities;
using RestaurantManagement.Api.Interfaces.Services;

namespace RestaurantManagement.Api.Seed;

public static class DatabaseSeeder
{
    private static readonly IReadOnlyDictionary<string, string> CategoryImageMap = new Dictionary<string, string>
    {
        ["Starters"] = "/brand/categories/starters.svg",
        ["Main Course"] = "/brand/categories/main-course.svg",
        ["Desserts"] = "/brand/categories/desserts.svg",
        ["Drinks"] = "/brand/categories/drinks.svg"
    };

    private static readonly IReadOnlyDictionary<string, string> MenuImageMap = new Dictionary<string, string>
    {
        ["Smoked Chicken Wings"] = "/brand/foods/chicken-wings.svg",
        ["Grilled River Fish"] = "/brand/foods/grilled-fish.svg",
        ["Beef Steak Platter"] = "/brand/foods/beef-steak.svg",
        ["Molten Lava Cake"] = "/brand/foods/lava-cake.svg",
        ["Classic Cold Coffee"] = "/brand/foods/cold-coffee.svg"
    };

    private static readonly HashSet<string> LegacyCategoryImages =
    [
        "/uploads/starters.jpg",
        "/uploads/main-course.jpg",
        "/uploads/desserts.jpg",
        "/uploads/drinks.jpg"
    ];

    private static readonly HashSet<string> LegacyMenuImages =
    [
        "/uploads/chicken-wings.jpg",
        "/uploads/grilled-fish.jpg",
        "/uploads/beef-steak.jpg",
        "/uploads/lava-cake.jpg",
        "/uploads/cold-coffee.jpg"
    ];

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var passwordService = serviceProvider.GetRequiredService<IPasswordService>();

        if (!await dbContext.Users.AnyAsync())
        {
            var users = new[]
            {
                CreateUser(passwordService, "System Admin", "admin@restaurant.com", "Admin@123", AppRoles.Admin, "01700000001"),
                CreateUser(passwordService, "Floor Manager", "manager@restaurant.com", "Manager@123", AppRoles.Manager, "01700000002"),
                CreateUser(passwordService, "Cash Counter", "cashier@restaurant.com", "Cashier@123", AppRoles.Cashier, "01700000003"),
                CreateUser(passwordService, "Head Chef", "chef@restaurant.com", "Chef@123", AppRoles.Chef, "01700000004"),
                CreateUser(passwordService, "Service Waiter", "waiter@restaurant.com", "Waiter@123", AppRoles.Waiter, "01700000005"),
                CreateUser(passwordService, "Regular Customer", "customer@restaurant.com", "Customer@123", AppRoles.Customer, "01700000006")
            };

            await dbContext.Users.AddRangeAsync(users);
            await dbContext.SaveChangesAsync();
        }

        if (!await dbContext.Categories.AnyAsync())
        {
            await dbContext.Categories.AddRangeAsync(
                new Category { Name = "Starters", Description = "Fresh opening bites", SortOrder = 1, ImageUrl = CategoryImageMap["Starters"] },
                new Category { Name = "Main Course", Description = "Chef specials and signature platters", SortOrder = 2, ImageUrl = CategoryImageMap["Main Course"] },
                new Category { Name = "Desserts", Description = "Sweet endings", SortOrder = 3, ImageUrl = CategoryImageMap["Desserts"] },
                new Category { Name = "Drinks", Description = "Hot and cold beverages", SortOrder = 4, ImageUrl = CategoryImageMap["Drinks"] }
            );

            await dbContext.SaveChangesAsync();
        }

        if (!await dbContext.MenuItems.AnyAsync())
        {
            var categoryMap = await dbContext.Categories.ToDictionaryAsync(x => x.Name, x => x.Id);
            await dbContext.MenuItems.AddRangeAsync(
                new MenuItem { CategoryId = categoryMap["Starters"], Name = "Smoked Chicken Wings", Description = "Crispy wings with house sauce", Price = 9.99m, DiscountPercentage = 5, PreparationTimeMinutes = 15, ImageUrl = MenuImageMap["Smoked Chicken Wings"] },
                new MenuItem { CategoryId = categoryMap["Main Course"], Name = "Grilled River Fish", Description = "Served with lemon herb rice", Price = 18.50m, DiscountPercentage = 0, PreparationTimeMinutes = 25, ImageUrl = MenuImageMap["Grilled River Fish"] },
                new MenuItem { CategoryId = categoryMap["Main Course"], Name = "Beef Steak Platter", Description = "Mashed potato and seasonal vegetables", Price = 24.99m, DiscountPercentage = 10, PreparationTimeMinutes = 30, ImageUrl = MenuImageMap["Beef Steak Platter"] },
                new MenuItem { CategoryId = categoryMap["Desserts"], Name = "Molten Lava Cake", Description = "Warm cake with vanilla scoop", Price = 7.25m, DiscountPercentage = 0, PreparationTimeMinutes = 12, ImageUrl = MenuImageMap["Molten Lava Cake"] },
                new MenuItem { CategoryId = categoryMap["Drinks"], Name = "Classic Cold Coffee", Description = "Blended with cream", Price = 4.50m, DiscountPercentage = 0, PreparationTimeMinutes = 5, ImageUrl = MenuImageMap["Classic Cold Coffee"] }
            );

            await dbContext.SaveChangesAsync();
        }

        await EnsureCatalogImagesAsync(dbContext);

        if (!await dbContext.Tables.AnyAsync())
        {
            var tables = Enumerable.Range(1, 10)
                .Select(index => new RestaurantTable
                {
                    TableNumber = $"T-{index:00}",
                    Seats = index <= 4 ? 4 : 6,
                    Status = TableStatuses.Available,
                    Location = index <= 5 ? "Ground Floor" : "Rooftop"
                });

            await dbContext.Tables.AddRangeAsync(tables);
            await dbContext.SaveChangesAsync();
        }

        var reservedTables = await dbContext.Tables.Where(x => x.Status == TableStatuses.Reserved).ToListAsync();
        if (reservedTables.Count > 0)
        {
            foreach (var table in reservedTables)
            {
                table.Status = TableStatuses.Available;
            }

            await dbContext.SaveChangesAsync();
        }

        if (!await dbContext.InventoryItems.AnyAsync())
        {
            await dbContext.InventoryItems.AddRangeAsync(
                new InventoryItem { Name = "Chicken Breast", Unit = "kg", QuantityInStock = 32, ReorderLevel = 10, CostPerUnit = 6.5m, SupplierName = "Fresh Farms" },
                new InventoryItem { Name = "Basmati Rice", Unit = "kg", QuantityInStock = 18, ReorderLevel = 8, CostPerUnit = 2.2m, SupplierName = "Golden Grain" },
                new InventoryItem { Name = "Coffee Beans", Unit = "kg", QuantityInStock = 4, ReorderLevel = 5, CostPerUnit = 11m, SupplierName = "Roast House", Notes = "Low stock alert example" }
            );

            await dbContext.SaveChangesAsync();
        }

        if (!await dbContext.Orders.AnyAsync())
        {
            var customer = await dbContext.Users.FirstAsync(x => x.Role == AppRoles.Customer);
            var waiter = await dbContext.Users.FirstAsync(x => x.Role == AppRoles.Waiter);
            var table = await dbContext.Tables.OrderBy(x => x.TableNumber).FirstAsync();
            var items = await dbContext.MenuItems.Take(2).ToListAsync();

            var order = new Order
            {
                OrderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-0001",
                CustomerId = customer.Id,
                WaiterId = waiter.Id,
                TableId = table.Id,
                Customer = customer,
                Waiter = waiter,
                Table = table,
                Status = OrderStatuses.Paid,
                OrderType = OrderTypes.DineIn,
                Notes = "Seeded sample order",
                Subtotal = 29.49m,
                DiscountAmount = 1.00m,
                TaxAmount = 2.28m,
                ServiceCharge = 1.42m,
                TotalAmount = 32.19m,
                Items =
                [
                    new OrderItem
                    {
                        MenuItemId = items[0].Id,
                        MenuItem = items[0],
                        Quantity = 2,
                        UnitPrice = items[0].Price,
                        DiscountAmount = 1.00m,
                        TotalPrice = 18.98m
                    },
                    new OrderItem
                    {
                        MenuItemId = items[1].Id,
                        MenuItem = items[1],
                        Quantity = 1,
                        UnitPrice = items[1].Price,
                        DiscountAmount = 0m,
                        TotalPrice = 10.51m
                    }
                ],
                Payments =
                [
                    new Payment
                    {
                        Amount = 32.19m,
                        PaymentMethod = PaymentMethods.Card,
                        Status = PaymentStatuses.Completed,
                        PaidAtUtc = DateTime.UtcNow,
                        InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-0001",
                        TransactionId = $"TXN-{Guid.NewGuid():N}"[..12]
                    }
                ]
            };

            table.Status = TableStatuses.Available;
            await dbContext.Orders.AddAsync(order);
            await dbContext.SaveChangesAsync();
        }
    }

    private static AppUser CreateUser(IPasswordService passwordService, string fullName, string email, string password, string role, string phoneNumber)
    {
        var (hash, salt) = passwordService.HashPassword(password);
        return new AppUser
        {
            FullName = fullName,
            Email = email,
            PasswordHash = hash,
            PasswordSalt = salt,
            Role = role,
            PhoneNumber = phoneNumber,
            Address = "Sample address",
            IsActive = true
        };
    }

    private static async Task EnsureCatalogImagesAsync(ApplicationDbContext dbContext)
    {
        var hasChanges = false;

        var categories = await dbContext.Categories.ToListAsync();
        foreach (var category in categories)
        {
            if (CategoryImageMap.TryGetValue(category.Name, out var imagePath) &&
                (string.IsNullOrWhiteSpace(category.ImageUrl) || LegacyCategoryImages.Contains(category.ImageUrl)))
            {
                category.ImageUrl = imagePath;
                hasChanges = true;
            }
        }

        var menuItems = await dbContext.MenuItems.ToListAsync();
        foreach (var menuItem in menuItems)
        {
            if (MenuImageMap.TryGetValue(menuItem.Name, out var imagePath) &&
                (string.IsNullOrWhiteSpace(menuItem.ImageUrl) || LegacyMenuImages.Contains(menuItem.ImageUrl)))
            {
                menuItem.ImageUrl = imagePath;
                hasChanges = true;
            }
        }

        if (hasChanges)
        {
            await dbContext.SaveChangesAsync();
        }
    }
}
