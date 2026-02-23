using System.Data;
using GJ.GeoDynamics.Infra.Interface;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace GJ.GeoDynamics.Infra;

public class DatabaseRepository : IDatabaseRepository
{
    private readonly string _connectionString;
    private readonly ILogger<DatabaseRepository> _logger;

    public DatabaseRepository(ILogger<DatabaseRepository> logger, string connectionString)
    {
        _logger = logger;
        _connectionString = connectionString;
    }

    public async Task ExecuteSqlCmd(string sCommand)
    {
        if (string.IsNullOrEmpty(sCommand)) return;

        await using var myConnection = GetConnection();
        try
        {
            await myConnection.OpenAsync();
           await using var cmd = new SqlCommand(sCommand, myConnection)
            {
                CommandType = CommandType.Text
            };
            await cmd.ExecuteNonQueryAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error executing sql: {SqlCommand}", sCommand);
            throw;
        }
    }

    public async Task<int> ExecuteNonQueryAsync(string sql, params SqlParameter[] parameters)
    {
        await using var myConnection = GetConnection();
        try
        {
            await myConnection.OpenAsync();
            await using var cmd = new SqlCommand(sql, myConnection);
            if (parameters != null) cmd.Parameters.AddRange(parameters);

            return await cmd.ExecuteNonQueryAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error executing non-query sql: {SqlCommand}", sql);
            throw;
        }
    }

    public async Task ClearTable(string tableName)
    {
        await ExecuteSqlCmd($"DELETE FROM {tableName};");
    }

    private SqlConnection GetConnection()
    {
        return new SqlConnection(_connectionString);
    }

    public async Task ExecuteSqlCmdWithParameters(string sql, params SqlParameter[] parameters)
    {
        await using var myConnection = GetConnection();
        try
        {
            await myConnection.OpenAsync();
            await using var cmd = new SqlCommand(sql, myConnection);
            if (parameters != null) cmd.Parameters.AddRange(parameters);

            await cmd.ExecuteNonQueryAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error executing parameterized sql: {SqlCommand}", sql);
            throw;
        }
    }
}