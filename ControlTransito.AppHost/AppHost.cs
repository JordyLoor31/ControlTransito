var builder = DistributedApplication.CreateBuilder(args);

var serviceBus = builder
    .AddAzureServiceBus("servicebus")
    .RunAsEmulator();

var postgres = builder
    .AddPostgres("postgres")
    .WithLifetime(ContainerLifetime.Persistent);

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