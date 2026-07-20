using ApiIngesta.Data;
using Azure.Messaging.ServiceBus;
using Microsoft.EntityFrameworkCore;

namespace ApiIngesta.Services;

public class ReintentoPendientesWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ServiceBusProducer _producer;

    public ReintentoPendientesWorker(
        IServiceScopeFactory scopeFactory,
        ServiceBusProducer producer)
    {
        _scopeFactory = scopeFactory;
        _producer = producer;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope =
                _scopeFactory.CreateScope();

            var db =
                scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var pendientes =
                await db.MensajesPendientes
                    .Where(x => !x.Procesado)
                    .OrderBy(x => x.FechaCreacion)
                    .ToListAsync();

            foreach (var mensaje in pendientes)
            {
                try
                {
                    await _producer.SendAsync(
                        mensaje.Payload);

                    mensaje.Procesado = true;

                    await db.SaveChangesAsync();

                    Console.WriteLine(
                        $"REENVIADO: {mensaje.Id}");
                }
                catch
                {
                }
            }

            await Task.Delay(
                TimeSpan.FromSeconds(10),
                stoppingToken);
        }
    }
}