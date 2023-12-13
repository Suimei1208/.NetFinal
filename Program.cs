using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Auth;
using Microsoft.Azure.Storage.Blob;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using NetTechnology_Final.Context;
using NetTechnology_Final.Services.EmailService;
using NetTechnology_Final.Services.Hash;
using NetTechnology_Final.Services.IMG;
using reCAPTCHA.AspNetCore;
using System.Text;

namespace NetTechnology_Final
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
           builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(option =>
                {
                    option.LoginPath = "/Accounts/Login";
                    option.AccessDeniedPath = "/Error/notfound";
 
                });
            builder.Services.AddSingleton(x => {
                var connectionString = x.GetService<IConfiguration>()["AzureStorage:ConnectionString"];
                var storageCredentials = new StorageCredentials(connectionString);
                var storageAccountUri = new Uri("https://avatarfinal.blob.core.windows.net/");
                var storageUri = new StorageUri(storageAccountUri);
                return new CloudBlobClient(storageAccountUri, storageCredentials);
            });
            builder.Services.AddSession();
            builder.Services.AddScoped<IBlobService, BlobService>();
            builder.Services.AddRecaptcha(builder.Configuration.GetSection("Recaptcha"));
            builder.Services.AddTransient<TokenService>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddDbContext<ApplicationDbContext>(options => options.
                UseSqlServer(builder.Configuration.GetConnectionString("Conn")));
            var app = builder.Build();


            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
                       
			app.UseAuthentication();
			app.UseAuthorization();

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseSession();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}