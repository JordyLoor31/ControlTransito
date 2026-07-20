using Azure.Messaging.ServiceBus;

namespace ApiMultas.Services;

public class InfraccionesConsumer : BackgroundService
{
    private readonly ServiceBusClient _client;

    public InfraccionesConsumer(ServiceBusClient client)
    {
        _client = client;
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

            await args.CompleteMessageAsync(
                args.Message);
        };

        processor.ProcessErrorAsync += args =>
        {
            Console.WriteLine(
                args.Exception.Message);

            return Task.CompletedTask;
        };

        await processor.StartProcessingAsync(
            stoppingToken);

        await Task.Delay(
            Timeout.Infinite,
            stoppingToken);
    }
}