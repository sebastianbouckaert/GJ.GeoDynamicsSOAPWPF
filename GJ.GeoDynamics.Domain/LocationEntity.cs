namespace GJ.GeoDynamics.Domain;

public class LocationEntity
{
    public AddressEntity? AddressEntity { get; set; }
    public string[] Pois { get; set; }
    public string? BadgeUser { get; set; }
    public string? BadgeNr { get; set; }
    public DateTime? GpsDateUtc { get; set; }
    public string? Heading { get; set; }
    public DateTime ReportDateUtc { get; set; }
    public string? Speed { get; set; }
    public string? VehicleId { get; set; }
    public AddressEntity? Address { get; set; }
}