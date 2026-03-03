using System.Data;
using GJ.GeoDynamics.Domain;
using GJ.GeoDynamics.Infra.Interface;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace GJ.GeoDynamics.Infra;

public sealed class VehicleRepository : IVehicleRepository
{
    private readonly ILogger<VehicleRepository> _logger;
    private readonly IDatabaseRepository _db;

    public VehicleRepository(ILogger<VehicleRepository> logger, IDatabaseRepository db)
    {
        _logger = logger;
        _db = db;
    }

    public async Task ReplaceAllAsync(IReadOnlyCollection<VehicleEntity> vehicles, CancellationToken cancellationToken = default)
    {
        // Note: cancellationToken is available for future extension; the underlying IDatabaseRepository
        // methods shown here don't accept it, so we can't pass it through yet.
        await _db.ClearTable("Info_Vehicles");

        foreach (var vehicle in vehicles)
        {
            const string sql =
                @"INSERT INTO Info_Vehicles (Code, Name, LastSyncTime, VehicleTypeId, VehicleId, LastPosition)
                  VALUES (@Code, @Name, @LastSyncTime, @VehicleTypeId, @VehicleId, @LastPosition)";

            var nameStr = vehicle.Name ?? string.Empty;
            var lastPosition = vehicle.LastPosition?.AddressEntity == null
                ? string.Empty
                : ReturnAddressString(vehicle.LastPosition.AddressEntity);

            var parameters = new[]
            {
                new SqlParameter("@Code", SqlDbType.NVarChar) { Value = vehicle.Code ?? string.Empty },
                new SqlParameter("@Name", SqlDbType.NVarChar, 50) { Value = nameStr.Length > 50 ? nameStr.Substring(0, 49) : nameStr },
                new SqlParameter("@LastSyncTime", SqlDbType.NVarChar) { Value = vehicle.LastSyncDateUtc ?? string.Empty },
                new SqlParameter("@VehicleTypeId", SqlDbType.NVarChar) { Value = vehicle.VehicleTypeId ?? string.Empty },
                new SqlParameter("@VehicleId", SqlDbType.NVarChar) { Value = vehicle.Id ?? string.Empty },
                new SqlParameter("@LastPosition", SqlDbType.NVarChar) { Value = lastPosition }
            };

            await _db.ExecuteNonQueryAsync(sql, parameters);
        }

        try
        {
            await _db.ExecuteSqlCmd("IF OBJECT_ID('JS_Info_Vehicles', 'U') IS NOT NULL DROP TABLE JS_Info_Vehicles;");
            await _db.ExecuteSqlCmd("SELECT * INTO JS_Info_Vehicles FROM Info_Vehicles;");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error copying table JS_Info_Vehicles");
            throw;
        }
    }

    private static string ReturnAddressString(AddressEntity address)
    {
        var result = $"{address.Street} {address.HouseNumber}, {address.PostalCode} {address.City} ({address.Country})";
        return result.Replace("'", "''").Trim();
    }
}