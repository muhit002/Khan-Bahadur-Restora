using Microsoft.EntityFrameworkCore;
using Npgsql;
using RestaurantManagement.Api.Data;
using RestaurantManagement.Api.Helpers;
using RestaurantManagement.Api.Interfaces.Repositories;
using RestaurantManagement.Api.Interfaces.Services;
using RestaurantManagement.Api.Middleware;
using RestaurantManagement.Api.Repositories;
using RestaurantManagement.Api.Seed;
using RestaurantManagement.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ??
            ["http://localhost:5173", "http://127.0.0.1:5173"];

        policy.WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IMenuItemService, MenuItemService>();
builder.Services.AddScoped<ITableService, TableService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();

var app = builder.Build();

app.UseStaticFiles();
app.UseCors("FrontendPolicy");
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<JwtAuthenticationMiddleware>();

app.MapControllers();
app.MapSwaggerDocumentation();

if (builder.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup"))
{
    try
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var migrations = dbContext.Database.GetMigrations();
        if (migrations.Any())
        {
            await dbContext.Database.MigrateAsync();
        }
        else
        {
            await dbContext.Database.EnsureCreatedAsync();
        }

        if (builder.Configuration.GetValue<bool>("Database:SeedOnStartup", true))
        {
            await DatabaseSeeder.SeedAsync(scope.ServiceProvider);
        }
    }
    catch (NpgsqlException ex)
    {
        const string databaseSetupHint =
            "Database startup failed. Update ConnectionStrings:DefaultConnection in backend/appsettings.json, " +
            "or set ConnectionStrings__DefaultConnection to a reachable PostgreSQL instance before running the API. " +
            "Confirm Host, Port, Database, Username, and Password are correct, and make sure the PostgreSQL service is running.";

        app.Logger.LogCritical(ex, "{DatabaseSetupHint}", databaseSetupHint);
        throw new InvalidOperationException(databaseSetupHint, ex);
    }
}

app.Run();
