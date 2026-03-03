using GJ.GeoDynamics.Domain;

namespace GJ.GeoDynamics.Infra.Interface;

public interface ILocationRepository
{
    Task InsertBatchAsync(IReadOnlyCollection<LocationEntity> locations, CancellationToken cancellationToken = default);

    Task RefreshSnapshotAsync(CancellationToken cancellationToken = default);
}