using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebApplication1.Models;
using WebApplication1.AppData;
using WebApplication1.SignalR;

namespace WebApplication1
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            string connection = Configuration.GetConnectionString("ConnectionString");
            services.AddDbContext<ApplicationContext>(options =>
                options.UseSqlServer(connection));

            services.AddIdentity<User, IdentityRole>(opts =>
            {
                opts.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+/ ";
                opts.User.RequireUniqueEmail = true;
                opts.Password.RequiredLength = 3;
                opts.Password.RequireNonAlphanumeric = false;
                opts.Password.RequireLowercase = false;
                opts.Password.RequireUppercase = false;
                opts.Password.RequireDigit = false;
            })
                .AddEntityFrameworkStores<ApplicationContext>().AddDefaultTokenProviders();
            services.AddAuthentication()
                    .AddGoogle(opts =>
                    {
                     opts.ClientId = "1059026201448-oe6661tcj1ruiboobh4n3gqu8206m2dk.apps.googleusercontent.com";
                     opts.ClientSecret = "_aTdHf8bXfxlTGJLKJlDq22-";
                     opts.SignInScheme = IdentityConstants.ExternalScheme;
                    });
            services.Configure<AppDataOptions>(Configuration.GetSection("AllAppSettings"));
            services.AddControllersWithViews();
            services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = true;
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapHub<ItemCommentsLikes>("/chat");
            });
            
        }
    }
}
