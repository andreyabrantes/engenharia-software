using System.Data;
using Microsoft.Data.Sqlite;

namespace BoraAli.Infrastructure.Data;

/// <summary>
/// Gerencia a sessão de conexão com o banco de dados SQLite
/// </summary>
public class DbSession : IDisposable
{
    private readonly string _connectionString;
    private IDbConnection? _connection;

    public DbSession(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IDbConnection Connection
    {
        get
        {
            if (_connection == null)
            {
                _connection = new SqliteConnection(_connectionString);
                _connection.Open();
                EnableWALMode();
                EnableForeignKeys();
            }
            return _connection;
        }
    }

    private void EnableWALMode()
    {
        using var cmd = _connection!.CreateCommand();
        cmd.CommandText = "PRAGMA journal_mode=WAL;";
        cmd.ExecuteNonQuery();
    }

    private void EnableForeignKeys()
    {
        using var cmd = _connection!.CreateCommand();
        cmd.CommandText = "PRAGMA foreign_keys=ON;";
        cmd.ExecuteNonQuery();
    }

    public void Dispose()
    {
        _connection?.Close();
        _connection?.Dispose();
    }
}
