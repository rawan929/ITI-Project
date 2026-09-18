using ITI.BLL.Services.Implementation;
using ITI.BLL.Services.Interface;
using ITI.DAL.Context;
using ITI.DAL.Models;
using ITI.DAL.Repo.Implementation;
using ITI.DAL.Repo.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ITI_Project
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddDbContext<AppDbcontext>(options =>
                   options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            // Repositories
            builder.Services.AddScoped<IHospitalRepo, HospitalRepo>();
            builder.Services.AddScoped<IBloodBankRepo, BloodBankRepo>();
            builder.Services.AddScoped<IDonorRepo, DonorRepo>();

            // Services
            builder.Services.AddScoped<IAdminService, AdminService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
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
           
            app.Run();
        }
    }
}
