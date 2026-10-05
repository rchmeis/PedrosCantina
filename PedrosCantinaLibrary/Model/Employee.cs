using System;
using System.Collections.Generic;

namespace PedrosCantinaLibrary.Model
{
    public class Employee
    {
        public string Id { get; set; } = Guid.NewGuid().ToString().Substring(0,8);
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Address { get; set; } = null;
        public string? PaymentInfo { get; set; } = null;
        public bool CanActAsLeader { get; set; }

        public List<Shift> Shifts { get; set; } = new List<Shift>();

        public Employee() { }

        public Employee(string firstName, string lastName, string email, string phoneNumber, bool canActAsLeader, string? paymentInfo = null, string? address = null)
        {
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            PhoneNumber = phoneNumber;
            CanActAsLeader = canActAsLeader;
            PaymentInfo = paymentInfo;
            Address = address;
        }

        public override string ToString()
        {
            return $"[{Id}] {FirstName} {LastName} - Kan agere som leder: {CanActAsLeader}";
        }
    }
}