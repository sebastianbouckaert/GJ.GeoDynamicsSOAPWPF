using GJ.GeoDynamics.Domain;

namespace GJ.GeoDynamics.Infra.Interface;

public interface IVehicleRepository
{
    Task ReplaceAllAsync(IReadOnlyCollection<VehicleEntity> vehicles, CancellationToken cancellationToken = default);
}