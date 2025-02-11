using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;

namespace backend.Data.Extensions
{
    public static class DatabaseCreatorExtensions
    {
        public static IEnumerable<(string Name, bool Exists)> GetTables(this IRelationalDatabaseCreator creator)
        {
            var dbContext = creator.GetType().GetField("_dependencies", 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance)
                ?.GetValue(creator) as RelationalDatabaseCreatorDependencies;

            var connection = dbContext?.Connection;
            if (connection == null) return Array.Empty<(string, bool)>();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT TABLE_NAME 
                FROM INFORMATION_SCHEMA.TABLES 
                WHERE TABLE_TYPE = 'BASE TABLE'";

            var tables = new List<(string Name, bool Exists)>();
            
            try
            {
                connection.Open();
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var tableName = reader.GetString(0);
                    tables.Add((tableName, true));
                }
            }
            finally
            {
                connection.Close();
            }

            return tables;
        }
    }
}
