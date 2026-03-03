using GJ.GeoDynamics.Domain;

namespace GJ.GeoDynamics.Infra.Interface;

public interface ITripOverviewRepository
{
    Task InsertFromOverviewsAsync(IReadOnlyCollection<TimeSheetDayEntity> overviews, CancellationToken cancellationToken = default);

    Task RefreshSnapshotAsync(CancellationToken cancellationToken = default);
}