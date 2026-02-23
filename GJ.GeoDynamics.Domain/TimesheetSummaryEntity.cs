namespace GJ.GeoDynamics.Domain;

public  class TimesheetSummaryEntity
{
    public string AmountMobilityDriver { get; set; }
    public string AmountMobilityHomeWork { get; set; }
    public string AmountMobilityPassenger { get; set; }
    public string MileageMobilityBirdFlightDriver { get; set; }
    public string MileageMobilityDrivenDriver { get; set; }
    public string MileageMobilityDrivenPassenger { get; set; }
    public string MileageMobilityHomeWork { get; set; }
    public string NormalHours { get; set; }
    public string Pause { get; set; }
    public string TotalHours { get; set; }
    public string TotalLoad { get; set; }
}