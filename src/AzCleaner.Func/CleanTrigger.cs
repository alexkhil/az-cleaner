using System.Threading.Tasks;
using AzCleaner.Func.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace AzCleaner.Func
{
    public class CleanTrigger
    {
        private readonly IAzCleaner _azCleaner;

        public CleanTrigger(IAzCleaner azCleaner)
        {
            _azCleaner = azCleaner;
        }

        [FunctionName("AutomaticTrigger")]
        public Task CleanAutomaticallyAsync([TimerTrigger("0 0 */12 * * *")] TimerInfo timer) => 
            _azCleaner.CleanAsync();

        [Disable]
        [FunctionName("ManualTrigger")]
        public async Task<IActionResult> CleanManuallyAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, nameof(HttpMethods.Delete), Route = "expiredresources")]
            HttpRequest req)
        {
            await _azCleaner.CleanAsync();
            return new OkResult();
        }
    }
}