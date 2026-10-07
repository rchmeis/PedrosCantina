using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PedrosCantinaLibrary.Model;
using PedrosCantinaLibrary.Repository;

namespace PedrosCantina.Pages.Employees
{
    public class IndexModel : PageModel
    {
        private readonly ICRUD<Employee> _repository;
                
        public List<Employee> Employees = new List<Employee>();

        [BindProperty]
        public Employee NewEmployee { get; set; } = new Employee();
       
        [BindProperty(SupportsGet = true)]  // SupportsGet = true gør, at feltet gribes automatisk fra URL'en (?SearchTerm=Pedro)
        public string? SearchTerm { get; set; }


        public IndexModel (ICRUD<Employee> repository)
        {
            _repository = repository;
        }


        public void OnGet()
        {
            var allEmployees = _repository.GetAll();

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                Employees = allEmployees.Where(e =>
                    (e.FirstName != null && e.FirstName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    (e.LastName != null && e.LastName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    (e.Email != null && e.Email.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    (e.PhoneNumber != null && e.PhoneNumber.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }
            else
            {
                Employees = allEmployees;
            }

        }

        public IActionResult OnPostCreate()
        {
            if (!ModelState.IsValid)
            {
                OnGet();
                return Page();
            }
            try
            {                
                _repository.Create(NewEmployee);
                TempData["SuccessMessage"] = "Medarbejder oprettet succesfuldt!";
                return RedirectToPage("/Employees/Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Kunne ikke gemme medarbejderen. Prøv venligst igen eller kontakt systemadministrator.");
                throw new Exception("Fejl ved oprettelse af medarbejder i databasen", ex);
            }
        }

        public IActionResult OnPostDelete(string id)
        {
            if ((!ModelState.IsValid)|| (string.IsNullOrEmpty(id)))
            {
                OnGet();
                return Page();
            }
            try
            {
                _repository.Delete(id);
                TempData["SuccessMessage"] = "Medarbejder slettet succesfuldt!";
                return RedirectToPage("/Employees/Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Kunne ikke slette medarbejderen. Prøv venligst igen eller kontakt systemadministrator.");
                OnGet();
                return Page();
            }
        }

        public IActionResult OnPostUpdateEmployee(string id)
        {
            if (!ModelState.IsValid)
            {
                OnGet();
                return Page();
            }
            try
            {
                _repository.Update(id, NewEmployee);
                TempData["SuccessMessage"] = "Medarbejder opdateret succesfuldt!";
                return RedirectToPage("/Employees/Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Kunne ikke opdatere medarbejderen. Prøv venligst igen eller kontakt systemadministrator.");
                OnGet();
                return Page();
            }
        }
    }
}
