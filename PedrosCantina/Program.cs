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

            // Add services to the container*******************************************************************
            builder.Services.AddRazorPages(options =>
            {
                
                options.Conventions.AuthorizeFolder("/"); // Kræver at man er logget ind som standard på alle sider
                options.Conventions.AuthorizeFolder("/Employees", "LederPolicy");
                options.Conventions.AuthorizePage("/Schedule/Day", "LederPolicy");
                options.Conventions.AllowAnonymousToPage("/Account/SignIn"); // Gør det tilladt at tilgå login og opret uden at være logget ind
                options.Conventions.AllowAnonymousToPage("/Account/SignUp");
                
            });

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme) //konfigurerer cookies
             .AddCookie(options =>
             {
                 options.LoginPath = "/Account/SignIn"; // Hvor brugeren sendes hen, hvis ikke de er logget ind
                 options.ExpireTimeSpan = TimeSpan.FromMinutes(20); // Hvor lang tid cookien lever
                 options.SlidingExpiration = true; //forlænger levetiden af cookie når der er aktivitet på siden
                 options.AccessDeniedPath = "/Schedule/Monthly"; //where the user gets redirect if access is denied

                 options.Events.OnRedirectToAccessDenied = context =>  //event der udløses af accessdenied
                 {
                     context.Response.Redirect("/Schedule/Monthly?accessDenied=true");
                     return Task.CompletedTask;
                 };
             });


            builder.Services.AddAuthorization(options =>  //tilføj og konfiguer autorisation (politikker)
            {
                options.AddPolicy("LederPolicy", policy => policy.RequireRole("Leder"));
                //options.AddPolicy("LederMedarbejderPolicy", policy => policy.RequireRole("leder", "medarbejder")); overflødig
            });

            //AddScoped opretter én instans per HTTP-forspørgsel,
            builder.Services.AddScoped<DbConnection>();//gør <T> tilgængelig via dependency injektion på tværs a siderne  
            builder.Services.AddScoped<PlanRepository>();
            builder.Services.AddScoped<ShiftRepository>();    
            builder.Services.AddScoped<ICRUD<Employee>, EmployeeRepository>(); // Dependency Injection: Når nogen beder om ICRUD<Employee>, giver vi dem EmployeeRepository
            builder.Services.AddScoped<IUserRepository<User>, UserRepository>();
            builder.Services.AddScoped<UserService>();
            
            //SERVICES END_____________________________________________________________________________________
         


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseRouting();       //Finner ud af, hvilken .cshtml-side browseren rent faktisk prøver at ramme.

            app.UseAuthentication(); //Tjekker cookien og finder ud af: "Hvem er det". Sætter User-objektet
            app.UseAuthorization();  //Hvad må brugeren? (har brugeren lov til at være på siden)

            app.MapStaticAssets();
            app.MapRazorPages()     //Finner ud af, hvilken .cshtml-side browseren rent faktisk prøver at ramme.
               .WithStaticAssets();

            app.Run();
        }
    }
}
