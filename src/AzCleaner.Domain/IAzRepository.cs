using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzCleaner.Domain
{
    public interface IAzRepository
    {
        Task<IReadOnlyCollection<string>> GetExpiredResourceIdsAsync();
        
        Task<IReadOnlyCollection<string>> GetEmptyResourceGroupNamesAsync();
        
        Task DeleteResourcesAsync(IEnumerable<string> resourceIds);

        Task DeleteResourceAsync(string resourceId);

        public Task DeleteResourceGroupsAsync(IEnumerable<string> resourceGroupNames);
    }
}