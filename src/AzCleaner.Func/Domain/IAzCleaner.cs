using System.Threading.Tasks;

namespace AzCleaner.Func.Domain;

public interface IAzCleaner
{
    Task CleanAsync();
}