using AzCleaner.Func.Domain;
using Azure;
using Azure.Core;
using Azure.ResourceManager;
using Azure.ResourceManager.ResourceGraph;
using Azure.ResourceManager.ResourceGraph.Models;
using Newtonsoft.Json.Linq;

namespace AzCleaner.Func.DataAccess;

public class AzV2Repository : IAzRepository
{
    private readonly ArmClient client;

    public AzV2Repository(ArmClient client)
    {
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

    public Task DeleteResourcesAsync(IEnumerable<string> resourceIds)
    {
        return Parallel.ForEachAsync(resourceIds, async (resourceId, ct) =>
        {
            var resourceIdentifier = new ResourceIdentifier(resourceId);
            var resource = await client.GetGenericResources().GetAsync(resourceIdentifier, ct);
            await resource.Value.DeleteAsync(WaitUntil.Started, ct);
        });
    }

    public Task DeleteResourceGroupsAsync(IEnumerable<string> resourceGroupNames)
    {
        return Parallel.ForEachAsync(resourceGroupNames, async (resourceGroupName, ct) =>
        {
            var defaultSub = await client.GetDefaultSubscriptionAsync(ct);
            var resourceGroup = await defaultSub.GetResourceGroupAsync(resourceGroupName, ct);
            await resourceGroup.Value.DeleteAsync(WaitUntil.Started, cancellationToken: CancellationToken.None);
        });
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
    }

    private static IReadOnlyCollection<string> ToResources(QueryResponse response) =>
        response.Count > 0
            ? JArray.Parse(response.Data.ToString()).Select(x => x["name"].ToString()).ToList()
            : new List<string>();
}
