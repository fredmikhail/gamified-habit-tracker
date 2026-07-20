using HabitTracker.Api.Domain.Enums;

namespace HabitTracker.Api.Services;

public sealed class XpService
{
    private const int PrimaryRewardPercentage = 70;

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
