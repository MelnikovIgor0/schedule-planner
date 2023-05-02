using DAL.Entities;
using DAL.Repositories.Interfaces;
using Dapper;

namespace DAL.Repositories;

public sealed class ScheduleRepository : BaseRepository, IScheduleRepository
{
    public ScheduleRepository() : base() { }

    public async Task CreateSchedule(string inputContent, 
        string id, string outputContent, CancellationToken ct)
    {
        string query = $"INSERT INTO schedules (uuid, input_content, output_content) " +
            $"VALUES ('{id}', '{inputContent}', '{outputContent}')";
        var connection = await GetAndOpenConnection();
        await connection.QueryAsync(query, ct);
    }

    public async Task<ScheduleEntity?> GetScheduleById(string id, CancellationToken ct)
    {
        string query = $"SELECT * FROM schedules WHERE uuid='{id}'";
        var connection = await GetAndOpenConnection();
        var schedules = await connection.QueryAsync<ScheduleEntity>(
            new CommandDefinition(query, ct));
        if (schedules.Count() == 0)
        {
            return null;
        }
        return schedules.First();
    }
}
