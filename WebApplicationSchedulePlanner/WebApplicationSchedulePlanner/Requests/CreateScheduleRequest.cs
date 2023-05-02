namespace WebApplicationSchedulePlanner.Requests;

public sealed record CreateScheduleRequest
{
    public IFormFile inputData { get; set; }
    // public int[] WorkDays { get; set; }
    
    // public int[] LessonPriorety { get; set; }
}
