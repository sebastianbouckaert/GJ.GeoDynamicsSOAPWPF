using System.Data;
using GJ.GeoDynamics.Domain;
using GJ.GeoDynamics.Infra.Interface;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace GJ.GeoDynamics.Infra;

public sealed class UserRepository : IUserRepository
{
    private readonly ILogger<UserRepository> _logger;
    private readonly IDatabaseRepository _db;

    public UserRepository(ILogger<UserRepository> logger, IDatabaseRepository db)
    {
        _logger = logger;
        _db = db;
    }

    public async Task ReplaceAllAsync(IReadOnlyCollection<UserEntity> users, CancellationToken cancellationToken = default)
    {
        await _db.ClearTable("Info_Users");

        foreach (var user in users)
        {
            const string sql =
                @"INSERT INTO Info_Users (Naam, badge, code, GUID, NavCode)
                  VALUES (@Name, @Badge, @Code, @GUID, @NavCode)";

            var nameStr = user.Name ?? string.Empty;

            var parameters = new[]
            {
                new SqlParameter("@Name", SqlDbType.NVarChar, 50)
                {
                    Value = nameStr.Length > 50 ? nameStr.Substring(0, 50) : nameStr
                },
                new SqlParameter("@Badge", SqlDbType.NVarChar) { Value = user.Badge?.InternalNumber ?? string.Empty },
                new SqlParameter("@Code", SqlDbType.NVarChar) { Value = user.Code ?? string.Empty },
                new SqlParameter("@GUID", SqlDbType.NVarChar) { Value = user.Id ?? string.Empty },
                new SqlParameter("@NavCode", SqlDbType.NVarChar) { Value = user.EmployerCode ?? string.Empty }
            };

            await _db.ExecuteNonQueryAsync(sql, parameters);
        }

        try
        {
            await _db.ExecuteSqlCmd("IF OBJECT_ID('JS_Info_Users', 'U') IS NOT NULL DROP TABLE JS_Info_Users;");
            await _db.ExecuteSqlCmd("SELECT * INTO JS_Info_Users FROM Info_Users;");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error copying table JS_Info_Users");
            throw;
        }
    }
}