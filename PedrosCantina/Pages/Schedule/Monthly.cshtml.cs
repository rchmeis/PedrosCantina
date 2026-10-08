using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PedrosCantinaLibrary.Model;
using PedrosCantinaLibrary.Repository;

namespace PedrosCantina.Pages.Schedule
{
    public class MonthlyModel : PageModel
    {
        private readonly PlanRepository _planRepo;
        private readonly ShiftRepository _shiftRepo;
        private readonly ICRUD<Employee> _employeeRepo;

        [BindProperty(SupportsGet = true)]
        public int Year { get; set; } = DateTime.Now.Year;

        [BindProperty(SupportsGet = true)]
        public int Month { get; set; } = DateTime.Now.Month;

        [BindProperty(SupportsGet = true)]
        public string? EmployeeId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        public MonthlyPlan? Plan { get; set; }
        public Employee? SelectedEmployee { get; set; }

        public int MonthlyWorkload { get; set; }
        public int YearlyWorkload { get; set; }

        public MonthlyModel(PlanRepository planRepo, ShiftRepository shiftRepo, ICRUD<Employee> employeeRepo)
        {
            _planRepo = planRepo;
            _shiftRepo = shiftRepo;
            _employeeRepo = employeeRepo;
        }

        public void OnGet()
        {
            Plan = _planRepo.GetFullMonthlyPlan(Year, Month);

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                var query = SearchTerm.Trim().ToLower();
                var allEmployees = _employeeRepo.GetAll();

                SelectedEmployee = allEmployees.FirstOrDefault(e =>
                    (!string.IsNullOrEmpty(e.FirstName) && e.FirstName.ToLower().Contains(query)) ||
                    (!string.IsNullOrEmpty(e.LastName) && e.LastName.ToLower().Contains(query)) ||
                    ((e.FirstName + " " + e.LastName).ToLower().Contains(query)) ||
                    (!string.IsNullOrEmpty(e.PhoneNumber) && e.PhoneNumber.Replace(" ", "").Contains(query.Replace(" ", "")))
                );

                if (SelectedEmployee != null)
                {
                    EmployeeId = SelectedEmployee.Id;
                }
                else
                {
                    TempData["SearchWarning"] = $"Ingen medarbejder fundet på søgningen '{SearchTerm}'.";
                }
            }
            else if (!string.IsNullOrEmpty(EmployeeId))
            {
                SelectedEmployee = _employeeRepo.GetById(EmployeeId);
            }

            if (SelectedEmployee != null && !string.IsNullOrEmpty(SelectedEmployee.Id))
            {
                MonthlyWorkload = _planRepo.GetEmployeeWorkload(SelectedEmployee.Id, Year, Month);
                YearlyWorkload = _planRepo.GetEmployeeWorkload(SelectedEmployee.Id, Year, null);
            }
        }

        // HANDLER: Tilføj medarbejder direkte via ShiftId
        public IActionResult OnPostAddShift(int shiftId, string employeeId, int year, int month, string? searchTerm)
        {
            if (shiftId > 0 && !string.IsNullOrEmpty(employeeId))
            {
                bool success = _shiftRepo.AddEmployeeToShift(shiftId, employeeId);
                var emp = _employeeRepo.GetById(employeeId);

                if (success)
                {
                    TempData["SuccessMessage"] = $"{emp?.FirstName} {emp?.LastName} blev tilføjet til vagten.";
                }
                else
                {
                    TempData["SearchWarning"] = $"{emp?.FirstName} {emp?.LastName} er allerede tilknyttet denne vagt.";
                }
            }

            return RedirectToPage(new { year = year, month = month, employeeId = employeeId, searchTerm = searchTerm });
        }

        // HANDLER: Fjern medarbejder direkte via ShiftId
        public IActionResult OnPostRemoveShift(int shiftId, string employeeId, int year, int month, string? searchTerm)
        {
            if (shiftId > 0 && !string.IsNullOrEmpty(employeeId))
            {
                _shiftRepo.RemoveEmployeeFromShift(shiftId, employeeId);
                var emp = _employeeRepo.GetById(employeeId);

                TempData["SuccessMessage"] = $"{emp?.FirstName} {emp?.LastName} blev fjernet fra vagten.";
            }

            return RedirectToPage(new { year = year, month = month, employeeId = employeeId, searchTerm = searchTerm });
        }

        public IActionResult OnPostCreatePlan()
        {
            try
            {
                _planRepo.CreateMonthlyPlan(Year, Month);
            }
            catch (Exception ex)
            {
                TempData["FailCreatePlan"] = "Fejl ved oprettelsen af månedsplan: " + ex.Message;
            }
            return RedirectToPage(new { year = Year, month = Month });
        }
    }
}