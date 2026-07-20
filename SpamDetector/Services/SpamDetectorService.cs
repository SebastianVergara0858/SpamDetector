using Microsoft.EntityFrameworkCore;
using SpamDetector.Data;
using SpamDetector.Models;

namespace SpamDetector.Services
{
    public class SpamDetectorService : ISpamDetectorService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public SpamDetectorService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<bool> IsSpamAsync(string ipAddress)
        {
            int maxRequests = _configuration.GetValue<int>("RateLimitingSettings:MaxRequests");
            int windowInSeconds = _configuration.GetValue<int>("RateLimitingSettings:WindowInSeconds");

            var windowStart = DateTime.UtcNow.AddSeconds(-windowInSeconds);

            int requestCount = await _context.RequestLogs
                .CountAsync(log => log.IpAddress == ipAddress && log.Timestamp >= windowStart);

            var currentLog = new RequestLog
            {
                IpAddress = ipAddress,
                Timestamp = DateTime.UtcNow
            };
            _context.RequestLogs.Add(currentLog);
            await _context.SaveChangesAsync();

            if (requestCount >= maxRequests)
            {
                return true;
            }

            return false;
        }
    }
}