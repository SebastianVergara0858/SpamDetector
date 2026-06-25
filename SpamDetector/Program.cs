using SpamDetector.Middleware;
using Microsoft.EntityFrameworkCore;
using SpamDetector.Data;
using SpamDetector.Middleware;
using SpamDetector.Services;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
// Registrar el detector de spam en el contenedor de dependencias
builder.Services.AddScoped<ISpamDetectorService, SpamDetectorService>();
var app = builder.Build();
app.UseMiddleware<SpamBlockerMiddleware>();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
