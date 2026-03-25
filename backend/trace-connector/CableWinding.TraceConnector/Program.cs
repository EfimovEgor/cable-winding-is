using CableWinding.TraceConnector;
using CableWinding.TraceConnector.Clients;
using CableWinding.TraceConnector.Simulators;

var builder = Host.CreateApplicationBuilder(args);

var apiBaseUrl = builder.Configuration["Api:BaseUrl"] ?? "http://localhost:5203";
builder.Services.AddHttpClient<ApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(10);
});
builder.Services.AddSingleton<TelemetrySimulator>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
