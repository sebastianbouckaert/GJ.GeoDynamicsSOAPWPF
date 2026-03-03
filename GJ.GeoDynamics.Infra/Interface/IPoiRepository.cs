using GJ.GeoDynamics.Domain;

namespace GJ.GeoDynamics.Infra.Interface;

public interface IPoiRepository
{
    Task ReplaceAllAsync(IReadOnlyCollection<PoiEntity> pois, CancellationToken cancellationToken = default);
}