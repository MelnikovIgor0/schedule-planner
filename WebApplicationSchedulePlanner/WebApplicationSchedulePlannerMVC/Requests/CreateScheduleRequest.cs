namespace WebApplicationSchedulePlannerMVC.Requests;

public sealed class CreateScheduleRequest
{
    [System.ComponentModel.DisplayName("Входной файл")]
    public IFormFile InputData { get; set; }
}
