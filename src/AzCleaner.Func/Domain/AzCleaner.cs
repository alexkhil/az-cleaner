using System.Threading.Tasks;

namespace AzCleaner.Func.Domain;

public class AzCleaner : IAzCleaner
{
    private readonly IAzRepository _azRepository;

    public AzCleaner(IAzRepository azRepository)
    {
        _azRepository = azRepository;
    }

    public async Task CleanAsync()
    {
        var expiredResources = await _azRepository.GetExpiredResourceIdsAsync();
        await _azRepository.DeleteResourcesAsync(expiredResources);

        var expiredResourceGroups = await _azRepository.GetEmptyResourceGroupNamesAsync();
        await _azRepository.DeleteResourceGroupsAsync(expiredResourceGroups);
    }
}