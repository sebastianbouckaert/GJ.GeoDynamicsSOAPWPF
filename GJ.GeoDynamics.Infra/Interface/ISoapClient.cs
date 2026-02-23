namespace GJ.GeoDynamics.Domain;

public interface ISoapClient
{
    Task<string> SendSoapRequestAsync(string endpoint, string soapAction, string soapEnvelope);
}