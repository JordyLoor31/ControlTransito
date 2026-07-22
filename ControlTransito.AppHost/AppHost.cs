var builder = DistributedApplication.CreateBuilder(args);

var serviceBus = builder
    .AddAzureServiceBus("servicebus")
    .RunAsEmulator();

serviceBus.AddServiceBusQueue(
    "infracciones-velocidad",
    "infracciones-velocidad");

var postgres = builder
    .AddPostgres("postgres")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithPgAdmin(o => o.WithHostPort(5555));

var db = postgres.AddDatabase("transitodb");

var apiIngesta = builder.AddProject<Projects.ApiIngesta>("apiingesta")
    .WithReference(serviceBus)
    .WithReference(db)
    .WaitFor(serviceBus)
    .WaitFor(db);

var apiMultas = builder.AddProject<Projects.ApiMultas>("apimultas")
    .WithReference(serviceBus)
    .WithReference(db)
    .WaitFor(serviceBus)
    .WaitFor(db);

builder.AddProject<Projects.ClienteCamara>("clientecamara")
    .WithReference(apiIngesta)
    .WaitFor(apiIngesta);

builder.AddProject<Projects.PortalCiudadano>("portalciudadano")
    .WithReference(apiMultas)
    .WaitFor(apiMultas);

builder.Build().Run();