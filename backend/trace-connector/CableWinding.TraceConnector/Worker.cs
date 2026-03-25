using CableWinding.TraceConnector.Clients;
using CableWinding.TraceConnector.Simulators;

namespace CableWinding.TraceConnector;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly ApiClient _api;
    private readonly TelemetrySimulator _simulator;

    public Worker(ILogger<Worker> logger, ApiClient api, TelemetrySimulator simulator)
    {
        _logger = logger;
        _api = api;
        _simulator = simulator;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TraceConnector started. Sending telemetry to API every 2 seconds.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var sample = _simulator.Generate();
                var dto = new TelemetryParamsDto(sample.Ts, sample.Speed, sample.Tension, sample.Step);
                var ok = await _api.PostTelemetryParamsAsync(dto, stoppingToken);
                if (!ok)
                    _logger.LogWarning("Failed to post telemetry");
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in telemetry loop");
            }

            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        }
    }
}
