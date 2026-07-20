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

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();