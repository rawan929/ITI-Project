using ITI.DAL.Models;
using Microsoft.AspNetCore.Identity;

namespace ITI_Project.Data
{
    /// <summary>
    /// Seeds the Identity roles and a default Admin account on application startup,
    /// since there is no Register option for creating an Admin from the UI.
    /// </summary>
    public static class DbSeeder
    {
        // Default admin credentials — change these (or move to configuration) before going to production.
        private const string AdminEmail = "admin@bloodlink.com";
        private const string AdminPassword = "Admin@123";

        public static async Task SeedAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            string[] roles = { "Admin", "Donor", "Hospital" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole<Guid>(role));
                }
            }

            var adminUser = await userManager.FindByEmailAsync(AdminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = AdminEmail,
                    Email = AdminEmail,
                    FullName = "System Administrator",
                    City = "N/A",
                    UserType = "Admin",
                    IsActive = true,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, AdminPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
            else if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
}
