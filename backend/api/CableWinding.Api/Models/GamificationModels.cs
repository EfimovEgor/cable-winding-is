namespace CableWinding.Api.Models;

public record GamificationLevelDto(
    int Level,
    string Title,
    int MinXp,
    int? MaxXp,
    int ProgressPercent,
    int RemainingXp
);

public record GamificationAchievementDto(
    string Id,
    string Title,
    string Description,
    bool Unlocked
);

public record GamificationHistoryEntryDto(
    DateTimeOffset Ts,
    double Speed,
    double Tension,
    double Step,
    int Quality,
    int Points,
    int Xp,
    int StreakAfter,
    string StatusLabel
);

public record GamificationSummaryDto(
    int TotalCycles,
    int StableCycles,
    int TotalPoints,
    int TotalXp,
    int CurrentStreak,
    int BestStreak,
    int AverageQuality,
    string Recommendation,
    GamificationLevelDto CurrentLevel,
    GamificationLevelDto? NextLevel,
    IReadOnlyList<GamificationAchievementDto> Achievements
);
