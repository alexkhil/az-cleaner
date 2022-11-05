using AzCleaner.Func.Domain;
using AzCleaner.Func.Tests.Infrastructure;
using Microsoft.Azure.Functions.Worker;

namespace AzCleaner.Func.Tests;

public class CleanTriggerTests
{
    [Theory, AutoMoqData]
    public async Task CleanAutomaticallyAsync_WhenCalled_CallsCleanAsync(
        [Frozen] Mock<IAzCleaner> azCleanerMock,
        TimerInfo timer,
        CleanTrigger sut)
    {
        await sut.CleanAutomaticallyAsync(timer);

        azCleanerMock.Verify(x => x.CleanAsync(), Times.Once);
    }

    [Theory, AutoMoqData]
    public async Task CleanManuallyAsync_WhenCalled_CallsCleanAsync(
        [Frozen] Mock<IAzCleaner> azCleanerMock,
        MockHttpRequestData request,
        CleanTrigger sut)
    {
        await sut.CleanManuallyAsync(request);

        azCleanerMock.Verify(x => x.CleanAsync(), Times.Once);
    }
}