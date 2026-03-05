using Klarity.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<GeminiService>();

// Permitir solicitudes desde la app de escritorio (CORS)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

app.UseCors();
app.MapControllers();

Console.WriteLine("╔══════════════════════════════════════╗");
Console.WriteLine("║      Klarity API  -  Escuchando      ║");
Console.WriteLine("║      http://localhost:5050            ║");
Console.WriteLine("╚══════════════════════════════════════╝");

app.Run("http://localhost:5050");
