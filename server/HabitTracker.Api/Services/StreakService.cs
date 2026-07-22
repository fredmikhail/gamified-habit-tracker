using HabitTracker.Api.Domain.Enums;
using HabitTracker.Api.Domain.ValueObjects;
using HabitTracker.Api.Domain.Entities;

namespace HabitTracker.Api.Services;

public sealed class StreakService
{
    public HabitStreakResult CalculateHabitStreak(
    DateOnly currentDate,
    WeekStartDay weekStartsOn,
    IReadOnlyCollection<HabitConfigurationVersion> configurationHistory,
    IReadOnlyCollection<HabitCompletion> completions)
    {
        var orderedConfigurations =
            configurationHistory
                .Where(configuration =>
                    configuration.EffectiveFromDate <= currentDate)
                .OrderBy(configuration =>
                    configuration.EffectiveFromDate)
                .ToList();

        if (orderedConfigurations.Count == 0)
        {
            return new HabitStreakResult(0, 0);
        }

        var currentConfiguration =
            orderedConfigurations.SingleOrDefault(configuration =>
                configuration.EffectiveFromDate <= currentDate
                && (
                    configuration.EffectiveToDateExclusive == null
                    || currentDate
                        < configuration.EffectiveToDateExclusive))
            ?? throw new InvalidOperationException(
                "The habit does not have a configuration effective on the current date.");

        var currentConfigurationIndex =
            orderedConfigurations.IndexOf(
                currentConfiguration);

        var segmentStartIndex =
            currentConfigurationIndex;

        while (
            segmentStartIndex > 0
            && orderedConfigurations[segmentStartIndex - 1]
                .FrequencyType
                == currentConfiguration.FrequencyType
            && orderedConfigurations[segmentStartIndex - 1]
                .EffectiveToDateExclusive
                == orderedConfigurations[segmentStartIndex]
                    .EffectiveFromDate)
        {
            segmentStartIndex--;
        }

        var currentFrequencyConfigurations =
            orderedConfigurations
                .Skip(segmentStartIndex)
                .Take(
                    currentConfigurationIndex
                    - segmentStartIndex
                    + 1)
                .ToList();

        var frequencyEffectiveFromDate =
            currentFrequencyConfigurations[0]
                .EffectiveFromDate;

        var completedDates =
            completions
                .Where(completion =>
                    completion.HabitId
                        == currentConfiguration.HabitId
                    && completion.UndoneAtUtc == null
                    && completion.CompletedDate
                        >= frequencyEffectiveFromDate
                    && completion.CompletedDate
                        <= currentDate)
                .Select(completion =>
                    completion.CompletedDate)
                .ToList();

        return currentConfiguration.FrequencyType switch
        {
            HabitFrequencyType.Daily =>
                CalculateDailyStreak(
                    frequencyEffectiveFromDate,
                    currentDate,
                    completedDates),

            HabitFrequencyType.Weekly =>
                CalculateWeeklyStreak(
                    currentDate,
                    weekStartsOn,
                    currentFrequencyConfigurations
                        .Select(configuration =>
                            new WeeklyStreakTarget(
                                configuration.EffectiveFromDate,
                                configuration
                                    .EffectiveToDateExclusive,
                                configuration.TargetCount))
                        .ToList(),
                    completedDates),

            _ => throw new ArgumentOutOfRangeException(
                nameof(currentConfiguration.FrequencyType),
                currentConfiguration.FrequencyType,
                "Habit frequency must be a defined value.")
        };
    }

