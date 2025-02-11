using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace backend.Data
{
    public static class DatabaseCreatorExtensions
    {
        public static IEnumerable<string> GetTables(this IRelationalDatabaseCreator creator)
        {
            var dbContext = (creator as RelationalDatabaseCreator)?.Dependencies?.RelationalConnection;
            if (dbContext?.ConnectionString == null) return Array.Empty<string>();

            using var connection = new SqlConnection(dbContext.ConnectionString);
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT TABLE_NAME 
                FROM INFORMATION_SCHEMA.TABLES 
                WHERE TABLE_TYPE = 'BASE TABLE'";

            var tables = new List<string>();
            
            try
            {
                connection.Open();
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    tables.Add(reader.GetString(0));
                }
            }
            finally
            {
                if (connection.State == System.Data.ConnectionState.Open)
                    connection.Close();
            }

            return tables;
        }

        public static bool TableExists(this IRelationalDatabaseCreator creator, string tableName)
        {
            return GetTables(creator).Any(t => t.Equals(tableName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
