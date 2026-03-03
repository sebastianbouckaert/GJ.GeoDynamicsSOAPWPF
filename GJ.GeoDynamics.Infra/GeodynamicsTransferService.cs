using System.Data;
using System.ServiceModel;
using System.Text;
using GJ.GeoDynamics.Common;
using GJ.GeoDynamics.Domain;
using GJ.GeoDynamics.Infra.Interface;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GeoDynamics.Functions;

public class GeodynamicsTransferService : IGeodynamicsTransferService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<GeodynamicsTransferService> _logger;
    private readonly IDatabaseRepository _repository;
    private readonly ISoapClient _soapClient;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPoiRepository _poiRepository;
    private readonly IClockingRepository _clockingRepository;
    private readonly IVehicleQueryRepository _vehicleQueryRepository;
    private readonly ILocationRepository _locationRepository;
    private readonly IUserQueryRepository _userQueryRepository;
    private readonly ITripOverviewRepository _tripOverviewRepository;


    public GeodynamicsTransferService(
        ILogger<GeodynamicsTransferService> logger,
        IDatabaseRepository repository,
        IConfiguration configuration,
        ISoapClient soapClient,
        IVehicleRepository vehicleRepository,
        IUserRepository userRepository,
        IPoiRepository poiRepository,
        IClockingRepository clockingRepository,
        IVehicleQueryRepository vehicleQueryRepository,
        ILocationRepository locationRepository,
        IUserQueryRepository userQueryRepository,
        ITripOverviewRepository tripOverviewRepository)
    {
        _logger = logger;
        _repository = repository;
        _configuration = configuration;
        _soapClient = soapClient;
        _vehicleRepository = vehicleRepository;
        _userRepository = userRepository;
        _poiRepository = poiRepository;
        _clockingRepository = clockingRepository;
        _vehicleQueryRepository = vehicleQueryRepository;
        _locationRepository = locationRepository;
        _userQueryRepository = userQueryRepository;
        _tripOverviewRepository = tripOverviewRepository;
    }

    public async Task TransferAllForce(DateTime startDate, DateTime endDate)
    {
        try
        {
            var currentEndDate = new DateTime(endDate.Year, endDate.Month, endDate.Day, 23, 59, 59);

            _logger.LogInformation("Starting transfer for {Date}", startDate.ToShortDateString());
            await Info_TripOverview_Custom(startDate, currentEndDate);
            await TransferAllUsersToTable();
            await TransferAllVehiclesToTable();
            await TransferAllPoiToTable();
            await TransferAllClockingsToTable(startDate, currentEndDate);
            await TransferLocations(startDate, currentEndDate);
         //  await Info_TripOverview_Custom(startDate, currentEndDate);

            _logger.LogInformation("Finished transfer for {Date}", startDate.ToShortDateString());
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Error during TransferAllForce: {Message}", ex.Message);
            throw;
        }
    }

    private CallerEntity GetCaller()
    {
        var c = new CallerEntity
        {
            CompanyName = _configuration["Geodynamics:CompanyName"] ?? "Jansen IS",
            LoginName = _configuration["Geodynamics:LoginName"] ?? "webservices",
            Password = _configuration["Geodynamics:Password"] ?? "aanmelden"
        };

        return c;
    }

    private IntegratorWebserviceSoapClient GetClient()
    {
        var httpBinding = new BasicHttpBinding
        {
            MaxReceivedMessageSize = int.MaxValue,
            MaxBufferSize = int.MaxValue,
            MessageEncoding = WSMessageEncoding.Text,
            TextEncoding = Encoding.UTF8
        };

        var uri = "http://secure.geodynamics.be/webservices/intellitracer/1.0/IntegratorWebservice.asmx";
        var endpoint = new EndpointAddress(new Uri(uri));

        var client = new IntegratorWebserviceSoapClient(httpBinding, endpoint);

        return client;
    }

    private async Task TransferAllUsersToTable()
    {
        var responseXml = await _soapClient.SendSoapRequestAsync(
            SoapConfiguration.Baseuri,
            SoapConfiguration.FullMethodLinks.UserGetAll,
            SoapEnvelopeBuilder.BuildUserGetAllEnvelope(GetCaller()));

        var users = IntelliTracerSoapParser.DeserializeUsers(responseXml);

        await _userRepository.ReplaceAllAsync(users);

        Console.WriteLine("TransferAllUsersToTable - Done!");
    }

    private async Task TransferAllVehiclesToTable()
    {
        var responseXml = await _soapClient.SendSoapRequestAsync(SoapConfiguration.Baseuri,
            SoapConfiguration.FullMethodLinks.VehicleGetAll,
            SoapEnvelopeBuilder.BuildVehicleGetAllEnvelope(GetCaller()));
        var vehicles = IntelliTracerSoapParser.DeserializeVehicle(responseXml);
        await _vehicleRepository.ReplaceAllAsync(vehicles);
        Console.WriteLine("TransferAllVehiclesToTable - Done!");
    }

    private async Task TransferAllPoiToTable()
    {
        var responseXml = await _soapClient.SendSoapRequestAsync(SoapConfiguration.Baseuri,
            SoapConfiguration.FullMethodLinks.PoiGetAll, SoapEnvelopeBuilder.BuildPoiGetAllEnvelope(GetCaller()));
        var pois = IntelliTracerSoapParser.DeserializePoi(responseXml);
        await _poiRepository.ReplaceAllAsync(pois);

        Console.WriteLine("TransferAllPoiToTable - Done!");
    }

    private async Task TransferAllClockingsToTable(DateTime startDate, DateTime endDate)
    {
        var responseXml = await _soapClient.SendSoapRequestAsync(SoapConfiguration.Baseuri,
            SoapConfiguration.FullMethodLinks.ClockingGetByDateRangeUtc,
            SoapEnvelopeBuilder.BuildClockingGetByDateRangeEnvelope(GetCaller(), startDate, endDate));

        var clockings = IntelliTracerSoapParser.DeserializeClockings(responseXml);

        await _clockingRepository.InsertBatchAndSnapshotAsync(clockings);

        Console.WriteLine("TransferAllClockingsToTable - Done!");
    }

    private async Task TransferLocations(DateTime startDate, DateTime endDate)
    {
        try
        {
            await _locationRepository.RefreshSnapshotAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error copying table JS_Info_Location_GetByVehicleIdListDateRange");
            throw;
        }
        
        
        var vehicleIds = await _vehicleQueryRepository.GetAllVehicleIdsAsync();

        foreach (var vehicleId in vehicleIds)
        {
            try
            {
                var envelope =
                    SoapEnvelopeBuilder.BuildLocationGetByVehicleIdDateRangeEnvelope(GetCaller(), vehicleId, startDate,
                        endDate);

                var responseXml = await _soapClient.SendSoapRequestAsync(SoapConfiguration.Baseuri,
                    SoapConfiguration.FullMethodLinks.LocationGetByVehicleIdDateRange, envelope);

                var locations = IntelliTracerSoapParser.DeserializeLocations(responseXml);
                await _locationRepository.InsertBatchAsync(locations);

                Console.WriteLine($"Inserted {locations.Count} locations for vehicle {vehicleId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error transferring locations for vehicle {VehicleId}", vehicleId);
            }
        }

        try
        {
            await _locationRepository.RefreshSnapshotAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error copying table JS_Info_Location_GetByVehicleIdListDateRange");
            throw;
        }
        finally
        {
            Console.WriteLine("TransferLocations - Done!");
        }
    }

    private async Task Info_TripOverview_Custom(DateTime startDate, DateTime endDate)
    {
        var userGuidStrings = await _userQueryRepository.GetAllUserGuidsAsync();

        if (userGuidStrings.Count == 0) return;

        var userGuids = new ArrayOfGuid();
        foreach (var g in userGuidStrings)
            userGuids.Add(g);

        var envelope =
            SoapEnvelopeBuilder.BuildTimeSheetGetByUserIdListDateRange(GetCaller(), userGuids, startDate, endDate);
        var responseXml = await _soapClient.SendSoapRequestAsync(SoapConfiguration.Baseuri,
            SoapConfiguration.FullMethodLinks.TimeSheetGetByUserIdListDateRange, envelope);
        var overviews = IntelliTracerSoapParser.DeserializeTimesheetDayEntities(responseXml);
        if (overviews == null || overviews.Count == 0) return;

        await _tripOverviewRepository.RefreshSnapshotAsync();
        await _tripOverviewRepository.InsertFromOverviewsAsync(overviews);


        Console.WriteLine("Info_TripOverview_Custom - Done!");
    }


    private string ReturnAddressString(AddressEntity address)
    {
        if (address == null) return "";

        var result = $"{address.Street} {address.HouseNumber}, {address.PostalCode} {address.City} ({address.Country})";

        return result.Replace("'", "''").Trim();
    }
}