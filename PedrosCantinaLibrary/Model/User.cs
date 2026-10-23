using System;
using System.Collections.Generic;
using System.Text;

namespace PedrosCantinaLibrary.Model
{
    public class User
    {
        public int? Id { get; set; }
        public string Username { get; set; }
        public string PasswordHashed { get; set; }
        public Employee? Employee { get; set; }
        public User() { }
        public User(int? id, string username, string passwordHash, Employee? employee)
        {
            Id = id; 
            Username = username;
            PasswordHashed = passwordHash;
            Employee = employee;
        }
    }
}
