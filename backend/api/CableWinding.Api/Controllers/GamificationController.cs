using CableWinding.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CableWinding.Api.Controllers;

[ApiController]
[Route("api/gamification")]
[AllowAnonymous]
public class GamificationController : ControllerBase
{
    private readonly IGamificationService _gamificationService;

    public GamificationController(IGamificationService gamificationService)
    {
        _gamificationService = gamificationService;
    }

    [HttpGet("summary")]
    public IActionResult GetSummary() => Ok(_gamificationService.GetSummary());

    [HttpGet("history")]
    public IActionResult GetHistory([FromQuery] int limit = 12) =>
        Ok(_gamificationService.GetHistory(limit));

    [HttpGet("achievements")]
    public IActionResult GetAchievements() => Ok(_gamificationService.GetAchievements());

    [HttpPost("recalculate")]
    public IActionResult Recalculate()
    {
        var summary = _gamificationService.GetSummary();
        return Ok(new
        {
            message = "Показатели геймификации пересчитаны по текущей истории телеметрии.",
            summary,
        });
    }
}
