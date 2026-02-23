using Microsoft.Data.SqlClient;

namespace GJ.GeoDynamics.Infra.Interface;

public interface IDatabaseRepository
{
    Task ExecuteSqlCmd(string sCommand);
    Task ClearTable(string tableName);
    Task<int> ExecuteNonQueryAsync(string sql, params SqlParameter[] parameters);
}