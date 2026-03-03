namespace GJ.GeoDynamics.Infra.Interface;

public interface IVehicleQueryRepository
{
    Task<List<string>> GetAllVehicleIdsAsync(CancellationToken cancellationToken = default);
}