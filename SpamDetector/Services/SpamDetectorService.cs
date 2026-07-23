using Microsoft.EntityFrameworkCore;
using SpamDetector.Data;
using SpamDetector.Models;
using System.Text.RegularExpressions;

namespace SpamDetector.Services
{
    public class SpamDetectorService : ISpamDetectorService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        private readonly string[] _suspiciousKeywords = new[]
        {
            "ganaste", "premio", "gratis", "urgente", "haz clic aquí",
            "oferta exclusiva", "dinero fácil", "cripto", "inversión"
        };

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

            return requestCount >= maxRequests;
        }

        public SpamAnalysisResult AnalyzeText(string content)
        {
            var result = new SpamAnalysisResult();
            if (string.IsNullOrWhiteSpace(content))
            {
                return result;
            }

            int score = 0;
            string lowerContent = content.ToLower();

            foreach (var keyword in _suspiciousKeywords)
            {
                if (lowerContent.Contains(keyword))
                {
                    score += 25;
                    result.Reasons.Add($"Contiene palabra sospechosa: '{keyword}'");
                }
            }

            int uppercaseCount = content.Count(char.IsUpper);
            if (content.Length > 10 && (double)uppercaseCount / content.Length > 0.4)
            {
                score += 30;
                result.Reasons.Add("El texto contiene un porcentaje excesivo de mayúsculas.");
            }

            int linkCount = Regex.Matches(content, @"http[s]?://").Count;
            if (linkCount > 1)
            {
                score += 25;
                result.Reasons.Add("Contiene múltiples enlaces externos.");
            }

            result.Score = Math.Min(score, 100);
            result.IsSpam = result.Score >= 50; 

            return result;
        }
    }
}