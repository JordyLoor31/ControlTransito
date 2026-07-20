using Azure.Messaging.ServiceBus;

namespace ApiIngesta.Services;

public class ServiceBusProducer
{
    private readonly ServiceBusClient _client;

    public ServiceBusProducer(ServiceBusClient client)
    {
        _client = client;
    }

    public async Task SendAsync(string json)
    {
        var sender = _client.CreateSender(
            "infracciones-velocidad");

        await sender.SendMessageAsync(
            new ServiceBusMessage(json));
    }
}