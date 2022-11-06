using AzCleaner.Func.Domain;
using Microsoft.Extensions.Logging;
using Polly;

namespace AzCleaner.Func.DataAccess;

internal class ResilientAzRepository : IAzRepository
{
    private readonly IAzRepository _azRepository;
    private readonly ILogger<ResilientAzRepository> _logger;
    private readonly IAsyncPolicy _retryPolicy;

    public ResilientAzRepository(
        IAzRepository azRepository,
        ILogger<ResilientAzRepository> logger,
        IAsyncPolicy retryPolicy)
    {
        _azRepository = azRepository;
        _logger = logger;
        _retryPolicy = retryPolicy;
    }

    public async Task<IReadOnlyCollection<string>> GetExpiredResourceIdsAsync(CancellationToken cancellationToken)
    {
        var resources = await _azRepository.GetExpiredResourceIdsAsync(cancellationToken);
        _logger.LogTrace("Found {count} expired resources", resources.Count);
        return resources;
    }

    public async Task<IReadOnlyCollection<string>> GetEmptyResourceGroupNamesAsync(CancellationToken cancellationToken)
    {
        var resources = await _azRepository.GetEmptyResourceGroupNamesAsync(cancellationToken);
        _logger.LogTrace("Found {count} expired resource groups", resources.Count);
        return resources;
    }

    public Task DeleteResourcesAsync(IEnumerable<string> resourceIds) =>
        Task.WhenAll(resourceIds.Select(DeleteResourceAsync));

    public async Task DeleteResourceAsync(string resourceId)
    {
        var result = await _retryPolicy.ExecuteAndCaptureAsync(() => _azRepository.DeleteResourceAsync(resourceId));

        if (result.Outcome == OutcomeType.Successful)
        {
            _logger.LogTrace("Resource deleted {resourceId}", resourceId);
        }
        else
        {
            _logger.LogWarning("Resource wasn't deleted {resourceId}", resourceId);
        }
    }

    public Task DeleteResourceGroupsAsync(IEnumerable<string> resourceGroupNames) =>
        Task.WhenAll(resourceGroupNames.Select(DeleteResourceGroupAsync));

    public async Task DeleteResourceGroupAsync(string resourceGroupName)
    {
        var result = await _retryPolicy.ExecuteAndCaptureAsync(() => _azRepository.DeleteResourceGroupAsync(resourceGroupName));

        if (result.Outcome == OutcomeType.Successful)
        {
            _logger.LogTrace("Resource group deleted {resourceGroupName}", resourceGroupName);
        }
        else
        {
            _logger.LogWarning("Resource group wasn't deleted {resourceGroupName}", resourceGroupName);
        }
    }
}