using PedrosCantinaLibrary.Model;
using PedrosCantinaLibrary.Repository;

namespace PedrosCantina
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages();
                                                                //AddScoped opretter én instans per HTTP-forspørgsel,
            builder.Services.AddScoped<DbConnection>();
            builder.Services.AddScoped<PlanRepository>();
            builder.Services.AddScoped<ShiftRepository>();            
            builder.Services.AddScoped<ICRUD<Employee>, EmployeeRepository>(); // Dependency Injection: Når nogen beder om ICRUD<Employee>, giver vi dem EmployeeRepository
            

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapRazorPages()
               .WithStaticAssets();

            app.Run();
        }
    }
}
