using System.Xml.Linq;
using GJ.GeoDynamics.Domain;

namespace GJ.GeoDynamics.Common;

public static class SoapEnvelopeBuilder
{
    public static string BuildUserGetAllEnvelope(CallerEntity caller)
    {
        var soapEnv = XNamespace.Get(SoapConfiguration.SoapEnvelopeNamespace);
        var web = XNamespace.Get(SoapConfiguration.WebServiceNamespace);

        var envelope = new XElement(soapEnv + SoapConfiguration.Elements.Envelope,
            new XAttribute(XNamespace.Xmlns + SoapConfiguration.Prefixes.SoapEnv, SoapConfiguration.SoapEnvelopeNamespace),
            new XAttribute(XNamespace.Xmlns + SoapConfiguration.Prefixes.Web, SoapConfiguration.WebServiceNamespace),
            new XElement(soapEnv + SoapConfiguration.Elements.Header),
            new XElement(soapEnv + SoapConfiguration.Elements.Body,
                new XElement(web + SoapConfiguration.Methods.UserGetAll,
                    CreateCallerElement(web, caller)
                )
            )
        );

        return CreateXmlDocument(envelope);
    }

    public static string BuildVehicleGetAllEnvelope(CallerEntity caller)
    {
        var soapEnv = XNamespace.Get(SoapConfiguration.SoapEnvelopeNamespace);
        var web = XNamespace.Get(SoapConfiguration.WebServiceNamespace);

        var envelope = new XElement(soapEnv + SoapConfiguration.Elements.Envelope,
            new XAttribute(XNamespace.Xmlns + SoapConfiguration.Prefixes.SoapEnv, SoapConfiguration.SoapEnvelopeNamespace),
            new XAttribute(XNamespace.Xmlns + SoapConfiguration.Prefixes.Web, SoapConfiguration.WebServiceNamespace),
            new XElement(soapEnv + SoapConfiguration.Elements.Header),
            new XElement(soapEnv + SoapConfiguration.Elements.Body,
                new XElement(web + SoapConfiguration.Methods.VehicleGetAll,
                    CreateCallerElement(web, caller)
                )
            )
        );

        return CreateXmlDocument(envelope);
    }

    public static string BuildPoiGetAllEnvelope(CallerEntity caller)
    {
        var soapEnv = XNamespace.Get(SoapConfiguration.SoapEnvelopeNamespace);
        var web = XNamespace.Get(SoapConfiguration.WebServiceNamespace);

        var envelope = new XElement(soapEnv + SoapConfiguration.Elements.Envelope,
            new XAttribute(XNamespace.Xmlns + SoapConfiguration.Prefixes.SoapEnv, SoapConfiguration.SoapEnvelopeNamespace),
            new XAttribute(XNamespace.Xmlns + SoapConfiguration.Prefixes.Web, SoapConfiguration.WebServiceNamespace),
            new XElement(soapEnv + SoapConfiguration.Elements.Header),
            new XElement(soapEnv + SoapConfiguration.Elements.Body,
                new XElement(web + SoapConfiguration.Methods.PoiGetAll,
                    CreateCallerElement(web, caller)
                )
            )
        );

        return CreateXmlDocument(envelope);
    }

    public static string BuildClockingGetByDateRangeEnvelope(CallerEntity caller, DateTime startDate, DateTime endDate)
    {
        var soapEnv = XNamespace.Get(SoapConfiguration.SoapEnvelopeNamespace);
        var web = XNamespace.Get(SoapConfiguration.WebServiceNamespace);

        var envelope = new XElement(soapEnv + SoapConfiguration.Elements.Envelope,
            new XAttribute(XNamespace.Xmlns + SoapConfiguration.Prefixes.SoapEnv, SoapConfiguration.SoapEnvelopeNamespace),
            new XAttribute(XNamespace.Xmlns + SoapConfiguration.Prefixes.Web, SoapConfiguration.WebServiceNamespace),
            new XElement(soapEnv + SoapConfiguration.Elements.Header),
            new XElement(soapEnv + SoapConfiguration.Elements.Body,
                new XElement(web + SoapConfiguration.Methods.ClockingGetByDateRangeUtc,
                    CreateCallerElement(web, caller),
                    new XElement(web + SoapConfiguration.Parameters.FromDateUtc, startDate.ToUniversalTime().ToString(SoapConfiguration.DateTimeFormat)),
                    new XElement(web + SoapConfiguration.Parameters.ToDateUtc, endDate.ToUniversalTime().ToString(SoapConfiguration.DateTimeFormat))
                )
            )
        );

        return CreateXmlDocument(envelope);
    }

