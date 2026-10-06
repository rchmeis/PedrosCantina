using Microsoft.Data.SqlClient;
using PedrosCantinaLibrary.Model;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace PedrosCantinaLibrary.Repository
{
    public class PlanRepository
    {
        private readonly DbConnection _db;
        public PlanRepository(DbConnection db) { _db = db; }
               
        public void CreateMonthlyPlan(int year, int month)
        {
            MonthlyPlan plan = new MonthlyPlan { Year = year, Month = month, Status = PlanStatus.Draft };
            plan.GenerateDays(); // Kalder internt dayPlan.InitializeShifts()

            _db.Connect();
            SqlTransaction transaction = _db._connection.BeginTransaction();
            try
            {                
                string sqlMonth = "INSERT INTO MonthlyPlan (Year, Month, Status) VALUES (@Y, @M, @S)";
                using (SqlCommand cmd = new SqlCommand(sqlMonth, _db._connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@Y", plan.Year);
                    cmd.Parameters.AddWithValue("@M", plan.Month);
                    cmd.Parameters.AddWithValue("@S", (int)plan.Status);
                    cmd.ExecuteNonQuery();
                }

                // Gem DayPlans og Shifts
                foreach (DayPlan day in plan.DayPlans)
                {
                    string sqlDay = "INSERT INTO DayPlan (Date, Year, Month, Note) VALUES (@D, @Y, @M, @N)";
                    using (SqlCommand cmd = new SqlCommand(sqlDay, _db._connection, transaction))
                    {
                        cmd.Parameters.AddWithValue("@D", day.Date);
                        cmd.Parameters.AddWithValue("@Y", day.Year);
                        cmd.Parameters.AddWithValue("@M", day.Month);
                        cmd.Parameters.AddWithValue("@N", (object)day.Note ?? DBNull.Value); //caster string til Object typen så begge sider af ?? operatoren er af samme type.
                        cmd.ExecuteNonQuery();
                    }

                    foreach (Shift shift in day.Shifts)
                    {
                        string sqlShift = "INSERT INTO Shift (Date, ShiftType) VALUES (@D, @T)";
                        using (SqlCommand cmd = new SqlCommand(sqlShift, _db._connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@D", shift.Date);
                            cmd.Parameters.AddWithValue("@T", (int)shift.Type);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                transaction.Commit();
            }
            catch(Exception ex)
            {
                transaction.Rollback();
                throw new Exception("Fejl i oprettelsen af månedsplan",ex);
            }
            finally { _db.Disconnect(); }
        }

        
        public MonthlyPlan? GetMonthlyPlanSummary(int year, int month)
        {
            MonthlyPlan? plan = null;
            _db.Connect();

            try
            {
                string sql = "SELECT Status FROM MonthlyPlan WHERE Year = @Y AND Month = @M";
                using (SqlCommand cmd = new SqlCommand(sql, _db._connection))
                {
                    cmd.Parameters.AddWithValue("@Y", year);
                    cmd.Parameters.AddWithValue("@M", month);
                    var statusObj = cmd.ExecuteScalar(); //returns the first column of the first row in the result set returned by the query.
                    if (statusObj != null)
                    {
                        plan = new MonthlyPlan { Year = year, Month = month, Status = (PlanStatus)Convert.ToInt32(statusObj) };
                    }
                }
            }
            catch(Exception ex)
            {
                throw new Exception("Fejl ved hentning af plan" + ex.Message);
            }
            finally
            {
                _db.Disconnect();
            }                 
            return plan;
        }

       
        public MonthlyPlan? GetFullMonthlyPlan(int year, int month)
        {
            MonthlyPlan? plan = null;
            _db.Connect();

            // Trin A: Hent Månedsplanen
            string sqlPlan = "SELECT Year, Month, Status FROM MonthlyPlan WHERE Year = @Y AND Month = @M";
            using (SqlCommand cmd = new SqlCommand(sqlPlan, _db._connection))
            {
                cmd.Parameters.AddWithValue("@Y", year);
                cmd.Parameters.AddWithValue("@M", month);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        plan = new MonthlyPlan
                        {
                            Year = reader.GetInt32(0),
                            Month = reader.GetInt32(1),
                            Status = (PlanStatus)reader.GetByte(2),
                            DayPlans = new List<DayPlan>()
                        };
                    }
                }
            }

            if (plan == null)
            {
                _db.Disconnect();
                return null;
            }

           
            string sqlDays = "SELECT Date, Note FROM DayPlan WHERE Year = @Y AND Month = @M";
            var dayDictionary = new Dictionary<DateTime, DayPlan>();

            using (SqlCommand cmd = new SqlCommand(sqlDays, _db._connection))
            {
                cmd.Parameters.AddWithValue("@Y", year);
                cmd.Parameters.AddWithValue("@M", month);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DateTime date = reader.GetDateTime(0);
                        DayPlan dayPlan = new DayPlan
                        {
                            Date = date,
                            Year = year,
                            Month = month,
                            Note = reader.IsDBNull(1) ? null : reader.GetString(1)                            
                        };
                        plan.DayPlans.Add(dayPlan);
                        dayDictionary[date] = dayPlan;
                    }
                }
            }

            
            string sqlShifts = @"
                SELECT s.Id, s.Date, s.ShiftType, e.Id AS EmpId, e.FirstName, e.LastName, e.CanActAsLeader
                FROM Shift s
                INNER JOIN DayPlan dp ON s.Date = dp.Date
                LEFT JOIN ShiftEmployee se ON s.Id = se.ShiftId
                LEFT JOIN Employee e ON se.EmployeeId = e.Id
                WHERE dp.Year = @Y AND dp.Month = @M";

            var shiftDictionary = new Dictionary<int, Shift>();

            using (SqlCommand cmd = new SqlCommand(sqlShifts, _db._connection))
            {
                cmd.Parameters.AddWithValue("@Y", year);
                cmd.Parameters.AddWithValue("@M", month);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int shiftId = reader.GetInt32(0);
                        DateTime date = reader.GetDateTime(1);
                        ShiftType type = (ShiftType)reader.GetInt32(2);

                        if (!shiftDictionary.TryGetValue(shiftId, out var shift))
                        {
                            shift = new Shift
                            {
                                Id = shiftId,
                                Date = date,
                                Type = type,
                                Employees = new List<Employee>()
                            };
                            shiftDictionary[shiftId] = shift;
                    
                            if (dayDictionary.TryGetValue(date, out var day))
                            {
                                day.AddShiftFromDatabase(shift);
                            }
                        }

                       
                        if (!reader.IsDBNull(3))
                        {
                            Employee emp = new Employee
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
            return plan;
        }

       
        public int GetEmployeeWorkload(string employeeId, int year, int? month)
        {
            int workload = 0;
            _db.Connect();

            try
            {
                string sql = @"
                SELECT COUNT(*) 
                FROM ShiftEmployee se
                INNER JOIN Shift s ON se.ShiftId = s.Id
                INNER JOIN DayPlan dp ON s.Date = dp.Date
                WHERE se.EmployeeId = @EmpId AND dp.Year = @Year";

                if (month.HasValue)
                {
                    sql += " AND dp.Month = @Month";
                }

                using (SqlCommand cmd = new SqlCommand(sql, _db._connection))
                {
                    cmd.Parameters.AddWithValue("@EmpId", employeeId);
                    cmd.Parameters.AddWithValue("@Year", year);
                    if (month.HasValue) cmd.Parameters.AddWithValue("@Month", month.Value);

                    workload = (int)cmd.ExecuteScalar();
                }
            }
            catch(Exception ex)
            {
                throw new Exception("Fejl ved hentning af belastning - prøv igen eller kontakt system administrator" + ex.Message);
            }
            finally
            {
                _db.Disconnect();
            }       
            
            return workload;
        }
    }
}