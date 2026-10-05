using System;
using System.Collections.Generic;
using System.Text;

namespace PedrosCantinaLibrary.Model
{
    public enum ShiftType
    {
        Morning,
        Afternoon
    }
    public static class ShiftTypeExtensions
    {
        public static TimeSpan GetStartTime(this ShiftType shiftType) => shiftType switch
        {
            ShiftType.Morning => new TimeSpan(9, 0, 0),
            ShiftType.Afternoon => new TimeSpan(14, 0, 0),
            _ => throw new ArgumentOutOfRangeException(nameof(shiftType))
        };

        public static TimeSpan GetEndTime(this ShiftType shiftType) => shiftType switch
        {
            ShiftType.Morning => new TimeSpan(14, 0, 0),
            ShiftType.Afternoon => new TimeSpan(19, 0, 0),
            _ => throw new ArgumentOutOfRangeException(nameof(shiftType))
        };

        public static int GetDurationInHours(this ShiftType shiftType)
        {
            return (int)(shiftType.GetEndTime() - shiftType.GetStartTime()).TotalHours;
        }
    }
}
