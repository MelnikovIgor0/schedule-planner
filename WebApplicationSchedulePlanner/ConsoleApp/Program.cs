using System.IO;
using BLL.Models;
using BLL.Services;
using BLL.Services.Interfaces;

ICsvParser parser = new CsvParser();

/*foreach (string line in File.ReadAllLines(@"F:\course_project\data examples\InputData.csv"))
{
    Console.WriteLine(line);
}*/

InputData data = parser.ParseDataFromCsv(
    File.ReadAllLines(@"F:\course_project\data examples\InputData.csv"));

Console.WriteLine("subjects:");
foreach (Subject subject in data.Subjects)
{
    Console.WriteLine(subject.Name + ", " + subject.Id);
}

Console.WriteLine("groups:");
foreach (Group group in data.Groups)
{
    Console.Write(group.Name + ", " + group.Id + ", " + group.NumberStudents + "; ");
    foreach (Subject s in group.Subjects)
    {
        Console.Write(s.Name + " ");
    }
    Console.WriteLine();
}

Console.WriteLine("students:");
foreach (Student student in data.Students)
{
    Console.Write(student.Name + ", " + student.Id + "; ");
    foreach (Group g in student.Groups)
    {
        Console.Write(g.Name + " ");
    }
    Console.WriteLine();
}

Console.WriteLine("auditorias:");
foreach (Auditorium auditorium in data.Auditorias)
{
    Console.WriteLine(auditorium.Name + ", " + auditorium.Id + ", " + auditorium.Capacity);
}

ISchedulePlanner planner = new SchedulePlanner();

foreach (ScheduleElement element in planner.GenerateSchedule(data, new List<int> { 1, 2, 3, 4, 5, 6, 7, 8})) {
    Console.WriteLine(element.Day + " " + element.Lesson + " " + element.LessonName + " " + element.AuditoriumName);
}