namespace CableWinding.Api.Models;

public record TelemetryParamsDto(
    DateTimeOffset Ts,
    double Speed,
    double Tension,
    double Step
);
