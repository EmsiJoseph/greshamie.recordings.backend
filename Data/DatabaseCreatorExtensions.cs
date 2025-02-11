using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace backend.Data
{
    public static class DatabaseCreatorExtensions
    {
        public static IEnumerable<string> GetTables(this IRelationalDatabaseCreator creator)
        {
            // Get the RelationalDatabaseCreator implementation
            var relationalCreator = creator as RelationalDatabaseCreator;
            if (relationalCreator == null) return Array.Empty<string>();

            // Access the connection through the context
            var context = relationalCreator.GetType()
                .GetField("_context", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.GetValue(relationalCreator) as DbContext;

            if (context == null) return Array.Empty<string>();

            var connection = context.Database.GetDbConnection();
            if (connection == null) return Array.Empty<string>();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT TABLE_NAME 
                FROM INFORMATION_SCHEMA.TABLES 
                WHERE TABLE_TYPE = 'BASE TABLE'";

            var tables = new List<string>();
            
            var wasOpen = connection.State == System.Data.ConnectionState.Open;
            try
            {
                if (!wasOpen)
                    connection.Open();

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    tables.Add(reader.GetString(0));
                }
            }
            finally
            {
                if (!wasOpen && connection.State == System.Data.ConnectionState.Open)
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
