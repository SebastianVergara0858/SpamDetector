using Microsoft.AspNetCore.Http;
using SpamDetector.Services;

namespace SpamDetector.Middleware
{
    public class SpamBlockerMiddleware
    {
        private readonly RequestDelegate _next;

        public SpamBlockerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ISpamDetectorService spamDetectorService)
        {
            // Capturar la IP del cliente de red de forma asíncrona
            string remoteIp = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            // Si es una petición local IPv6 loopback, la normalizamos a string plano
            if (remoteIp == "::1") remoteIp = "127.0.0.1";

            // Evaluar mediante el servicio de negocio si excede el Rate Limiting
            bool isSpam = await spamDetectorService.IsSpamAsync(remoteIp);

            if (isSpam)
            {
                // Cortocircuitar el pipeline: respondemos con 429 y no dejamos avanzar a los controladores
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.Response.ContentType = "text/plain";
                await context.Response.WriteAsync("Error 429: Demasiadas peticiones. Tráfico bloqueado por SpamDetector.");
                return; // Detiene el flujo por completo
            }

            // Si el tráfico es legítimo, continúa al siguiente componente o controlador
            await _next(context);
        }
    }
}