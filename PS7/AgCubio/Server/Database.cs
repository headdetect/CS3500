using MySql.Data.MySqlClient;
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
        public const string ConnectionString = "server=atr.eng.utah.edu;database=cs3500_emiramon;uid=cs3500_emiramon;password=PSWRD";

        /// <summary>
        /// Executes the SQL query.
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <returns></returns>
        public static IEnumerable<string> ExecuteSql(string sql)
        {
            using (MySqlConnection conn = new MySqlConnection(ConnectionString))
            {
                // Open a connection
                conn.Open();

                // Create a command
                MySqlCommand command = conn.CreateCommand();
                command.CommandText = "";

                // Execute the command and cycle through the DataReader object
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        yield return reader.GetString(0);
                    }
                }
            }
        }
    }
}
