using GJ.GeoDynamics.Domain;

namespace GJ.GeoDynamics.Infra.Interface;

public interface IClockingRepository
{
    /// <summary>
    /// Inserts the provided clockings into Info_Clockings and refreshes the JS_Info_Clockings snapshot.
    /// NOTE: This follows your current behavior (no table clear / no dedupe).
    /// </summary>
    Task InsertBatchAndSnapshotAsync(IReadOnlyCollection<ClockingEntity> clockings, CancellationToken cancellationToken = default);
}