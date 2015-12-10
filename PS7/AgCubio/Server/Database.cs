using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public static class Database
    {
        /// <summary>
        /// The connection string.
        /// </summary>
        public const string ConnectionString = "server=atr.eng.utah.edu;database=;uid=;password=";

        /// <summary>
        /// Executes the SQL query.
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <returns></returns>
        public static object ExecuteSql(string sql)
        {
            //TODO: finish connecting to database
            /**  // Connect to the DB
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    // Open a connection
                    conn.Open();

                    // Create a command
                    MySqlCommand command = conn.CreateCommand();
                    command.CommandText = "";

                    // Execute the command and cycle through the DataReader object
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }*/
            return null;
        }
    }
}
