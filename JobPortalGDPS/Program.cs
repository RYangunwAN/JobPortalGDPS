using JobPortalGDPS.Data;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace JobPortalGDPS
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Configure DapperRepository with the connection string from appsettings.json
            builder.Services.AddTransient<DapperRepository>(provider =>
                new DapperRepository(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Add cookie-based authentication
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";    // Redirect to login page if not authenticated
                    options.LogoutPath = "/Account/Logout";  // Handle logout
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // Cookie expiration time
                    options.SlidingExpiration = true; // Extend cookie expiration on activity
                });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error"); // Handle exceptions in production
                app.UseHsts(); // Enforce HTTPS
            }

            app.UseHttpsRedirection(); // Redirect HTTP requests to HTTPS
            app.UseStaticFiles(); // Serve static files from wwwroot

            app.UseRouting(); // Enable routing
            app.UseAuthentication(); // Enable authentication
            app.UseAuthorization(); // Enable authorization

            // Set default route to login page
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Account}/{action=Login}/{id?}");

            app.Run(); // Run the application
        }
    }
}
