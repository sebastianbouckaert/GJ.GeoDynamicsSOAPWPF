namespace GJ.GeoDynamics.Common;

public static class SoapConfiguration
{
    public const string Baseuri = "http://secure.geodynamics.be/webservices/intellitracer/1.0/IntegratorWebservice.asmx";

    public const string SoapEnvelopeNamespace = "http://schemas.xmlsoap.org/soap/envelope/";
    public const string WebServiceNamespace = "http://www.geodynamics.be/webservices";

    public const string XmlVersion = "1.0";
    public const string XmlEncoding = "utf-8";
    public const string DateTimeFormat = "yyyy-MM-ddTHH:mm:ss";

    public static class Elements
    {
        public const string Envelope = "Envelope";
        public const string Header = "Header";
        public const string Body = "Body";
        public const string Caller = "caller";
        public const string CompanyName = "CompanyName";
        public const string LoginName = "LoginName";
        public const string Password = "Password";
    }

    public static class Methods
    {
        public const string UserGetAll = "User_GetAll";
        public const string VehicleGetAll = "Vehicle_GetAll";
        public const string PoiGetAll = "Poi_GetAll";
        public const string ClockingGetByDateRangeUtc = "Clocking_GetByDateRangeUtc";
        public const string LocationGetByVehicleIdDateRange = "Location_GetByVehicleIdDateRange";
        public const string TimeSheetGetByUserIdListDateRange = "TimeSheet_GetByUserIdListDateRange";
    }

    public static class FullMethodLinks
    {
        public const string UserGetAll = "http://www.geodynamics.be/webservices/User_GetAll";
        public const string VehicleGetAll = "http://www.geodynamics.be/webservices/Vehicle_GetAll";
        public const string PoiGetAll = "http://www.geodynamics.be/webservices/Poi_GetAll";
        public const string ClockingGetByDateRangeUtc = "http://www.geodynamics.be/webservices/Clocking_GetByDateRangeUtc";
        public const string LocationGetByVehicleIdDateRange = "http://www.geodynamics.be/webservices/Location_GetByVehicleIdDateRange";
        public const string TimeSheetGetByUserIdListDateRange = "http://www.geodynamics.be/webservices/TimeSheet_GetByUserIdListDateRange";
    }


    public static class Parameters
    {
        public const string FromDateUtc = "fromDateUtc";
        public const string ToDateUtc = "toDateUtc";
        public const string StartDateUtc = "startDateUtc";
        public const string EndDateUtc = "endDateUtc";
        public const string VehicleId = "vehicleId";
        public const string UserIdList = "userIdList";
        public const string Guid = "guid";
    }

    public static class Prefixes
    {
        public const string SoapEnv = "soapenv";
        public const string Web = "web";
    }
}