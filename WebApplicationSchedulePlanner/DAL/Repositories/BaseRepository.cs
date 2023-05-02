using System.Transactions;
using Npgsql;
using DAL.Repositories.Interfaces;

namespace DAL.Repositories;

public abstract class BaseRepository : IDbRepository
{
    protected BaseRepository()
    {
    }

    protected async Task<NpgsqlConnection> GetAndOpenConnection()
    {
        var connection = new NpgsqlConnection(Constants.ConnectionString);
        await connection.OpenAsync();
        connection.ReloadTypes();
        return connection;
    }
}