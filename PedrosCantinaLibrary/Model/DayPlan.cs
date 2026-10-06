using PedrosCantinaLibrary.Model;
using System;
using System.Collections.Generic;

namespace PedrosCantinaLibrary.Model
{
    public class DayPlan
    {
        public DateTime Date { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public string? Note { get; set; }

        private readonly List<Shift> _shifts = new List<Shift>();
        public IReadOnlyList<Shift> Shifts => _shifts.AsReadOnly();

        public DayPlan() { }

        // Kaldes ved oprettelse af ny plan via koden, ikke når der læses fra DB
        public void InitializeShifts()
        {
            _shifts.Clear();
            _shifts.Add(new Shift { Date = this.Date, Type = ShiftType.Morning });
            _shifts.Add(new Shift { Date = this.Date, Type = ShiftType.Afternoon });
        }

        // Bruges af Repository til at tilføje vagter fra databasen
        public void AddShiftFromDatabase(Shift shift)
        {
            _shifts.Add(shift);
        }

        // Domæne-logik: Er kravene for dagen opfyldt? (Altid 1 leder, 2-3 personer pr vagt)
        public bool IsRequirementsMet()
        {
            if (_shifts.Count != 2) return false;
            return _shifts.All(s => s.IsValid());
        }

    }
}