    public HabitStreakResult CalculateDailyStreak(
        DateOnly effectiveFromDate,
        DateOnly currentDate,
        IReadOnlyCollection<DateOnly> completedDates)
    {
        var validCompletedDates =
            completedDates
                .Where(date =>
                    date >= effectiveFromDate
                    && date <= currentDate)
                .Distinct()
                .OrderBy(date => date)
                .ToList();

        var completedDateSet =
            validCompletedDates.ToHashSet();

        var currentStreak = 0;

        var currentStreakDate =
            completedDateSet.Contains(currentDate)
                ? currentDate
                : currentDate.AddDays(-1);

        while (
            currentStreakDate >= effectiveFromDate
            && completedDateSet.Contains(currentStreakDate))
        {
            currentStreak++;

            currentStreakDate =
                currentStreakDate.AddDays(-1);
        }

        var longestStreak = 0;
        var runningStreak = 0;
        DateOnly? previousCompletedDate = null;

        foreach (var completedDate in validCompletedDates)
        {
            if (
                previousCompletedDate.HasValue
                && completedDate
                    == previousCompletedDate.Value.AddDays(1))
            {
                runningStreak++;
            }
            else
            {
                runningStreak = 1;
            }

            longestStreak =
                Math.Max(
                    longestStreak,
                    runningStreak);

            previousCompletedDate = completedDate;
        }

        return new HabitStreakResult(
            currentStreak,
            longestStreak);
    }

    public HabitStreakResult CalculateWeeklyStreak(
        DateOnly currentDate,
        WeekStartDay weekStartsOn,
        IReadOnlyCollection<WeeklyStreakTarget> targetHistory,
        IReadOnlyCollection<DateOnly> completedDates)
    {
        var orderedTargets =
            targetHistory
                .Where(target =>
                    target.EffectiveFromDate <= currentDate)
                .OrderBy(target =>
                    target.EffectiveFromDate)
                .ToList();

        if (orderedTargets.Count == 0)
        {
            return new HabitStreakResult(0, 0);
        }

        foreach (var target in orderedTargets)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(
                target.TargetCount,
                1);

            ArgumentOutOfRangeException.ThrowIfGreaterThan(
                target.TargetCount,
                7);
        }

        var effectiveFromDate =
            orderedTargets[0].EffectiveFromDate;

        var validCompletedDates =
            completedDates
                .Where(date =>
                    date >= effectiveFromDate
                    && date <= currentDate)
                .Distinct()
                .ToHashSet();

        var firstWeek =
            CalendarPeriodCalculator.GetWeekPeriod(
                effectiveFromDate,
                weekStartsOn);

        var currentWeek =
            CalendarPeriodCalculator.GetWeekPeriod(
                currentDate,
                weekStartsOn);

        var currentStreak = 0;
        var longestStreak = 0;
        var runningStreak = 0;

        for (
            var weekStart = firstWeek.StartDateInclusive;
            weekStart <= currentWeek.StartDateInclusive;
            weekStart = weekStart.AddDays(7))
        {
            var weekEnd =
                weekStart.AddDays(6);

            var evaluationDate =
                weekStart < effectiveFromDate
                    ? effectiveFromDate
                    : weekStart;

            var applicableTarget =
                orderedTargets.Single(target =>
                    target.EffectiveFromDate <= evaluationDate
                    && (
                        target.EffectiveToDateExclusive == null
                        || evaluationDate
                            < target.EffectiveToDateExclusive));

            var completionCount =
                validCompletedDates.Count(date =>
                    date >= weekStart
                    && date <= weekEnd);

            var isSuccessful =
                completionCount
                    >= applicableTarget.TargetCount;

            var isFirstPartialWeek =
                weekStart == firstWeek.StartDateInclusive
                && effectiveFromDate > weekStart;

            var isCurrentWeek =
                weekStart == currentWeek.StartDateInclusive;

            if (isFirstPartialWeek && !isSuccessful)
            {
                continue;
            }

            if (isCurrentWeek && !isSuccessful)
            {
                currentStreak = runningStreak;
                continue;
            }

            if (isSuccessful)
            {
                runningStreak++;

                longestStreak =
                    Math.Max(
                        longestStreak,
                        runningStreak);
            }
            else
            {
                runningStreak = 0;
            }

            currentStreak = runningStreak;
        }

        return new HabitStreakResult(
            currentStreak,
            longestStreak);
    }
}
