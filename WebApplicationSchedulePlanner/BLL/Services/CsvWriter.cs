using BLL.Models;
using BLL.Services.Interfaces;
using System.Text;

namespace BLL.Services;

public sealed class CsvWriter : ICsvWriter
{
    public string WriteCsv(List<ScheduleElement> elements)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("день;номер пары;предмет;аудитория;");
        foreach (ScheduleElement element in elements)
        {
            sb.AppendLine(element.Day + ";" + element.Lesson + ";" + element.LessonName + ";" + element.AuditoriumName + ";");
        }
        return sb.ToString();
    }
}
