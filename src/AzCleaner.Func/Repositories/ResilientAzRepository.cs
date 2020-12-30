using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzCleaner.Domain;
using Microsoft.Extensions.Logging;
using Polly;

namespace AzCleaner.Func.Repositories
{
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

        public async Task<IReadOnlyCollection<string>> GetExpiredResourcesAsync()
        {
            var resources = await _azRepository.GetExpiredResourcesAsync();
            _logger.LogTrace("Found {count} expired resources", resources.Count);
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
    }
}