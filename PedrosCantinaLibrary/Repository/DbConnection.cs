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
        
        
        //public void Connect()
        //{
        //    var config = new ConfigurationBuilder().AddUserSecrets(Assembly.GetExecutingAssembly(), optional:true).Build();
        //    string connectionString = config["ConnectionString"]; //config virker som en dictionary, hvor vi kan hente vores connectionString
        //    _connection = new SqlConnection(connectionString);
        //    _connection.Open();
         
        //}

        public SqlConnection Connect()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory) // Tells the app where to look for files
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true) // For production (Simply.com)
                .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true) // For local development
                .Build();

            string connectionString = config["ConnectionString"];

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new Exception("Database ConnectionString is missing from configuration!");
            }
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            return connection;
        }

    }
}
