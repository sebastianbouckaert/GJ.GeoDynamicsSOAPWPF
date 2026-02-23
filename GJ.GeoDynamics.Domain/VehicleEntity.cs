namespace GJ.GeoDynamics.Domain;

public class VehicleEntity
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? LastSyncDateUtc { get; set; }
    public string? VehicleTypeId { get; set; }
    public string? Id { get; set; }
    public LastPositionEntity? LastPosition { get; set; }
}