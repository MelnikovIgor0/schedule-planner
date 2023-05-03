namespace WebApplicationSchedulePlannerMVC.Requests;

public sealed record CreateScheduleRequest
{
    public IFormFile InputData { get; set; }
}
