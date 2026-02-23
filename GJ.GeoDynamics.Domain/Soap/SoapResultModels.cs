using System.Xml.Serialization;

namespace GJ.GeoDynamics.Domain;

public class User_GetAllResult
{
    [XmlElement("UserEntity")] public List<UserEntity> Users { get; set; } = new();
}

public class Vehicle_GetAllResult
{
    [XmlElement("VehicleEntity")] public List<VehicleEntity> Vehicles { get; set; } = new();
}

public class Poi_GetAllResult
{
    [XmlElement("PoiEntity")] public List<PoiEntity> Pois { get; set; } = new();
}

public class Clocking_GetAllResult
{
    [XmlElement("ClockingEntity")] public List<ClockingEntity> Clockings { get; set; } = new();
}

public class Locations_GetAllResponse
{
    [XmlElement("LocationEntity")] public List<LocationEntity> Locations { get; set; } = new();
}