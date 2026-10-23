
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using PedrosCantinaLibrary.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace PedrosCantinaLibrary.Repository
{
    public class UserRepository : IUserRepository<User>
    {
        private readonly DbConnection _db;

        public UserRepository(DbConnection db)
        {
            _db = db;
        }

        public bool IsUsernameAvailable(string username)
        {
            using SqlConnection connection = _db.Connect();          
            string checkQuery = "SELECT COUNT(1) FROM Users WHERE Username = @username;";

            try
            {
                using SqlCommand cmdCheck = new SqlCommand(checkQuery, connection);                
                cmdCheck.Parameters.AddWithValue("@username", username);
                int userCount = (int)cmdCheck.ExecuteScalar();

                if (userCount > 0)
                {
                    return false; //user already exists
                }
                return true;
                
            }          
            catch (Exception ex)
            {
                throw new Exception("Fejl ved læsning af bruger i databasen", ex);
            }

        }
        public User GetByUsername(string username)
        {
            using SqlConnection connection = _db.Connect();
            try
            {
                // Vi behøver ikke COUNT(1) først. Vi kører bare SELECT'en direkte!
                string getUser = """
                    SELECT 
                    u.Id AS UserId, 
                    u.Username, 
                    u.PasswordHashed, 
                    u.EmployeeId,
                    e.* FROM Users u 
                    JOIN Employee e ON u.EmployeeId = e.Id 
                    WHERE u.Username = @username
                    """;

                using (SqlCommand cmdGetUser = new SqlCommand(getUser, connection))
                {
                    cmdGetUser.Parameters.AddWithValue("@username", username);

                    using SqlDataReader reader = cmdGetUser.ExecuteReader();
                    if (reader.Read()) 
                    {
                        User user = new User
                        {
                            Id = Convert.ToInt32(reader["UserId"]),
                            Username = reader["Username"].ToString(),
                            PasswordHashed = reader["PasswordHashed"].ToString(),
                            Employee = MapEmployee(reader)
                        };

                        return user;
                    }
                }

                // Hvis læseren ikke finder noget, returneres null automatisk
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception("Fejl ved læsning af bruger i databasen", ex);
            }
        }
        public User Create(User item)
        {
            using SqlConnection connection = _db.Connect();

            try
            {
                //Hvis brugeren ikke findes, opret den
                string createQuery = """
                        INSERT INTO Users (Username, PasswordHashed, EmployeeId) 
                        VALUES (@username, @passwordHashed, @employeeId)
                        """;

                using (SqlCommand cmd = new SqlCommand(createQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@username", item.Username);
                    cmd.Parameters.AddWithValue("@passwordHashed", item.PasswordHashed);
                    cmd.Parameters.AddWithValue("@employeeId", item.Employee.Id);
                    cmd.ExecuteNonQuery();
                }
                return item;
            }
            catch (Exception ex)
            {
                throw new Exception("Fejl ved tilføjelse af bruger til databasen", ex);
            }
        }

        public User Delete(string id)
        {
            throw new NotImplementedException();
        }

        public List<User> GetAll()
        {
            List<User> users = new List<User>();
            using SqlConnection connection = _db.Connect(); 
            try
            { 
                string userQuery = """
                SELECT 
                u.Id AS UserId, 
                u.Username, 
                u.PasswordHashed, 
                u.EmployeeId,
                e.*
                FROM Users u 
                JOIN Employee e ON u.EmployeeId = e.Id
                WHERE u.Id=@id
                """;
                using SqlCommand cmd = new SqlCommand(userQuery, connection);
                using SqlDataReader reader = cmd.ExecuteReader();
                    
                while (reader.Read())
                {
                    if (!reader.IsDBNull(reader.GetOrdinal("UserId")))
                    {
                        User user = new User
                        {
                            Id = Convert.ToInt32(reader["UserId"]),
                            Username = reader["Username"].ToString(),
                            PasswordHashed = reader["PasswordHashed"].ToString(),
                            Employee = MapEmployee(reader)
                        };
                        users.Add(user);
                    }
                }
                    
            }catch(Exception ex)
            {
                throw new Exception("Fejl ved hentning af brugere fra databasen",ex);
            }          
            return users;
        }

        public User GetById(int id)
        {
            using SqlConnection connection = _db.Connect();
            try
            {
                string getQuery = """
                SELECT 
                u.Id AS UserId, 
                u.Username, 
                u.PasswordHashed, 
                u.EmployeeId,
                e.*
                FROM Users u 
                JOIN Employee e ON u.EmployeeId = e.Id
                WHERE u.Id=@id
                """;
                using SqlCommand cmd = new SqlCommand(getQuery, connection);
                cmd.Parameters.AddWithValue("@id", id);

                using SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    User user = new User
                    {
                        Id = Convert.ToInt32(reader["UserId"]),
                        Username = reader["Username"].ToString(),
                        PasswordHashed = reader["PasswordHashed"].ToString(),
                        Employee = MapEmployee(reader)
                    };
                    return user;
                }
            }catch(Exception ex)
            {
                throw new Exception("Fejl under hentning fra databasen", ex);
            }
        
            return null;
            
        }

        public User Update(string id, User item)
        {
            throw new NotImplementedException();
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

        #endregion

    }



}
