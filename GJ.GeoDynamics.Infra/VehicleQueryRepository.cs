using System.Data;
using GJ.GeoDynamics.Infra.Interface;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace GJ.GeoDynamics.Infra;

public sealed class VehicleQueryRepository : IVehicleQueryRepository
{
    private readonly string _connectionString;

    public VehicleQueryRepository(IConfiguration configuration)
    {
        _connectionString = configuration["SqlConnectionString"]
                            ?? throw new InvalidOperationException("SqlConnectionString is missing");
    }

    public async Task<List<string>> GetAllVehicleIdsAsync(CancellationToken cancellationToken = default)
    {
        var vehicleIds = new List<string>();

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand("SELECT VehicleId FROM Info_Vehicles;", connection);
        command.CommandType = CommandType.Text;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            if (!reader.IsDBNull(0))
                vehicleIds.Add(reader.GetString(0));
        }

        return vehicleIds;
    }
}