using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PedrosCantinaLibrary.Servcices;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using PedrosCantinaLibrary.Model;

namespace PedrosCantina.Pages.Account
{
    public class SignInModel : PageModel
    {
        private readonly UserService _userService;

        public SignInModel(UserService userService)
        {
            _userService = userService;
        }

        [BindProperty]
        public string Username { get; set; }

        [BindProperty]
        public string Password { get; set; }

        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            User user = _userService.ValidateUser(Username, Password);
            if (user == null)
            {
                ErrorMessage = "Ugyldigt brugernavn eller adgangskode.";
                return Page();
            }
            
            string input = user.Employee.CanActAsLeader ? "Leder" : "Medarbejder";
            string initials = user.Employee.FirstName[0].ToString() + user.Employee.LastName[0].ToString();

            //Opret "claims" (identitets-informationer om brugeren)
            //Vi pakker brugernes username ind i en "Claim", så systemet ved, hvem der er logget ind fremover.
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, input),// input manuelt sat til "Leder" eller "Medarbejder"
                new Claim(ClaimTypes.GivenName, user.Employee.FirstName),
                new Claim("initials", initials) // F.eks. "PG"
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme); //Her fortæller du systemet, hvilken type sikkerhedssystem der har godkendt dette ID-kort. Ved at skrive CookieAuthenticationDefaults siger du: "Dette ID-kort er udstedt via vores Cookie-system"

            //nu logges ind og udsted cookien i browseren!
            //Denne linje fortæller ASP.NET Core, at brugeren nu er godkendt, og udsteder sikkerheds-cookien til browseren.
            await HttpContext.SignInAsync(
                 CookieAuthenticationDefaults.AuthenticationScheme,
                 new ClaimsPrincipal(claimsIdentity));

            if (user.Employee.CanActAsLeader)
            {
                return RedirectToPage("/Index"); // Lederen ryger til dashboardet
            }
            else
            {
                return RedirectToPage("/Schedule/Monthly"); // Medarbejderen ryger direkte til vagtplanen
            }         
        }
    }
}

