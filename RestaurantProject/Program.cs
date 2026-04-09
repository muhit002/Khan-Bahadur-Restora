namespace RestaurantProject
{
    using DataAccess.Context;
    using Microsoft.EntityFrameworkCore;

    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            ApplicationDbContext.ConnectionString = ResolveConnectionString(builder.Configuration);

            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            try
            {
                using var dbContext = new ApplicationDbContext();
                dbContext.Database.Migrate();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "Unable to initialize the EDB/PostgreSQL database. Update ConnectionStrings:DefaultConnection or EDB_CONNECTION_STRING with the correct PostgreSQL credentials.",
                    ex);
            }

            app.Run();
        }

        private static string ResolveConnectionString(ConfigurationManager configuration)
        {
            return Environment.GetEnvironmentVariable("EDB_CONNECTION_STRING")
                ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                ?? configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "ConnectionStrings:DefaultConnection is missing. Update appsettings.json or set EDB_CONNECTION_STRING.");
        }
    }
}
