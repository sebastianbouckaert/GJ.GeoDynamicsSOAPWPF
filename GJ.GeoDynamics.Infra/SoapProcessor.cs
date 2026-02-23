using System.Net.Http.Headers;
using System.Text;
using GJ.GeoDynamics.Domain;
using Microsoft.Extensions.Logging;

namespace GJ.GeoDynamics.Infra;

public class SoapClient : ISoapClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SoapClient> _logger;

    public SoapClient(ILogger<SoapClient> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<string> SendSoapRequestAsync(string endpoint, string soapAction, string soapEnvelope)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            request.Headers.Add("SOAPAction", $"\"{soapAction}\"");

            request.Content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("text/xml")
            {
                CharSet = "utf-8"
            };

            using var response = await _httpClient.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("SOAP request failed: HTTP {StatusCode} {ReasonPhrase}\n{Body}",
                    (int)response.StatusCode, response.ReasonPhrase, body);
                throw new HttpRequestException($"HTTP {(int)response.StatusCode} {response.ReasonPhrase}\n{body}");
            }

            return body;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending SOAP request to {Endpoint} with action {SoapAction}", endpoint, soapAction);
            throw;
        }
    }
}