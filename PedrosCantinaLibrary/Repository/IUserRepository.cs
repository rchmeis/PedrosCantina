using System;
using System.Collections.Generic;
using System.Text;

namespace PedrosCantinaLibrary.Repository
{
    public interface IUserRepository<User>
    {
        public User Create(User item);
        public List<User> GetAll();
        public User GetById(int id);
        public User Update(string id, User item);
        public User Delete(string id);
        public bool IsUsernameAvailable(string username);
        public User GetByUsername(string username);
        
    }
}

