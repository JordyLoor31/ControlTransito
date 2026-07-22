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
            // Un fallo aquí (BD o broker caídos) no debe detener el host:
            // desde .NET 6 una excepción no controlada en un BackgroundService
            // apaga toda la aplicación.
            try
            {
                using var scope =
                    _scopeFactory.CreateScope();

                var db =
                    scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var pendientes =
                    await db.MensajesPendientes
                        .Where(x => !x.Procesado)
                        .OrderBy(x => x.FechaCreacion)
                        .ToListAsync(stoppingToken);

                foreach (var mensaje in pendientes)
                {
                    try
                    {
                        await _producer.SendAsync(
                            mensaje.Payload);

                        mensaje.Procesado = true;

                        await db.SaveChangesAsync(stoppingToken);

                        Console.WriteLine(
                            $"REENVIADO: {mensaje.Id}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            $"NO SE PUDO REENVIAR {mensaje.Id}: {ex.Message}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"ERROR EN WORKER DE REINTENTO: {ex.Message}");
            }

            await Task.Delay(
                TimeSpan.FromSeconds(10),
                stoppingToken);
        }
    }
}