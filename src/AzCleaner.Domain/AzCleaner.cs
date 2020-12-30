using System.Threading.Tasks;

namespace AzCleaner.Domain
{
    public class AzCleaner : IAzCleaner
    {
        private readonly IAzRepository _azRepository;

        public AzCleaner(IAzRepository azRepository)
        {
            _azRepository = azRepository;
        }
        
        public async Task CleanAsync()
        {
            var expiredResources = await _azRepository.GetExpiredResourcesAsync(); 
            await _azRepository.DeleteResourcesAsync(expiredResources);
        }
    }
}