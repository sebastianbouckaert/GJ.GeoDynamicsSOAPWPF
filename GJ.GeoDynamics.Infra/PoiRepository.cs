using System.Data;
using GJ.GeoDynamics.Domain;
using GJ.GeoDynamics.Infra.Interface;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace GJ.GeoDynamics.Infra;

public sealed class PoiRepository : IPoiRepository
{
    private readonly ILogger<PoiRepository> _logger;
    private readonly IDatabaseRepository _db;

    public PoiRepository(ILogger<PoiRepository> logger, IDatabaseRepository db)
    {
        _logger = logger;
        _db = db;
    }

    public async Task ReplaceAllAsync(IReadOnlyCollection<PoiEntity> pois, CancellationToken cancellationToken = default)
    {
        await _db.ClearTable("Info_Poi");

        foreach (var poi in pois)
        {
            const string sql = @"
                INSERT INTO Info_Poi (Code, GuidPoi, Name, Priority, Address, Address_City, Address_Country, Address_HouseNumber, Address_PostalCode, Address_street, Address_Submunicipality)
                VALUES (@Code, @GuidPoi, @Name, @Priority, @Address, @Address_City, @Address_Country, @Address_HouseNumber, @Address_PostalCode, @Address_street, @Address_Submunicipality)";

            var parameters = new[]
            {
                new SqlParameter("@Code", SqlDbType.NVarChar) { Value = poi.Code ?? string.Empty },
                new SqlParameter("@GuidPoi", SqlDbType.NVarChar) { Value = poi.Id ?? string.Empty },
                new SqlParameter("@Name", SqlDbType.NVarChar) { Value = poi.Name ?? string.Empty },
                new SqlParameter("@Priority", SqlDbType.NVarChar) { Value = poi.Priority ?? string.Empty },
                new SqlParameter("@Address", SqlDbType.NVarChar) { Value = poi.Address == null ? string.Empty : ReturnAddressString(poi.Address) },
                new SqlParameter("@Address_City", SqlDbType.NVarChar) { Value = poi.Address?.City ?? string.Empty },
                new SqlParameter("@Address_Country", SqlDbType.NVarChar) { Value = poi.Address?.Country ?? string.Empty },
                new SqlParameter("@Address_HouseNumber", SqlDbType.NVarChar) { Value = poi.Address?.HouseNumber ?? string.Empty },
                new SqlParameter("@Address_PostalCode", SqlDbType.NVarChar) { Value = poi.Address?.PostalCode ?? string.Empty },
                new SqlParameter("@Address_street", SqlDbType.NVarChar) { Value = poi.Address?.Street ?? string.Empty },
                new SqlParameter("@Address_Submunicipality", SqlDbType.NVarChar) { Value = poi.Address?.Submunicipality ?? string.Empty }
            };

            await _db.ExecuteNonQueryAsync(sql, parameters);
        }

        try
        {
            await _db.ExecuteSqlCmd("IF OBJECT_ID('JS_Info_Poi', 'U') IS NOT NULL DROP TABLE JS_Info_Poi;");
            await _db.ExecuteSqlCmd("SELECT * INTO JS_Info_Poi FROM Info_Poi;");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error copying table JS_Info_Poi");
            throw;
        }
    }

    private static string ReturnAddressString(AddressEntity address)
    {
        var result = $"{address.Street} {address.HouseNumber}, {address.PostalCode} {address.City} ({address.Country})";
        return result.Replace("'", "''").Trim();
    }
}