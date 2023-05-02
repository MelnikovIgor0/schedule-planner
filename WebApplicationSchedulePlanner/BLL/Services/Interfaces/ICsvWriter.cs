using BLL.Models;

namespace BLL.Services.Interfaces;

public interface ICsvWriter
{
    string WriteCsv(List<ScheduleElement> elements);
}
