namespace DAL.Entities;

public sealed record ScheduleEntity
{
    public string Uuid { get; init; }
    public string InputContent { get; init; }
    public string OutputContent { get; set; }
}
