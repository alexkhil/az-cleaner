using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using AzCleaner.Func.Domain;
using Moq;
using Xunit;

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
            .Setup(x => x.GetExpiredResourceIdsAsync())
            .ReturnsAsync(expiredResourceIds);

        azRepositoryMock
            .Setup(x => x.DeleteResourcesAsync(expiredResourceIds))
            .Returns(Task.CompletedTask);

        // Act
        await sut.CleanAsync();

        // Assert
        azRepositoryMock.Verify(
            x => x.DeleteResourcesAsync(expiredResourceIds),
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
            .Setup(x => x.GetEmptyResourceGroupNamesAsync())
            .ReturnsAsync(expiredResourceGroupIds);

        azRepositoryMock
            .Setup(x => x.DeleteResourceGroupsAsync(expiredResourceGroupIds))
            .Returns(Task.CompletedTask);

        // Act
        await sut.CleanAsync();

        // Assert
        azRepositoryMock.Verify(
            x => x.DeleteResourceGroupsAsync(expiredResourceGroupIds),
            Times.Once);
    }
}