using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PedrosCantinaLibrary.Model;
using PedrosCantinaLibrary.Repository;
using System;

namespace PedrosCantina.Pages.Employees
{
    public class EditModel : PageModel
    {
        private readonly ICRUD<Employee> _repository;

        [BindProperty]
        public Employee Employee { get; set; } = new Employee();

        public EditModel(ICRUD<Employee> repository)
        {
            _repository = repository;
        }

        // Modtager ID direkte fra URL'en (f.eks. /Employees/Edit/123-abc. Routing!)
        public IActionResult OnGet(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "Ugyldigt medarbejder-ID.";
                return RedirectToPage("/Employees/Index");
            }

            try
            {
                Employee = _repository.GetById(id);

                if (Employee == null)
                {
                    TempData["ErrorMessage"] = $"Medarbejder med ID '{id}' blev ikke fundet.";
                    return RedirectToPage("/Employees/Index");
                }

                return Page();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Der opstod en fejl ved hentning af medarbejderen.";
                return RedirectToPage("/Employees/Index");
            }
        }

        public IActionResult OnPost(string id)
        {
            if (string.IsNullOrEmpty(Employee.Id))
            {
                Employee.Id = id;
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {             
                _repository.Update(Employee.Id, Employee);

                TempData["SuccessMessage"] = $"Medarbejder {Employee.FirstName} {Employee.LastName} blev opdateret succesfuldt!";
                return RedirectToPage("/Employees/Index");
            }
            catch (Exception ex)
            {
                string errorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                ModelState.AddModelError(string.Empty,$" Databasefejl: {errorMessage}. Kunne ikke opdatere medarbejderen i databasen. Prøv venligst igen.");
                return Page();
            }
        }
    }
}