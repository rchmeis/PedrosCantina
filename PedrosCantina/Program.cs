using Microsoft.AspNetCore.Authentication.Cookies;
using PedrosCantinaLibrary.Model;
using PedrosCantinaLibrary.Repository;
using PedrosCantinaLibrary.Servcices;

namespace PedrosCantina
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages(options =>
            {
                // Kræver at man er logget ind som standard på alle sider
                options.Conventions.AuthorizeFolder("/");

                // Gør det tilladt at tilgå login og opret uden at være logget ind
                options.Conventions.AllowAnonymousToPage("/Account/SignIn");
                options.Conventions.AllowAnonymousToPage("/Account/SignUp");
            });
            //AddScoped opretter én instans per HTTP-forspørgsel,
            builder.Services.AddScoped<DbConnection>();
            builder.Services.AddScoped<PlanRepository>();
            builder.Services.AddScoped<ShiftRepository>();          //gør repo tilgængelig via dependency injektion på tværs a siderne      
            builder.Services.AddScoped<ICRUD<Employee>, EmployeeRepository>(); // Dependency Injection: Når nogen beder om ICRUD<Employee>, giver vi dem EmployeeRepository
            builder.Services.AddScoped<IUserRepository<User>, UserRepository>();
            builder.Services.AddScoped<UserService>();
            
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/Account/SignIn"; // Hvor brugeren sendes hen, hvis ikke de er logget ind
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Hvor lang tid cookien lever
            });


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

            app.UseAuthentication(); //huske denne. Hvem er brugeren?
            app.UseAuthorization();  //Hvad må brugeren?

            app.MapStaticAssets();
            app.MapRazorPages()
               .WithStaticAssets();

            app.Run();
        }
    }
}
