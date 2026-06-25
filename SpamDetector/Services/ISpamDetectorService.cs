using System.Threading.Tasks;

namespace SpamDetector.Services
{
    public interface ISpamDetectorService
    {
        Task<bool> IsSpamAsync(string ipAddress);
    }
}