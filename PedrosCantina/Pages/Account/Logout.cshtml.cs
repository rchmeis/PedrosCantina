using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PedrosCantina.Pages.Account
{

    public class LogoutModel : PageModel
    {
        public async Task<IActionResult> OnPostAsync()
        {
            // Slet cookien fra browseren
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Send lederen tilbage til login-siden
            return RedirectToPage("/Account/SignIn");
        }
        //    public void OnGet()
        //    {
        //    }

        //public class LogoutModel : PageModel
        //{
        //    public async Task<IActionResult> OnGetAsync()
        //    {
        //        // Slet cookien fra browseren ved et GET-klik (f.eks. et link)
        //        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        //        return RedirectToPage("/Account/SignIn");
        //    }
        //}
    }
}
