using System.Xml.Serialization;
using GJ.GeoDynamics.Common;

namespace GJ.GeoDynamics.Domain;

public static class IntelliTracerSoapParser
{
    public static List<UserEntity> DeserializeUsers(string soapXml)
    {
        var serializer = new XmlSerializer(typeof(SoapEnvelope<UserSoapBody>));

        using var sr = new StringReader(soapXml);
        var envelope = (SoapEnvelope<UserSoapBody>?)serializer.Deserialize(sr);

        return envelope?.Body?.Response?.Result?.Users ?? new List<UserEntity>();
    }

    public static List<VehicleEntity> DeserializeVehicle(string soapXml)
    {
        var serializer = new XmlSerializer(typeof(SoapEnvelope<VehicleSoapBody>));
        using var sr = new StringReader(soapXml);
        var envelope = (SoapEnvelope<VehicleSoapBody>?)serializer.Deserialize(sr);
        return envelope?.Body?.Response?.Result?.Vehicles ?? new List<VehicleEntity>();
    }

    public static List<LocationEntity> DeserializeLocations(string soapXml)
    {
        var serializer = new XmlSerializer(typeof(SoapEnvelope<LocationsSoapBody>));
        using var sr = new StringReader(soapXml);
        var envelope = (SoapEnvelope<LocationsSoapBody>?)serializer.Deserialize(sr);
        return envelope?.Body?.Response?.Result?.Locations ?? new List<LocationEntity>();
    }

    public static List<PoiEntity> DeserializePoi(string soapXml)
    {
        var serializer = new XmlSerializer(typeof(SoapEnvelope<PoiSoapBody>));
        using var sr = new StringReader(soapXml);
        var envelope = (SoapEnvelope<PoiSoapBody>?)serializer.Deserialize(sr);
        return envelope?.Body?.Response?.Result?.Pois ?? new List<PoiEntity>();
    }

    public static List<ClockingEntity> DeserializeClockings(string soapXml)
    {
        var serializer = new XmlSerializer(typeof(SoapEnvelope<ClockingSoapBody>));

        using var sr = new StringReader(soapXml);
        var envelope = (SoapEnvelope<ClockingSoapBody>?)serializer.Deserialize(sr);

        return envelope?.Body?.Response?.Result?.Clockings ?? new List<ClockingEntity>();
    }

    public static List<TimeSheetDayEntity> DeserializeTimesheetDayEntities(string soapXml)
    {
        var serializer = new XmlSerializer(typeof(SoapEnvelope<TimesheetSoapBody>));

        using var sr = new StringReader(soapXml);
        var envelope = (SoapEnvelope<TimesheetSoapBody>?)serializer.Deserialize(sr);

        return envelope?.Body?.Response?.Result?.Days ?? new List<TimeSheetDayEntity>();
    }

    [XmlRoot("Envelope", Namespace = SoapConfiguration.SoapEnvelopeNamespace)]
    public class SoapEnvelope<TBody>
    {
        [XmlElement("Body", Namespace = SoapConfiguration.SoapEnvelopeNamespace)]
        public TBody? Body { get; set; }
    }

    public class UserSoapBody
    {
        [XmlElement("User_GetAllResponse", Namespace = SoapConfiguration.WebServiceNamespace)]
        public UserGetAllResponse? Response { get; set; }
    }

    public class VehicleSoapBody
    {
        [XmlElement("Vehicle_GetAllResponse", Namespace = SoapConfiguration.WebServiceNamespace)]
        public VehicleGetAllResponse? Response { get; set; }
    }

    public class PoiSoapBody
    {
        [XmlElement("Poi_GetAllResponse", Namespace = SoapConfiguration.WebServiceNamespace)]
        public PoiGetAllResponse? Response { get; set; }
    }

    public class ClockingSoapBody
    {
        // IMPORTANT: this must match the SOAP body element name in your XML:
        // <Clocking_GetByDateRangeUtcResponse xmlns="http://www.geodynamics.be/webservices">
        [XmlElement("Clocking_GetByDateRangeUtcResponse", Namespace = SoapConfiguration.WebServiceNamespace)]
        public ClockingGetByDateRangeUtcResponse? Response { get; set; }
    }

    public class ClockingGetByDateRangeUtcResponse
    {
        // IMPORTANT: must match <Clocking_GetByDateRangeUtcResult>
        [XmlElement("Clocking_GetByDateRangeUtcResult", Namespace = SoapConfiguration.WebServiceNamespace)]
        public ClockingGetByDateRangeUtcResult? Result { get; set; }
    }

    public class ClockingGetByDateRangeUtcResult
    {
        [XmlElement("ClockingEntity", Namespace = SoapConfiguration.WebServiceNamespace)]
        public List<ClockingEntity> Clockings { get; set; } = new();
    }

    public class LocationsSoapBody
    {
        [XmlElement("Location_GetByVehicleIdDateRangeResponse", Namespace = SoapConfiguration.WebServiceNamespace)]
        public LocationsGetAllResponse? Response { get; set; }
    }

    public class LocationsGetAllResponse
    {
        [XmlElement("Location_GetByVehicleIdDateRangeResult")]
        public LocationsGetAllResult? Result { get; set; }
    }

    public class LocationsGetAllResult
    {
        [XmlElement("LocationEntity")] public List<LocationEntity> Locations { get; set; } = new();
    }

    public class TimesheetSoapBody
    {
        [XmlElement("TimeSheet_GetByUserIdListDateRangeResponse", Namespace = SoapConfiguration.WebServiceNamespace)]
        public TimesheetGetByUserIdListResponse? Response { get; set; }
    }

    public class TimesheetGetByUserIdListResponse
    {
        [XmlElement("TimeSheet_GetByUserIdListDateRangeResult")]
        public TimesheetGetByUserIdListResult? Result { get; set; }
    }

    public class TimesheetGetByUserIdListResult
    {
        [XmlElement("TimeSheetDayEntity")] public List<TimeSheetDayEntity> Days { get; set; } = new();
    }
}