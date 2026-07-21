using HabitTracker.Api.Domain.Enums;
using HabitTracker.Api.Domain.ValueObjects;

namespace HabitTracker.Api.Services;

public sealed class XpService
{
    private const int PrimaryRewardPercentage = 70;

    private const int AttributeBaseXp = 100;
    private const int AttributeXpGrowthPerLevel = 25;

    private const int OverallBaseXp = 200;
    private const int OverallXpGrowthPerLevel = 50;

    public IReadOnlyDictionary<AttributeType, int> CalculateRewards(
        HabitCategory category,
        HabitDifficulty difficulty)
    {
        var totalXp = GetTotalXp(difficulty);

        var (primaryAttribute, secondaryAttribute) =
            GetRewardAttributes(category);

        var primaryXp =
            totalXp * PrimaryRewardPercentage / 100;

        var secondaryXp =
            totalXp - primaryXp;

        return new Dictionary<AttributeType, int>
        {
            [primaryAttribute] = primaryXp,
            [secondaryAttribute] = secondaryXp
        };
    }

    public LevelProgress CalculateAttributeLevelProgress(
    int currentXp)
    {
        return CalculateLevelProgress(
            currentXp,
            AttributeBaseXp,
            AttributeXpGrowthPerLevel);
    }

    public LevelProgress CalculateOverallLevelProgress(
        int totalXp)
    {
        return CalculateLevelProgress(
            totalXp,
            OverallBaseXp,
            OverallXpGrowthPerLevel);
    }

    private static LevelProgress CalculateLevelProgress(
    int currentXp,
    int baseXp,
    int growthPerLevel)
    {
        if (currentXp < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(currentXp),
                currentXp,
                "XP cannot be negative.");
        }

        var level = 1;
        var xpIntoCurrentLevel = currentXp;

        var xpNeededForNextLevel =
            GetXpNeededForNextLevel(
                level,
                baseXp,
                growthPerLevel);

        while (xpIntoCurrentLevel
            >= xpNeededForNextLevel)
        {
            xpIntoCurrentLevel -=
                xpNeededForNextLevel;

            level++;

            xpNeededForNextLevel =
                GetXpNeededForNextLevel(
                    level,
                    baseXp,
                    growthPerLevel);
        }

        return new LevelProgress(
            Level: level,
            XpIntoCurrentLevel:
                xpIntoCurrentLevel,
            XpNeededForNextLevel:
                xpNeededForNextLevel);
    }

    private static int GetXpNeededForNextLevel(
        int currentLevel,
        int baseXp,
        int growthPerLevel)
    {
        return checked(
            baseXp
            + growthPerLevel
            * (currentLevel - 1));
    }

    private static int GetTotalXp(
        HabitDifficulty difficulty)
    {
        return difficulty switch
        {
            HabitDifficulty.Easy => 10,
            HabitDifficulty.Medium => 20,
            HabitDifficulty.Hard => 30,
            HabitDifficulty.Elite => 50,
            _ => throw new ArgumentOutOfRangeException(
                nameof(difficulty),
                difficulty,
                "Unsupported habit difficulty.")
        };
    }

    private static (
        AttributeType Primary,
        AttributeType Secondary)
        GetRewardAttributes(HabitCategory category)
    {
        return category switch
        {
            HabitCategory.FitnessAndMovement =>
                (AttributeType.Fitness, AttributeType.Discipline),

            HabitCategory.HealthAndRecovery =>
                (AttributeType.Vitality, AttributeType.Discipline),

            HabitCategory.LearningAndSkills =>
                (AttributeType.Mind, AttributeType.Focus),

            HabitCategory.WorkAndCareer =>
                (AttributeType.Focus, AttributeType.Discipline),

            HabitCategory.DailyLifeAndOrganization =>
                (AttributeType.Discipline, AttributeType.Focus),

            HabitCategory.MoneyAndFinance =>
                (AttributeType.Mind, AttributeType.Discipline),

            HabitCategory.RelationshipsAndCommunity =>
                (AttributeType.Social, AttributeType.Resilience),

            HabitCategory.EmotionalWellBeing =>
                (AttributeType.Resilience, AttributeType.Vitality),

            HabitCategory.SpiritualityAndPurpose =>
                (AttributeType.Purpose, AttributeType.Resilience),

            HabitCategory.CreativityAndRecreation =>
                (AttributeType.Mind, AttributeType.Vitality),

            HabitCategory.SelfControlAndBoundaries =>
                (AttributeType.Discipline, AttributeType.Resilience),

            HabitCategory.GeneralGrowth =>
                (AttributeType.Discipline, AttributeType.Mind),

            _ => throw new ArgumentOutOfRangeException(
                nameof(category),
                category,
                "Unsupported habit category.")
        };
    }
}
