using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzCleaner.Domain
{
    public interface IAzRepository
    {
        Task<IReadOnlyCollection<string>> GetExpiredResourcesAsync();
        
        Task DeleteResourcesAsync(IEnumerable<string> resourceIds);
        
        Task DeleteResourceAsync(string resourceId);
    }
}