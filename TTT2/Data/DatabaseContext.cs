using MySqlConnector;
using System.Data;

namespace TTT2.Data
{
    public class DatabaseContext : IDisposable
    {
        private readonly MySqlConnection _connection;

        public DatabaseContext()
        {
            var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
            _connection = new MySqlConnection(connectionString);
        }
        public async Task OpenAsync()
        {
            if (_connection.State == ConnectionState.Closed)
            {
                await _connection.OpenAsync();
            }
        }

        public async Task<int> ExecuteNonQueryAsync(string query, params MySqlParameter[] parameters)
        {
            using var command = new MySqlCommand(query, _connection);
            command.Parameters.AddRange(parameters);
            return await command.ExecuteNonQueryAsync();
        }

        public async Task<MySqlDataReader> ExecuteQueryAsync(string query, params MySqlParameter[] parameters)
        {
            using var command = new MySqlCommand(query, _connection);
            command.Parameters.AddRange(parameters);
            return await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);
        }

        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}
