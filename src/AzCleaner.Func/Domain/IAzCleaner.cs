namespace AzCleaner.Func.Domain;

public interface IAzCleaner
{
    Task CleanAsync(CancellationToken cancellationToken);
}