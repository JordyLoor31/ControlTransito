using ApiIngesta.Data;
using ApiIngesta.Services;
using Azure.Messaging.ServiceBus;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("transitodb"));
});

builder.Services.AddSingleton<ServiceBusClient>(
    sp => new ServiceBusClient(
        builder.Configuration.GetConnectionString("servicebus")!));

builder.Services.AddSingleton<ServiceBusProducer>();

builder.Services.AddHostedService<ReintentoPendientesWorker>();

var app = builder.Build();

// Aplicar migraciones automáticamente (crea las tablas si no existen)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    for (var intento = 1; ; intento++)
    {
        try
        {
            db.Database.Migrate();
            break;
        }
        catch (Exception ex) when (intento < 10)
        {
            Console.WriteLine(
                $"BD NO DISPONIBLE (intento {intento}): {ex.Message}");

            await Task.Delay(TimeSpan.FromSeconds(3));
        }
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();