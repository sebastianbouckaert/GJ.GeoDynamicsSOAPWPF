namespace GJ.GeoDynamics.Domain;

public class PoiEntity
{
    public string? Code { get; set; }
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Priority { get; set; }
    public AddressEntity? Address { get; set; }
}