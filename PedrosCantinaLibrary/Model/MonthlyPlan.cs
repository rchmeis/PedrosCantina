using System.Collections.Generic;
using PedrosCantinaLibrary.Model;

namespace PedrosCantinaLibrary.Model
{
    public class MonthlyPlan
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public PlanStatus Status { get; set; }

        public List<DayPlan> DayPlans { get; set; } = new List<DayPlan>();

        public MonthlyPlan() { }

        public void GenerateDays()
        {
            int daysInMonth = DateTime.DaysInMonth(Year, Month);
            for (int i = 1; i <= daysInMonth; i++)
            {
                var dayPlan = new DayPlan { Date = new DateTime(Year, Month, i), Year = this.Year, Month = this.Month };
                dayPlan.InitializeShifts();
                DayPlans.Add(dayPlan);
            }
        }
    }
}
//namespace PedrosCantinaLibrary.Model
//{
//    public class MonthlyPlan
//    {    
//        public int Year { get; set; }
//        public int Month { get; set; }
//        public PlanStatus Status { get; set; }

//        public List<DayPlan> DayPlans { get; set; } = new List<DayPlan>();

//        public MonthlyPlan() { }

//        public MonthlyPlan(int year, int month, PlanStatus status)
//        {
//            Year = year;
//            Month = month;
//            Status = status;               
//        }

//        public override string ToString()
//        {
//            return $"Plan: {Month}/{Year} (Status: {Status})";
//        }
//    }
//}