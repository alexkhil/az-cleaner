using AzCleaner.Func.Domain;
using Azure;
using Azure.Core;
using Azure.ResourceManager;
using Azure.ResourceManager.ResourceGraph;
using Azure.ResourceManager.ResourceGraph.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace AzCleaner.Func.DataAccess;

public class AzRepository : IAzRepository
{
    private readonly ILogger<AzRepository> logger;
    private readonly ArmClient client;

    public AzRepository(ILogger<AzRepository> logger, ArmClient client)
    {
        this.logger = logger;
        this.client = client;
    }

    public Task<IReadOnlyCollection<string>> GetEmptyResourceGroupNamesAsync(
        CancellationToken cancellationToken) =>
        ExecuteQueryAsync(@"
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
            | project name",
            cancellationToken);

    public Task<IReadOnlyCollection<string>> GetExpiredResourceIdsAsync(
        CancellationToken cancellationToken) =>
        ExecuteQueryAsync(@"
            Resources
            | where todatetime(['tags']['expireOn']) < now()
            | project name = id",
            cancellationToken);

    public Task DeleteResourcesAsync(
        IEnumerable<string> resourceIds,
        CancellationToken cancellationToken)
    {
        var deleteResourceTasks = resourceIds.Select(async resourceId =>
        {
            var resourceIdentifier = new ResourceIdentifier(resourceId);
            var resource = await client.GetGenericResources().GetAsync(resourceIdentifier, cancellationToken);
            logger.LogInformation("Going to delete: {resource}", resourceId);
            await resource.Value.DeleteAsync(WaitUntil.Started, cancellationToken);
        }).ToList();

        return Task.WhenAll(deleteResourceTasks);
    }

    public Task DeleteResourceGroupsAsync(
        IEnumerable<string> resourceGroupNames,
        CancellationToken cancellationToken)
    {
        var deleteGroupTasks = resourceGroupNames.Select(async resourceGroupName =>
        {
            var defaultSub = await client.GetDefaultSubscriptionAsync(cancellationToken);
            var resourceGroup = await defaultSub.GetResourceGroupAsync(resourceGroupName, cancellationToken);
            await resourceGroup.Value.DeleteAsync(WaitUntil.Started, default, cancellationToken);
        }).ToList();

        return Task.WhenAll(deleteGroupTasks);
    }

    private async Task<IReadOnlyCollection<string>> ExecuteQueryAsync(
        string query,
        CancellationToken cancellationToken)
    {
        var defaultSub = await client.GetDefaultSubscriptionAsync(cancellationToken);
        var tenants = client.GetTenants().GetAllAsync(cancellationToken);
        var expiredResources = new HashSet<string>();
        await foreach (var tenant in tenants)
        {
            var resourceQuery = new QueryContent(query)
            {
                Subscriptions = { defaultSub.Data.SubscriptionId }
            };
            var resources = await tenant.ResourcesAsync(resourceQuery, cancellationToken);
            foreach (var resource in ToResources(resources.Value))
            {
                expiredResources.Add(resource);
            }
        }

        return expiredResources;

        static IReadOnlyCollection<string> ToResources(QueryResponse response) =>
            response.Count > 0
                ? JArray.Parse(response.Data.ToString()).Select(x => x["name"].ToString()).ToList()
                : new List<string>();
    }
}
