using System.Xml.Serialization;
using GJ.GeoDynamics.Common;

namespace GJ.GeoDynamics.Domain;

[XmlType(Namespace = SoapConfiguration.WebServiceNamespace)]
public partial class UserGetAllResponse
{
    [XmlElement("User_GetAllResult")] public User_GetAllResult? Result { get; set; }
}

[XmlType(Namespace = SoapConfiguration.WebServiceNamespace)]
public partial class VehicleGetAllResponse
{
    [XmlElement("Vehicle_GetAllResult")] public Vehicle_GetAllResult? Result { get; set; }
}

[XmlType(Namespace = SoapConfiguration.WebServiceNamespace)]
public class PoiGetAllResponse
{
    [XmlElement("Poi_GetAllResult")] public Poi_GetAllResult? Result { get; set; }
}

[XmlType(Namespace = SoapConfiguration.WebServiceNamespace)]
public class ClockingGetAllResponse
{
    [XmlElement("Clocking_GetAllResult")] public Clocking_GetAllResult? Result { get; set; }
}

[XmlType(Namespace = SoapConfiguration.WebServiceNamespace)]
public class LocationsGetAllResponse
{
    [XmlElement("Locations_GetAllResponse")]
    public Locations_GetAllResponse? Result { get; set; }
}