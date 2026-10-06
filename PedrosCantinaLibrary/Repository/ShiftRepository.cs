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
       
        public DayPlan? GetDayPlan(DateTime date)
        {
            DayPlan? dayPlan = null;
            _db.Connect();

            try
            {
                string sqlDay = "SELECT Date, Year, Month, Note FROM DayPlan WHERE Date = @Date";
                using (SqlCommand cmd = new SqlCommand(sqlDay, _db._connection))
                {
                    cmd.Parameters.AddWithValue("@Date", date.Date);
                    using (SqlDataReader reader = cmd.ExecuteReader())
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

                
                string sqlShifts = @"
                SELECT s.Id, s.Date, s.ShiftType, e.Id AS EmpId, e.FirstName, e.LastName, e.CanActAsLeader
                FROM Shift s
                LEFT JOIN ShiftEmployee se ON s.Id = se.ShiftId
                LEFT JOIN Employee e ON se.EmployeeId = e.Id
                WHERE s.Date = @Date";

                var shiftDictionary = new Dictionary<int, Shift>();         //Dictionary bruges til at forhindre skabelse af mange shiftobjekter, da feks flere medarbejdere vil resulterer i flere rows med samme s.Id

                using (SqlCommand cmd = new SqlCommand(sqlShifts, _db._connection))
                {
                    cmd.Parameters.AddWithValue("@Date", date.Date);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int shiftId = reader.GetInt32(0);
                            DateTime shiftDate = reader.GetDateTime(1);
                            ShiftType type = (ShiftType)Convert.ToByte(reader[2]); // Håndterer både TINYINT og INT

                            if (!shiftDictionary.TryGetValue(shiftId, out var shift))  //her sørger vi for, at der kun laves et nyt shift objekt hvis det ikke findes i forvejen.
                            {
                                shift = new Shift
                                {
                                    Id = shiftId,
                                    Date = shiftDate,
                                    Type = type,
                                    Employees = new List<Employee>()
                                };
                                shiftDictionary[shiftId] = shift;

                                // Tilføjer vagten til Dagsplanen 
                                dayPlan.AddShiftFromDatabase(shift); 
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
                
            }
            catch (Exception ex) { }
            finally
            {
                _db.Disconnect();                
            }
            return dayPlan;
        }


        public bool AddEmployeeToShift(int shiftId, string employeeId)
        {
            try
            {
                _db.Connect();
                string sql = "INSERT INTO ShiftEmployee (ShiftId, EmployeeId) VALUES (@ShiftId, @EmpId)";

                using (SqlCommand cmd = new SqlCommand(sql, _db._connection))
                {
                    cmd.Parameters.AddWithValue("@ShiftId", shiftId);
                    cmd.Parameters.AddWithValue("@EmpId", employeeId);

                    cmd.ExecuteNonQuery();
                    return true; 
                }
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {               
                return false;
            }
            finally
            {
                _db.Disconnect();
            }
        }


        public bool RemoveEmployeeFromShift(int shiftId, string employeeId)
        {
            bool success = false;
            _db.Connect();
            try
            {
                string sql = "DELETE FROM ShiftEmployee WHERE ShiftId = @ShiftId AND EmployeeId = @EmpId";
                using (SqlCommand cmd = new SqlCommand(sql, _db._connection))
                {
                    cmd.Parameters.AddWithValue("@ShiftId", shiftId);
                    cmd.Parameters.AddWithValue("@EmpId", employeeId);                    
                    cmd.ExecuteNonQuery();
             
                    return success = true;
                }
            }
            catch(Exception ex)
            {
                throw new Exception("Fejl under sletning af medarbejder fra vagt", ex);
            }
            finally
            {
                _db.Disconnect();
            }           
        }

        
        public bool UpdateDayNote(DateTime date, string? note)
        {
            _db.Connect();
            bool success = false;

            try
            {
                string sql = "UPDATE DayPlan SET Note = @Note WHERE Date = @Date";
                using (var cmd = new SqlCommand(sql, _db._connection))
                {
                    cmd.Parameters.AddWithValue("@Note", (object?)note ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Date", date.Date);
                    
                    int rowsAffected = cmd.ExecuteNonQuery();
                    if(rowsAffected == 0)
                    {
                        throw new Exception($"Fejl ved opdatering af dagsnote - Ingen dagsplan fundet for datoen {date.ToShortDateString()}");
                    }   
                   
                    return success=true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Fejl ved opdatering af dagsnote", ex);
            }
            finally
            {
                _db.Disconnect();
            }
        }
    }
}