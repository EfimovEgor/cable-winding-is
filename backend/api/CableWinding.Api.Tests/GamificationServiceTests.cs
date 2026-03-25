using CableWinding.Api.Models;
using CableWinding.Api.Services;

namespace CableWinding.Api.Tests;

public class GamificationServiceTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public void GetSummary_ReturnsStableProgress_WhenTelemetryIsWithinTargetRange()
    {
        var store = new TelemetryStore();
        store.Update(new TelemetryParamsDto(DateTimeOffset.UtcNow.AddSeconds(-20), 65.1, 3.24, 1.0));
        store.Update(new TelemetryParamsDto(DateTimeOffset.UtcNow.AddSeconds(-10), 64.8, 3.20, 1.01));
        store.Update(new TelemetryParamsDto(DateTimeOffset.UtcNow, 65.0, 3.27, 0.99));

        var service = new GamificationService(store);

        var summary = service.GetSummary();

        Assert.Equal(3, summary.TotalCycles);
        Assert.True(summary.TotalPoints > 0);
        Assert.True(summary.TotalXp > 0);
        Assert.Equal(3, summary.StableCycles);
        Assert.True(summary.CurrentStreak >= 1);
        Assert.Contains(summary.Achievements, achievement => achievement.Id == "first_cycle" && achievement.Unlocked);
        Assert.True(summary.AverageQuality >= 85);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void GetHistory_ReturnsNewestEntriesFirst()
    {
        var store = new TelemetryStore();
        var first = DateTimeOffset.UtcNow.AddMinutes(-2);
        var second = DateTimeOffset.UtcNow.AddMinutes(-1);

        store.Update(new TelemetryParamsDto(first, 60, 3.0, 0.95));
        store.Update(new TelemetryParamsDto(second, 66, 3.3, 1.0));

        var service = new GamificationService(store);

        var history = service.GetHistory(2);

        Assert.Equal(2, history.Count);
        Assert.Equal(second, history[0].Ts);
        Assert.Equal(first, history[1].Ts);
    }
}
