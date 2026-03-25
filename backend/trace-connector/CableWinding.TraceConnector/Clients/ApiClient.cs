using System.Net.Http.Json;
using System.Text.Json;

namespace CableWinding.TraceConnector.Clients;

public class ApiClient
{
    private readonly HttpClient _http;
    private readonly ILogger<ApiClient> _logger;
    private readonly JsonSerializerOptions _json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public ApiClient(HttpClient http, ILogger<ApiClient> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<bool> PostTelemetryParamsAsync(TelemetryParamsDto dto, CancellationToken ct = default)
    {
        try
        {
            var res = await _http.PostAsJsonAsync("/api/telemetry/params", dto, _json, ct);
            if (res.IsSuccessStatusCode)
            {
                _logger.LogDebug("Telemetry sent: Speed={Speed}, Tension={Tension}", dto.Speed, dto.Tension);
                return true;
            }
            _logger.LogWarning("API returned {StatusCode}", res.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to post telemetry");
            return false;
        }
    }
}

public record TelemetryParamsDto(DateTimeOffset Ts, double Speed, double Tension, double Step);
