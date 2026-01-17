using ArchitectApp.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using DotNetEnv;

namespace ArchitectApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Env.Load();

            var builder = WebApplication.CreateBuilder(args);

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");

            if (!string.IsNullOrEmpty(dbPassword))
            {
                var csBuilder = new SqlConnectionStringBuilder(connectionString);
                csBuilder.Password = dbPassword;
                csBuilder.UserID = Environment.GetEnvironmentVariable("DB_USER") ?? "sa";

                connectionString = csBuilder.ConnectionString;
            }

            builder.Services.AddDbContext<ArchitectDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddHttpClient("EmailService", client =>
            {
                client.Timeout = TimeSpan.FromSeconds(10);
            });

            builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ArchitectDbContext>();

            // Add MVC
            builder.Services.AddControllersWithViews();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseSwagger();
            app.UseSwaggerUI();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<ArchitectDbContext>();

                    context.Database.Migrate();

                    Console.WriteLine("Migration completed successfully!");
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occured during migration!");
                }
            }

            app.Run();
        }
    }
}
