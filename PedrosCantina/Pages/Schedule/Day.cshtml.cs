using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PedrosCantinaLibrary.Model;
using PedrosCantinaLibrary.Repository;
using System;
using System.Collections.Generic;

namespace PedrosCantina.Pages.Schedule
{
    public class DayModel : PageModel
    {
        private readonly ShiftRepository _shiftRepo;
        private readonly ICRUD<Employee> _employeeRepo;

        public DayModel(ShiftRepository shiftRepo, ICRUD<Employee> employeeRepo)
        {
            _shiftRepo = shiftRepo;
            _employeeRepo = employeeRepo;
        }

        public DayPlan? DayPlan { get; set; }
        public List<Employee> AllEmployees { get; set; } = new List<Employee>();

        [BindProperty]
        public string? Note { get; set; }

        [BindProperty]
        public string? SelectedEmployeeId { get; set; }

        public IActionResult OnGet(DateTime date)
        {
            if (date == DateTime.MinValue)
            {
                date = DateTime.Today;
            }

            DayPlan = _shiftRepo.GetDayPlan(date);

            if (DayPlan == null)
            {
                return RedirectToPage("/Schedule/Monthly");
            }

            Note = DayPlan.Note;
            AllEmployees = _employeeRepo.GetAll();

            return Page();
        }

        // Tilføj medarbejder til en specifik vagt
        public IActionResult OnPostAddEmployee(DateTime date, int shiftId)
        {
            if (!string.IsNullOrEmpty(SelectedEmployeeId))
            {
                _shiftRepo.AddEmployeeToShift(shiftId, SelectedEmployeeId);
            }

            return RedirectToPage(new { date = date.ToString("yyyy-MM-dd") });
        }

        // Fjern medarbejder fra en specifik vagt
        public IActionResult OnPostRemoveEmployee(DateTime date, int shiftId, string employeeId)
        {
            _shiftRepo.RemoveEmployeeFromShift(shiftId, employeeId);
            return RedirectToPage(new { date = date.ToString("yyyy-MM-dd") });
        }

        // Gem dagsnoten
        public IActionResult OnPostSaveNote(DateTime date)
        {
            _shiftRepo.UpdateDayNote(date, Note);
            return RedirectToPage(new { date = date.ToString("yyyy-MM-dd") });
        }
    }
}