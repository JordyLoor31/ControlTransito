using Azure.Messaging.ServiceBus;
using ApiMultas.Data;
using ApiMultas.Models;
using Shared.Contracts;
using System.Text.Json;

namespace ApiMultas.Services;

public class InfraccionesConsumer : BackgroundService
{
    private readonly ServiceBusClient _client;
    private readonly IServiceScopeFactory _scopeFactory;

    public InfraccionesConsumer(
        ServiceBusClient client,
        IServiceScopeFactory scopeFactory)
    {
        _client = client;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        Console.WriteLine("CONSUMER INICIADO");

        var processor =
            _client.CreateProcessor(
                "infracciones-velocidad");

        processor.ProcessMessageAsync += async args =>
        {
            var body =
                args.Message.Body.ToString();

            Console.WriteLine(
                $"RECIBIDO: {body}");

            var evento =
                JsonSerializer.Deserialize<InfraccionDetectadaEvent>(
                    body);

            if (evento is null)
            {
                Console.WriteLine(
                    $"MENSAJE ENVIADO A DLQ: {body}");

                await args.DeadLetterMessageAsync(
                    args.Message,
                    "JSON_INVALIDO",
                    "No se pudo deserializar el mensaje");

                return;
            }

            if (string.IsNullOrWhiteSpace(evento.Placa))
            {
                Console.WriteLine(
                    $"MENSAJE ENVIADO A DLQ: {body}");

                await args.DeadLetterMessageAsync(
                    args.Message,
                    "PLACA_INVALIDA",
                    "La placa está vacía");

                return;
            }

            if (evento.Velocidad <= 0)
            {
                Console.WriteLine(
                    $"MENSAJE ENVIADO A DLQ: {body}");

                await args.DeadLetterMessageAsync(
                    args.Message,
                    "VELOCIDAD_INVALIDA",
                    "La velocidad debe ser mayor que cero");

                return;
            }

            using var scope =
                _scopeFactory.CreateScope();

            var db =
                scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var multa = new Multa
            {
                Id = Guid.NewGuid(),
                Placa = evento.Placa,
                Valor = 150,
                FechaEmision = DateTime.UtcNow,
                Pagada = false
            };

            db.Multas.Add(multa);

            await db.SaveChangesAsync();

            Console.WriteLine(
                $"MULTA CREADA PARA {evento.Placa}");

            await args.CompleteMessageAsync(
                args.Message);
        };

        processor.ProcessErrorAsync += args =>
        {
            Console.WriteLine(
                $"ERROR: {args.Exception}");

            return Task.CompletedTask;
        };

        await processor.StartProcessingAsync(
            stoppingToken);

        await Task.Delay(
            Timeout.Infinite,
            stoppingToken);
    }
}