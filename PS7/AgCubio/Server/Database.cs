using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
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
        /// <param name="paramss">Mysql paramaters to insert. SQLi's not welcome here.</param>
        /// <returns></returns>
        public static IEnumerable<Dictionary<string, string>> ExecuteSql(string sql, params KeyValuePair<string, string>[] paramss)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                // Open a connection
                conn.Open();

                // Create a command
                var command = conn.CreateCommand();
                command.CommandText = sql;

                foreach (var param in paramss)
                {
                    command.Parameters.AddWithValue(param.Key, param.Value);
                }

                // Execute the command and cycle through the DataReader object
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var dictionary = new Dictionary<string, string>();

                        for (var i = 0; i < reader.FieldCount; i++)
                        {
                            dictionary.Add(reader.GetName(i), reader.GetString(i));
                        }

                        yield return dictionary;

                    }
                }
            }
        }
    }
}
