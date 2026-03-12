using System.Text;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GeoDynamics.Functions;

public class SqlProcessorFunction
{
    private readonly ILogger _logger;
    private readonly IGeodynamicsTransferService _transferService;

    public SqlProcessorFunction(ILoggerFactory loggerFactory, IGeodynamicsTransferService transferService)
    {
        _logger = loggerFactory.CreateLogger<SqlProcessorFunction>();
        _transferService = transferService;
    }

    [Function("ProcessSqlTasks")]
    public async Task Run([TimerTrigger("%MyTimerSchedule%")] TimerInfo myTimer)
    {
        _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

        try
        {
            var today = DateTime.UtcNow.AddDays(-1).Date;
            await _transferService.TransferAllForce(today, today);

            _logger.LogInformation("SQL tasks processed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, new StringBuilder().Append("Error processing SQL tasks.").ToString());
        }

        _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus?.Next}");
    }
}