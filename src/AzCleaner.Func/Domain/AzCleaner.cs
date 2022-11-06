namespace AzCleaner.Func.Domain;

public class AzCleaner : IAzCleaner
{
    private readonly IAzRepository _azRepository;

    public AzCleaner(IAzRepository azRepository)
    {
        _azRepository = azRepository;
    }

    public async Task CleanAsync(CancellationToken cancellationToken)
    {
        var expiredResources = await _azRepository.GetExpiredResourceIdsAsync(cancellationToken);
        await _azRepository.DeleteResourcesAsync(expiredResources);

        var expiredResourceGroups = await _azRepository.GetEmptyResourceGroupNamesAsync(cancellationToken);
        await _azRepository.DeleteResourceGroupsAsync(expiredResourceGroups);
    }
}