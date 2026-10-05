using Microsoft.Data.SqlClient;
using PedrosCantinaLibrary.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace PedrosCantinaLibrary.Repository
{
    public class EmployeeRepository : ICRUD<Employee>
    {
        internal DbConnection _DBConnection = new DbConnection();
        public Employee Create(Employee item)
        {

            _DBConnection.Connect();
            try
            {
                string query = $"INSERT INTO Employee (Id, FirstName, LastName, Email, PhoneNumber, Address, PaymentInfo, CanActAsLeader) " +
                            "VALUES (@Id, @FirstName, @LastName, @Email, @PhoneNumber, @Address, @PaymentInfo, @CanActAsLeader);";
                using (SqlCommand command = new SqlCommand(query, _DBConnection._connection))
                {
                    string validId = string.IsNullOrWhiteSpace(item.Id) ? Guid.NewGuid().ToString() : item.Id;
                    command.Parameters.AddWithValue("@Id", validId);

                    AddParameters(command, item);                

                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected <= 0)
                    {
                        throw new Exception("Ingen rækker blev indsat i Employee-tabellen.");
                    }
                    return item;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Fejl ved oprettelse af medarbejder i databasen", ex);
            }
            finally
            {
                _DBConnection.Disconnect();
            }

        }

        public List<Employee> GetAll()
        {
            List<Employee> employees = new List<Employee>();
            _DBConnection.Connect();
            try
            {
                string query = "SELECT * FROM Employee";
                using (SqlCommand command = new SqlCommand(query, _DBConnection._connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            employees.Add(MapEmployee(reader));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Fejl ved hentning af medarbejdere fra databasen", ex);
            }
            finally
            {
                _DBConnection.Disconnect();
            }

            return employees;
        }
        public Employee GetById(string id)
        {
            _DBConnection.Connect();
            try
            {
                string query = "SELECT * FROM Employee WHERE Id = @Id";
                using (SqlCommand command = new SqlCommand(query, _DBConnection._connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapEmployee(reader);
                        }

                        throw new Exception($"Medarbejder med Id '{id}' blev ikke fundet i databasen.");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Fejl ved hentning af medarbejder med Id '{id}'.", ex);
            }
            finally
            {
                _DBConnection.Disconnect();
            }
        }
        public Employee Update(string id, Employee item)
        {
            _DBConnection.Connect();
            try
            {
                string updateQuery = "UPDATE Employee " +
                                "SET FirstName = @FirstName, LastName = @LastName, Email = @Email, PhoneNumber = @PhoneNumber, Address = @Address, PaymentInfo = @PaymentInfo, CanActAsLeader = @CanActAsLeader " +
                                "OUTPUT INSERTED.* " +
                                "WHERE Id = @Id";
                using (SqlCommand command = new SqlCommand(updateQuery, _DBConnection._connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    AddParameters(command, item);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapEmployee(reader);
                        }

                        throw new Exception($"Opdatering mislykkedes. Medarbejder med Id '{id}' blev ikke fundet");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Fejl ved opdatering af medarbejder med Id '{id}' i databasen", ex);
            }
            finally
            {
                _DBConnection.Disconnect();
            }

        }
        public Employee Delete(string id)
        {
            _DBConnection.Connect();
            try
            {
                string deleteQuery = "DELETE FROM Employee WHERE Id = @Id OUTPUT DELETED.*";
                using (SqlCommand command = new SqlCommand(deleteQuery, _DBConnection._connection))
                {
                    command.Parameters.AddWithValue("@Id", id);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapEmployee(reader);
                        }

                        throw new Exception($"Sletning mislykkedes. Medarbejder med Id '{id}' blev ikke fundet");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Fejl ved sletning af medarbejder med Id '{id}' i databasen", ex);
            }
            finally
            {
                _DBConnection.Disconnect();
            }
        }


        #region Private Helper Methods

        /// <summary>
        /// Privat hjælpemetode (DRY) til at mappe en SQL-række til et Employee-objekt.
        /// </summary>
        private Employee MapEmployee(SqlDataReader reader)
        {
            return new Employee
            {
                Id = reader["Id"].ToString()!,
                FirstName = reader["FirstName"].ToString()!,
                LastName = reader["LastName"].ToString()!,
                Email = reader["Email"].ToString()!,
                PhoneNumber = reader["PhoneNumber"].ToString()!,
                Address = reader["Address"] != DBNull.Value ? reader["Address"].ToString() : null,
                PaymentInfo = reader["PaymentInfo"] != DBNull.Value ? reader["PaymentInfo"].ToString() : null,
                CanActAsLeader = Convert.ToBoolean(reader["CanActAsLeader"])
            };
        }

        /// <summary>
        /// Privat hjælpemetode til at tilføje fælles parametre ved Create og Update.
        /// </summary>
   
        private void AddParameters(SqlCommand command, Employee item)
        {            
            command.Parameters.AddWithValue("@FirstName", (object?)item.FirstName ?? DBNull.Value);
            command.Parameters.AddWithValue("@LastName", (object?)item.LastName ?? DBNull.Value);
            command.Parameters.AddWithValue("@Email", (object?)item.Email ?? DBNull.Value);
            command.Parameters.AddWithValue("@PhoneNumber", (object?)item.PhoneNumber ?? DBNull.Value);
            command.Parameters.AddWithValue("@Address", (object?)item.Address ?? DBNull.Value);
            command.Parameters.AddWithValue("@PaymentInfo", (object?)item.PaymentInfo ?? DBNull.Value);
            command.Parameters.AddWithValue("@CanActAsLeader", item.CanActAsLeader);
        #endregion
        }
    }
}
