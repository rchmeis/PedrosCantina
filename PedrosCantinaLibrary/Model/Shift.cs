using PedrosCantinaLibrary.Model;
using System;
using System.Collections.Generic;

namespace PedrosCantinaLibrary.Model
{
        public class Shift
    {
        public int Id { get; set; } 
        public DateTime Date { get; set; }
        public ShiftType Type { get; set; }
        public List<Employee> Employees { get; set; } = new List<Employee>();

        public Shift() { }

        public TimeSpan StartTime => Type.GetStartTime();
        public TimeSpan EndTime => Type.GetEndTime();
        public int DurationInHours => Type.GetDurationInHours();

        // Valideringsregel fra opgaven
        public bool IsValid()
        {
            bool hasLeader = Employees.Any(e => e.CanActAsLeader);
            bool correctCount = Employees.Count >= 2 && Employees.Count <= 3;
            return hasLeader && correctCount;
        }
    }
    
}