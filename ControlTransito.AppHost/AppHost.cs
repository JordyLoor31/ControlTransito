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

builder.AddProject<Projects.ApiIngesta>("apiingesta")
    .WithReference(serviceBus)
    .WithReference(db);

builder.AddProject<Projects.ApiMultas>("apimultas")
    .WithReference(serviceBus)
    .WithReference(db);

builder.AddProject<Projects.ClienteCamara>("clientecamara");

builder.AddProject<Projects.PortalCiudadano>("portalciudadano");

builder.Build().Run();