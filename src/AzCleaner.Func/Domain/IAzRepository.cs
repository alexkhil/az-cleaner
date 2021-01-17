using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzCleaner.Func.Domain
{
    public interface IAzRepository
    {
        Task<IReadOnlyCollection<string>> GetExpiredResourceIdsAsync();
        
        Task<IReadOnlyCollection<string>> GetEmptyResourceGroupNamesAsync();
        
        Task DeleteResourcesAsync(IEnumerable<string> resourceIds);

        Task DeleteResourceAsync(string resourceId);

        Task DeleteResourceGroupsAsync(IEnumerable<string> resourceGroupNames);

        Task DeleteResourceGroupAsync(string resourceGroupName);
    }
}