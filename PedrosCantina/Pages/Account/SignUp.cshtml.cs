using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PedrosCantinaLibrary.Servcices;
using System.ComponentModel.DataAnnotations;

namespace PedrosCantina.Pages.Account
{
    public class SignUpModel : PageModel
    {
        private readonly UserService _userService;
        public SignUpModel(UserService userService)
        {
            _userService = userService;
        }

        [BindProperty]
        [Required(ErrorMessage="Der skal være et brugernavn")]
        [MinLength(6,ErrorMessage="brugernavn skal være mindst 6 tegn")]
        public string Username { get; set; }
        [BindProperty]
        [Required]
        [MinLength(10,ErrorMessage="Password skal være mindst 10 tegn")]
        public string Password { get; set; }
        [BindProperty]
        [Required(ErrorMessage="Indtast venligst dit ID")]
        public string EmployeeId { get; set; }

        public string ErrorMessage { get; set; }        


        public void OnGet()
        {
        }
        public IActionResult OnPost()
        {
            if (!ModelState.IsValid) return Page();

            try
            {
                bool register = _userService.Register(Username, Password, EmployeeId);
                if (!register)
                {
                    ErrorMessage = "Brugernavnet er allerede optaget.";
                    return Page();
                }
            }
            catch(Exception ex)
            {
                ErrorMessage = "Der opstod en fejl i systemet. Prøv igen senere eller kontakt admin.";
                return Page();
            }

            // Send lederen videre til login-siden efter oprettelse
            return RedirectToPage("/Account/SignIn");
        }
    }
}
