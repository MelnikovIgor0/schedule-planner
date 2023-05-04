using Microsoft.AspNetCore.Mvc;
using BLL.Models;
using BLL.Services;
using BLL.Services.Interfaces;
using DAL.Entities;
using DAL.Repositories;
using DAL.Repositories.Interfaces;
using WebApplicationSchedulePlannerMVC.Requests;

namespace WebApplicationSchedulePlannerMVC.Controllers;

[Controller]
[Route("")]
public class MainController : Controller
{
    private IScheduleRepository _scheduleRepository;
    private ICsvParser _csvParser;
    private ICsvWriter _csvWriter;
    private ISchedulePlanner _planner;
    private const string HOST = "https://localhost:7262/";

    public MainController()
    {
        _scheduleRepository = new ScheduleRepository();
        _csvParser = new CsvParser();
        _csvWriter = new CsvWriter();
        _planner = new SchedulePlanner();
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        return Redirect("create-schedule");
    }

    [HttpPost("create-schedule")]
    public async Task<IActionResult> CreateSchedulePost(CreateScheduleRequest request,
        CancellationToken ct)
    {
        if (request.InputData.Length > 1000000)
        {
            return BadRequest("Загруженный файл слишком большой");
        }
        if (request.InputData.Length > 0)
        {
            var filePath = Path.GetTempFileName();
            using (var stream = System.IO.File.Create(filePath))
            {
                await request.InputData.CopyToAsync(stream);
            }
            string[] input = System.IO.File.ReadAllLines(filePath);

            InputData data;
            try
            {
                data = _csvParser.ParseDataFromCsv(input);
            }
            catch (ArgumentException exception)
            {
                return BadRequest(exception.Message);
            }
            List<ScheduleElement> schedule = _planner.GenerateSchedule(data);
            string generatedSchedule = _csvWriter.WriteCsv(schedule);
            string newId = Guid.NewGuid().ToString();
            while ((await _scheduleRepository.GetScheduleById(newId, ct)) is not null)
            {
                newId = Guid.NewGuid().ToString();
            }
            await _scheduleRepository.CreateSchedule(string.Join('\n', input),
                newId, generatedSchedule, ct);

            ViewData["Download"] = $"{HOST}download-schedule/{newId}";
            ViewData["Watch"] = $"{HOST}watch-schedule/{newId}";
            return View();
        }
        return BadRequest("Загружен пустой файл");
    }

    [HttpGet("create-schedule")]
    public async Task<IActionResult> CreateSchedule(CreateScheduleRequest request,
        CancellationToken ct)
    {
        return View();
    }

    [HttpGet("download-schedule/{id}")]
    public async Task<IActionResult> GetSchedule(string id, CancellationToken ct)
    {
        ScheduleEntity? schedule = await _scheduleRepository.GetScheduleById(id, ct);
        if (schedule is null)
        {
            return StatusCode(404, "Расписание с данным идентификатором не найдено");
        }
        System.IO.File.WriteAllText("buffer_file.csv", schedule?.OutputContent, System.Text.Encoding.UTF8);
        Stream streamAns = System.IO.File.OpenRead("buffer_file.csv");
        var result = new FileStreamResult(streamAns, "application/octet-stream")
        { FileDownloadName = "schedule_" + schedule?.Uuid + ".csv" };
        return result;
    }

    [HttpGet("watch-schedule/{id}")]
    public async Task<IActionResult> WatchSchedule(string id, CancellationToken ct)
    {
        ScheduleEntity? schedule = await _scheduleRepository.GetScheduleById(id, ct);
        if (schedule is null)
        {
            return StatusCode(404, "Расписание с данным идентификатором не найдено");
        }
        List<ScheduleElement> elements = new List<ScheduleElement>();
        Dictionary<string, int> mappingDayOfWeek = new();
        mappingDayOfWeek["пн"] = 0;
        mappingDayOfWeek["вт"] = 1;
        mappingDayOfWeek["ср"] = 2;
        mappingDayOfWeek["чт"] = 3;
        mappingDayOfWeek["пт"] = 4;
        mappingDayOfWeek["сб"] = 5;
        mappingDayOfWeek["вс"] = 6;
        int counter = -1;
        foreach (string line in schedule?.OutputContent.Split(
            Environment.NewLine, StringSplitOptions.RemoveEmptyEntries))
        {
            if (counter == -1)
            {
                ++counter;
                continue;
            }
            string[] data = line.Split(';', 
                StringSplitOptions.RemoveEmptyEntries);
            elements.Add(new ScheduleElement
            {
                Id = ++counter,
                Day = mappingDayOfWeek[data[0]],
                Lesson = int.Parse(data[1]),
                LessonName = data[2],
                AuditoriumName = data[3]
            });
        }
        ViewData["Schedule"] = elements;
        ViewData["Download"] = $"{HOST}download-schedule/{id}";
        return View();
    }
}
