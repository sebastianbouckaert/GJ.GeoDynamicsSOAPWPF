namespace GJ.GeoDynamics.Domain;

public class ClockingEntity
{
    public string? DateTimeLocal { get; set; }
    public string? Description { get; set; }
    public string? JobNumber { get; set; }
    public LocationEntity? Location { get; set; }
    public string[]? Pois { get; set; }
    public VehicleEntity? Vehicle { get; set; }
    public UserEntity? User { get; set; }
    public string? Type { get; set; }
}