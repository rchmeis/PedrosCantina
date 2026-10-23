using Microsoft.AspNetCore.Identity;
using PedrosCantinaLibrary.Model;
using PedrosCantinaLibrary.Repository;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace PedrosCantinaLibrary.Servcices
{
    public class UserService
    {
        private readonly IUserRepository<User> _userRepository;
        private readonly ICRUD<Employee> _employeeRepository;
        private readonly PasswordHasher<User> _passwordHasher = new PasswordHasher<User>();
        public UserService(IUserRepository<User> userRepository, ICRUD<Employee> employeeRepository)
        {
            _userRepository = userRepository;
            _employeeRepository = employeeRepository;
        }
        public bool Register(string username, string password, string employeeId)
        {            
            if (_userRepository.IsUsernameAvailable(username))
            {
                User user = new User
                {
                    Username = username,
                    Employee = _employeeRepository.GetById(employeeId)
                };
                user.PasswordHashed = _passwordHasher.HashPassword(user, password);
                _userRepository.Create(user);
                return true;
            }
            return false;
            
        }

        public User ValidateUser(string username, string password)
        {
            User user = _userRepository.GetByUsername(username);

            if (user == null)
            {
                return null;
            }
            //check input password mod userPW. returnerer Success, Failed, ell SuccessRehashNeeded
            PasswordVerificationResult result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHashed, password);

            //returner brugeren hvis PW er korrekt, ellers null. 
            return result == PasswordVerificationResult.Success ? user : null;
           
        }



    }
}
