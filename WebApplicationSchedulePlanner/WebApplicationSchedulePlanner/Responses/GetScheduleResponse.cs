namespace WebApplicationSchedulePlanner.Responses;

public sealed record GetScheduleResponse
{
    public IFormFile Result { get; set; }
}
