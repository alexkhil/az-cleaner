using System.Net;
using AzCleaner.Func.Domain;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace AzCleaner.Func;

public class CleanTrigger
{
    private readonly IAzCleaner _azCleaner;

    public CleanTrigger(IAzCleaner azCleaner)
    {
        _azCleaner = azCleaner;
    }

    [Function("AutomaticTrigger")]
    public Task CleanAutomaticallyAsync([TimerTrigger("0 0 */12 * * *")] TimerInfo timer) =>
        _azCleaner.CleanAsync();

    [Function("ManualTrigger")]
    public async Task<HttpResponseData> CleanManuallyAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, nameof(HttpMethod.Delete), Route = "expiredresources")]
        HttpRequestData req)
    {
        await _azCleaner.CleanAsync();
        return req.CreateResponse(HttpStatusCode.OK);
    }
}