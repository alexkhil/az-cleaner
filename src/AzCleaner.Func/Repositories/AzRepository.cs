using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzCleaner.Domain;
using Microsoft.Azure.Management.ResourceGraph;
using Microsoft.Azure.Management.ResourceGraph.Models;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;

namespace AzCleaner.Func.Repositories
{
    internal class AzRepository : IAzRepository
    {
        private readonly IResourceManager _resourceManager;
        private readonly IResourceGraphClient _resourceGraphClient;

        public AzRepository(IResourceManager resourceManager, IResourceGraphClient resourceGraphClient)
        {
            _resourceManager = resourceManager;
            _resourceGraphClient = resourceGraphClient;
        }
        
        public async Task<IReadOnlyCollection<string>> GetExpiredResourcesAsync()
        {
            const string query = "Resources | where todatetime(tags[\"ExpireOn\"]) < now() | project id";
            var subscriptions = new[] {_resourceManager.SubscriptionId};
            var response = await _resourceGraphClient.ResourcesAsync(new QueryRequest(subscriptions, query));
            return response.ToResources();
        }

        public Task DeleteResourcesAsync(IEnumerable<string> resourceIds) =>
            Task.WhenAll(resourceIds.Select(DeleteResourceAsync));

        public Task DeleteResourceAsync(string resourceId) =>
            _resourceManager.GenericResources.DeleteAsync(
                ResourceUtils.GroupFromResourceId(resourceId),
                ResourceUtils.ResourceProviderFromResourceId(resourceId),
                ResourceUtils.ParentResourcePathFromResourceId(resourceId) ?? string.Empty,
                ResourceUtils.ResourceTypeFromResourceId(resourceId),
                ResourceUtils.NameFromResourceId(resourceId));
    }
}