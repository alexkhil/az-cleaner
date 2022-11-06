using System.Net;
using AzCleaner.Func.Domain;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace AzCleaner.Func;

public class TestFunc
{
    private readonly ILogger<TestFunc> logger;
    private readonly IAzRepository azRepository;

    public TestFunc(ILogger<TestFunc> logger, IAzRepository azRepository)
    {
        this.logger = logger;
        this.azRepository = azRepository;
    }

    [Function(nameof(TestFunc))]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, nameof(HttpMethod.Get), Route = "tags")]
        HttpRequestData req,
        CancellationToken cancellationToken)
    {
        var expiredResources = await azRepository.GetExpiredResourceIdsAsync(cancellationToken);
        var emptyResourceGroups = await azRepository.GetEmptyResourceGroupNamesAsync(cancellationToken);

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(new
        {
            expiredResources = expiredResources,
            emptyResourceGroups = emptyResourceGroups
        }, cancellationToken);

        return response;
    }
}
