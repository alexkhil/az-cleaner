using System.Threading.Tasks;

namespace AzCleaner.Domain
{
    public interface IAzCleaner
    {
        Task CleanAsync();
    }
}