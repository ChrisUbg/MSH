using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MSH.Infrastructure.Entities;

namespace MSH.Infrastructure.Data;

public static class RoleSeeder
{
    public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Create roles if they don't exist
        string[] roleNames = { "Admin", "Standard", "Guest" };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // Create first admin user if no users exist
        if (!await userManager.Users.AnyAsync())
        {
            var adminUser = new IdentityUser
            {
                UserName = "admin@msh.local",
                Email = "admin@msh.local",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");

                // Create corresponding ApplicationUser
                var appUser = new User
                {
                    UserName = adminUser.UserName!,
                    Email = adminUser.Email,
                    IsActive = true,
                    LastLogin = DateTime.UtcNow,
                    CreatedById = Guid.Empty, // System user
                    UpdatedById = Guid.Empty
                };
                context.ApplicationUsers.Add(appUser);
                await context.SaveChangesAsync();
            }
        }
    }
} 