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

    public GeodynamicsTransferService(ILogger<GeodynamicsTransferService> logger, IDatabaseRepository repository, IConfiguration configuration, ISoapClient soapClient)
    {
        _logger = logger;
        _repository = repository;
        _configuration = configuration;
        _soapClient = soapClient;
    }

    public async Task TransferAllForce(DateTime startDate, DateTime endDate)
    {
        try
        {
            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                var currentStartDate = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);
                var currentEndDate = new DateTime(date.Year, date.Month, date.Day, 23, 59, 59);

                _logger.LogInformation("Starting transfer for {Date}", currentStartDate.ToShortDateString());

                await TransferAllUsersToTable();
                await TransferAllVehiclesToTable();
                await TransferAllPoiToTable();
                await TransferAllClockingsToTable(currentStartDate, currentEndDate);
                await TransferLocations(currentStartDate, currentEndDate);
                await Info_TripOverview_Custom(currentStartDate, currentEndDate);

                _logger.LogInformation("Finished transfer for {Date}", currentStartDate.ToShortDateString());
            }
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Error during TransferAllForce: {Message}", ex.Message);
            throw;
        }
    }

    private CallerEntity GetCaller()
    {
        var c= new CallerEntity
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
        try
        {
            var responseXml = await _soapClient.SendSoapRequestAsync(SoapConfiguration.Baseuri, SoapConfiguration.FullMethodLinks.UserGetAll, SoapEnvelopeBuilder.BuildUserGetAllEnvelope(GetCaller()));
            var users = IntelliTracerSoapParser.DeserializeUsers(responseXml);

            await _repository.ClearTable("Info_Users");
            var counter = 0;
            foreach (var user in users)
            {
                const string sql = @"INSERT INTO Info_Users (Naam, badge, code, GUID, NavCode) VALUES (@Name, @Badge, @Code, @GUID, @NavCode)";

                var nameStr = user.Name;
                var parameters = new[]
                {
                    new SqlParameter("@Name", SqlDbType.NVarChar, 50) { Value = nameStr.Length > 50 ? nameStr.Substring(0, 50) : nameStr },
                    new SqlParameter("@Badge", SqlDbType.NVarChar) { Value = user.Badge?.InternalNumber ?? "" },
                    new SqlParameter("@Code", SqlDbType.NVarChar) { Value = user.Code ?? "" },
                    new SqlParameter("@GUID", SqlDbType.NVarChar) { Value = user.Id ?? "" },
                    new SqlParameter("@NavCode", SqlDbType.NVarChar) { Value = user.EmployerCode ?? "" }
                };

                await _repository.ExecuteNonQueryAsync(sql, parameters);
                counter++;
            }

            await _repository.ExecuteSqlCmd("IF OBJECT_ID('JS_Info_Users', 'U') IS NOT NULL DROP TABLE JS_Info_Users;");
            await _repository.ExecuteSqlCmd("SELECT * INTO JS_Info_Users FROM Info_Users;");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error copying table JS_Info_Users");
            throw;
        }
        finally
        {
            Console.WriteLine("TransferAllUsersToTable - Done!");
        }
    }

    private async Task TransferAllVehiclesToTable()
    {
        var counter = 0;
        var responseXml = await _soapClient.SendSoapRequestAsync(SoapConfiguration.Baseuri, SoapConfiguration.FullMethodLinks.VehicleGetAll, SoapEnvelopeBuilder.BuildVehicleGetAllEnvelope(GetCaller()));
        var vehicles = IntelliTracerSoapParser.DeserializeVehicle(responseXml);

        await _repository.ClearTable("Info_Vehicles");

        foreach (var vehicle in vehicles)
        {
            const string sql = @"INSERT INTO Info_Vehicles (Code, Name, LastSyncTime, VehicleTypeId, VehicleId, LastPosition) VALUES (@Code, @Name, @LastSyncTime, @VehicleTypeId, @VehicleId, @LastPosition)";
            var nameStr = vehicle.Name ?? "";
            var parameters = new[]
            {
                new SqlParameter("@Code", SqlDbType.NVarChar) { Value = vehicle.Code ?? "" },
                new SqlParameter("@Name", SqlDbType.NVarChar, 50) { Value = nameStr.Length > 50 ? nameStr.Substring(0, 49) : nameStr },
                new SqlParameter("@LastSyncTime", SqlDbType.NVarChar) { Value = vehicle.LastSyncDateUtc ?? "" },
                new SqlParameter("@VehicleTypeId", SqlDbType.NVarChar) { Value = vehicle.VehicleTypeId ?? "" },
                new SqlParameter("@VehicleId", SqlDbType.NVarChar) { Value = vehicle.Id ?? "" },
                new SqlParameter("@LastPosition", SqlDbType.NVarChar) { Value = vehicle.LastPosition?.AddressEntity == null ? "" : ReturnAddressString(vehicle.LastPosition.AddressEntity) }
            };

            await _repository.ExecuteNonQueryAsync(sql, parameters);
        }

        try
        {
            await _repository.ExecuteSqlCmd("IF OBJECT_ID('JS_Info_Vehicles', 'U') IS NOT NULL DROP TABLE JS_Info_Vehicles;");
            await _repository.ExecuteSqlCmd("SELECT * INTO JS_Info_Vehicles FROM Info_Vehicles;");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error copying table JS_Info_Vehicles");
            throw;
        }
        finally
        {
            Console.WriteLine("TransferAllVehiclesToTable - Done!");
        }
    }

    private async Task TransferAllPoiToTable()
    {
        var counter = 0;
        var responseXml = await _soapClient.SendSoapRequestAsync(SoapConfiguration.Baseuri, SoapConfiguration.FullMethodLinks.PoiGetAll, SoapEnvelopeBuilder.BuildPoiGetAllEnvelope(GetCaller()));
        var pois = IntelliTracerSoapParser.DeserializePoi(responseXml);

        await _repository.ClearTable("Info_Poi");

        foreach (var poi in pois)
        {
            const string sql = @"
                    INSERT INTO Info_Poi (Code, GuidPoi, Name, Priority, Address, Address_City, Address_Country, Address_HouseNumber, Address_PostalCode, Address_street, Address_Submunicipality)
                    VALUES (@Code, @GuidPoi, @Name, @Priority, @Address, @Address_City, @Address_Country, @Address_HouseNumber, @Address_PostalCode, @Address_street, @Address_Submunicipality)";

            var parameters = new[]
            {
                new SqlParameter("@Code", SqlDbType.NVarChar) { Value = poi.Code ?? "" },
                new SqlParameter("@GuidPoi", SqlDbType.NVarChar) { Value = poi.Id ?? "" },
                new SqlParameter("@Name", SqlDbType.NVarChar) { Value = poi.Name ?? "" },
                new SqlParameter("@Priority", SqlDbType.NVarChar) { Value = poi.Priority ?? "" },
                new SqlParameter("@Address", SqlDbType.NVarChar) { Value = poi.Address == null ? "" : ReturnAddressString(poi.Address) },
                new SqlParameter("@Address_City", SqlDbType.NVarChar) { Value = poi.Address?.City ?? "" },
                new SqlParameter("@Address_Country", SqlDbType.NVarChar) { Value = poi.Address?.Country ?? "" },
                new SqlParameter("@Address_HouseNumber", SqlDbType.NVarChar) { Value = poi.Address?.HouseNumber ?? "" },
                new SqlParameter("@Address_PostalCode", SqlDbType.NVarChar) { Value = poi.Address?.PostalCode ?? "" },
                new SqlParameter("@Address_street", SqlDbType.NVarChar) { Value = poi.Address?.Street ?? "" },
                new SqlParameter("@Address_Submunicipality", SqlDbType.NVarChar) { Value = poi.Address?.Submunicipality ?? "" }
            };

            await _repository.ExecuteNonQueryAsync(sql, parameters);
        }

        try
        {
            await _repository.ExecuteSqlCmd("IF OBJECT_ID('JS_Info_Poi', 'U') IS NOT NULL DROP TABLE JS_Info_Poi;");
            await _repository.ExecuteSqlCmd("SELECT * INTO JS_Info_Poi FROM Info_Poi;");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error copying table JS_Info_Poi");
            throw;
        }
        finally
        {
            Console.WriteLine("TransferAllPoiToTable - Done!");
        }
    }

    private async Task TransferAllClockingsToTable(DateTime startDate, DateTime endDate)
    {
        var counter = 0;
        var responseXml = await _soapClient.SendSoapRequestAsync(SoapConfiguration.Baseuri, SoapConfiguration.FullMethodLinks.ClockingGetByDateRangeUtc, SoapEnvelopeBuilder.BuildClockingGetByDateRangeEnvelope(GetCaller(), startDate, endDate));
        var clockings = IntelliTracerSoapParser.DeserializeClockings(responseXml);

        if (!clockings.Any()) return;

        foreach (var clock in clockings)
        {
            const string sql = @"
                    INSERT INTO Info_Clockings (DateTimeUtc, Description, JobNumber, LocationAddressCity, LocationAddressStreet, LocationAddressNumber, LocationAddressCode, LocationAddressCountry, Pois, ClockVehicleName, ClockVehicleCode, UserBadge, UserCode, UserEmployerCode, UserName, Type, ClockVehicleGuid, LocationAddress, UserGuid)
                    VALUES (@DateTimeUtc, @Description, @JobNumber, @LocationAddressCity, @LocationAddressStreet, @LocationAddressNumber, @LocationAddressCode, @LocationAddressCountry, @Pois, @ClockVehicleName, @ClockVehicleCode, @UserBadge, @UserCode, @UserEmployerCode, @UserName, @Type, @ClockVehicleGuid, @LocationAddress, @UserGuid)";

            var descStr = clock.Description ?? "";
            var vehicleName = clock.Vehicle?.Name ?? "";
            var userName = clock.User?.Name ?? "";

            var parameters = new[]
            {
                new SqlParameter("@DateTimeUtc", SqlDbType.NVarChar) { Value = clock.DateTimeLocal ?? "" },
                new SqlParameter("@Description", SqlDbType.NVarChar, 50) { Value = descStr.Length > 49 ? descStr.Substring(0, 49) : descStr },
                new SqlParameter("@JobNumber", SqlDbType.NVarChar) { Value = clock.JobNumber?.Replace("/", "") ?? "" },
                new SqlParameter("@LocationAddressCity", SqlDbType.NVarChar) { Value = clock.Location?.Address?.City ?? "" },
                new SqlParameter("@LocationAddressStreet", SqlDbType.NVarChar) { Value = clock.Location?.Address?.Street ?? "" },
                new SqlParameter("@LocationAddressNumber", SqlDbType.NVarChar) { Value = clock.Location?.Address?.HouseNumber ?? "" },
                new SqlParameter("@LocationAddressCode", SqlDbType.NVarChar) { Value = clock.Location?.Address?.PostalCode ?? "" },
                new SqlParameter("@LocationAddressCountry", SqlDbType.NVarChar) { Value = clock.Location?.Address?.Country ?? "" },
                new SqlParameter("@Pois", SqlDbType.NVarChar) { Value = clock.Pois?.FirstOrDefault() ?? "" },
                new SqlParameter("@ClockVehicleName", SqlDbType.NVarChar, 50) { Value = vehicleName.Length > 49 ? vehicleName.Substring(0, 49) : vehicleName },
                new SqlParameter("@ClockVehicleCode", SqlDbType.NVarChar) { Value = clock.Vehicle?.Code ?? "" },
                new SqlParameter("@UserBadge", SqlDbType.NVarChar) { Value = clock.User?.Badge?.InternalNumber ?? "" },
                new SqlParameter("@UserCode", SqlDbType.NVarChar) { Value = clock.User?.Code ?? "" },
                new SqlParameter("@UserEmployerCode", SqlDbType.NVarChar) { Value = clock.User?.EmployerCode ?? "" },
                new SqlParameter("@UserName", SqlDbType.NVarChar, 50) { Value = userName.Length > 50 ? userName.Substring(0, 50) : userName },
                new SqlParameter("@Type", SqlDbType.NVarChar) { Value = clock.Type ?? "" },
                new SqlParameter("@ClockVehicleGuid", SqlDbType.NVarChar) { Value = clock.Vehicle?.Id ?? "" },
                new SqlParameter("@LocationAddress", SqlDbType.NVarChar) { Value = clock.Location?.Address == null ? "" : ReturnAddressString(clock.Location.Address) },
                new SqlParameter("@UserGuid", SqlDbType.NVarChar) { Value = clock.User?.Id ?? "" }
            };

            await _repository.ExecuteNonQueryAsync(sql, parameters);
        }

        try
        {
            await _repository.ExecuteSqlCmd("IF OBJECT_ID('JS_Info_Clockings', 'U') IS NOT NULL DROP TABLE JS_Info_Clockings;");
            await _repository.ExecuteSqlCmd("SELECT * INTO JS_Info_Clockings FROM Info_Clockings;");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error copying table JS_Info_Clockings");
            throw;
        }
        finally
        {
            Console.WriteLine("TransferAllPoiToTable - Done!");
        }
    }

    private async Task TransferLocations(DateTime startDate, DateTime endDate)
    {
        var connectionString = _configuration["SqlConnectionString"]!;
        var vehicleIds = new List<string>();
        using (var connection = new SqlConnection(connectionString))
        {
            await connection.OpenAsync();
            using (var command = new SqlCommand("Select VehicleId from Info_Vehicles;", connection))
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync()) vehicleIds.Add(reader.GetString(0));
            }
        }

        foreach (var vehicleId in vehicleIds)
            try
            {
                var counter = 0;
                var enveloppe = SoapEnvelopeBuilder.BuildLocationGetByVehicleIdDateRangeEnvelope(GetCaller(), vehicleId, startDate, endDate);
                var responseXml = await _soapClient.SendSoapRequestAsync(SoapConfiguration.Baseuri, SoapConfiguration.FullMethodLinks.LocationGetByVehicleIdDateRange, enveloppe);
                var locations = IntelliTracerSoapParser.DeserializeLocations(responseXml);

                // var response = await webService.Location_GetByVehicleIdDateRangeAsync(caller, vehicleId, startDate, endDate);
                // var locations = response.Body.Location_GetByVehicleIdDateRangeResult;
                Console.WriteLine($"Inserted location {counter++} of {locations.Count()} for vehicle {vehicleId}");
                if (locations == null) continue;

                foreach (var loc in locations)
                {
                    const string sql = @"
                            INSERT INTO Info_Location_GetByVehicleIdListDateRange (Location_AddressString, Location_AddressNr, Location_AddressCity, Location_AddressCountry, Location_AddressPostalCode, Location_AddressStreet, Location_AddressSub, Location_Poi_1, Location_Poi_2, Location_Poi_3, Location_BadgeUser, Location_BadgeNr, Location_GpsDateUtc, Location_Heading, Location_ReportDateUtc, Location_Speed, Location_VehicleId)
                            VALUES (@AddressString, @AddressNr, @AddressCity, @AddressCountry, @AddressPostalCode, @AddressStreet, @AddressSub, @Poi1, @Poi2, @Poi3, @BadgeUser, @BadgeNr, @GpsDateUtc, @Heading, @ReportDateUtc, @Speed, @VehicleId)";

                    var parameters = new[]
                    {
                        new SqlParameter("@AddressString", SqlDbType.NVarChar) { Value = loc.AddressEntity == null ? "" : ReturnAddressString(loc.AddressEntity) },
                        new SqlParameter("@AddressNr", SqlDbType.NVarChar) { Value = loc.AddressEntity?.HouseNumber ?? "" },
                        new SqlParameter("@AddressCity", SqlDbType.NVarChar) { Value = loc.AddressEntity?.City ?? "" },
                        new SqlParameter("@AddressCountry", SqlDbType.NVarChar) { Value = loc.AddressEntity?.Country ?? "" },
                        new SqlParameter("@AddressPostalCode", SqlDbType.NVarChar) { Value = loc.AddressEntity?.PostalCode ?? "" },
                        new SqlParameter("@AddressStreet", SqlDbType.NVarChar) { Value = loc.AddressEntity?.Street ?? "" },
                        new SqlParameter("@AddressSub", SqlDbType.NVarChar) { Value = loc.AddressEntity?.Submunicipality ?? "" },
                        new SqlParameter("@Poi1", SqlDbType.NVarChar) { Value = loc.Pois?.Length >= 1 ? loc.Pois[0] : "" },
                        new SqlParameter("@Poi2", SqlDbType.NVarChar) { Value = loc.Pois?.Length >= 2 ? loc.Pois[1] : "" },
                        new SqlParameter("@Poi3", SqlDbType.NVarChar) { Value = loc.Pois?.Length >= 3 ? loc.Pois[2] : "" },
                        new SqlParameter("@BadgeUser", SqlDbType.NVarChar) { Value = loc.BadgeUser ?? "" },
                        new SqlParameter("@BadgeNr", SqlDbType.NVarChar) { Value = loc.BadgeNr ?? "" },
                        new SqlParameter("@GpsDateUtc", SqlDbType.NVarChar) { Value = loc.GpsDateUtc?.ToLocalTime().ToString() ?? "" },
                        new SqlParameter("@Heading", SqlDbType.NVarChar) { Value = loc.Heading ?? "" },
                        new SqlParameter("@ReportDateUtc", SqlDbType.NVarChar) { Value = loc.ReportDateUtc.ToLocalTime().ToString() },
                        new SqlParameter("@Speed", SqlDbType.NVarChar) { Value = loc.Speed ?? "" },
                        new SqlParameter("@VehicleId", SqlDbType.NVarChar) { Value = loc.VehicleId ?? "" }
                    };

                    await _repository.ExecuteNonQueryAsync(sql, parameters);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error transferring locations for vehicle {VehicleId}", vehicleId);
            }

        try
        {
            await _repository.ExecuteSqlCmd("IF OBJECT_ID('JS_Info_Location_GetByVehicleIdListDateRange', 'U') IS NOT NULL DROP TABLE JS_Info_Location_GetByVehicleIdListDateRange;");
            await _repository.ExecuteSqlCmd("SELECT * INTO JS_Info_Location_GetByVehicleIdListDateRange FROM Info_Location_GetByVehicleIdListDateRange;");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error copying table JS_Info_Location_GetByVehicleIdListDateRange");
            throw;
        }
        finally
        {
            Console.WriteLine("TransferLocations - Done!");
        }
    }

    private async Task Info_TripOverview_Custom(DateTime startDate, DateTime endDate)
    {
        var connectionString = _configuration["SqlConnectionString"]!;
        var userGuids = new ArrayOfGuid();
        using (var connection = new SqlConnection(connectionString))
        {
            await connection.OpenAsync();
            await using (var command = new SqlCommand("Select Guid from Info_Users;", connection))
            await using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync()) userGuids.Add(reader.GetString(0));
            }
        }

        if (!userGuids.Any()) return;

        var options = new TimesheetOverviewOptions
        {
            IncludeClockings = true,
            IncludeSummary = true,
            IncludeTimesheets = true,
            IncludeTrips = true
        };

        var counter = 0;
        var enveloppe = SoapEnvelopeBuilder.BuildTimeSheetGetByUserIdListDateRange(GetCaller(), userGuids, startDate, endDate);
        var responseXml = await _soapClient.SendSoapRequestAsync(SoapConfiguration.Baseuri, SoapConfiguration.FullMethodLinks.TimeSheetGetByUserIdListDateRange, enveloppe);
        var overviews = IntelliTracerSoapParser.DeserializeTimesheetDayEntities(responseXml);

        if (overviews == null) return;

        foreach (var overview in overviews)
        {
            counter = 0;
            var summary = overview.Summary;
            foreach (var ev in overview.Events)
            {
                try
                {
                    const string sql = @"
                        INSERT INTO Info_TripOverview_Custom (Alg_SyncStart, Alg_SyncStop, Alg_UserId, Event_IsCostCalculation, Event_IsLoadUnload, Event_JobNumber, Event_Mileage, Event_MileageBirdFlight, Event_PoisAtStartLocation, Event_PoisAtStartLocation2, Event_PoisAtStartLocation3, Event_PoisAtStopLocation, Event_PoisAtStopLocation2, Event_PoisAtStopLocation3, Event_RoundedTotalWork, Event_StartDateTimeUtc, Event_StopDateTimeUtc, Event_StartLocation_Address, Event_StopLocation_Address, Event_StartVehicleId, Event_StopVehicleId, Event_Type, Summary_AmountMobilityDriver, Summary_AmountMobilityHomeWork, Summary_AmountMobilityPassenger, Summary_MileageMobilityBirdFlightDriver, Summary_MileageMobilityDrivenDriver, Summary_MileageMobilityDrivenPassenger, Summary_MileageMobilityHomeWork, Summary_NormalHours, Summary_Pause, Summary_TotalHours, Summary_TotalLoad)
                        VALUES (@Alg_SyncStart, @Alg_SyncStop, @Alg_UserId, @Event_IsCostCalculation, @Event_IsLoadUnload, @Event_JobNumber, @Event_Mileage, @Event_MileageBirdFlight, @Event_PoisAtStartLocation, @Event_PoisAtStartLocation2, @Event_PoisAtStartLocation3, @Event_PoisAtStopLocation, @Event_PoisAtStopLocation2, @Event_PoisAtStopLocation3, @Event_RoundedTotalWork, @Event_StartDateTimeUtc, @Event_StopDateTimeUtc, @Event_StartLocation_Address, @Event_StopLocation_Address, @Event_StartVehicleId, @Event_StopVehicleId, @Event_Type, @Summary_AmountMobilityDriver, @Summary_AmountMobilityHomeWork, @Summary_AmountMobilityPassenger, @Summary_MileageMobilityBirdFlightDriver, @Summary_MileageMobilityDrivenDriver, @Summary_MileageMobilityDrivenPassenger, @Summary_MileageMobilityHomeWork, @Summary_NormalHours, @Summary_Pause, @Summary_TotalHours, @Summary_TotalLoad)";


                    var jobNumber = ev.JobNumber == null ? "" : ev.JobNumber.Length > 50 ? ev.JobNumber?.Substring(0, 50).Replace("/", "") ?? "" : ev.JobNumber.Replace("/", "");

                    var parameters = new[]
                    {
                        new SqlParameter("@Alg_SyncStart", SqlDbType.NVarChar) { Value = overview.StartDatetimeLocal.FormatToSqlDateTimeString() },
                        new SqlParameter("@Alg_SyncStop", SqlDbType.NVarChar) { Value = overview.StopDatetimeLocal.FormatToSqlDateTimeString() },
                        new SqlParameter("@Alg_UserId", SqlDbType.NVarChar) { Value = overview.UserId ?? "" },
                        new SqlParameter("@Event_IsCostCalculation", SqlDbType.NVarChar) { Value = ev.IsCostCalculation ?? "" },
                        new SqlParameter("@Event_IsLoadUnload", SqlDbType.NVarChar) { Value = ev.IsLoadUnload ?? "" },
                        new SqlParameter("@Event_JobNumber", SqlDbType.NVarChar) { Value = jobNumber },
                        new SqlParameter("@Event_Mileage", SqlDbType.NVarChar) { Value = ev.Mileage ?? "" },
                        new SqlParameter("@Event_MileageBirdFlight", SqlDbType.NVarChar) { Value = ev.MileageBirdFlight ?? "" },
                        new SqlParameter("@Event_PoisAtStartLocation", SqlDbType.NVarChar) { Value = ev.PoisAtStartLocation?.Length >= 1 ? ev.PoisAtStartLocation[0]?.ToString() ?? "" : "" },
                        new SqlParameter("@Event_PoisAtStartLocation2", SqlDbType.NVarChar) { Value = ev.PoisAtStartLocation?.Length >= 2 ? ev.PoisAtStartLocation[1]?.ToString() ?? "" : "" },
                        new SqlParameter("@Event_PoisAtStartLocation3", SqlDbType.NVarChar) { Value = ev.PoisAtStartLocation?.Length >= 3 ? ev.PoisAtStartLocation[2]?.ToString() ?? "" : "" },
                        new SqlParameter("@Event_PoisAtStopLocation", SqlDbType.NVarChar) { Value = ev.PoisAtStopLocation?.Length >= 1 ? ev.PoisAtStopLocation[0]?.ToString() ?? "" : "" },
                        new SqlParameter("@Event_PoisAtStopLocation2", SqlDbType.NVarChar) { Value = ev.PoisAtStopLocation?.Length >= 2 ? ev.PoisAtStopLocation[1]?.ToString() ?? "" : "" },
                        new SqlParameter("@Event_PoisAtStopLocation3", SqlDbType.NVarChar) { Value = ev.PoisAtStopLocation?.Length >= 3 ? ev.PoisAtStopLocation[2]?.ToString() ?? "" : "" },
                        new SqlParameter("@Event_RoundedTotalWork", SqlDbType.NVarChar) { Value = ev.RoundedTotalWork ?? "" },
                        new SqlParameter("@Event_StartDateTimeUtc", SqlDbType.NVarChar) { Value = ev.StartDateTimeLocal.FormatToSqlDateTimeString() },
                        new SqlParameter("@Event_StopDateTimeUtc", SqlDbType.NVarChar) { Value = ev.StopDateTimeLocal.FormatToSqlDateTimeString() },
                        new SqlParameter("@Event_StartLocation_Address", SqlDbType.NVarChar) { Value = ev.StartLocation?.Address == null ? "" : ReturnAddressString(ev.StartLocation.Address).Length>100? ReturnAddressString(ev.StartLocation.Address).Substring(0, 100):ReturnAddressString(ev.StartLocation.Address) },
                        new SqlParameter("@Event_StopLocation_Address", SqlDbType.NVarChar) { Value = ev.StopLocation?.Address == null ? "" :  ReturnAddressString(ev.StopLocation.Address).Length>100? ReturnAddressString(ev.StopLocation.Address).Substring(0, 100):ReturnAddressString(ev.StopLocation.Address) },
                        new SqlParameter("@Event_StartVehicleId", SqlDbType.NVarChar) { Value = ev.StartVehicleId ?? "" },
                        new SqlParameter("@Event_StopVehicleId", SqlDbType.NVarChar) { Value = ev.StopVehicleId ?? "" },
                        new SqlParameter("@Event_Type", SqlDbType.NVarChar) { Value = ev.Type ?? "" },
                        new SqlParameter("@Summary_AmountMobilityDriver", SqlDbType.NVarChar) { Value = summary?.AmountMobilityDriver ?? "" },
                        new SqlParameter("@Summary_AmountMobilityHomeWork", SqlDbType.NVarChar) { Value = summary?.AmountMobilityHomeWork ?? "" },
                        new SqlParameter("@Summary_AmountMobilityPassenger", SqlDbType.NVarChar) { Value = summary?.AmountMobilityPassenger ?? "" },
                        new SqlParameter("@Summary_MileageMobilityBirdFlightDriver", SqlDbType.NVarChar) { Value = summary?.MileageMobilityBirdFlightDriver ?? "" },
                        new SqlParameter("@Summary_MileageMobilityDrivenDriver", SqlDbType.NVarChar) { Value = summary?.MileageMobilityDrivenDriver ?? "" },
                        new SqlParameter("@Summary_MileageMobilityDrivenPassenger", SqlDbType.NVarChar) { Value = summary?.MileageMobilityDrivenPassenger ?? "" },
                        new SqlParameter("@Summary_MileageMobilityHomeWork", SqlDbType.NVarChar) { Value = summary?.MileageMobilityHomeWork ?? "" },
                        new SqlParameter("@Summary_NormalHours", SqlDbType.NVarChar) { Value = summary?.NormalHours ?? "" },
                        new SqlParameter("@Summary_Pause", SqlDbType.NVarChar) { Value = summary?.Pause ?? "" },
                        new SqlParameter("@Summary_TotalHours", SqlDbType.NVarChar) { Value = summary?.TotalHours ?? "" },
                        new SqlParameter("@Summary_TotalLoad", SqlDbType.NVarChar) { Value = summary?.TotalLoad ?? "" }
                    };

                    await _repository.ExecuteNonQueryAsync(sql, parameters);
                    Console.WriteLine($"Inserted trip overview {counter++} of {overviews.Count()} for user {overview.UserId}");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        try
        {
            await _repository.ExecuteSqlCmd("IF OBJECT_ID('JS_Info_TripOverview_Custom', 'U') IS NOT NULL DROP TABLE JS_Info_TripOverview_Custom;");
            await _repository.ExecuteSqlCmd("SELECT * INTO JS_Info_TripOverview_Custom FROM Info_TripOverview_Custom;");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error copying table JS_Info_TripOverview_Custom");
            throw;
        }
        finally
        {
            Console.WriteLine("Info_TripOverview_Custom - Done!");
        }
    }

    private string ReturnAddressString(AddressEntity address)
    {
        if (address == null) return "";

        var result = $"{address.Street} {address.HouseNumber}, {address.PostalCode} {address.City} ({address.Country})";

        return result.Replace("'", "''").Trim();
    }
}