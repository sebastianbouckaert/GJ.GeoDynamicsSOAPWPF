using System.Data;
using GJ.GeoDynamics.Common;
using GJ.GeoDynamics.Domain;
using GJ.GeoDynamics.Infra.Interface;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace GJ.GeoDynamics.Infra;

public sealed class TripOverviewRepository : ITripOverviewRepository
{
    private readonly ILogger<TripOverviewRepository> _logger;
    private readonly IDatabaseRepository _db;

    public TripOverviewRepository(ILogger<TripOverviewRepository> logger, IDatabaseRepository db)
    {
        _logger = logger;
        _db = db;
    }

    public async Task InsertFromOverviewsAsync(IReadOnlyCollection<TimeSheetDayEntity> overviews, CancellationToken cancellationToken = default)
    {
        if (overviews.Count == 0) return;

        foreach (var overview in overviews)
        {
            var summary = overview.Summary;

            foreach (var ev in overview.Events)
            {
                const string sql = @"
                    INSERT INTO Info_TripOverview_Custom
                    (Alg_SyncStart, Alg_SyncStop, Alg_UserId,
                     Event_IsCostCalculation, Event_IsLoadUnload, Event_JobNumber, Event_Mileage, Event_MileageBirdFlight,
                     Event_PoisAtStartLocation, Event_PoisAtStartLocation2, Event_PoisAtStartLocation3,
                     Event_PoisAtStopLocation, Event_PoisAtStopLocation2, Event_PoisAtStopLocation3,
                     Event_RoundedTotalWork, Event_StartDateTimeUtc, Event_StopDateTimeUtc,
                     Event_StartLocation_Address, Event_StopLocation_Address, Event_StartVehicleId, Event_StopVehicleId, Event_Type,
                     Summary_AmountMobilityDriver, Summary_AmountMobilityHomeWork, Summary_AmountMobilityPassenger,
                     Summary_MileageMobilityBirdFlightDriver, Summary_MileageMobilityDrivenDriver, Summary_MileageMobilityDrivenPassenger, Summary_MileageMobilityHomeWork,
                     Summary_NormalHours, Summary_Pause, Summary_TotalHours, Summary_TotalLoad)
                    VALUES
                    (@Alg_SyncStart, @Alg_SyncStop, @Alg_UserId,
                     @Event_IsCostCalculation, @Event_IsLoadUnload, @Event_JobNumber, @Event_Mileage, @Event_MileageBirdFlight,
                     @Event_PoisAtStartLocation, @Event_PoisAtStartLocation2, @Event_PoisAtStartLocation3,
                     @Event_PoisAtStopLocation, @Event_PoisAtStopLocation2, @Event_PoisAtStopLocation3,
                     @Event_RoundedTotalWork, @Event_StartDateTimeUtc, @Event_StopDateTimeUtc,
                     @Event_StartLocation_Address, @Event_StopLocation_Address, @Event_StartVehicleId, @Event_StopVehicleId, @Event_Type,
                     @Summary_AmountMobilityDriver, @Summary_AmountMobilityHomeWork, @Summary_AmountMobilityPassenger,
                     @Summary_MileageMobilityBirdFlightDriver, @Summary_MileageMobilityDrivenDriver, @Summary_MileageMobilityDrivenPassenger, @Summary_MileageMobilityHomeWork,
                     @Summary_NormalHours, @Summary_Pause, @Summary_TotalHours, @Summary_TotalLoad)";

                var jobNumber =
                    ev.JobNumber == null ? string.Empty :
                    ev.JobNumber.Length > 50 ? (ev.JobNumber.Substring(0, 50).Replace("/", "")) :
                    ev.JobNumber.Replace("/", "");

                var startAddr = FormatAddressMax100(ev.StartLocation?.Address);
                var stopAddr = FormatAddressMax100(ev.StopLocation?.Address);

                var parameters = new[]
                {
                    new SqlParameter("@Alg_SyncStart", SqlDbType.NVarChar) { Value = overview.StartDatetimeLocal.FormatToSqlDateTimeString() },
                    new SqlParameter("@Alg_SyncStop", SqlDbType.NVarChar) { Value = overview.StopDatetimeLocal.FormatToSqlDateTimeString() },
                    new SqlParameter("@Alg_UserId", SqlDbType.NVarChar) { Value = overview.UserId ?? string.Empty },

                    new SqlParameter("@Event_IsCostCalculation", SqlDbType.NVarChar) { Value = ev.IsCostCalculation ?? string.Empty },
                    new SqlParameter("@Event_IsLoadUnload", SqlDbType.NVarChar) { Value = ev.IsLoadUnload ?? string.Empty },
                    new SqlParameter("@Event_JobNumber", SqlDbType.NVarChar) { Value = jobNumber },
                    new SqlParameter("@Event_Mileage", SqlDbType.NVarChar) { Value = ev.Mileage ?? string.Empty },
                    new SqlParameter("@Event_MileageBirdFlight", SqlDbType.NVarChar) { Value = ev.MileageBirdFlight ?? string.Empty },

                    new SqlParameter("@Event_PoisAtStartLocation", SqlDbType.NVarChar) { Value = ev.PoisAtStartLocation?.Length >= 1 ? ev.PoisAtStartLocation[0]?.ToString() ?? string.Empty : string.Empty },
                    new SqlParameter("@Event_PoisAtStartLocation2", SqlDbType.NVarChar) { Value = ev.PoisAtStartLocation?.Length >= 2 ? ev.PoisAtStartLocation[1]?.ToString() ?? string.Empty : string.Empty },
                    new SqlParameter("@Event_PoisAtStartLocation3", SqlDbType.NVarChar) { Value = ev.PoisAtStartLocation?.Length >= 3 ? ev.PoisAtStartLocation[2]?.ToString() ?? string.Empty : string.Empty },

                    new SqlParameter("@Event_PoisAtStopLocation", SqlDbType.NVarChar) { Value = ev.PoisAtStopLocation?.Length >= 1 ? ev.PoisAtStopLocation[0]?.ToString() ?? string.Empty : string.Empty },
                    new SqlParameter("@Event_PoisAtStopLocation2", SqlDbType.NVarChar) { Value = ev.PoisAtStopLocation?.Length >= 2 ? ev.PoisAtStopLocation[1]?.ToString() ?? string.Empty : string.Empty },
                    new SqlParameter("@Event_PoisAtStopLocation3", SqlDbType.NVarChar) { Value = ev.PoisAtStopLocation?.Length >= 3 ? ev.PoisAtStopLocation[2]?.ToString() ?? string.Empty : string.Empty },

                    new SqlParameter("@Event_RoundedTotalWork", SqlDbType.NVarChar) { Value = ev.RoundedTotalWork ?? string.Empty },
                    new SqlParameter("@Event_StartDateTimeUtc", SqlDbType.NVarChar) { Value = ev.StartDateTimeLocal.FormatToSqlDateTimeString() },
                    new SqlParameter("@Event_StopDateTimeUtc", SqlDbType.NVarChar) { Value = ev.StopDateTimeLocal.FormatToSqlDateTimeString() },

                    new SqlParameter("@Event_StartLocation_Address", SqlDbType.NVarChar) { Value = startAddr },
                    new SqlParameter("@Event_StopLocation_Address", SqlDbType.NVarChar) { Value = stopAddr },

                    new SqlParameter("@Event_StartVehicleId", SqlDbType.NVarChar) { Value = ev.StartVehicleId ?? string.Empty },
                    new SqlParameter("@Event_StopVehicleId", SqlDbType.NVarChar) { Value = ev.StopVehicleId ?? string.Empty },
                    new SqlParameter("@Event_Type", SqlDbType.NVarChar) { Value = ev.Type ?? string.Empty },

                    new SqlParameter("@Summary_AmountMobilityDriver", SqlDbType.NVarChar) { Value = summary?.AmountMobilityDriver ?? string.Empty },
                    new SqlParameter("@Summary_AmountMobilityHomeWork", SqlDbType.NVarChar) { Value = summary?.AmountMobilityHomeWork ?? string.Empty },
                    new SqlParameter("@Summary_AmountMobilityPassenger", SqlDbType.NVarChar) { Value = summary?.AmountMobilityPassenger ?? string.Empty },
                    new SqlParameter("@Summary_MileageMobilityBirdFlightDriver", SqlDbType.NVarChar) { Value = summary?.MileageMobilityBirdFlightDriver ?? string.Empty },
                    new SqlParameter("@Summary_MileageMobilityDrivenDriver", SqlDbType.NVarChar) { Value = summary?.MileageMobilityDrivenDriver ?? string.Empty },
                    new SqlParameter("@Summary_MileageMobilityDrivenPassenger", SqlDbType.NVarChar) { Value = summary?.MileageMobilityDrivenPassenger ?? string.Empty },
                    new SqlParameter("@Summary_MileageMobilityHomeWork", SqlDbType.NVarChar) { Value = summary?.MileageMobilityHomeWork ?? string.Empty },
                    new SqlParameter("@Summary_NormalHours", SqlDbType.NVarChar) { Value = summary?.NormalHours ?? string.Empty },
                    new SqlParameter("@Summary_Pause", SqlDbType.NVarChar) { Value = summary?.Pause ?? string.Empty },
                    new SqlParameter("@Summary_TotalHours", SqlDbType.NVarChar) { Value = summary?.TotalHours ?? string.Empty },
                    new SqlParameter("@Summary_TotalLoad", SqlDbType.NVarChar) { Value = summary?.TotalLoad ?? string.Empty }
                };

                await _db.ExecuteNonQueryAsync(sql, parameters);
            }
        }
    }

    public async Task RefreshSnapshotAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _db.ExecuteSqlCmd("IF OBJECT_ID('JS_Info_TripOverview_Custom', 'U') IS NOT NULL DROP TABLE JS_Info_TripOverview_Custom;");
            await _db.ExecuteSqlCmd("SELECT * INTO JS_Info_TripOverview_Custom FROM Info_TripOverview_Custom;");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error copying table JS_Info_TripOverview_Custom");
            throw;
        }
    }

    private static string FormatAddressMax100(AddressEntity? address)
    {
        if (address == null) return string.Empty;

        var s = $"{address.Street} {address.HouseNumber}, {address.PostalCode} {address.City} ({address.Country})"
            .Replace("'", "''")
            .Trim();

        return s.Length > 100 ? s.Substring(0, 100) : s;
    }
}