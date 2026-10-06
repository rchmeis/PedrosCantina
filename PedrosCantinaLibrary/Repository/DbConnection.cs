using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace PedrosCantinaLibrary.Repository
{
    public class DbConnection
    {
        internal SqlConnection _connection;
        
        public void Connect()
        {
            var config = new ConfigurationBuilder().AddUserSecrets(Assembly.GetExecutingAssembly(), optional:true).Build();
            string connectionString = config["ConnectionString"]; //config virker som en dictionary, hvor vi kan hente vores connectionString
            _connection = new SqlConnection(connectionString);
            _connection.Open();
         
        }

        public void Disconnect()
        {
            if (_connection != null && _connection.State == System.Data.ConnectionState.Open)
            {
                _connection.Close();
            }
          
        }
    }
}
