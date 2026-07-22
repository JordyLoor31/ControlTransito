using ClienteCamara.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddControllers();
builder.Services.AddHttpClient();

// URL inyectada por Aspire (WithReference); fallback al puerto de launchSettings
builder.Services.AddHttpClient("ApiIngesta", client =>
{
    var url = builder.Configuration["services:apiingesta:https:0"]
        ?? builder.Configuration["services:apiingesta:http:0"]
        ?? "https://localhost:7036";

    client.BaseAddress = new Uri(url);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapControllers();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();