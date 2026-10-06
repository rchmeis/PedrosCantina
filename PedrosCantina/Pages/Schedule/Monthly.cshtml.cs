using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PedrosCantinaLibrary.Model;
using PedrosCantinaLibrary.Repository;

namespace PedrosCantina.Pages.Schedule
{
    public class MonthlyModel : PageModel
    {
        private readonly PlanRepository _planRepo;
        private readonly ICRUD<Employee> _employeeRepo;

        [BindProperty(SupportsGet = true)]
        public int Year { get; set; } = DateTime.Now.Year;

        [BindProperty(SupportsGet = true)]
        public int Month { get; set; } = DateTime.Now.Month;

        // KRAV 3: Routing ID. Hvis denne har en værdi (fx ?employeeId=1), slår siden over i "Medarbejder-tilstand"
        [BindProperty(SupportsGet = true)]
        public string? EmployeeId { get; set; }

        public MonthlyPlan? Plan { get; set; }
        public Employee? SelectedEmployee { get; set; }

        // KRAV 3.2: Belastning
        public int MonthlyWorkload { get; set; }
        public int YearlyWorkload { get; set; }

        public MonthlyModel(PlanRepository planRepo, ICRUD<Employee> employeeRepo)
        {
            _planRepo = planRepo;
            _employeeRepo = employeeRepo;
        }

        public void OnGet()
        {
            // Henter hele planen inkl. dage, vagter og tilknyttede medarbejdere
            Plan = _planRepo.GetFullMonthlyPlan(Year, Month);

            // KRAV 3: Tjekker om vi ankom fra Employees/Index
            if (!string.IsNullOrEmpty(EmployeeId))
            {
                SelectedEmployee = _employeeRepo.GetById(EmployeeId); // Hent medarbejderens navn

                if (SelectedEmployee != null)
                {
                    // Hent belastningstal fra databasen
                    MonthlyWorkload = _planRepo.GetEmployeeWorkload(EmployeeId, Year, Month);
                    YearlyWorkload = _planRepo.GetEmployeeWorkload(EmployeeId, Year, null); // null for hele året
                }
            }
        }

        public IActionResult OnPostCreatePlan()
        {
            try
            {
                _planRepo.CreateMonthlyPlan(Year, Month);
            }
            catch(Exception ex)
            {
                TempData["FailCreatePlan"] = "Fejl ved oprettelsen af månedsplan - kontakt systemadministrator" + ex.Message;
            }            
            return RedirectToPage(new { year = Year, month = Month });
        }
    }
}