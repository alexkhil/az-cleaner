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
    public Task CleanAutomaticallyAsync(
        [TimerTrigger("0 0 */12 * * *")] TimerInfo timer,
        CancellationToken cancellationToken) =>
        _azCleaner.CleanAsync(cancellationToken);

    [Function("ManualTrigger")]
    public async Task<HttpResponseData> CleanManuallyAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, nameof(HttpMethod.Delete), Route = "expiredresources")]
        HttpRequestData req,
        CancellationToken cancellationToken)
    {
        await _azCleaner.CleanAsync(cancellationToken);
        return req.CreateResponse(HttpStatusCode.OK);
    }
}