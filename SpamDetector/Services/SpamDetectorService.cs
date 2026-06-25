using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SpamDetector.Data;
using SpamDetector.Models;

namespace SpamDetector.Services
{
    public class SpamDetectorService : ISpamDetectorService
    {
        private readonly ApplicationDbContext _context;
        private const int MaxRequests = 10;
        private const int TimeWindowSeconds = 30; // Ventana de tiempo configurable

        public SpamDetectorService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsSpamAsync(string ipAddress)
        {
            var now = DateTime.UtcNow;
            var timeThreshold = now.AddSeconds(-TimeWindowSeconds);

            // 1. Registrar la petición actual en la base de datos
            var log = new RequestLog
            {
                IpAddress = ipAddress,
                Timestamp = now
            };
            _context.RequestLogs.Add(log);
            await _context.SaveChangesAsync();

            // 2. Contar cuántas peticiones ha hecho esta IP en la ventana de tiempo
            int requestCount = await _context.RequestLogs
                .Where(r => r.IpAddress == ipAddress && r.Timestamp >= timeThreshold)
                .CountAsync();

            // 3. Si tiene 10 o más, se activa el bloqueo de Spam
            return requestCount >= MaxRequests;
        }
    }
}