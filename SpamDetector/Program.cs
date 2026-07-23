using Microsoft.EntityFrameworkCore;
using SpamDetector.Data;
using SpamDetector.Middleware;
using SpamDetector.Services;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ISpamDetectorService, SpamDetectorService>();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.ContentType = "application/json";

        var response = new
        {
            success = false,
            message = "Has realizado demasiadas peticiones. Por favor, inténtalo de nuevo en unos momentos.",
            errorCode = 429
        };

        await context.HttpContext.Response.WriteAsJsonAsync(response, cancellationToken);
    };

});


var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseRateLimiter();

app.UseMiddleware<SpamBlockerMiddleware>();

app.UseAuthorization();

app.MapRazorPages();

app.Run();