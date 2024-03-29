namespace AzCleaner.Func.Domain;

public interface IAzRepository
{
    Task<IReadOnlyCollection<string>> GetExpiredResourceIdsAsync(CancellationToken cancellationToken);

    Task<IReadOnlyCollection<string>> GetEmptyResourceGroupNamesAsync(CancellationToken cancellationToken);

    Task DeleteResourcesAsync(IEnumerable<string> resourceIds, CancellationToken cancellationToken);

    Task DeleteResourceGroupsAsync(IEnumerable<string> resourceGroupNames, CancellationToken cancellationToken);
}