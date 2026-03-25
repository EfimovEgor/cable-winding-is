using CableWinding.Api.Models;
using CableWinding.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CableWinding.Api.Controllers;

[ApiController]
[Route("api/telemetry")]
[Authorize]
public class TelemetryController : ControllerBase
{
    private readonly ITelemetryStore _store;

    public TelemetryController(ITelemetryStore store) => _store = store;

    /// <summary>Проверка доступности сервиса телеметрии</summary>
    [HttpGet("ping")]
    public IActionResult Ping() => Ok("telemetry ok");

    /// <summary>Получить последние параметры намотки (для UI мониторинга)</summary>
    [HttpGet("latest")]
    [Authorize(Roles = "Operator")]
    public IActionResult GetLatest()
    {
        var latest = _store.Latest;
        if (latest == null)
            return Ok(new { receivedAt = (DateTimeOffset?)null, params_ = (object?)null });
        return Ok(new
        {
            receivedAt = _store.LatestReceivedAt,
            params_ = new
            {
                latest.Ts,
                latest.Speed,
                latest.Tension,
                latest.Step
            }
        });
    }

    /// <summary>История телеметрии для графиков (подготовка к TRACE MODE 7)</summary>
    [HttpGet("history")]
    [Authorize(Roles = "Operator")]
    public IActionResult GetHistory([FromQuery] int limit = 120)
    {
        var samples = _store.GetHistory(Math.Min(limit, 200));
        return Ok(samples.Select(s => new { s.Ts, s.Speed, s.Tension, s.Step }));
    }

    /// <summary>Принять параметры от TraceConnector / симулятора</summary>
    [HttpPost("params")]
    [AllowAnonymous]
    public IActionResult PostParams([FromBody] TelemetryParamsDto dto)
    {
        _store.Update(dto);
        return Ok(new { receivedAt = DateTimeOffset.UtcNow, dto });
    }
}
