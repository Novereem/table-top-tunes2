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
        
        public async Task<T> ExecuteScalarAsync<T>(string query, params MySqlParameter[] parameters)
        {
            using var command = new MySqlCommand(query, _connection);
            command.Parameters.AddRange(parameters);

            await OpenAsync();
            var result = await command.ExecuteScalarAsync();

            if (result == null || result == DBNull.Value)
                return default(T)!;
            
            if (typeof(T) == typeof(int) && result is long longValue)
                return (T)(object)(int)longValue;

            return (T)result;
        }

        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}
