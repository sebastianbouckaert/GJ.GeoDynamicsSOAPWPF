namespace GeoDynamics.Functions;

public interface IGeodynamicsTransferService
{
    Task TransferAllForce(DateTime startDate, DateTime endDate);
}