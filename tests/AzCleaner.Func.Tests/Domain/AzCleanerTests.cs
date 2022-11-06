using AzCleaner.Func.Domain;

namespace AzCleaner.Func.Tests.Domain;

public class AzCleanerTests
{
    [Theory, AutoMoqData]
    public async Task CleanAsync_WhenCalled_DeletesExpiredResources(
        [Frozen] Mock<IAzRepository> azRepositoryMock,
        IReadOnlyCollection<string> expiredResourceIds,
        Func.Domain.AzCleaner sut)
    {
        // Arrange
        azRepositoryMock
            .Setup(x => x.GetExpiredResourceIdsAsync(CancellationToken.None))
            .ReturnsAsync(expiredResourceIds);

        azRepositoryMock
            .Setup(x => x.DeleteResourcesAsync(expiredResourceIds, CancellationToken.None))
            .Returns(Task.CompletedTask);

        // Act
        await sut.CleanAsync(CancellationToken.None);

        // Assert
        azRepositoryMock.Verify(
            x => x.DeleteResourcesAsync(expiredResourceIds, CancellationToken.None),
            Times.Once);
    }

    [Theory, AutoMoqData]
    public async Task CleanAsync_WhenCalled_DeletesEmptyResourceGroups(
        [Frozen] Mock<IAzRepository> azRepositoryMock,
        IReadOnlyCollection<string> expiredResourceGroupIds,
        Func.Domain.AzCleaner sut)
    {
        // Arrange
        azRepositoryMock
            .Setup(x => x.GetEmptyResourceGroupNamesAsync(CancellationToken.None))
            .ReturnsAsync(expiredResourceGroupIds);

        azRepositoryMock
            .Setup(x => x.DeleteResourceGroupsAsync(expiredResourceGroupIds, CancellationToken.None))
            .Returns(Task.CompletedTask);

        // Act
        await sut.CleanAsync(CancellationToken.None);

        // Assert
        azRepositoryMock.Verify(
            x => x.DeleteResourceGroupsAsync(expiredResourceGroupIds, CancellationToken.None),
            Times.Once);
    }
}