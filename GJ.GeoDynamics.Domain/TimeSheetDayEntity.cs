namespace GJ.GeoDynamics.Domain;

public class TimeSheetDayEntity
{
    public string? StartDatetimeLocal { get; set; }
    public string? StopDatetimeLocal { get; set; }
    public string? UserId { get; set; }
    public TimesheetSummaryEntity? Summary { get; set; }
    public TimeSheetEventEntity[] Events { get; set; }
}