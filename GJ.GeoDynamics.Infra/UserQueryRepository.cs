using System.Data;
using GJ.GeoDynamics.Infra.Interface;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace GJ.GeoDynamics.Infra;

public sealed class UserQueryRepository : IUserQueryRepository
{
    private readonly string _connectionString;

    public UserQueryRepository(IConfiguration configuration)
    {
        _connectionString = configuration["SqlConnectionString"]
                            ?? throw new InvalidOperationException("SqlConnectionString is missing");
    }

    public async Task<List<string>> GetAllUserGuidsAsync(CancellationToken cancellationToken = default)
    {
        var userGuids = new List<string>();

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand("SELECT Guid FROM Info_Users;", connection)
        {
            CommandType = CommandType.Text
        };

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            if (!reader.IsDBNull(0))
                userGuids.Add(reader.GetString(0));
        }

        return userGuids;
    }
}