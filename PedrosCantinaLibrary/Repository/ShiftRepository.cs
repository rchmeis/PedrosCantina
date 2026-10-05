using Microsoft.Data.SqlClient;
using PedrosCantinaLibrary.Model;
using System;
using System.Collections.Generic;
using System.Data.Common;

namespace PedrosCantinaLibrary.Repository
{
    public class ShiftRepository
    {
        private readonly DbConnection _db;
        public ShiftRepository(DbConnection db) { _db = db; }

        // Henter én enkelt dagsplan inklusiv dens vagter og tilknyttede medarbejdere
        public DayPlan? GetDayPlan(DateTime date)
        {
            DayPlan? dayPlan = null;
            _db.Connect();

            // 1. Hent dagsplanen
            string sqlDay = "SELECT Date, Year, Month, Note FROM DayPlan WHERE Date = @Date";
            using (var cmd = new SqlCommand(sqlDay, _db._connection))
            {
                cmd.Parameters.AddWithValue("@Date", date.Date);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        dayPlan = new DayPlan
                        {
                            Date = reader.GetDateTime(0),
                            Year = reader.GetInt32(1),
                            Month = reader.GetInt32(2),
                            Note = reader.IsDBNull(3) ? null : reader.GetString(3)
                        };
                    }
                }
            }

            if (dayPlan == null)
            {
                _db.Disconnect();
                return null;
            }

            // 2. Hent vagter og tilknyttede medarbejdere
            string sqlShifts = @"
                SELECT s.Id, s.Date, s.ShiftType, e.Id AS EmpId, e.FirstName, e.LastName, e.CanActAsLeader
                FROM Shift s
                LEFT JOIN ShiftEmployee se ON s.Id = se.ShiftId
                LEFT JOIN Employee e ON se.EmployeeId = e.Id
                WHERE s.Date = @Date";

            var shiftDictionary = new Dictionary<int, Shift>();

            using (var cmd = new SqlCommand(sqlShifts, _db._connection))
            {
                cmd.Parameters.AddWithValue("@Date", date.Date);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int shiftId = reader.GetInt32(0);
                        DateTime shiftDate = reader.GetDateTime(1);
                        ShiftType type = (ShiftType)Convert.ToByte(reader[2]); // Håndterer både TINYINT og INT

                        if (!shiftDictionary.TryGetValue(shiftId, out var shift))
                        {
                            shift = new Shift
                            {
                                Id = shiftId,
                                Date = shiftDate,
                                Type = type,
                                Employees = new List<Employee>()
                            };
                            shiftDictionary[shiftId] = shift;

                            // Tilføjer vagten til Dagsplanen via din LoadShiftFromDatabase metode
                            dayPlan.LoadShiftFromDatabase(shift);
                        }

                        if (!reader.IsDBNull(3))
                        {
                            var emp = new Employee
                            {
                                Id = reader.GetString(3),
                                FirstName = reader.IsDBNull(4) ? "" : reader.GetString(4),
                                LastName = reader.IsDBNull(5) ? "" : reader.GetString(5),
                                CanActAsLeader = !reader.IsDBNull(6) && reader.GetBoolean(6)
                            };
                            shift.Employees.Add(emp);
                        }
                    }
                }
            }

            _db.Disconnect();
            return dayPlan;
        }

        // Tilføjer en medarbejder til en vagt
        public void AddEmployeeToShift(int shiftId, string employeeId)
        {
            _db.Connect();
            string sql = "INSERT INTO ShiftEmployee (ShiftId, EmployeeId) VALUES (@ShiftId, @EmpId)";
            using (var cmd = new SqlCommand(sql, _db._connection))
            {
                cmd.Parameters.AddWithValue("@ShiftId", shiftId);
                cmd.Parameters.AddWithValue("@EmpId", employeeId);
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
                {
                    // Håndterer hvis medarbejderen allerede er tilføjet til vagten (Primary Key / Unique constraint)
                }
            }
            _db.Disconnect();
        }

        // Fjerner en medarbejder fra en vagt
        public void RemoveEmployeeFromShift(int shiftId, string employeeId)
        {
            _db.Connect();
            string sql = "DELETE FROM ShiftEmployee WHERE ShiftId = @ShiftId AND EmployeeId = @EmpId";
            using (var cmd = new SqlCommand(sql, _db._connection))
            {
                cmd.Parameters.AddWithValue("@ShiftId", shiftId);
                cmd.Parameters.AddWithValue("@EmpId", employeeId);
                cmd.ExecuteNonQuery();
            }
            _db.Disconnect();
        }

        // Opdaterer dagens note
        public void UpdateDayNote(DateTime date, string? note)
        {
            _db.Connect();
            string sql = "UPDATE DayPlan SET Note = @Note WHERE Date = @Date";
            using (var cmd = new SqlCommand(sql, _db._connection))
            {
                cmd.Parameters.AddWithValue("@Note", (object?)note ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Date", date.Date);
                cmd.ExecuteNonQuery();
            }
            _db.Disconnect();
        }
    }
}