﻿using Microsoft.AspNetCore.Mvc;
using BLL.Models;
using BLL.Services;
using BLL.Services.Interfaces;
using DAL.Entities;
using DAL.Repositories;
using DAL.Repositories.Interfaces;

namespace WebApplicationSchedulePlanner.Controllers;

[ApiController]
[Route("")]
public class MainController : Controller
{
    private IScheduleRepository _scheduleRepository;
    private ICsvParser _csvParser;
    private ICsvWriter _csvWriter;
    private ISchedulePlanner _planner;

    public MainController()
    {
        _scheduleRepository = new ScheduleRepository();
        _csvParser = new CsvParser();
        _csvWriter = new CsvWriter();
        _planner = new SchedulePlanner();
    }

    [HttpGet("index")]
    public async Task<IActionResult> Index()
    {
        return Ok();
    }

    [HttpPost("create-schedule")]
    public async Task<IActionResult> CreateSchedule(IFormFile inputData, 
        [FromHeader] List<int> lessonPriorety, CancellationToken ct)
    {
        if (inputData.Length > 0)
        {
            var filePath = Path.GetTempFileName();
            using (var stream = System.IO.File.Create(filePath))
            {
                await inputData.CopyToAsync(stream);
            }
            string[] input = System.IO.File.ReadAllLines(filePath);

            InputData data;
            // try
            // {
                data = _csvParser.ParseDataFromCsv(input);
            // } catch (ArgumentException exception)
            // {
            //     return BadRequest(exception.Message);
            // }
            List<ScheduleElement> schedule = _planner.GenerateSchedule(data,
                new List<int>(lessonPriorety));
            string generatedSchedule = _csvWriter.WriteCsv(schedule);
            string newId = Guid.NewGuid().ToString();
            await _scheduleRepository.CreateSchedule(String.Join('\n', input),
                newId, generatedSchedule, ct);
            System.IO.File.WriteAllText("buffer_file.csv", generatedSchedule, System.Text.Encoding.UTF8);
            Stream streamAns = System.IO.File.OpenRead("buffer_file.csv");
            var result = new FileStreamResult(streamAns, "application/octet-stream")
            { FileDownloadName = "schedule_" + newId + ".csv" };
            return result;
        }
        return BadRequest("empty input file!");
    }

    [HttpGet("schedule/{id}")]
    public async Task<IActionResult> GetSchedule(string id, CancellationToken ct)
    {
        ScheduleEntity? schedule = await _scheduleRepository.GetScheduleById(id, ct);
        if (schedule is null)
        {
            return StatusCode(404, "Schedule with specified id does not exist");
        }
        System.IO.File.WriteAllText("buffer_file.csv", schedule?.OutputContent, System.Text.Encoding.UTF8);
        Stream streamAns = System.IO.File.OpenRead("buffer_file.csv");
        var result = new FileStreamResult(streamAns, "application/octet-stream")
        { FileDownloadName = "schedule_" + schedule?.Uuid + ".csv" };
        return result;
    }
}