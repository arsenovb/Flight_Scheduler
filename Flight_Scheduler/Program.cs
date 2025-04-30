using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Flight_Scheduler.Data;

namespace Flight_Scheduler
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddDbContext<Flight_SchedulerContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("Flight_SchedulerContext") ?? throw new InvalidOperationException("Connection string 'Flight_SchedulerContext' not found.")));
           

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
