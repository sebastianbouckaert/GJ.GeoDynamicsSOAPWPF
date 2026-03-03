using System.Data;
using GJ.GeoDynamics.Common;
using GJ.GeoDynamics.Domain;
using GJ.GeoDynamics.Infra.Interface;
using Microsoft.Data.SqlClient;

namespace GJ.GeoDynamics.Infra;

public sealed class LocationRepository : ILocationRepository
{
    private readonly IDatabaseRepository _db;

    public LocationRepository(IDatabaseRepository db)
    {
        _db = db;
    }

    public async Task InsertBatchAsync(IReadOnlyCollection<LocationEntity> locations, CancellationToken cancellationToken = default)
    {
        if (locations.Count == 0) return;

        foreach (var loc in locations)
        {
            const string sql = @"
                INSERT INTO Info_Location_GetByVehicleIdListDateRange
                (Location_AddressString, Location_AddressNr, Location_AddressCity, Location_AddressCountry, Location_AddressPostalCode, Location_AddressStreet, Location_AddressSub,
                 Location_Poi_1, Location_Poi_2, Location_Poi_3,
                 Location_BadgeUser, Location_BadgeNr, Location_GpsDateUtc, Location_Heading, Location_ReportDateUtc, Location_Speed, Location_VehicleId)
                VALUES
                (@AddressString, @AddressNr, @AddressCity, @AddressCountry, @AddressPostalCode, @AddressStreet, @AddressSub,
                 @Poi1, @Poi2, @Poi3,
                 @BadgeUser, @BadgeNr, @GpsDateUtc, @Heading, @ReportDateUtc, @Speed, @VehicleId)";

            var parameters = new[]
            {
                new SqlParameter("@AddressString", SqlDbType.NVarChar) { Value = loc.AddressEntity == null ? string.Empty : ReturnAddressString(loc.AddressEntity) },
                new SqlParameter("@AddressNr", SqlDbType.NVarChar) { Value = loc.AddressEntity?.HouseNumber ?? string.Empty },
                new SqlParameter("@AddressCity", SqlDbType.NVarChar) { Value = loc.AddressEntity?.City ?? string.Empty },
                new SqlParameter("@AddressCountry", SqlDbType.NVarChar) { Value = loc.AddressEntity?.Country ?? string.Empty },
                new SqlParameter("@AddressPostalCode", SqlDbType.NVarChar) { Value = loc.AddressEntity?.PostalCode ?? string.Empty },
                new SqlParameter("@AddressStreet", SqlDbType.NVarChar) { Value = loc.AddressEntity?.Street ?? string.Empty },
                new SqlParameter("@AddressSub", SqlDbType.NVarChar) { Value = loc.AddressEntity?.Submunicipality ?? string.Empty },
                new SqlParameter("@Poi1", SqlDbType.NVarChar) { Value = loc.Pois?.Length >= 1 ? loc.Pois[0] : string.Empty },
                new SqlParameter("@Poi2", SqlDbType.NVarChar) { Value = loc.Pois?.Length >= 2 ? loc.Pois[1] : string.Empty },
                new SqlParameter("@Poi3", SqlDbType.NVarChar) { Value = loc.Pois?.Length >= 3 ? loc.Pois[2] : string.Empty },
                new SqlParameter("@BadgeUser", SqlDbType.NVarChar) { Value = loc.BadgeUser ?? string.Empty },
                new SqlParameter("@BadgeNr", SqlDbType.NVarChar) { Value = loc.BadgeNr ?? string.Empty },
                new SqlParameter("@GpsDateUtc", SqlDbType.NVarChar) { Value = loc.GpsDateUtc?.ToString().FormatToSqlDateTimeString() ?? string.Empty },
                new SqlParameter("@Heading", SqlDbType.NVarChar) { Value = loc.Heading ?? string.Empty },
                new SqlParameter("@ReportDateUtc", SqlDbType.NVarChar) { Value = loc.ReportDateUtc.ToString().FormatToSqlDateTimeString() },
                new SqlParameter("@Speed", SqlDbType.NVarChar) { Value = loc.Speed ?? string.Empty },
                new SqlParameter("@VehicleId", SqlDbType.NVarChar) { Value = loc.VehicleId ?? string.Empty }
            };

            await _db.ExecuteNonQueryAsync(sql, parameters);
        }
    }

    public async Task RefreshSnapshotAsync(CancellationToken cancellationToken = default)
    {
        await _db.ExecuteSqlCmd("IF OBJECT_ID('JS_Info_Location_GetByVehicleIdListDateRange', 'U') IS NOT NULL DROP TABLE JS_Info_Location_GetByVehicleIdListDateRange;");
        await _db.ExecuteSqlCmd("SELECT * INTO JS_Info_Location_GetByVehicleIdListDateRange FROM Info_Location_GetByVehicleIdListDateRange;");
    }

    private static string ReturnAddressString(AddressEntity address)
    {
        var result = $"{address.Street} {address.HouseNumber}, {address.PostalCode} {address.City} ({address.Country})";
        return result.Replace("'", "''").Trim();
    }
}