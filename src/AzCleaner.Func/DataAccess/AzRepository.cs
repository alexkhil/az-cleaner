using AzCleaner.Func.Domain;
using Microsoft.Azure.Management.ResourceGraph;
using Microsoft.Azure.Management.ResourceGraph.Models;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;

namespace AzCleaner.Func.DataAccess;

internal class AzRepository : IAzRepository
{
    private readonly IResourceManager _resourceManager;
    private readonly IResourceGraphClient _resourceGraphClient;

    public AzRepository(IResourceManager resourceManager, IResourceGraphClient resourceGraphClient)
    {
        _resourceManager = resourceManager;
        _resourceGraphClient = resourceGraphClient;
    }

    public Task<IReadOnlyCollection<string>> GetExpiredResourceIdsAsync() =>
        ExecuteQuery(@"
            Resources
            | where todatetime(['tags']['expireOn']) < now()
            | project name = id");

    public Task<IReadOnlyCollection<string>> GetEmptyResourceGroupNamesAsync() =>
        ExecuteQuery(@"
            ResourceContainers
            | where type == ""microsoft.resources/subscriptions/resourcegroups""
            | extend rgAndSub = strcat(resourceGroup, ""--"", subscriptionId)
            | join kind=leftouter (
                Resources
                | extend rgAndSub = strcat(resourceGroup, ""--"", subscriptionId)
                | summarize count() by rgAndSub
            ) on rgAndSub
            | where isnull(count_)
            | project-away rgAndSub1, count_
            | project name");

    public Task DeleteResourcesAsync(IEnumerable<string> resourceIds) =>
        Task.WhenAll(resourceIds.Select(DeleteResourceAsync));

    public Task DeleteResourceAsync(string resourceId) =>
        _resourceManager.GenericResources.DeleteAsync(
            ResourceUtils.GroupFromResourceId(resourceId),
            ResourceUtils.ResourceProviderFromResourceId(resourceId),
            ResourceUtils.ParentResourcePathFromResourceId(resourceId) ?? string.Empty,
            ResourceUtils.ResourceTypeFromResourceId(resourceId),
            ResourceUtils.NameFromResourceId(resourceId));

    public Task DeleteResourceGroupsAsync(IEnumerable<string> resourceGroupNames) =>
        Task.WhenAll(resourceGroupNames.Select(DeleteResourceGroupAsync));

    public Task DeleteResourceGroupAsync(string resourceGroupName) =>
        _resourceManager.ResourceGroups.DeleteByNameAsync(resourceGroupName);

    private async Task<IReadOnlyCollection<string>> ExecuteQuery(string query)
    {
        var subscriptions = new[] { _resourceManager.SubscriptionId };
        var response = await _resourceGraphClient.ResourcesAsync(new QueryRequest(subscriptions, query));
        return response.ToResources();
    }
}