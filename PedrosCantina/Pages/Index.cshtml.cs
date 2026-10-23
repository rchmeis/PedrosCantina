using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PedrosCantina.Pages
{
   
    public class IndexModel : PageModel
    {
       
          public IActionResult OnGet()
        {
            // 1. Er brugeren slet ikke logget ind? 
            // (Program.cs fanger det i forvejen pga. AuthorizeFolder("/"), men for en sikkerheds skyld)
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Account/SignIn");
            }

            // 2. Er brugeren logget ind, men IKKE er leder (dvs. medarbejder)?
            if (!User.IsInRole("Leder"))
            {
                // Så sender vi dem elegant videre til månedsplanen!
                return RedirectToPage("/Schedule/Monthly");
            }

            // 3. Hvis de ER leder, så får de lov til at se forsiden (Index)
            return Page();
        }
    }
    
}
