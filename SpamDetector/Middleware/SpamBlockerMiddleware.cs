using System.Threading.Tasks;
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
            // Obtener la IP remota del cliente que hace la petición
            var ipAddress = context.Connection.RemoteIpAddress?.ToString();

            if (!string.IsNullOrEmpty(ipAddress))
            {
                // Evaluar si la IP debe ser bloqueada
                bool isSpam = await spamDetectorService.IsSpamAsync(ipAddress);

                if (isSpam)
                {
                    // Si es spam, cortamos el flujo inmediatamente y devolvemos Código HTTP 429 (Too Many Requests)
                    context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.Response.ContentType = "text/plain";
                    await context.Response.WriteAsync("Acceso denegado: Demasiadas peticiones consecutivas (Deteccion de Spam).");
                    return; // Detiene el pipeline aquí mismo
                }
            }

            // Si todo está limpio, se permite que la petición continúe a la página MVC
            await _next(context);
        }
    }
}