using System.Data;
using System.Globalization;
using GJ.GeoDynamics.Domain;
using GJ.GeoDynamics.Infra.Interface;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace GJ.GeoDynamics.Infra;

public sealed class ClockingRepository : IClockingRepository
{
    private readonly ILogger<ClockingRepository> _logger;
    private readonly IDatabaseRepository _db;

    public ClockingRepository(ILogger<ClockingRepository> logger, IDatabaseRepository db)
    {
        _logger = logger;
        _db = db;
    }

public async Task InsertBatchAndSnapshotAsync(IReadOnlyCollection<ClockingEntity> clockings, CancellationToken cancellationToken = default)
{
    await _db.ClearTable("Info_Clockings");

    if (clockings.Count == 0)
    {
        try
        {
            await _db.ExecuteSqlCmd("IF OBJECT_ID('JS_Info_Clockings', 'U') IS NOT NULL DROP TABLE JS_Info_Clockings;");
            await _db.ExecuteSqlCmd("SELECT * INTO JS_Info_Clockings FROM Info_Clockings;");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error copying table JS_Info_Clockings");
            throw;
        }

        return;
    }

    foreach (var clock in clockings)
    {
        const string sql = @"
            INSERT INTO Info_Clockings (DateTimeUtc, Description, JobNumber, LocationAddressCity, LocationAddressStreet, LocationAddressNumber, LocationAddressCode, LocationAddressCountry, Pois, ClockVehicleName, ClockVehicleCode, UserBadge, UserCode, UserEmployerCode, UserName, Type, ClockVehicleGuid, LocationAddress, UserGuid)
            VALUES (@DateTimeUtc, @Description, @JobNumber, @LocationAddressCity, @LocationAddressStreet, @LocationAddressNumber, @LocationAddressCode, @LocationAddressCountry, @Pois, @ClockVehicleName, @ClockVehicleCode, @UserBadge, @UserCode, @UserEmployerCode, @UserName, @Type, @ClockVehicleGuid, @LocationAddress, @UserGuid)";

        var descStr = clock.Description ?? string.Empty;
        var vehicleName = clock.Vehicle?.Name ?? string.Empty;
        var userName = clock.User?.Name ?? string.Empty;

        var parameters = new[]
        {
            new SqlParameter("@DateTimeUtc", SqlDbType.NVarChar) { Value = FormatClockDateTime(clock.DateTimeLocal) },
            new SqlParameter("@Description", SqlDbType.NVarChar, 50) { Value = descStr.Length > 49 ? descStr.Substring(0, 49) : descStr },
            new SqlParameter("@JobNumber", SqlDbType.NVarChar) { Value = clock.JobNumber?.Replace("/", "") ?? string.Empty },
            new SqlParameter("@LocationAddressCity", SqlDbType.NVarChar) { Value = clock.Location?.Address?.City ?? string.Empty },
            new SqlParameter("@LocationAddressStreet", SqlDbType.NVarChar) { Value = clock.Location?.Address?.Street ?? string.Empty },
            new SqlParameter("@LocationAddressNumber", SqlDbType.NVarChar) { Value = clock.Location?.Address?.HouseNumber ?? string.Empty },
            new SqlParameter("@LocationAddressCode", SqlDbType.NVarChar) { Value = clock.Location?.Address?.PostalCode ?? string.Empty },
            new SqlParameter("@LocationAddressCountry", SqlDbType.NVarChar) { Value = clock.Location?.Address?.Country ?? string.Empty },
            new SqlParameter("@Pois", SqlDbType.NVarChar) { Value = clock.Pois?.FirstOrDefault() ?? string.Empty },
            new SqlParameter("@ClockVehicleName", SqlDbType.NVarChar, 50) { Value = vehicleName.Length > 49 ? vehicleName.Substring(0, 49) : vehicleName },
            new SqlParameter("@ClockVehicleCode", SqlDbType.NVarChar) { Value = clock.Vehicle?.Code ?? string.Empty },
            new SqlParameter("@UserBadge", SqlDbType.NVarChar) { Value = clock.User?.Badge?.InternalNumber ?? string.Empty },
            new SqlParameter("@UserCode", SqlDbType.NVarChar) { Value = clock.User?.Code ?? string.Empty },
            new SqlParameter("@UserEmployerCode", SqlDbType.NVarChar) { Value = clock.User?.EmployerCode ?? string.Empty },
            new SqlParameter("@UserName", SqlDbType.NVarChar, 50) { Value = userName.Length > 50 ? userName.Substring(0, 50) : userName },
            new SqlParameter("@Type", SqlDbType.NVarChar) { Value = clock.Type ?? string.Empty },
            new SqlParameter("@ClockVehicleGuid", SqlDbType.NVarChar) { Value = clock.Vehicle?.Id ?? string.Empty },
            new SqlParameter("@LocationAddress", SqlDbType.NVarChar) { Value = clock.Location?.Address == null ? string.Empty : ReturnAddressString(clock.Location.Address) },
            new SqlParameter("@UserGuid", SqlDbType.NVarChar) { Value = clock.User?.Id ?? string.Empty }
        };

        await _db.ExecuteNonQueryAsync(sql, parameters);
    }

    try
    {
        await _db.ExecuteSqlCmd("IF OBJECT_ID('JS_Info_Clockings', 'U') IS NOT NULL DROP TABLE JS_Info_Clockings;");
        await _db.ExecuteSqlCmd("SELECT * INTO JS_Info_Clockings FROM Info_Clockings;");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error copying table JS_Info_Clockings");
        throw;
    }
}

    private static string FormatClockDateTime(string? dateTimeLocal)
    {
        if (string.IsNullOrWhiteSpace(dateTimeLocal))
            return string.Empty;

        // SOAP typically sends: 2026-02-26T04:16:28 (no timezone)
        var formats = new[]
        {
            "yyyy-MM-dd'T'HH:mm:ss",
            "yyyy-MM-dd'T'HH:mm:ss.fff",
            "yyyy-MM-dd HH:mm:ss"
        };

        if (DateTime.TryParseExact(dateTimeLocal, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
            return dt.ToString("dd/MM/yyyy H:mm:ss", CultureInfo.InvariantCulture);

        // Fallback: keep original if it’s in an unexpected format
        return dateTimeLocal;
    }

    private static string ReturnAddressString(AddressEntity address)
    {
        var result = $"{address.Street} {address.HouseNumber}, {address.PostalCode} {address.City} ({address.Country})";
        return result.Replace("'", "''").Trim();
    }
}