    public static string BuildLocationGetByVehicleIdDateRangeEnvelope(CallerEntity caller, string vehicleId, DateTime startDate, DateTime endDate)
    {
        var soapEnv = XNamespace.Get(SoapConfiguration.SoapEnvelopeNamespace);
        var web = XNamespace.Get(SoapConfiguration.WebServiceNamespace);

        var envelope = new XElement(soapEnv + SoapConfiguration.Elements.Envelope,
            new XAttribute(XNamespace.Xmlns + SoapConfiguration.Prefixes.SoapEnv, SoapConfiguration.SoapEnvelopeNamespace),
            new XAttribute(XNamespace.Xmlns + SoapConfiguration.Prefixes.Web, SoapConfiguration.WebServiceNamespace),
            new XElement(soapEnv + SoapConfiguration.Elements.Header),
            new XElement(soapEnv + SoapConfiguration.Elements.Body,
                new XElement(web + SoapConfiguration.Methods.LocationGetByVehicleIdDateRange,
                    CreateCallerElement(web, caller),
                    new XElement(web + SoapConfiguration.Parameters.VehicleId, vehicleId),
                    new XElement(web + SoapConfiguration.Parameters.FromDateUtc, startDate.ToString(SoapConfiguration.DateTimeFormat)),
                    new XElement(web + SoapConfiguration.Parameters.ToDateUtc, endDate.ToString(SoapConfiguration.DateTimeFormat))
                )
            )
        );

        return CreateXmlDocument(envelope);
    }

    public static string BuildTimeSheetGetByUserIdListDateRange(CallerEntity caller, List<string> ids, DateTime startDate, DateTime endDate)
    {
        var soapEnv = XNamespace.Get(SoapConfiguration.SoapEnvelopeNamespace);
        var web = XNamespace.Get(SoapConfiguration.WebServiceNamespace);

        var envelope = new XElement(soapEnv + SoapConfiguration.Elements.Envelope,
            new XAttribute(XNamespace.Xmlns + SoapConfiguration.Prefixes.SoapEnv, SoapConfiguration.SoapEnvelopeNamespace),
            new XAttribute(XNamespace.Xmlns + SoapConfiguration.Prefixes.Web, SoapConfiguration.WebServiceNamespace),
            new XElement(soapEnv + SoapConfiguration.Elements.Header),
            new XElement(soapEnv + SoapConfiguration.Elements.Body,
                new XElement(web + SoapConfiguration.Methods.TimeSheetGetByUserIdListDateRange,
                    CreateCallerElement(web, caller),
                    new XElement(web + SoapConfiguration.Parameters.UserIdList,
                        ids.Select(id => new XElement(web + SoapConfiguration.Parameters.Guid, id))
                    ),
                    new XElement(web + SoapConfiguration.Parameters.FromDateUtc, startDate.ToString(SoapConfiguration.DateTimeFormat)),
                    new XElement(web + SoapConfiguration.Parameters.ToDateUtc, endDate.ToString(SoapConfiguration.DateTimeFormat))
                )
            )
        );

        return CreateXmlDocument(envelope);
    }


    private static XElement CreateCallerElement(XNamespace web, CallerEntity caller)
    {
        return new XElement(web + SoapConfiguration.Elements.Caller,
            new XElement(web + SoapConfiguration.Elements.CompanyName, caller.CompanyName),
            new XElement(web + SoapConfiguration.Elements.LoginName, caller.LoginName),
            new XElement(web + SoapConfiguration.Elements.Password, caller.Password)
        );
    }

    private static string CreateXmlDocument(XElement envelope)
    {
        return new XDeclaration(SoapConfiguration.XmlVersion, SoapConfiguration.XmlEncoding, null) + Environment.NewLine + envelope;
    }
}