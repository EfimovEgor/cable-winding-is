using System.Net.Http.Json;
using CableWinding.Api.Models;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CableWinding.Api.Tests;

public class GamificationApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public GamificationApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task SummaryEndpoint_ReturnsCalculatedData()
    {
        using var client = _factory.CreateClient();

        await client.PostAsJsonAsync("/api/telemetry/params", new TelemetryParamsDto(DateTimeOffset.UtcNow.AddSeconds(-15), 64.9, 3.2, 1.0));
        await client.PostAsJsonAsync("/api/telemetry/params", new TelemetryParamsDto(DateTimeOffset.UtcNow.AddSeconds(-10), 65.2, 3.3, 1.01));
        await client.PostAsJsonAsync("/api/telemetry/params", new TelemetryParamsDto(DateTimeOffset.UtcNow.AddSeconds(-5), 64.7, 3.18, 0.99));

        var response = await client.GetAsync("/api/gamification/summary");

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<GamificationSummaryDto>();

        Assert.NotNull(payload);
        Assert.True(payload.TotalCycles >= 3);
        Assert.True(payload.TotalPoints > 0);
        Assert.NotEmpty(payload.Achievements);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task HistoryAndAchievementsEndpoints_ReturnExpectedCollections()
    {
        using var client = _factory.CreateClient();

        await client.PostAsJsonAsync("/api/telemetry/params", new TelemetryParamsDto(DateTimeOffset.UtcNow, 58.0, 2.7, 0.84));

        var historyResponse = await client.GetAsync("/api/gamification/history?limit=5");
        var achievementsResponse = await client.GetAsync("/api/gamification/achievements");

        historyResponse.EnsureSuccessStatusCode();
        achievementsResponse.EnsureSuccessStatusCode();

        var history = await historyResponse.Content.ReadFromJsonAsync<List<GamificationHistoryEntryDto>>();
        var achievements = await achievementsResponse.Content.ReadFromJsonAsync<List<GamificationAchievementDto>>();

        Assert.NotNull(history);
        Assert.NotNull(achievements);
        Assert.NotEmpty(history);
        Assert.NotEmpty(achievements);
    }
}
