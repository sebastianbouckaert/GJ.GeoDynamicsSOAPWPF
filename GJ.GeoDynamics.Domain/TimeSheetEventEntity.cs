namespace GJ.GeoDynamics.Domain;

public class TimeSheetEventEntity
{
    public string? IsCostCalculation { get; set; }
    public string? IsLoadUnload { get; set; }
    public string? JobNumber { get; set; }
    public string? Mileage { get; set; }
    public string? MileageBirdFlight { get; set; }
    public object[] PoisAtStartLocation { get; set; }
    public object[] PoisAtStopLocation { get; set; }
    public string? RoundedTotalWork { get; set; }
    public string? StartDateTimeLocal { get; set; }
    public string? StopDateTimeLocal { get; set; }
    public LocationEntity StartLocation { get; set; }
    public LocationEntity StopLocation { get; set; }
    public string StartVehicleId { get; set; }
    public string StopVehicleId { get; set; }
    public string Type { get; set; }
}