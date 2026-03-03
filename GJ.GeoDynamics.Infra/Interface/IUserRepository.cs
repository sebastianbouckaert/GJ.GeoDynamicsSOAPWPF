using GJ.GeoDynamics.Domain;

namespace GJ.GeoDynamics.Infra.Interface;

public interface IUserRepository
{
    Task ReplaceAllAsync(IReadOnlyCollection<UserEntity> users, CancellationToken cancellationToken = default);
}