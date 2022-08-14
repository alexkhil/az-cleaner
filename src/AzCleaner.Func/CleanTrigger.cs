using System.Net;
using AzCleaner.Func.Domain;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace AzCleaner.Func;

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

    [FunctionName("ManualTrigger")]
    public async Task<HttpResponseData> CleanManuallyAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, nameof(HttpMethod.Delete), Route = "expiredresources")]
        HttpRequestData req)
    {
        await _azCleaner.CleanAsync();
        return req.CreateResponse(HttpStatusCode.OK);
    }
}