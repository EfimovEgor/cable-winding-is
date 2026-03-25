using CableWinding.Api.Models;

namespace CableWinding.Api.Services;

public interface ITelemetryStore
{
    TelemetryParamsDto? Latest { get; }
    DateTimeOffset? LatestReceivedAt { get; }
    IReadOnlyList<TelemetrySample> GetHistory(int maxCount = 120);
    void Update(TelemetryParamsDto dto);
}

public record TelemetrySample(DateTimeOffset Ts, double Speed, double Tension, double Step);

public class TelemetryStore : ITelemetryStore
{
    private const int HistoryCapacity = 200;
    private readonly object _lock = new();
    private TelemetryParamsDto? _latest;
    private DateTimeOffset? _receivedAt;
    private readonly Queue<TelemetrySample> _history = new();

    public TelemetryParamsDto? Latest
    {
        get { lock (_lock) return _latest; }
    }

    public DateTimeOffset? LatestReceivedAt
    {
        get { lock (_lock) return _receivedAt; }
    }

    public IReadOnlyList<TelemetrySample> GetHistory(int maxCount = 120)
    {
        lock (_lock)
        {
            return _history.TakeLast(maxCount).ToList();
        }
    }

    public void Update(TelemetryParamsDto dto)
    {
        lock (_lock)
        {
            _latest = dto;
            _receivedAt = DateTimeOffset.UtcNow;
            _history.Enqueue(new TelemetrySample(dto.Ts, dto.Speed, dto.Tension, dto.Step));
            while (_history.Count > HistoryCapacity)
                _history.Dequeue();
        }
    }
}
