using CableWinding.Api.Models;

namespace CableWinding.Api.Services;

public interface IGamificationService
{
    GamificationSummaryDto GetSummary();
    IReadOnlyList<GamificationHistoryEntryDto> GetHistory(int limit = 12);
    IReadOnlyList<GamificationAchievementDto> GetAchievements();
}

internal sealed record EvaluatedCycle(
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

public class GamificationService : IGamificationService
{
    private const double SpeedTarget = 65.0;
    private const double TensionTarget = 3.25;
    private const double StepTarget = 1.0;
    private const double SpeedTolerance = 7.0;
    private const double TensionTolerance = 0.35;
    private const double StepTolerance = 0.12;
    private static readonly string[] LevelTitles =
    {
        "Стажер намотки",
        "Оператор линии",
        "Уверенный оператор",
        "Эксперт намотки",
        "Мастер смены",
        "Технолог намотки",
    };

    private readonly ITelemetryStore _telemetryStore;

    public GamificationService(ITelemetryStore telemetryStore)
    {
        _telemetryStore = telemetryStore;
    }

    public GamificationSummaryDto GetSummary()
    {
        var cycles = EvaluateCycles(_telemetryStore.GetHistory(120));
        var totalXp = cycles.Sum(c => c.Xp);
        var achievements = BuildAchievements(cycles, totalXp);
        var currentLevel = GetCurrentLevel(totalXp);

        return new GamificationSummaryDto(
            TotalCycles: cycles.Count,
            StableCycles: cycles.Count(c => c.Quality >= 85),
            TotalPoints: cycles.Sum(c => c.Points),
            TotalXp: totalXp,
            CurrentStreak: cycles.LastOrDefault()?.StreakAfter ?? 0,
            BestStreak: cycles.Count == 0 ? 0 : cycles.Max(c => c.StreakAfter),
            AverageQuality: cycles.Count == 0 ? 0 : (int)Math.Round(cycles.Average(c => c.Quality)),
            Recommendation: BuildRecommendation(cycles.LastOrDefault()),
            CurrentLevel: currentLevel,
            NextLevel: GetNextLevel(currentLevel.Level, totalXp),
            Achievements: achievements
        );
    }

    public IReadOnlyList<GamificationHistoryEntryDto> GetHistory(int limit = 12)
    {
        return EvaluateCycles(_telemetryStore.GetHistory(120))
            .TakeLast(Math.Clamp(limit, 1, 50))
            .Reverse()
            .Select(c => new GamificationHistoryEntryDto(
                c.Ts,
                c.Speed,
                c.Tension,
                c.Step,
                c.Quality,
                c.Points,
                c.Xp,
                c.StreakAfter,
                c.StatusLabel
            ))
            .ToList();
    }

    public IReadOnlyList<GamificationAchievementDto> GetAchievements()
    {
        var cycles = EvaluateCycles(_telemetryStore.GetHistory(120));
        return BuildAchievements(cycles, cycles.Sum(c => c.Xp));
    }

    private static IReadOnlyList<EvaluatedCycle> EvaluateCycles(IReadOnlyList<TelemetrySample> samples)
    {
        var ordered = samples.OrderBy(s => s.Ts).ToList();
        var result = new List<EvaluatedCycle>(ordered.Count);
        var streak = 0;

        foreach (var sample in ordered)
        {
            var speedScore = ScoreMetric(sample.Speed, SpeedTarget, SpeedTolerance);
            var tensionScore = ScoreMetric(sample.Tension, TensionTarget, TensionTolerance);
            var stepScore = ScoreMetric(sample.Step, StepTarget, StepTolerance);

            var quality = (int)Math.Round(speedScore * 0.4 + tensionScore * 0.35 + stepScore * 0.25);
            var stable = quality >= 85;
            streak = stable ? streak + 1 : 0;

            var points = (int)Math.Round(quality * 0.7);
            if (stable)
            {
                points += 20;
            }

            if (streak >= 3)
            {
                points += 10;
            }

            var xp = points + (quality >= 95 ? 30 : quality >= 90 ? 20 : quality >= 80 ? 12 : 5);
            result.Add(new EvaluatedCycle(
                sample.Ts,
                sample.Speed,
                sample.Tension,
                sample.Step,
                Math.Clamp(quality, 0, 100),
                points,
                xp,
                streak,
                GetStatusLabel(quality)
            ));
        }

        return result;
    }

    private static List<GamificationAchievementDto> BuildAchievements(IReadOnlyList<EvaluatedCycle> cycles, int totalXp)
    {
        return
        [
            new(
                "first_cycle",
                "Первый стабильный цикл",
                "Выполнить первую производственную операцию под контролем системы.",
                cycles.Count >= 1
            ),
            new(
                "high_quality",
                "Контроль качества",
                "Получить качество не ниже 95% хотя бы в одном цикле.",
                cycles.Any(c => c.Quality >= 95)
            ),
            new(
                "clean_streak",
                "Стабильная серия",
                "Достичь серии из 3 качественных циклов подряд.",
                cycles.Any(c => c.StreakAfter >= 3)
            ),
            new(
                "shift_pace",
                "Темп смены",
                "Набрать не менее 400 XP в рамках текущей истории операций.",
                totalXp >= 400
            ),
            new(
                "consistent_operator",
                "Уверенный оператор",
                "Поддерживать среднее качество не ниже 85%.",
                cycles.Count >= 3 && cycles.Average(c => c.Quality) >= 85
            ),
        ];
    }

    private static GamificationLevelDto GetCurrentLevel(int totalXp)
    {
        const int xpPerLevel = 180;
        var level = Math.Min(totalXp / xpPerLevel + 1, LevelTitles.Length);
        var currentBase = (level - 1) * xpPerLevel;
        var currentProgress = level == LevelTitles.Length
            ? 100
            : (int)Math.Round(((double)(totalXp - currentBase) / xpPerLevel) * 100);
        var remaining = level == LevelTitles.Length ? 0 : level * xpPerLevel - totalXp;

        return new GamificationLevelDto(
            level,
            LevelTitles[level - 1],
            currentBase,
            level == LevelTitles.Length ? null : level * xpPerLevel - 1,
            Math.Clamp(currentProgress, 0, 100),
            Math.Max(remaining, 0)
        );
    }

    private static GamificationLevelDto? GetNextLevel(int currentLevel, int totalXp)
    {
        const int xpPerLevel = 180;
        if (currentLevel >= LevelTitles.Length)
        {
            return null;
        }

        var nextLevel = currentLevel + 1;
        return new GamificationLevelDto(
            nextLevel,
            LevelTitles[nextLevel - 1],
            (nextLevel - 1) * xpPerLevel,
            nextLevel * xpPerLevel - 1,
            0,
            nextLevel * xpPerLevel - totalXp
        );
    }

    private static int ScoreMetric(double actual, double target, double tolerance)
    {
        var deviation = Math.Abs(actual - target);
        if (deviation <= tolerance)
        {
            return 100;
        }

        if (deviation >= tolerance * 3)
        {
            return 0;
        }

        return (int)Math.Round(100 - ((deviation - tolerance) / (tolerance * 2)) * 100);
    }

    private static string GetStatusLabel(int quality)
    {
        if (quality >= 92)
        {
            return "Отлично";
        }

        if (quality >= 80)
        {
            return "Стабильно";
        }

        if (quality >= 65)
        {
            return "Требует внимания";
        }

        return "Риск отклонения";
    }

    private static string BuildRecommendation(EvaluatedCycle? lastCycle)
    {
        if (lastCycle is null)
        {
            return "После поступления телеметрии система покажет уровень стабильности процесса и рекомендации.";
        }

        if (lastCycle.Quality >= 92)
        {
            return "Параметры намотки устойчивы. Сохраните текущий режим и продолжайте серию качественных циклов.";
        }

        if (lastCycle.Tension < TensionTarget - TensionTolerance || lastCycle.Tension > TensionTarget + TensionTolerance)
        {
            return "Уточните регулировку натяжения: сейчас именно этот параметр сильнее всего влияет на итоговую оценку.";
        }

        if (lastCycle.Speed < SpeedTarget - SpeedTolerance || lastCycle.Speed > SpeedTarget + SpeedTolerance)
        {
            return "Стабилизируйте скорость намотки, чтобы уменьшить отклонения и повысить качество цикла.";
        }

        if (lastCycle.Step < StepTarget - StepTolerance || lastCycle.Step > StepTarget + StepTolerance)
        {
            return "Проверьте шаг намотки и равномерность укладки, чтобы не терять очки за точность процесса.";
        }

        return "Процесс в допустимой зоне, но есть резерв по точности. Попробуйте удерживать параметры ближе к целевым значениям.";
    }
}
