using BLL.Models;
using BLL.Services.Interfaces;
using System.Text;

namespace BLL.Services;

public sealed class CsvWriter : ICsvWriter
{
    private string GetDay(int day)
    {
        switch (day)
        {
            case 0:
                return "пн";
            case 1:
                return "вт";
            case 2:
                return "ср";
            case 3:
                return "чт";
            case 4:
                return "пт";
            case 5:
                return "сб";
            case 6:
                return "вс";
            default:
                throw new ArgumentException("Указан некорректный день недели");
        }
    }
    public string WriteCsv(List<ScheduleElement> elements)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("день;номер пары;предмет;аудитория;");
        foreach (ScheduleElement element in elements)
        {
            sb.AppendLine(GetDay(element.Day) + ";" + element.Lesson + ";" +
                element.LessonName + ";" + element.AuditoriumName + ";");
        }
        return sb.ToString();
    }
}
