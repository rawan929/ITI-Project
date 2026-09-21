using ITI.BLL.Services.Implementation;
using ITI.BLL.Services.Interface;
using ITI.DAL.Context;
using ITI.DAL.Models;
using ITI.DAL.Repo.Implementation;
using ITI.DAL.Repo.Interface;
using ITI_Project.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace ITI_Project
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Force an invariant culture for every request thread. Without this, on a
            // server/dev machine whose OS/browser locale is Arabic (dd/MM/yyyy, etc.),
            // the default MVC model binder can fail to parse the ISO 8601 strings that
            // <input type="datetime-local"> sends (e.g. "2026-09-22T09:00"), which makes
            // ModelState invalid and silently kicks the user back to the same form —
            // exactly what looked like "Confirm Appointment does nothing".
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddDbContext<AppDbcontext>(options =>
                   options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            // Repositories
            builder.Services.AddScoped<IHospitalRepo, HospitalRepo>();
            builder.Services.AddScoped<IBloodBankRepo, BloodBankRepo>();
            builder.Services.AddScoped<IDonorRepo, DonorRepo>();
            builder.Services.AddScoped<IDonorMatchingRepo, DonorMatchingRepo>();

            // Services
            builder.Services.AddScoped<IAdminService, AdminService>();
            builder.Services.AddScoped<IHospitalService, HospitalService>();
            builder.Services.AddScoped<IBloodRequestService, BloodRequestService>();
            builder.Services.AddScoped<IDonationRequestService, DonationRequestService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IDonorService, DonorService>();
            builder.Services.AddScoped<IBloodBankService, BloodBankService>();
            builder.Services.AddScoped<IAppointmentService, AppointmentService>();
            builder.Services.AddScoped<IDonorMatchingService, DonorMatchingService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            //  Identity
            builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequiredLength = 6;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<AppDbcontext>()
            .AddDefaultTokenProviders();


            var app = builder.Build();

            // Seed roles + default Admin account so there's a way to log into the Admin dashboard
            // (Admin cannot self-register through the Register page).
            using (var scope = app.Services.CreateScope())
            {
                await DbSeeder.SeedAsync(scope.ServiceProvider);
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();
           
            await app.RunAsync();
        }
    }
}
