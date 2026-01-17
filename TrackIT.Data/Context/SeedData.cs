using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TrackIT.Core.Entities;
using TrackIT.Data.Context;

namespace TrackIT.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            // 1. Get the necessary services from the DI container
            var context = serviceProvider.GetRequiredService<AppDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // 2. Apply Migrations automatically (Optional but convenient for Docker)
            await context.Database.MigrateAsync();

            // 3. Seed Roles (Admin vs Employee)
            string[] roles = ["Admin", "Employee"];
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 4. Seed Admin User
            if (await userManager.FindByEmailAsync("admin@trackit.com") == null)
            {
                var adminUser = new AppUser
                {
                    UserName = "admin@trackit.com", // Identity requires UserName
                    Email = "admin@trackit.com",
                    FullName = "System Administrator",
                    Department = "IT Operations",
                    EmailConfirmed = true
                };

                // Create user with a hardcoded password
                // Note: In real production, use Secrets Manager for this password!
                var result = await userManager.CreateAsync(adminUser, "Admin@123");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // 5. Seed Sample Data (Categories)
            if (!await context.Categories.AnyAsync())
            {
                var categories = new List<Category>
                {
                    new Category { Name = "Laptops" },
                    new Category { Name = "Monitors" },
                    new Category { Name = "Peripherals" },
                    new Category { Name = "Licenses" }
                };

                await context.Categories.AddRangeAsync(categories);
                await context.SaveChangesAsync();
            }
        }
    }
}