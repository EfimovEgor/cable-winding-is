namespace CableWinding.TraceConnector.Simulators;

public class TelemetrySimulator
{
    private readonly Random _rnd = new();

    public TelemetrySample Generate()
    {
        return new TelemetrySample
        {
            Ts = DateTimeOffset.UtcNow,
            Speed = 50 + _rnd.NextDouble() * 30,
            Tension = 2.5 + _rnd.NextDouble() * 1.5,
            Step = 0.8 + _rnd.NextDouble() * 0.4
        };
    }
}

public class TelemetrySample
{
    public DateTimeOffset Ts { get; set; }
    public double Speed { get; set; }
    public double Tension { get; set; }
    public double Step { get; set; }
}
