namespace GJ.GeoDynamics.Infra.Interface;

public interface IUserQueryRepository
{
    Task<List<string>> GetAllUserGuidsAsync(CancellationToken cancellationToken = default);
}