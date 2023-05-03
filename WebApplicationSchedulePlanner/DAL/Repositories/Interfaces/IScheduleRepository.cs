using DAL.Entities;

namespace DAL.Repositories.Interfaces;

public interface IScheduleRepository
{
    Task<ScheduleEntity?> GetScheduleById(string id, CancellationToken ct);

    Task CreateSchedule(string inputContent, string id, string outputContent, CancellationToken ct);
}
